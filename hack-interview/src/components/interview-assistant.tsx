"use client"

import React, { useState, useEffect, useCallback, useRef } from "react"
import dynamic from 'next/dynamic'
import { Card, CardContent, CardHeader, CardTitle, CardFooter } from "@/components/ui/card"
import { ScrollArea } from "@/components/ui/scroll-area"
import { Input } from "@/components/ui/input"
import { Button } from "@/components/ui/button"
import { Mic, Send, Clock } from "lucide-react"

// Importazione dinamica di HubConnectionBuilder per evitare problemi di SSR
const HubConnectionBuilder = dynamic(() => 
  import("@microsoft/signalr").then((module) => module.HubConnectionBuilder),
  { ssr: false }
)

interface ChatMessage {
  role: "interviewer" | "assistant";
  content: string;
}

interface AudioDevice {
  id: number;
  name: string;
}

const InterviewAssistant: React.FC = () => {
  const [isRecording, setIsRecording] = useState(false)
  const [partialTranscription, setPartialTranscription] = useState("")
  const [finalTranscription, setFinalTranscription] = useState("")
  const [chatHistory, setChatHistory] = useState<ChatMessage[]>([])
  const [inputMessage, setInputMessage] = useState("")
  const [isLoading, setIsLoading] = useState(false)
  const [error, setError] = useState<string | null>(null)
  const [audioDevices, setAudioDevices] = useState<AudioDevice[]>([])
  const [selectedDeviceId, setSelectedDeviceId] = useState<number>(-1)
  const hubConnectionRef = useRef<any>(null)
  const scrollAreaRef = useRef<HTMLDivElement>(null)

  useEffect(() => {
    let isMounted = true
    const initializeHub = async () => {
      if (typeof window === 'undefined') return // Verifica se siamo nel browser

      try {
        const HubConnection = (await import("@microsoft/signalr")).HubConnectionBuilder
        const hubConnection = new HubConnection()
          .withUrl(process.env.NEXT_PUBLIC_TRANSCRIPTION_HUB_URL || "http://localhost:5800/transcriptionHub")
          .withAutomaticReconnect()
          .build()

        hubConnection.on("TranscriptionStarted", () => {
          if (isMounted) {
            setIsRecording(true)
            setPartialTranscription("")
            // setFinalTranscription("")
          }
        })

        hubConnection.on("TranscriptionStopped", () => {
          if (isMounted) {
            setIsRecording(false)
            if (finalTranscription.trim()) {
              handleSendMessage(finalTranscription.trim())
            }
            setPartialTranscription("")
            setFinalTranscription("")
          }
        })

        hubConnection.on("ReceivePartialTranscription", (transcriptionText: string) => {
          if (isMounted) {
            setPartialTranscription(transcriptionText)
          }
        })

        hubConnection.on("ReceiveTranscription", (transcriptionText: string) => {
          if (isMounted) {
            setFinalTranscription(transcriptionText)
          }
        })

        hubConnection.on("TranscriptionError", (errorMessage: string) => {
          if (isMounted) {
            setError(errorMessage)
          }
        })
      
        await hubConnection.start()
        console.log("Connected to TranscriptionHub")
        hubConnectionRef.current = hubConnection

        // Fetch available audio devices
        const devices = await hubConnection.invoke("GetAvailableAudioDevices")
        if (isMounted) {
          setAudioDevices(devices.map((device: string) => {
            const [id, name] = device.split(': ')
            return { id: parseInt(id), name }
          }))
        }
      } catch (err) {
        console.error("Error connecting to hub:", err)
        if (isMounted) {
          setError("Error connecting to transcription service. Please try again later.")
        }
      }
    }

    initializeHub()

    return () => {
      isMounted = false
      if (hubConnectionRef.current) {
        hubConnectionRef.current.stop()
      }
    }
  }, [])

  const toggleRecording = useCallback(async () => {
    if (!hubConnectionRef.current) {
      console.error("Hub connection not established");
      setError("Transcription service is not connected. Please try again.")
      return
    }
  
    try {
      console.log("Attempting to toggle recording. Current state:", isRecording);
      if (isRecording) {
        console.log("Invoking StopTranscription");
        await hubConnectionRef.current.invoke("StopTranscription")
      } else {
        console.log("Invoking StartTranscription with device ID:", selectedDeviceId);
        await hubConnectionRef.current.invoke("StartTranscription", selectedDeviceId)
      }
      console.log("Transcription toggled successfully");
    } catch (err) {
      console.error("Error toggling transcription:", err)
      setError(`Error managing transcription: ${err.message}`)
    }
  }, [isRecording, selectedDeviceId])

  const handleSendMessage = useCallback(async (message: string) => {
    if (message.trim()) {
      setIsLoading(true)
      setChatHistory(prev => [...prev, { role: "interviewer", content: message }])
      setInputMessage("")
      setPartialTranscription("")
      setFinalTranscription("")

      try {
        // Simuliamo una chiamata API al backend
        const response = await new Promise(resolve => setTimeout(() => resolve({
          content: "Questa è una risposta simulata dell'assistente. In un'implementazione reale, qui riceveresti la risposta dal tuo backend."
        }), 1000))

        setChatHistory(prev => [...prev, { role: "assistant", content: (response as any).content }])
      } catch (error) {
        console.error("Error getting assistant response:", error)
        setError("Failed to get assistant response. Please try again.")
      } finally {
        setIsLoading(false)
      }
    }
  }, [])

  useEffect(() => {
    if (scrollAreaRef.current) {
      scrollAreaRef.current.scrollTop = scrollAreaRef.current.scrollHeight
    }
  }, [chatHistory])

  return (
    <div className="container mx-auto p-4 bg-gray-900 text-gray-100 min-h-screen">
      <h1 className="text-3xl font-bold mb-6 text-center">Assistente per Colloquio Senior .NET Developer</h1>
      <Card className="bg-gray-800 text-gray-100">
        <CardHeader className="flex flex-row items-center justify-between">
          <CardTitle>Trascrizione e Suggerimenti</CardTitle>
          <div className="flex items-center space-x-2">
            <Clock className="h-4 w-4" />
            <span>00:00</span>
          </div>
        </CardHeader>
        <CardContent className="space-y-4">
          <ScrollArea className="h-[400px] w-full rounded-md border p-4" ref={scrollAreaRef}>
            {chatHistory.map((msg, index) => (
              <div key={index} className={`mb-4 ${msg.role === "interviewer" ? "text-blue-400" : "text-green-400"}`}>
                <strong>{msg.role === "interviewer" ? "Intervistatore: " : "Assistente: "}</strong>
                <p>{msg.content}</p>
              </div>
            ))}
            {isLoading && <div className="text-yellow-400">L'assistente sta scrivendo...</div>}
            {finalTranscription && (
              <div className="mb-4 text-blue-400">
                <strong>Intervistatore: </strong>
                <p>{finalTranscription}</p>
              </div>
            )}
          </ScrollArea>
          {error && (
            <div className="text-red-500 text-sm">
              <p>{error}</p>
            </div>
          )}
          <div className="flex items-center space-x-2">
            <select
              className="bg-gray-700 text-white rounded p-2"
              onChange={(e) => setSelectedDeviceId(parseInt(e.target.value))}
              value={selectedDeviceId}
            >
              <option value="-1">Seleziona dispositivo</option>
              {audioDevices.map((device) => (
                <option key={device.id} value={device.id}>
                  {device.name}
                </option>
              ))}
            </select>
            <Button onClick={toggleRecording} variant={isRecording ? "destructive" : "default"}>
              <Mic className="mr-2 h-4 w-4" />
              {isRecording ? "Stop" : "Start"} Recording
            </Button>
            <Input 
              placeholder="Trascrizione in tempo reale..." 
              className="flex-grow"
              value={partialTranscription}
              readOnly
            />
          </div>
        </CardContent>
        <CardFooter>
          <div className="flex w-full items-center space-x-2">
            <Input
              type="text"
              placeholder="Scrivi la tua domanda qui..."
              value={inputMessage}
              onChange={(e) => setInputMessage(e.target.value)}
              className="flex-grow"
              onKeyPress={(e) => e.key === 'Enter' && handleSendMessage(inputMessage)}
            />
            <Button onClick={() => handleSendMessage(inputMessage)} disabled={isLoading}>
              <Send className="h-4 w-4" />
            </Button>
          </div>
        </CardFooter>
      </Card>
    </div>
  )
}

export default InterviewAssistant
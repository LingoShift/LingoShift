using System;
using System.IO;
using NAudio.Wave;
using Vosk;
using Newtonsoft.Json.Linq;

public class VoiceActivator
{
    private VoskRecognizer recognizer;
    private WaveInEvent waveIn;
    private bool isActivated = false;
    private bool isListening = true;
    private string activationPhrase;
    private string deactivationPhrase;
    private MemoryStream audioBuffer = new MemoryStream();
    private DateTime lastAudioTime = DateTime.Now;
    private int bufferIntervalMs = 500; // Intervallo di analisi in millisecondi

    public event Action OnActivation;
    public event Action<string> OnTranscription;
    public event Action OnDeactivation;

    public VoiceActivator(string modelPath, string activationPhrase, string deactivationPhrase)
    {
        this.activationPhrase = activationPhrase.ToLowerInvariant();
        this.deactivationPhrase = deactivationPhrase.ToLowerInvariant();

        Vosk.Vosk.SetLogLevel(-1);

        var model = new Model(modelPath);
        recognizer = new VoskRecognizer(model, 16000.0f);

        waveIn = new WaveInEvent();

        // Imposta la frequenza di campionamento
        waveIn.WaveFormat = new WaveFormat(16000, 16, 1);

        waveIn.DataAvailable += WaveIn_DataAvailable;
        waveIn.RecordingStopped += WaveIn_RecordingStopped;
    }

    public void Start()
    {
        waveIn.StartRecording();
    }

    public void Stop()
    {
        waveIn.StopRecording();
        waveIn.Dispose();
        recognizer.Dispose();
    }

    public void StopListening()
    {
        isListening = false;
        isActivated = false;
    }

    public void StartListening()
    {
        isListening = true;
    }

    public void ResetActivation()
    {
        isActivated = false;
    }

    private void WaveIn_RecordingStopped(object sender, StoppedEventArgs e)
    {
        if (e.Exception != null)
        {
            Console.WriteLine("Errore durante la registrazione: " + e.Exception.Message);
        }
    }

    private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
    {
        try
        {
            // Accumula l'audio nel buffer
            audioBuffer.Write(e.Buffer, 0, e.BytesRecorded);

            // Se è passato un intervallo di tempo specifico, analizza l'audio
            if ((DateTime.Now - lastAudioTime).TotalMilliseconds > bufferIntervalMs)
            {
                var buffer = audioBuffer.ToArray();
                audioBuffer.SetLength(0); // Resetta il buffer
                lastAudioTime = DateTime.Now;

                if (recognizer.AcceptWaveform(buffer, buffer.Length))
                {
                    var result = recognizer.Result();
                    ProcessResult(result);
                }
                else
                {
                    var partialResult = recognizer.PartialResult();
                    ProcessPartialResult(partialResult);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine("Errore durante l'elaborazione dell'audio: " + ex.Message);
        }
    }

    private void ProcessResult(string result)
    {
        var json = JObject.Parse(result);
        var text = (string)json["text"];

        if (!string.IsNullOrEmpty(text) && isListening)
        {
            text = text.ToLowerInvariant();

            if (!isActivated)
            {
                if (text.Contains(activationPhrase))
                {
                    isActivated = true;
                    OnActivation?.Invoke();
                }
            }
            else
            {
                if (text.Contains(deactivationPhrase))
                {
                    isActivated = false;
                    OnDeactivation?.Invoke();
                }
                else
                {
                    OnTranscription?.Invoke(text);
                }
            }
        }
    }

    private void ProcessPartialResult(string partialResult)
    {
        // Puoi implementare la gestione dei risultati parziali se necessario
    }
}

using LingoShift.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Speech.Recognition;
using System.Threading.Tasks;

namespace LingoShift.Services
{
    public class SpeechRecognitionService : ITranscriptionService, IDisposable
    {
        private readonly ILogger<SpeechRecognitionService> _logger;
        private readonly IHubContext<TranscriptionHub> _hubContext;
        private readonly ConcurrentDictionary<string, SpeechRecognitionEngine> _recognizers = new();


        public SpeechRecognitionService(ILogger<SpeechRecognitionService> logger, IHubContext<TranscriptionHub> hubContext)
        {
            _logger = logger;
            _hubContext = hubContext;
        }

        public Task StartTranscriptionAsync(string connectionId)
        {
            if (_recognizers.TryGetValue(connectionId, out var existingRecognizer))
            {
                _logger.LogWarning($"Transcription already started for connection {connectionId}");
                return Task.CompletedTask;
            }

            var recognizer = new SpeechRecognitionEngine(new System.Globalization.CultureInfo("it-IT"));
            recognizer.LoadGrammar(new DictationGrammar());

            recognizer.SpeechRecognized += async (s, e) =>
            {
                await _hubContext.Clients.Client(connectionId).SendAsync("ReceiveTranscription", e.Result.Text);
            };

            recognizer.SpeechHypothesized += async (s, e) =>
            {
                await _hubContext.Clients.Client(connectionId).SendAsync("ReceivePartialTranscription", e.Result.Text);
            };

            recognizer.SetInputToDefaultAudioDevice();
            recognizer.RecognizeAsync(RecognizeMode.Multiple);

            _recognizers[connectionId] = recognizer;
            _logger.LogInformation($"Started transcription for connection {connectionId}");

            return Task.CompletedTask;
        }

        public Task StopTranscriptionAsync(string connectionId)
        {
            if (_recognizers.TryRemove(connectionId, out var recognizer))
            {
                recognizer.RecognizeAsyncStop();
                recognizer.Dispose();
                _logger.LogInformation($"Stopped transcription for connection {connectionId}");
            }
            else
            {
                _logger.LogWarning($"No active transcription found for connection {connectionId}");
            }

            return Task.CompletedTask;
        }

        public void Dispose()
        {
            foreach (var recognizer in _recognizers.Values)
            {
                recognizer.RecognizeAsyncStop();
                recognizer.Dispose();
            }
            _recognizers.Clear();
        }

        public Task StartTranscriptionAsync(string connectionId, int deviceId)
        {
            throw new NotImplementedException();
        }

        public List<(int DeviceId, string DeviceName)> GetAvailableAudioDevices()
        {
            throw new NotImplementedException();
        }
    }
}

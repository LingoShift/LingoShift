using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using NAudio.Wave;
using Newtonsoft.Json.Linq;
using Vosk;
using Microsoft.AspNetCore.SignalR;
using LingoShift.Hubs;

namespace LingoShift.Services
{
    public class VoskTranscriptionService : ITranscriptionService, IDisposable
    {
        private readonly ILogger<VoskTranscriptionService> _logger;
        private readonly IHubContext<TranscriptionHub> _hubContext;
        private VoskRecognizer _recognizer;
        private IWaveIn _waveIn;
        private readonly object _lock = new object();
        private bool _isTranscribing = false;
        private bool _disposed = false;
        private readonly string _modelPath;
        private string _currentConnectionId;

        public VoskTranscriptionService(ILogger<VoskTranscriptionService> logger, IHubContext<TranscriptionHub> hubContext, string modelPath)
        {
            _logger = logger;
            _hubContext = hubContext;
            _modelPath = modelPath;
            Vosk.Vosk.SetLogLevel(-1);
            InitializeRecognizer();
        }

        private void InitializeRecognizer()
        {
            var model = new Model(_modelPath);
            _recognizer = new VoskRecognizer(model, 16000.0f);
        }

        public Task StartTranscriptionAsync(string connectionId, int deviceId)
        {
            lock (_lock)
            {
                if (_isTranscribing)
                {
                    _logger.LogWarning($"Transcription already in progress. Stopping previous transcription.");
                    StopTranscriptionAsync(_currentConnectionId).Wait();
                }

                _currentConnectionId = connectionId;

                try
                {
                    _waveIn = deviceId == -1 ? new WaveInEvent() : new WaveInEvent { DeviceNumber = deviceId };
                    _waveIn.WaveFormat = new WaveFormat(16000, 16, 1);
                    _waveIn.DataAvailable += WaveIn_DataAvailable;
                    _waveIn.RecordingStopped += WaveIn_RecordingStopped;

                    _waveIn.StartRecording();
                    _isTranscribing = true;
                    _logger.LogInformation($"Started transcription for connection {connectionId} using device {deviceId}");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to start recording for connection {connectionId} using device {deviceId}");
                    throw;
                }
            }

            return Task.CompletedTask;
        }

        public Task StopTranscriptionAsync(string connectionId)
        {
            lock (_lock)
            {
                if (!_isTranscribing || _currentConnectionId != connectionId)
                {
                    _logger.LogWarning($"No active transcription found for connection {connectionId}");
                    return Task.CompletedTask;
                }

                _waveIn?.StopRecording();
                _waveIn?.Dispose();
                _waveIn = null;
                _isTranscribing = false;
                _currentConnectionId = null;
                _logger.LogInformation($"Stopped transcription for connection {connectionId}");
            }

            return Task.CompletedTask;
        }

        public List<(int DeviceId, string DeviceName)> GetAvailableAudioDevices()
        {
            var devices = new List<(int DeviceId, string DeviceName)>();
            for (int i = -1; i < WaveInEvent.DeviceCount; i++)
            {
                try
                {
                    var capabilities = WaveInEvent.GetCapabilities(i);
                    devices.Add((i, capabilities.ProductName));
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, $"Unable to get capabilities for device {i}");
                }
            }
            return devices;
        }

        private async void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            if (_recognizer.AcceptWaveform(e.Buffer, e.BytesRecorded))
            {
                var result = _recognizer.Result();
                await ProcessResult(result);
            }
            else
            {
                var partialResult = _recognizer.PartialResult();
                await ProcessPartialResult(partialResult);
            }
        }

        private async Task ProcessResult(string result)
        {
            var json = JObject.Parse(result);
            var text = (string)json["text"];
            if (!string.IsNullOrEmpty(text))
            {
                await _hubContext.Clients.Client(_currentConnectionId).SendAsync("ReceiveTranscription", text);
            }
        }

        private async Task ProcessPartialResult(string partialResult)
        {
            var json = JObject.Parse(partialResult);
            var text = (string)json["partial"];
            if (!string.IsNullOrEmpty(text))
            {
                await _hubContext.Clients.Client(_currentConnectionId).SendAsync("ReceivePartialTranscription", text);
            }
        }

        private void WaveIn_RecordingStopped(object sender, StoppedEventArgs e)
        {
            _logger.LogInformation($"Recording stopped for connection {_currentConnectionId}");
            if (e.Exception != null)
            {
                _logger.LogError(e.Exception, "Error during recording");
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                _waveIn?.Dispose();
                _recognizer?.Dispose();
                _disposed = true;
            }
        }
    }
}
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.Logging;
using LingoShift.Services;
using System.Linq;

namespace LingoShift.Hubs
{
    public class TranscriptionHub : Hub
    {
        private readonly ILogger<TranscriptionHub> _logger;
        private readonly ITranscriptionService _transcriptionService;

        public TranscriptionHub(ILogger<TranscriptionHub> logger, ITranscriptionService transcriptionService)
        {
            _logger = logger;
            _transcriptionService = transcriptionService;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation($"Client connected: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            _logger.LogInformation($"Client disconnected: {Context.ConnectionId}. Exception: {exception?.Message}");
            await _transcriptionService.StopTranscriptionAsync(Context.ConnectionId);
            await base.OnDisconnectedAsync(exception);
        }
        public async Task<string[]> GetAvailableAudioDevices()
        {
            var devices = _transcriptionService.GetAvailableAudioDevices();
            return devices.Select(d => $"{d.DeviceId}: {d.DeviceName}").ToArray();
        }

        public async Task StartTranscription(int deviceId = -1)
        {
            try
            {
                _logger.LogInformation($"StartTranscription requested by {Context.ConnectionId} for device {deviceId}");
                await _transcriptionService.StartTranscriptionAsync(Context.ConnectionId, deviceId);
                await Clients.Caller.SendAsync("TranscriptionStarted");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in StartTranscription for {Context.ConnectionId}");
                await Clients.Caller.SendAsync("TranscriptionError", $"Failed to start transcription: {ex.Message}");
            }
        }

        public async Task StopTranscription()
        {
            try
            {
                _logger.LogInformation($"StopTranscription requested by {Context.ConnectionId}");
                await _transcriptionService.StopTranscriptionAsync(Context.ConnectionId);
                await Clients.Caller.SendAsync("TranscriptionStopped");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in StopTranscription for {Context.ConnectionId}");
                await Clients.Caller.SendAsync("TranscriptionError", $"Failed to stop transcription: {ex.Message}");
            }
        }
    }
}
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LingoShift.Services
{
    public interface ITranscriptionService
    {
        Task StartTranscriptionAsync(string connectionId, int deviceId);
        Task StopTranscriptionAsync(string connectionId);
        List<(int DeviceId, string DeviceName)> GetAvailableAudioDevices();
    }
}

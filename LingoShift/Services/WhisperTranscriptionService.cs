using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LingoShift.Application.Interfaces;
using NAudio.Wave;
using Whisper.net;

namespace LingoShift.Services;

public sealed class WhisperTranscriptionService : IDisposable
{
    private readonly string _modelPath;
    private readonly IPopupService _popupService;
    private readonly StringBuilder _result = new();
    private CancellationTokenSource _cts;
    private Task _processingTask;
    private WaveInEvent _waveIn;
    private readonly WaveFormat _waveFormat;
    private WhisperFactory _factory;
    private WhisperProcessor _processor;
    private bool _isDisposed;

    private readonly MemoryStream _audioDataStream = new();
    private readonly object _lock = new();

    public WhisperTranscriptionService(IPopupService popupService)
    {
        _modelPath = "C:/Models/ggml-base.bin";
        _popupService = popupService;
        _waveFormat = new WaveFormat(16000, 16, 1);
    }

    public Task StartTranscriptionAsync()
    {
        if (!File.Exists(_modelPath))
        {
            _popupService.ShowTranslationPopup($"Modello non trovato: {_modelPath}");
            return Task.CompletedTask;
        }

        _cts = new CancellationTokenSource();

        _factory = WhisperFactory.FromPath(_modelPath);
        _processor = _factory.CreateBuilder()
            .WithLanguage("en")
            .WithoutSuppressBlank()
            .Build();

        InitializeAudioCapture();

        _processingTask = Task.Run(() => ProcessAudioDataAsync(_cts.Token));

        _waveIn.StartRecording();
        _popupService.ShowTranslationPopup("Trascrizione avviata. Parla nel microfono.");
        return Task.CompletedTask;
    }

    public void StopTranscription()
    {
        _cts?.Cancel();
        _waveIn?.StopRecording();
        _processingTask?.Wait();
        _popupService.ShowTranslationPopup("Trascrizione terminata.");
    }

    private void InitializeAudioCapture()
    {
        _waveIn = new WaveInEvent
        {
            WaveFormat = _waveFormat,
            BufferMilliseconds = 100, // Aumenta se necessario
            DeviceNumber = 1,
        };

        _waveIn.DataAvailable += (s, a) =>
        {
            lock (_lock)
            {
                _audioDataStream.Write(a.Buffer, 0, a.BytesRecorded);
            }
        };
    }

    private async Task ProcessAudioDataAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            int minAudioLength = _waveFormat.AverageBytesPerSecond * 5; // 5 secondi di audio

            byte[] audioData = null;

            lock (_lock)
            {
                if (_audioDataStream.Length >= minAudioLength)
                {
                    audioData = _audioDataStream.ToArray();
                    _audioDataStream.SetLength(0);
                    _audioDataStream.Position = 0;
                }
            }

            if (audioData != null)
            {
                try
                {
                    using var memoryStream = new MemoryStream(audioData);
                    using var wavStream = new MemoryStream();
                    WriteWavHeader(wavStream, _waveFormat, audioData.Length);
                    memoryStream.Position = 0;
                    await memoryStream.CopyToAsync(wavStream);
                    wavStream.Position = 0;

                    await foreach (var segment in _processor.ProcessAsync(wavStream, cancellationToken))
                    {
                        if (!string.IsNullOrWhiteSpace(segment.Text.Trim()) && segment.Text.Trim() != "[BLANK_AUDIO]")
                        {
                            _result.Append(segment.Text);
                            _popupService.UpdateTranslationPopup(_result.ToString());
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _popupService.ShowTranslationPopup($"Errore durante la trascrizione: {ex.Message}");
                }
            }

            // Attendi un po' prima di verificare nuovamente
            await Task.Delay(500, cancellationToken);
        }
    }

    private void WriteWavHeader(Stream stream, WaveFormat format, int dataLength)
    {
        using var writer = new BinaryWriter(stream, Encoding.UTF8, true);
        writer.Write(Encoding.ASCII.GetBytes("RIFF"));
        writer.Write(36 + dataLength);
        writer.Write(Encoding.ASCII.GetBytes("WAVE"));
        writer.Write(Encoding.ASCII.GetBytes("fmt "));
        writer.Write(16);
        writer.Write((short)1);
        writer.Write((short)format.Channels);
        writer.Write(format.SampleRate);
        writer.Write(format.AverageBytesPerSecond);
        writer.Write((short)format.BlockAlign);
        writer.Write((short)format.BitsPerSample);
        writer.Write(Encoding.ASCII.GetBytes("data"));
        writer.Write(dataLength);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (!_isDisposed)
        {
            if (disposing)
            {
                StopTranscription();
                _processor?.Dispose();
                _factory?.Dispose();
                _waveIn?.Dispose();
            }

            _isDisposed = true;
        }
    }
}
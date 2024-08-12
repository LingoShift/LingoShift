using LingoShift.Application.Interfaces;

namespace LingoShift.Infrastructure.PlatformSpecificServices;

public class WindowsClipboardService : IClipboardService
{
    private readonly WindowsNativeClipboardService _textService;

    public WindowsClipboardService(WindowsNativeClipboardService textService)
    {
        _textService = textService;
    }

    public async Task<string> GetTextAsync()
    {
        return await _textService.GetSelectedTextAsync();
    }

    public async Task SetTextAsync(string text)
    {
        await _textService.ReplaceSelectedTextAsync(text);
    }
}
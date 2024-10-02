using LingoShift.Application.Interfaces;

namespace LingoShift.Infrastructure.PlatformSpecificServices;

public class WindowsClipboardService : IClipboardService
{
    private readonly WindowsNativeClipboardService _textService;

    public WindowsClipboardService(WindowsNativeClipboardService textService)
    {
        _textService = textService;
    }

    public async Task<string> GetClipBoardTextAsync()
    {
        return await _textService.GetClipboardTextAsync();
    }

    public async Task<string> SelectAllAndCopyTextAsync()
    {
        return await _textService.SelectCopyAndGetTextAsync();
    }

    public async Task PasteTextAsync(string text)
    {
        await _textService.SetClipboardTextAsync(text);
        await _textService.PasteFromClipboardAsync();
    }

    public async Task SetClipBoardTextAsync(string text)
    {
        await _textService.SetClipboardTextAsync(text);
    }

}
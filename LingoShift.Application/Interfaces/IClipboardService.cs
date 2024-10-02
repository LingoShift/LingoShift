namespace LingoShift.Application.Interfaces;

public interface IClipboardService
{
    Task<string> GetClipBoardTextAsync();
    Task SetClipBoardTextAsync(string text);
    Task<string> SelectAllAndCopyTextAsync();
    Task PasteTextAsync(string text);
}

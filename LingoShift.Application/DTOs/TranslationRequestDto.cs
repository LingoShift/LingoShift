namespace LingoShift.Application.DTOs;

public class TranslationRequestDto
{
    public string? SourceText { get; set; }
    public string? SourceLanguageCode { get; set; }
    public string? TargetLanguageCode { get; set; }
}

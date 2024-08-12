namespace LingoShift.Domain.ValueObjects;

public record SourceText(string Value);
public record TranslatedText(string Value);

public class Language
{
    public string Code { get; private set; }
    public string Name { get; private set; }

    public Language(string code, string name)
    {
        Code = code;
        Name = name;
    }
}

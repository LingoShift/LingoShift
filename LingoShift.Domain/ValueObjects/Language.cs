namespace LingoShift.Domain.ValueObjects;

public record SourceText(string Value);
public record TranslatedText(string Value);

public class Language
{
    public string Code { get; set; }
    public string Name { get; set; }
    public static Language English { get; } = new Language("en", "English");
    public static Language Italian { get; } = new Language("it", "Italian");
    public static Language Spanish { get; } = new Language("es", "Spanish");

    public Language() { }

    public Language(string code, string name)
    {
        Code = code;
        Name = name;
    }

    public static IEnumerable<Language> GetValues()
    {
        yield return English;
        yield return Italian;
        yield return Spanish;
    }
}

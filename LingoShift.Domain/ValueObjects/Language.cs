using System;
using System.Collections.Generic;
using System.Linq;

namespace LingoShift.Domain.ValueObjects
{
    public class Language
    {
        public string Value { get; }

        private Language(string value)
        {
            Value = value;
        }

        public static Language FromString(string value)
        {
            return GetValues().FirstOrDefault(l => l.Value.Equals(value, StringComparison.OrdinalIgnoreCase)) 
                ?? throw new ArgumentException($"Invalid Language value: {value}");
        }

        public static implicit operator string(Language language) => language.Value;

        public override string ToString() => Value;

        public override bool Equals(object obj)
        {
            if (obj is Language language)
            {
                return Value.Equals(language.Value, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        public override int GetHashCode() => Value.GetHashCode();

        public static IEnumerable<Language> GetValues()
        {
            yield return new Language("English");
            yield return new Language("Spanish");
            yield return new Language("French");
            // Add more languages as needed
        }
    }
}
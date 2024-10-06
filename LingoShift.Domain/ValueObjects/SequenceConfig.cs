using System;
using System.Collections.Generic;
using System.Linq;

namespace LingoShift.Domain.ValueObjects
{
    public class SequenceConfig
    {
        public string Id { get; private set; }
        public string SequenceName { get; private set; }
        public string Sequence { get; private set; }
        public SequenceAction Action { get; private set; }
        public Language TargetLanguage { get; private set; }
        public bool UseLLM { get; private set; }
        public bool ShowPopup { get; private set; }

        public SequenceConfig(string id, string sequenceName, string sequence, SequenceAction action, Language targetLanguage, bool useLLM, bool showPopup)
        {
            Id = id;
            SequenceName = sequenceName;
            Sequence = sequence;
            Action = action;
            TargetLanguage = targetLanguage;
            UseLLM = useLLM;
            ShowPopup = showPopup;
        }

        public static SequenceConfig CreateDefault()
        {
            return new SequenceConfig(
                Guid.NewGuid().ToString(),
                string.Empty,
                string.Empty,
                SequenceAction.Translate,
                Language.GetValues().First(),
                false,
                false
            );
        }
    }

    public class SequenceAction
    {
        public static SequenceAction Translate = new SequenceAction("Translate");
        public static SequenceAction TranslateAndReplace = new SequenceAction("Translate and Replace");
        public static SequenceAction TranslateAndCopy = new SequenceAction("Translate and Copy");

        public string Value { get; private set; }

        private SequenceAction(string value)
        {
            Value = value;
        }

        public static SequenceAction FromString(string value)
        {
            return GetValues().FirstOrDefault(a => a.Value == value) ?? throw new ArgumentException($"Invalid SequenceAction value: {value}");
        }

        public static implicit operator string(SequenceAction action) => action.Value;

        public override string ToString() => Value;

        public override bool Equals(object? obj)
        {
            if (obj is SequenceAction action)
            {
                return Value == action.Value;
            }

            return false;
        }

        public override int GetHashCode() => Value.GetHashCode();

        public static IEnumerable<SequenceAction> GetValues()
        {
            yield return Translate;
            yield return TranslateAndReplace;
            yield return TranslateAndCopy;
        }
    }
}
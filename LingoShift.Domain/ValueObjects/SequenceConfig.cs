namespace LingoShift.Domain.ValueObjects
{
    public class SequenceConfig
    {
        public string SequenceName { get; set; }
        public string Sequence { get; set; }
        public SequenceAction Action { get; set; }
        public Language TargetLanguage { get; set; }
        public bool UseLLM { get; set; }
        public bool ShowPopup { get; set; }

        public SequenceConfig()
        {
            SequenceName = string.Empty;
            Sequence = string.Empty;
            Action = SequenceAction.GetValues().First();
            TargetLanguage = Language.GetValues().First();
            UseLLM = false;
            ShowPopup = false;
        }
    }

    public class SequenceAction
    {
        public static SequenceAction Translate = new SequenceAction("Translate");
        public static SequenceAction TranslateAndReplace = new SequenceAction("Translate and Replace");
        public static SequenceAction TranslateAndCopy = new SequenceAction("Translate and Copy");

        public string Value { get; set; }

        public SequenceAction() { }

        private SequenceAction(string value)
        {
            Value = value;
        }

        public static implicit operator string(SequenceAction action) => action.Value;
        public static implicit operator SequenceAction(string value) => new SequenceAction(value);

        public override string ToString() => Value;

        public override bool Equals(object obj)
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
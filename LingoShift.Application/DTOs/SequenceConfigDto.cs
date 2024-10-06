using LingoShift.Domain.ValueObjects;

namespace LingoShift.Application.DTOs
{
    public class SequenceConfigDto
    {
        public string Id { get; set; }
        public string SequenceName { get; set; }
        public string Sequence { get; set; }
        public string Action { get; set; }
        public string TargetLanguage { get; set; }
        public bool UseLLM { get; set; }
        public bool ShowPopup { get; set; }

        public static SequenceConfigDto FromDomain(SequenceConfig sequenceConfig)
        {
            return new SequenceConfigDto
            {
                Id = sequenceConfig.Id,
                SequenceName = sequenceConfig.SequenceName,
                Sequence = sequenceConfig.Sequence,
                Action = sequenceConfig.Action,
                TargetLanguage = sequenceConfig.TargetLanguage,
                UseLLM = sequenceConfig.UseLLM,
                ShowPopup = sequenceConfig.ShowPopup
            };
        }

        public SequenceConfig ToDomain()
        {
            return new SequenceConfig(
                Id,
                SequenceName,
                Sequence,
                SequenceAction.FromString(Action),
                Language.FromString(TargetLanguage),
                UseLLM,
                ShowPopup
            );
        }
    }
}
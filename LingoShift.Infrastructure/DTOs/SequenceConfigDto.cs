using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using LingoShift.Domain.ValueObjects;
using System;

namespace LingoShift.Infrastructure.DTOs
{
    public class SequenceConfigDto
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string SequenceName { get; set; }
        public string Sequence { get; set; }
        public string Action { get; set; }
        public string TargetLanguage { get; set; }
        public bool UseLLM { get; set; }
        public bool ShowPopup { get; set; }

        public static SequenceConfigDto FromDomain(SequenceConfig domain)
        {
            if (domain == null)
                throw new ArgumentNullException(nameof(domain));

            if (!ObjectId.TryParse(domain.Id, out _))
                throw new ArgumentException("Invalid ObjectId", nameof(domain.Id));

            return new SequenceConfigDto
            {
                Id = domain.Id,
                SequenceName = domain.SequenceName,
                Sequence = domain.Sequence,
                Action = domain.Action.ToString(),
                TargetLanguage = domain.TargetLanguage.Value,
                UseLLM = domain.UseLLM,
                ShowPopup = domain.ShowPopup
            };
        }

        public SequenceConfig ToDomain()
        {
            SequenceAction action;
            Language language;

            try
            {
                action = SequenceAction.FromString(Action);
                language = Language.FromString(TargetLanguage);
            }
            catch (ArgumentException ex)
            {
                throw new InvalidOperationException("Invalid data in DTO", ex);
            }

            return new SequenceConfig(
                Id,
                SequenceName,
                Sequence,
                action,
                language,
                UseLLM,
                ShowPopup
            );
        }
    }
}
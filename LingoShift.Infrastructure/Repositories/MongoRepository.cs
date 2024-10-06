using LingoShift.Domain.ValueObjects;
using MongoDB.Driver;
using Microsoft.Extensions.Options;

namespace LingoShift.Infrastructure.Repositories
{
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }
    }
    public class MongoRepository
    {
        private readonly IMongoCollection<SequenceConfig> _sequenceCollection;
        private readonly IMongoCollection<OpenAiConfig> _openAiConfig;

        public MongoRepository(IOptions<MongoDbSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase(settings.Value.DatabaseName);
            _sequenceCollection = database.GetCollection<SequenceConfig>("SequenceConfig");
            _openAiConfig = database.GetCollection<OpenAiConfig>("OpenAiConfig");
        }

        public async Task<List<SequenceConfig>> GetAllAsync()
        {
            return await _sequenceCollection.Find(_ => true).ToListAsync();
        }

        public async Task<SequenceConfig> GetByIdAsync(string id)
        {
            return await _sequenceCollection.Find(s => s.SequenceName == id).FirstOrDefaultAsync();
        }

        public async Task CreateAsync(SequenceConfig sequenceConfig)
        {
            await _sequenceCollection.InsertOneAsync(sequenceConfig);
        }

        public async Task UpdateAsync(string id, SequenceConfig sequenceConfig)
        {
            await _sequenceCollection.ReplaceOneAsync(s => s.SequenceName == id, sequenceConfig);
        }

        public async Task DeleteAsync(string id)
        {
            await _sequenceCollection.DeleteOneAsync(s => s.SequenceName == id);
        }

        public async Task<bool> ExistsAsync(string id)
        {
            var count = await _sequenceCollection.CountDocumentsAsync(s => s.SequenceName == id);
            return count > 0;
        }

        internal async Task<OpenAiConfig?> GetSettingAsync(string v)
        {
            return await _openAiConfig.Find(_ => true).FirstOrDefaultAsync();
        }
    }
}
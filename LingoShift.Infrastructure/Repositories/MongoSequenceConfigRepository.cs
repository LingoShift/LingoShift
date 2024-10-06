using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LingoShift.Domain.Repositories;
using LingoShift.Domain.ValueObjects;
using LingoShift.Infrastructure.DTOs;
using MongoDB.Driver;

namespace LingoShift.Infrastructure.Repositories
{
    public class MongoSequenceConfigRepository : ISequenceConfigRepository
    {
        private readonly IMongoCollection<SequenceConfigDto> _collection;

        public MongoSequenceConfigRepository(IMongoDatabase database)
        {
            _collection = database.GetCollection<SequenceConfigDto>("sequenceConfigs");
        }

        public async Task<SequenceConfig> GetByIdAsync(string id)
        {
            var dto = await _collection.Find(x => x.Id == id).FirstOrDefaultAsync();
            return dto?.ToDomain();
        }

        public async Task<IEnumerable<SequenceConfig>> GetAllAsync()
        {
            var dtos = await _collection.Find(_ => true).ToListAsync();
            return dtos.ConvertAll(dto => dto.ToDomain());
        }

        public async Task AddAsync(SequenceConfig sequenceConfig)
        {
            var dto = SequenceConfigDto.FromDomain(sequenceConfig);
            await _collection.InsertOneAsync(dto);
        }

        public async Task<bool> UpdateAsync(SequenceConfig sequenceConfig)
        {
            var dto = SequenceConfigDto.FromDomain(sequenceConfig);
            var result = await _collection.ReplaceOneAsync(
                x => x.Id == dto.Id,
                dto,
                new ReplaceOptions { IsUpsert = false }
            );

            return result.ModifiedCount > 0;
        }

        public async Task<bool> DeleteAsync(string id)
        {
            var result = await _collection.DeleteOneAsync(x => x.Id == id);
            return result.DeletedCount > 0;
        }
    }
}
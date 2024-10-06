using System.Collections.Generic;
using System.Threading.Tasks;
using LingoShift.Domain.ValueObjects;

namespace LingoShift.Domain.Repositories
{
    public interface ISequenceConfigRepository
    {
        Task<SequenceConfig> GetByIdAsync(string id);
        Task<IEnumerable<SequenceConfig>> GetAllAsync();
        Task AddAsync(SequenceConfig sequenceConfig);
        Task<bool> UpdateAsync(SequenceConfig sequenceConfig);
        Task<bool> DeleteAsync(string id);
    }
}
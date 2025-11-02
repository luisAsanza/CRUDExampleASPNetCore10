using Entities;
using System.Linq.Expressions;

namespace RepositoryContracts
{
    public interface IPersonsRepository
    {
        Task<Person> AddAsync(Person person);
        Task<Person?> GetAsync(Guid personId);
        Task<List<Person>> GetAllAsync();
        Task<List<Person>> GetFilteredPersonsAsync(Expression<Func<Person, bool>> predicate);
        Task<bool> DeleteAsync(Guid personId);
        Task<Person> UpdateAsync(Person person);
    }
}

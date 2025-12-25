using Entities;
using System.Linq.Expressions;

namespace RepositoryContracts
{
    /// <summary>
    /// Represents DAL for managing countries.
    /// </summary>
    public interface ICountriesRepository
    {
        Task<Country> AddAsync(Country country);
        Task<Country?> GetAsync(Guid countryId);
        Task<Country?> GetByCountryNameAsync(string countryName);
        Task<List<Country>> GetAllAsync();
        Task<bool> AnyAsync(Expression<Func<Country, bool>> predicate);
        Task AddRangeAsync(IEnumerable<Country> countries);
    }
}

using Entities;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;
using System.Linq.Expressions;

namespace Repositories
{
    public class CountriesRepository : ICountriesRepository
    {

        private readonly ApplicationDbContext _db;

        public CountriesRepository(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<Country> AddAsync(Country country)
        {
            _db.Countries.Add(country);
            await _db.SaveChangesAsync();

            return country;
        }

        public async Task<bool> AnyAsync(Expression<Func<Country, bool>> predicate)
        {
            return await _db.Countries.AnyAsync(predicate);
        }

        public Task<Country?> GetAsync(Guid countryId)
        {
            return _db.Countries.FirstOrDefaultAsync(t => t.CountryId == countryId);
        }

        public Task<List<Country>> GetAllAsync()
        {
            return _db.Countries.ToListAsync();
        }

        public Task<Country?> GetByCountryNameAsync(string countryName)
        {
            return _db.Countries.FirstOrDefaultAsync(t => t.CountryName == countryName);
        }

        public Task AddRangeAsync(IEnumerable<Country> countries)
        {
            _db.Countries.AddRange(countries);
            return _db.SaveChangesAsync();
        }
    }
}

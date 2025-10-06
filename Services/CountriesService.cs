using Entities;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using ServiceContracts.DTO;

namespace Services
{
    public class CountriesService : ICountriesService
    {
        private readonly PersonsDbContext _dbContext;

        public CountriesService(PersonsDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<CountryResponse> AddCountry(CountryAddRequest? countryAddRequest)
        {
            if (countryAddRequest == null)
                throw new ArgumentNullException(nameof(countryAddRequest));

            if (countryAddRequest.CountryName == null)
                throw new ArgumentException(nameof(countryAddRequest.CountryName));

            if (await _dbContext.Countries.AnyAsync(c => c.CountryName == countryAddRequest.CountryName))
                throw new ArgumentException(nameof(countryAddRequest.CountryName));

            Country country = countryAddRequest.ToCountry();

            country.CountryId = Guid.NewGuid();

            _dbContext.Countries.Add(country);
            await _dbContext.SaveChangesAsync();

            return country.ToCountryResponse();
        }

        public Task<List<CountryResponse>> GetAllCountries()
        {
            var countryResponseList = _dbContext.Countries.Select(c => c.ToCountryResponse()).ToListAsync();
            return countryResponseList;
        }

        public async Task<CountryResponse?> GetCountry(Guid? countryId)
        {
            if (countryId == null) return null;

            Country? country = await _dbContext.Countries.FirstOrDefaultAsync(c => c.CountryId == countryId);

            if (country == null) return null;

            return country.ToCountryResponse();
        }
    }
}

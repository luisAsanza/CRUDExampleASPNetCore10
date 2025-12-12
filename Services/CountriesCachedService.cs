using Microsoft.AspNetCore.Http;
using ServiceContracts;
using ServiceContracts.DTO;

namespace Services
{
    public class CountriesCachedService : ICountriesService
    {
        private const string GetAllCountriesCacheKey = "GET_ALL_COUNTRIES_CACHE_KEY";
        private const int GetAllCountriesCacheDurationMinutes = 60;
        private readonly ICountriesService _countriesService;
        private readonly ICacheService _cacheService;

        public CountriesCachedService(ICountriesService countriesService, ICacheService cacheService)
        {
            _countriesService = countriesService;
            _cacheService = cacheService;
        }

        public async Task<CountryResponse> AddCountry(CountryAddRequest? countryAddRequest)
        {
            var result = await _countriesService.AddCountry(countryAddRequest);

            // Invalidate the cache for GetAllCountries
            _cacheService.Remove(GetAllCountriesCacheKey);

            return result;
        }

        public async Task<List<CountryResponse>> GetAllCountries()
        {
            // Try to get the list of countries from cache
            if(_cacheService.TryGetValue<List<CountryResponse>>(GetAllCountriesCacheKey, 
                out List<CountryResponse>? cachedCountries))
            {
                return await Task.FromResult(cachedCountries!);
            }

            // If not found in cache, get from the underlying service
            var result = await _countriesService.GetAllCountries();

            // Store the result in cache
            _cacheService.Set(GetAllCountriesCacheKey, result, absoluteExpirationMinutes : GetAllCountriesCacheDurationMinutes);

            return result;
        }

        public Task<CountryResponse?> GetCountry(Guid? countryId)
        {
            return _countriesService.GetCountry(countryId);
        }

        public async Task<int> UploadCountriesFromExcelFile(IFormFile formFile)
        {
            var result = await _countriesService.UploadCountriesFromExcelFile(formFile);

            // Invalidate the cache for GetAllCountries
            _cacheService.Remove(GetAllCountriesCacheKey);

            return result;
        }
    }
}

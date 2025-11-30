using Castle.Core.Logging;
using Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OfficeOpenXml;
using RepositoryContracts;
using ServiceContracts;
using ServiceContracts.DTO;

namespace Services
{
    public class CountriesService : ICountriesService
    {
        private readonly ICountriesRepository _countriesRepository;
        private readonly ICacheService _cacheService;
        private const string COUNTRIES_CACHE_KEY = "countries_all";
        private readonly ILogger<CountriesService> _logger;

        public CountriesService(ICountriesRepository countriesRepository, 
            ICacheService cacheService, ILogger<CountriesService> logger)
        {
            _countriesRepository = countriesRepository;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task<CountryResponse> AddCountry(CountryAddRequest? countryAddRequest)
        {
            if (countryAddRequest == null)
                throw new ArgumentNullException(nameof(countryAddRequest));

            if (countryAddRequest.CountryName == null)
                throw new ArgumentException(nameof(countryAddRequest.CountryName));

            if (await _countriesRepository.AnyAsync(c => c.CountryName == countryAddRequest.CountryName))
                throw new ArgumentException(nameof(countryAddRequest.CountryName));

            Country country = countryAddRequest.ToCountry();

            country.CountryId = Guid.NewGuid();

            await _countriesRepository.AddAsync(country);

            // Invalidate cache when new country is added
            _cacheService.Remove(COUNTRIES_CACHE_KEY);

            return country.ToCountryResponse();
        }

        public async Task<List<CountryResponse>> GetAllCountries()
        {
            // Try to get from cache first
            if (_cacheService.TryGetValue(COUNTRIES_CACHE_KEY, out List<CountryResponse>? cachedCountries))
            {
                return cachedCountries!;
            }

            // If not in cache, fetch from repository
            var countries = await _countriesRepository.GetAllAsync();
            var countryResponseList = countries.Select(c => c.ToCountryResponse()).ToList();

            // Store in cache for 30 minutes
            _cacheService.Set(COUNTRIES_CACHE_KEY, countryResponseList, absoluteExpirationMinutes: 30);

            return countryResponseList;
        }

        public async Task<CountryResponse?> GetCountry(Guid? countryId)
        {
            if (!countryId.HasValue) return null;

            Country? country = await _countriesRepository.GetAsync(countryId.Value);

            if (country == null) return null;

            return country.ToCountryResponse();
        }

        public async Task<int> UploadCountriesFromExcelFile(IFormFile formFile)
        {
            if(formFile == null || formFile.Length == 0 || !formFile.FileName.EndsWith(".xlsx"))
            {
                throw new ArgumentException("Invalid file");
            }

            await using MemoryStream memoryStream = new MemoryStream();
            await formFile.CopyToAsync(memoryStream);

            using (ExcelPackage excelPackage = new ExcelPackage(memoryStream))
            {
                var worksheet = excelPackage.Workbook.Worksheets.FirstOrDefault();
                if(worksheet == null) return 0;
                var rowCount = worksheet.Dimension.Rows;
                const int headerRow = 1;
                var validCountries = new List<Country>();

                for (var row = headerRow + 1; row<=rowCount; row++)
                {
                    var cellValue = worksheet.Cells[row, 1].Value?.ToString();

                    //Add validated country to the list
                    if (!string.IsNullOrWhiteSpace(cellValue) 
                        && !await _countriesRepository.AnyAsync(c => c.CountryName == cellValue))
                    {
                        validCountries.Add(new Country()
                        {
                            CountryId = Guid.NewGuid(),
                            CountryName = cellValue
                        });
                    }
                }

                await _countriesRepository.AddRangeAsync(validCountries);

                // Invalidate cache when countries are uploaded
                _cacheService.Remove(COUNTRIES_CACHE_KEY);

                return validCountries.Count();
            }
        }
    }
}

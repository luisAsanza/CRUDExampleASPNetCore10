using Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using ServiceContracts;
using ServiceContracts.DTO;

namespace Services
{
    public class CountriesService : ICountriesService
    {
        private readonly ApplicationDbContext _dbContext;

        public CountriesService(ApplicationDbContext dbContext)
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
                        && !await _dbContext.Countries.AnyAsync(c => c.CountryName == cellValue))
                    {
                        validCountries.Add(new Country()
                        {
                            CountryId = Guid.NewGuid(),
                            CountryName = cellValue
                        });
                    }
                }

                _dbContext.AddRange(validCountries);
                await _dbContext.SaveChangesAsync();
                return validCountries.Count();
            }
        }
    }
}

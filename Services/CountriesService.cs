using Entities;
using ServiceContracts;
using ServiceContracts.DTO;

namespace Services
{
    public class CountriesService : ICountriesService
    {
        private readonly List<Country> _countries;

        public CountriesService()
        {
            _countries = new List<Country>
            {
                new Country { CountryID = Guid.Parse("11111111-1111-1111-1111-111111111111"), CountryName = "Argentina" },
                new Country { CountryID = Guid.Parse("22222222-2222-2222-2222-222222222222"), CountryName = "Brazil" },
                new Country { CountryID = Guid.Parse("33333333-3333-3333-3333-333333333333"), CountryName = "Canada" },
                new Country { CountryID = Guid.Parse("44444444-4444-4444-4444-444444444444"), CountryName = "Denmark" },
                new Country { CountryID = Guid.Parse("55555555-5555-5555-5555-555555555555"), CountryName = "Egypt" },
                new Country { CountryID = Guid.Parse("66666666-6666-6666-6666-666666666666"), CountryName = "France" },
                new Country { CountryID = Guid.Parse("77777777-7777-7777-7777-777777777777"), CountryName = "Germany" },
                new Country { CountryID = Guid.Parse("88888888-8888-8888-8888-888888888888"), CountryName = "Hungary" },
                new Country { CountryID = Guid.Parse("99999999-9999-9999-9999-999999999999"), CountryName = "India" },
                new Country { CountryID = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), CountryName = "Japan" }
            };
        }

        public CountryResponse AddCountry(CountryAddRequest? countryAddRequest)
        {
            if (countryAddRequest == null)
                throw new ArgumentNullException(nameof(countryAddRequest));

            if (countryAddRequest.CountryName == null)
                throw new ArgumentException(nameof(countryAddRequest.CountryName));

            if (_countries.Any(c => c.CountryName == countryAddRequest.CountryName))
                throw new ArgumentException(nameof(countryAddRequest.CountryName));

            Country country = countryAddRequest.ToCountry();

            country.CountryID = Guid.NewGuid();

            _countries.Add(country);

            return country.ToCountryResponse();
        }

        public List<CountryResponse> GetAllCountries()
        {
            var countryResponseList = _countries.Select(c => c.ToCountryResponse()).ToList();
            return countryResponseList;
        }

        public CountryResponse? GetCountry(Guid? countryId)
        {
            if (countryId == null) return null;

            Country? country = _countries.FirstOrDefault(c => c.CountryID == countryId);

            if (country == null) return null;

            return country.ToCountryResponse();
        }
    }
}

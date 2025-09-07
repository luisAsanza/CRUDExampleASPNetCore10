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
            _countries = new List<Country>();
        }

        public CountryResponse AddCountry(CountryAddRequest? countryAddRequest)
        {
            if(countryAddRequest == null) 
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

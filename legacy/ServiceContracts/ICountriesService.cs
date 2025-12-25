using Microsoft.AspNetCore.Http;
using ServiceContracts.DTO;

namespace ServiceContracts
{
    /// <summary>
    /// Represents business logic for manipulating Country entity
    /// </summary>
    public interface ICountriesService
    {
        /// <summary>
        /// Adds a country object to the list of countries
        /// </summary>
        /// <param name="countryAddRequest">Country object to add</param>
        /// <returns>Returns the country object after adding it (including newly generated country id)</returns>
        Task<CountryResponse> AddCountry(CountryAddRequest? countryAddRequest);

        /// <summary>
        /// Return all countries from the db
        /// </summary>
        /// <returns></returns>
        Task<List<CountryResponse>> GetAllCountries();

        /// <summary>
        /// Returns a country object based onthe given country id
        /// </summary>
        /// <param name="countryId">Country ID (Guid) to search</param>
        /// <returns>Matching country as CountrResponse object</returns>
        Task<CountryResponse?> GetCountry(Guid? countryId);

        Task<int> UploadCountriesFromExcelFile(IFormFile formFile);
    }
}

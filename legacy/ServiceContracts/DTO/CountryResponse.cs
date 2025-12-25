using Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceContracts.DTO
{
    /// <summary>
    /// DTO class that is used as return type for most of CountriesService methods
    /// </summary>
    public class CountryResponse
    {
        public Guid CountryId { get; set; }
        public string? CountryName { get; set; }

        public override string ToString()
        {
            return $"CountryID: {CountryId}, CountryName: {CountryName ?? "N/A"}";
        }
    }

    public static class CountryExtensions
    {
        public static CountryResponse ToCountryResponse(this Country country)
        {
            return new CountryResponse
            {
                CountryId = country.CountryId,
                CountryName = country.CountryName
            };
        }
    }
}

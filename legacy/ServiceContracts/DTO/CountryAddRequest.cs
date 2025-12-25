using Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServiceContracts.DTO
{
    public class CountryAddRequest
    {
        public string? CountryName { get; set; }

        public Country ToCountry()
        {
            return new Country()
            {
                CountryId = new Guid(),
                CountryName = CountryName
            };
        }
    }
}

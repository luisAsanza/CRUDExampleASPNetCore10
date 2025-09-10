using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ServiceContracts.Enums
{
    public enum PersonSearchOptions
    {
        PersonName,
        Email,
        DateOfBirth,
        Age,
        Gender,
        Country,
        Address,
        ReceiveNewsLetter
    }
}

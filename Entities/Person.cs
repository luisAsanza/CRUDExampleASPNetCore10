using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Entities
{
    public class Person
    {
        [Key]
        public Guid PersonId { get; set; }

        [StringLength(40)]
        public string? PersonName { get; set; }

        [StringLength(40)]
        public string? Email { get; set; }


        public DateOnly? DateOfBirth { get; set; }

        [StringLength(10)]
        public string? Gender { get; set; }


        public Guid? CountryId { get; set; }

        [StringLength(200)]
        public string? Address { get; set; }

        public bool ReceiveNewsLetters { get; set; }

        public string? TIN { get; set; }

        public Country? Country { get; set; }
    }
}

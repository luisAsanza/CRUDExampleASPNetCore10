using Entities;

namespace ServiceContracts.DTO
{
    public class PersonResponse
    {
        public Guid PersonId { get; set; }
        public string? PersonName { get; set; }
        public string? Email { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public Guid? CountryId { get; set; }
        public string? Country { get; set; }
        public string? Address { get; set; }
        public bool ReceiveNewsLetters { get; set; }
        public int? Age { get; set; }

        public override bool Equals(object? obj)
        {
            if(obj == null) return false;

            if (obj is not PersonResponse other)
                return false;

            return PersonId == other.PersonId &&
                   PersonName == other.PersonName &&
                   Email == other.Email &&
                   DateOfBirth == other.DateOfBirth &&
                   Gender == other.Gender &&
                   CountryId == other.CountryId &&
                   Address == other.Address &&
                   ReceiveNewsLetters == other.ReceiveNewsLetters &&
                   Age == other.Age;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public static class PersonExtensions
    {
        public static PersonResponse ToPersonResponse(this Person person)
        {
            return new PersonResponse()
            {
                PersonId = person.PersonId,
                PersonName = person.PersonName,
                Email = person.Email,
                DateOfBirth = person.DateOfBirth,
                Gender = person.Gender,
                CountryId = person.CountryId,
                Address = person.Address,
                ReceiveNewsLetters = person.ReceiveNewsLetters,
                Age = person.DateOfBirth == null ? null : CalculateAge(person.DateOfBirth.Value),
                Country = person.Country?.CountryName
            };
        }

        public static int CalculateAge(DateOnly dateOfBirth)
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            int age = today.Year - dateOfBirth.Year;

            // If birthday hasn't occurred yet this year, subtract one
            if (dateOfBirth > today.AddYears(-age))
                age--;

            return age;
        }
    }
}

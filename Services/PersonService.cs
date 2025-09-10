using Entities;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Services.Helpers;
using System.Globalization;

namespace Services
{
    public class PersonService : IPersonService
    {
        private readonly List<Person> _persons;
        private readonly ICountriesService _countriesService;

        public PersonService(ICountriesService countriesService)
        {
            _persons = GetHardcodedPersons();
            _countriesService = countriesService;
        }

        public PersonResponse AddPerson(PersonAddRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            //Validate person name
            //if (string.IsNullOrWhiteSpace(request.PersonName))
            //    throw new ArgumentException("PersonName can't be blank", nameof(request.PersonName));

            //Model Validations
            ValidationHelper.ModelValidation(request);

            Person person = request.ToPerson();

            person.PersonId = new Guid();

            _persons.Add(person);
            PersonResponse response = ConvertPersonToPersonResponse(person);

            return response;
        }

        private PersonResponse ConvertPersonToPersonResponse(Person person)
        {
            var response = person.ToPersonResponse();
            response.Country = _countriesService.GetCountry(person.CountryId)?.CountryName;
            return response;
        }

        public List<PersonResponse> GetAllPersons()
        {
            var allPersons = _persons.Select(p => p.ToPersonResponse()).ToList();

            return allPersons;
        }

        public PersonResponse? GetPerson(Guid? personId)
        {
            if (personId == null)
                return null;

            Person? person = _persons.FirstOrDefault(p => p.PersonId == personId);

            if (person == null)
                return null;

            return person.ToPersonResponse();
        }

        public List<PersonResponse> GetFilteredPersons(PersonSearchOptions searchBy, string? searchString)
        {
            List<PersonResponse> allPersons = GetAllPersons();
            List<PersonResponse> matchingPersons = allPersons;

            if(string.IsNullOrWhiteSpace(searchString))
                return matchingPersons;

            switch (searchBy)
            {
                case PersonSearchOptions.PersonName:
                    matchingPersons = allPersons
                        .Where(p => p.PersonName != null && p.PersonName.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                    break;
                case PersonSearchOptions.Email:
                    matchingPersons = allPersons
                        .Where(p => p.Email != null && p.Email.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                    break;
                case PersonSearchOptions.DateOfBirth:
                    if(DateOnly.TryParseExact(searchString, "dd MM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly dateOfBirthSearch))
                    {
                        matchingPersons = allPersons
                            .Where(p => p.DateOfBirth.HasValue && p.DateOfBirth == dateOfBirthSearch).ToList();
                    } else
                    {
                        matchingPersons = allPersons;
                    }
                    break;
                case PersonSearchOptions.Age:
                    matchingPersons = allPersons
                        .Where(p => p.Age.HasValue && int.TryParse(searchString, out int result) && p.Age == result)
                        .ToList(); break;
                case PersonSearchOptions.Gender:
                    matchingPersons = allPersons
                        .Where(p => p.Gender != null && p.Gender.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                    break;
                case PersonSearchOptions.Address:
                    matchingPersons = allPersons
                        .Where(p => p.Address != null && p.Address.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                    break;
                case PersonSearchOptions.ReceiveNewsLetter:
                    matchingPersons = [.. allPersons.Where(p => p.ReceiveNewsLetters.ToString().Equals(searchString, StringComparison.OrdinalIgnoreCase))];
                    break;
                default:
                    matchingPersons = allPersons;
                    break;
            }

            return matchingPersons;
        }

        public List<PersonResponse> GetSortedPersons(List<PersonResponse> allPersons, PersonSearchOptions? sortBy, SortOrderOptions sortOrder)
        {
            if (sortBy == null)
                return allPersons;

            List<PersonResponse> sortedPersons = (sortBy, sortOrder) switch
            {
                //ASC

                (PersonSearchOptions.PersonName, SortOrderOptions.ASC) =>
                allPersons.OrderBy(p => p.PersonName, StringComparer.OrdinalIgnoreCase).ToList(),

                (PersonSearchOptions.Email, SortOrderOptions.ASC) =>
                allPersons.OrderBy(p => p.Email, StringComparer.OrdinalIgnoreCase).ToList(),

                (PersonSearchOptions.DateOfBirth, SortOrderOptions.ASC) =>
                allPersons.OrderBy(p => p.DateOfBirth).ToList(),

                (PersonSearchOptions.Age, SortOrderOptions.ASC) =>
                allPersons.OrderBy(p => p.Age).ToList(),

                (PersonSearchOptions.Gender, SortOrderOptions.ASC) =>
                allPersons.OrderBy(p => p.Gender, StringComparer.OrdinalIgnoreCase).ToList(),

                (PersonSearchOptions.Country, SortOrderOptions.ASC) =>
                allPersons.OrderBy(p => p.Country, StringComparer.OrdinalIgnoreCase).ToList(),

                (PersonSearchOptions.Address, SortOrderOptions.ASC) =>
                allPersons.OrderBy(p => p.Address, StringComparer.OrdinalIgnoreCase).ToList(),

                (PersonSearchOptions.ReceiveNewsLetter, SortOrderOptions.ASC) =>
                allPersons.OrderBy(p => p.ReceiveNewsLetters).ToList(),

                //DESC

                (PersonSearchOptions.PersonName, SortOrderOptions.DESC) =>
                allPersons.OrderByDescending(p => p.PersonName, StringComparer.OrdinalIgnoreCase).ToList(),

                (PersonSearchOptions.Email, SortOrderOptions.DESC) =>
                allPersons.OrderByDescending(p => p.Email, StringComparer.OrdinalIgnoreCase).ToList(),

                (PersonSearchOptions.DateOfBirth, SortOrderOptions.DESC) =>
                allPersons.OrderByDescending(p => p.DateOfBirth).ToList(),

                (PersonSearchOptions.Age, SortOrderOptions.DESC) =>
                allPersons.OrderByDescending(p => p.Age).ToList(),

                (PersonSearchOptions.Gender, SortOrderOptions.DESC) =>
                allPersons.OrderByDescending(p => p.Gender, StringComparer.OrdinalIgnoreCase).ToList(),

                (PersonSearchOptions.Country, SortOrderOptions.DESC) =>
                allPersons.OrderByDescending(p => p.Country, StringComparer.OrdinalIgnoreCase).ToList(),

                (PersonSearchOptions.Address, SortOrderOptions.DESC) =>
                allPersons.OrderByDescending(p => p.Address, StringComparer.OrdinalIgnoreCase).ToList(),

                (PersonSearchOptions.ReceiveNewsLetter, SortOrderOptions.DESC) =>
                allPersons.OrderByDescending(p => p.ReceiveNewsLetters).ToList(),

                _ => allPersons
            };

            return sortedPersons;
        }

        public PersonResponse UpdateResponse(PersonUpdateRequest? personUpdateRequest)
        {
            if (personUpdateRequest == null)
                throw new ArgumentNullException(nameof(personUpdateRequest));

            ValidationHelper.ModelValidation(personUpdateRequest);

            Person? result = _persons.Where(p => p.PersonId == personUpdateRequest.PersonId).FirstOrDefault();

            if (result == null)
                throw new ArgumentException("Given Person ID  doesn't exist");

            result.PersonName = personUpdateRequest.PersonName;
            result.Email = personUpdateRequest.Email;
            result.DateOfBirth = personUpdateRequest.DateOfBirth;
            result.Gender = personUpdateRequest.Gender.ToString();
            result.CountryId = personUpdateRequest.CountryId;
            result.Address = personUpdateRequest.Address;
            result.ReceiveNewsLetters = personUpdateRequest.ReceiveNewsLetters;

            return result.ToPersonResponse();
        }

        public bool DeletePerson(Guid? personId)
        {
            if (personId == null)
                throw new ArgumentNullException(nameof(personId));

            Person? person = _persons.FirstOrDefault(p => p.PersonId == personId);

            if (person == null)
                return false;

            _persons.RemoveAll(p => p.PersonId == personId);

            return true;
        }

        private List<Person> GetHardcodedPersons()
        {
            var persons = new List<Person>
            {
                new Person
                {
                    PersonId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-000000000001"),
                    PersonName = "Alice Johnson",
                    Email = "alice.johnson@example.com",
                    DateOfBirth = new DateOnly(1990, 5, 12),
                    Gender = "Female",
                    CountryId = Guid.Parse("11111111-1111-1111-1111-111111111111"), // Argentina
                    Address = "123 Main St, Springfield",
                    ReceiveNewsLetters = true
                },
                new Person
                {
                    PersonId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-000000000002"),
                    PersonName = "Bob Smith",
                    Email = "bob.smith@example.com",
                    DateOfBirth = new DateOnly(1985, 11, 3),
                    Gender = "Male",
                    CountryId = Guid.Parse("22222222-2222-2222-2222-222222222222"), // Brazil
                    Address = "456 Oak Ave, Rivertown",
                    ReceiveNewsLetters = false
                },
                new Person
                {
                    PersonId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-000000000003"),
                    PersonName = "Carla Mendes",
                    Email = "carla.mendes@example.com",
                    DateOfBirth = new DateOnly(1992, 7, 21),
                    Gender = "Female",
                    CountryId = Guid.Parse("33333333-3333-3333-3333-333333333333"), // Canada
                    Address = "789 Pine Rd, Lakeside",
                    ReceiveNewsLetters = true
                },
                new Person
                {
                    PersonId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-000000000004"),
                    PersonName = "David Lee",
                    Email = "david.lee@example.com",
                    DateOfBirth = new DateOnly(1988, 2, 14),
                    Gender = "Male",
                    CountryId = Guid.Parse("44444444-4444-4444-4444-444444444444"), // Denmark
                    Address = "321 Birch Blvd, Hilltown",
                    ReceiveNewsLetters = false
                },
                new Person
                {
                    PersonId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-000000000005"),
                    PersonName = "Elena Rossi",
                    Email = "elena.rossi@example.com",
                    DateOfBirth = new DateOnly(1995, 9, 30),
                    Gender = "Female",
                    CountryId = Guid.Parse("55555555-5555-5555-5555-555555555555"), // Egypt
                    Address = "654 Cedar St, Seaview",
                    ReceiveNewsLetters = true
                },
                new Person
                {
                    PersonId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-000000000006"),
                    PersonName = "Frank Miller",
                    Email = "frank.miller@example.com",
                    DateOfBirth = new DateOnly(1983, 4, 8),
                    Gender = "Male",
                    CountryId = Guid.Parse("66666666-6666-6666-6666-666666666666"), // France
                    Address = "987 Maple Dr, Brookfield",
                    ReceiveNewsLetters = false
                },
                new Person
                {
                    PersonId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-000000000007"),
                    PersonName = "Grace Kim",
                    Email = "grace.kim@example.com",
                    DateOfBirth = new DateOnly(1991, 12, 19),
                    Gender = "Female",
                    CountryId = Guid.Parse("77777777-7777-7777-7777-777777777777"), // Germany
                    Address = "159 Elm St, Greenfield",
                    ReceiveNewsLetters = true
                },
                new Person
                {
                    PersonId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-000000000008"),
                    PersonName = "Henry Adams",
                    Email = "henry.adams@example.com",
                    DateOfBirth = new DateOnly(1987, 6, 25),
                    Gender = "Male",
                    CountryId = Guid.Parse("88888888-8888-8888-8888-888888888888"), // Hungary
                    Address = "753 Walnut Ave, Riverbend",
                    ReceiveNewsLetters = false
                },
                new Person
                {
                    PersonId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-000000000009"),
                    PersonName = "Isabella Cruz",
                    Email = "isabella.cruz@example.com",
                    DateOfBirth = new DateOnly(1993, 8, 5),
                    Gender = "Female",
                    CountryId = Guid.Parse("99999999-9999-9999-9999-999999999999"), // India
                    Address = "852 Chestnut St, Sunnyvale",
                    ReceiveNewsLetters = true
                },
                new Person
                {
                    PersonId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-000000000010"),
                    PersonName = "Jack Thompson",
                    Email = "jack.thompson@example.com",
                    DateOfBirth = new DateOnly(1989, 1, 17),
                    Gender = "Male",
                    CountryId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), // Japan
                    Address = "951 Poplar Rd, Fairview",
                    ReceiveNewsLetters = false
                }
            };


            return persons;
        }
    }
}

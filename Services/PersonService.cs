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
        private readonly ICountriesService _countriesService;
        private readonly PersonsDbContext _dbContext;

        public PersonService(ICountriesService countriesService, PersonsDbContext dbContext)
        {
            _countriesService = countriesService;
            _dbContext = dbContext;
        }

        public PersonResponse AddPerson(PersonAddRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            //Validate person name
            if (string.IsNullOrWhiteSpace(request.PersonName))
                throw new ArgumentException("PersonName can't be blank", nameof(request.PersonName));

            //Model Validations
            ValidationHelper.ModelValidation(request);

            Person person = request.ToPerson();

            person.PersonId = new Guid();

            _dbContext.Persons.Add(person);
            _dbContext.SaveChanges();
            PersonResponse response = ConvertPersonToPersonResponse(person);

            return response;
        }

        public List<PersonResponse> GetAllPersons()
        {
            var allPersons = _dbContext.Persons
                .ToList() //Switch to in-memory processing
                .Select(p => ConvertPersonToPersonResponse(p))
                .ToList();

            return allPersons;
        }

        public PersonResponse? GetPerson(Guid? personId)
        {
            if (personId == null)
                return null;

            Person? person = _dbContext.Persons.FirstOrDefault(p => p.PersonId == personId);

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

            Person? result = _dbContext.Persons.Where(p => p.PersonId == personUpdateRequest.PersonId).FirstOrDefault();

            if (result == null)
                throw new ArgumentException("Given Person ID  doesn't exist");

            result.PersonName = personUpdateRequest.PersonName;
            result.Email = personUpdateRequest.Email;
            result.DateOfBirth = personUpdateRequest.DateOfBirth;
            result.Gender = personUpdateRequest.Gender.ToString();
            result.CountryId = personUpdateRequest.CountryId;
            result.Address = personUpdateRequest.Address;
            result.ReceiveNewsLetters = personUpdateRequest.ReceiveNewsLetters;

            _dbContext.SaveChanges(); //UPDATE

            return result.ToPersonResponse();
        }

        public bool DeletePerson(Guid? personId)
        {
            if (personId == null)
                throw new ArgumentNullException(nameof(personId));

            Person? person = _dbContext.Persons.FirstOrDefault(p => p.PersonId == personId);

            if (person == null)
                return false;

            _dbContext.Remove(person);
            _dbContext.SaveChanges();

            return true;
        }

        private PersonResponse ConvertPersonToPersonResponse(Person person)
        {
            var response = person.ToPersonResponse();
            response.Country = _countriesService.GetCountry(person.CountryId)?.CountryName;
            return response;
        }
    }
}

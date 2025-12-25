using Entities;
using Exceptions;
using Microsoft.Extensions.Logging;
using RepositoryContracts;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Services.Helpers;
using System.Globalization;
using System.Linq.Expressions;

namespace Services
{
    public class PersonService : IPersonService
    {
        private readonly IPersonsRepository _personsRepository;
        private readonly ILogger<PersonService> _logger;

        public PersonService(IPersonsRepository personsRepository, ILogger<PersonService> logger)
        {
            _personsRepository = personsRepository;
            _logger = logger;
        }

        public async Task<PersonResponse> AddPerson(PersonAddRequest request)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            //Validate person name
            if (string.IsNullOrWhiteSpace(request.PersonName))
                throw new ArgumentException("PersonName can't be blank", nameof(request.PersonName));

            //Model Validations
            ValidationHelper.ModelValidation(request);

            Person person = request.ToPerson();

            person.PersonId = new Guid();

            var saved = await _personsRepository.AddAsync(person);
            PersonResponse response = saved.ToPersonResponse();

            return response;
        }

        public async Task<List<PersonResponse>> GetAllPersons()
        {
            _logger.LogInformation("GetAllPersons of PersonService");

            var allPersons = await _personsRepository.GetAllAsync();
            return allPersons.Select(person => person.ToPersonResponse()).ToList();
        }

        public async Task<PersonResponse?> GetPerson(Guid? personId)
        {
            if (personId == null)
                return null;

            Person? person = await _personsRepository.GetAsync(personId.Value);

            if (person == null)
                return null;

            return person.ToPersonResponse();
        }

        public async Task<List<PersonResponse>> GetFilteredPersons(PersonSearchOptions searchBy, string? searchString)
        {
            _logger.LogInformation("GetFilteredPersons of PersonService");

            if(string.IsNullOrWhiteSpace(searchString))
                return (await _personsRepository.GetAllAsync()).Select(p => p.ToPersonResponse()).ToList();

            Expression<Func<Person, bool>> predicate = p => false;

            switch (searchBy)
            {
                case PersonSearchOptions.PersonName:
                    predicate = p => p.PersonName != null && p.PersonName.Contains(searchString);
                    break;

                case PersonSearchOptions.Email:
                        predicate = p => p.Email != null && p.Email.Contains(searchString);
                    break;

                case PersonSearchOptions.DateOfBirth:
                    if (DateOnly.TryParseExact(searchString, "dd MM yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly dateOfBirthSearch))
                        predicate = p => p.DateOfBirth.HasValue && p.DateOfBirth == dateOfBirthSearch;
                    break;

                case PersonSearchOptions.Age:
                    if (int.TryParse(searchString, out int searchAge))
                    {
                        var ageYearsAgo = DateOnly.FromDateTime(DateTime.Today.AddYears(-searchAge));
                        predicate = p => p.DateOfBirth.HasValue && p.DateOfBirth < ageYearsAgo && ageYearsAgo.AddYears(-1) < p.DateOfBirth;
                    }
                    break;
                case PersonSearchOptions.Gender:
                    predicate = p => p.Gender != null && p.Gender.Contains(searchString);
                    break;
                case PersonSearchOptions.Address:
                    predicate = p => p.Address != null && p.Address.Contains(searchString);
                    break;
                case PersonSearchOptions.ReceiveNewsLetter:
                    if(bool.TryParse(searchString, out bool receiveNewsLetterSearch))
                    {
                        predicate = p => p.ReceiveNewsLetters == receiveNewsLetterSearch;
                    }                        
                    break;
                default:
                    break;
            }

            var matchingPersons = await _personsRepository.GetFilteredPersonsAsync(predicate);

            return matchingPersons.Select(p => p.ToPersonResponse()).ToList();
        }

        public Task<List<PersonResponse>> GetSortedPersons(List<PersonResponse> allPersons, PersonSearchOptions? sortBy, SortOrderOptions sortOrder)
        {
            _logger.LogInformation("GetSortedPersons of PersonService");

            if (sortBy == null)
                return Task.FromResult<List<PersonResponse>>(allPersons);

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

            return Task.FromResult<List<PersonResponse>>(sortedPersons);
        }

        public async Task<PersonResponse> UpdatePerson(PersonUpdateRequest? personUpdateRequest)
        {
            if (personUpdateRequest == null)
                throw new ArgumentNullException(nameof(personUpdateRequest));

            ValidationHelper.ModelValidation(personUpdateRequest);

            Person? current = await _personsRepository.GetAsync(personUpdateRequest.PersonId);

            if (current == null)
                throw new InvalidPersonIdException("Given Person ID  doesn't exist", nameof(personUpdateRequest.PersonId));

            current.PersonName = personUpdateRequest.PersonName;
            current.Email = personUpdateRequest.Email;
            current.DateOfBirth = personUpdateRequest.DateOfBirth;
            current.Gender = personUpdateRequest.Gender.ToString();
            current.CountryId = personUpdateRequest.CountryId;
            current.Address = personUpdateRequest.Address;
            current.ReceiveNewsLetters = personUpdateRequest.ReceiveNewsLetters;

            await _personsRepository.UpdateAsync(current); //UPDATE

            return current.ToPersonResponse();
        }

        public async Task<bool> DeletePerson(Guid? personId)
        {
            if (personId == null)
                throw new ArgumentNullException(nameof(personId));

            Person? person = await _personsRepository.GetAsync(personId.Value);

            if (person == null)
                return false;

            await _personsRepository.DeleteAsync(personId.Value);

            return true;
        }
    }
}

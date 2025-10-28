using CsvHelper;
using CsvHelper.Configuration;
using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using OfficeOpenXml;
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
        private readonly ApplicationDbContext _dbContext;

        public PersonService(ICountriesService countriesService, ApplicationDbContext dbContext)
        {
            _countriesService = countriesService;
            _dbContext = dbContext;
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

            _dbContext.Persons.Add(person);
            await _dbContext.SaveChangesAsync();
            PersonResponse response = person.ToPersonResponse();

            return response;
        }

        public Task<List<PersonResponse>> GetAllPersons()
        {
            var allPersons = _dbContext.Persons.Include(t => t.Country)
                .Select(p => p.ToPersonResponse())
                .ToListAsync();

            return allPersons;
        }

        public async Task<PersonResponse?> GetPerson(Guid? personId)
        {
            if (personId == null)
                return null;

            Person? person = await _dbContext.Persons.Include(t => t.Country).FirstOrDefaultAsync(p => p.PersonId == personId);

            if (person == null)
                return null;

            return person.ToPersonResponse();
        }

        public async Task<List<PersonResponse>> GetFilteredPersons(PersonSearchOptions searchBy, string? searchString)
        {
            List<PersonResponse> allPersons = await GetAllPersons();
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

        public Task<List<PersonResponse>> GetSortedPersons(List<PersonResponse> allPersons, PersonSearchOptions? sortBy, SortOrderOptions sortOrder)
        {
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

            Person? result = await _dbContext.Persons.Where(p => p.PersonId == personUpdateRequest.PersonId).FirstOrDefaultAsync();

            if (result == null)
                throw new ArgumentException("Given Person ID  doesn't exist");

            result.PersonName = personUpdateRequest.PersonName;
            result.Email = personUpdateRequest.Email;
            result.DateOfBirth = personUpdateRequest.DateOfBirth;
            result.Gender = personUpdateRequest.Gender.ToString();
            result.CountryId = personUpdateRequest.CountryId;
            result.Address = personUpdateRequest.Address;
            result.ReceiveNewsLetters = personUpdateRequest.ReceiveNewsLetters;

            await _dbContext.SaveChangesAsync(); //UPDATE

            return result.ToPersonResponse();
        }

        public async Task<bool> DeletePerson(Guid? personId)
        {
            if (personId == null)
                throw new ArgumentNullException(nameof(personId));

            Person? person = await _dbContext.Persons.FirstOrDefaultAsync(p => p.PersonId == personId);

            if (person == null)
                return false;

            _dbContext.Remove(person);
            await _dbContext.SaveChangesAsync();

            return true;
        }

        public async Task<MemoryStream> GetPersonsCSV()
        {
            MemoryStream memoryStream = new MemoryStream();
            using StreamWriter streamWriter = new StreamWriter(memoryStream, leaveOpen: true);
            using CsvWriter csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);
            {
                csvWriter.WriteHeader<PersonResponse>();
                await csvWriter.NextRecordAsync();

                var persons = await GetAllPersons();
                await csvWriter.WriteRecordsAsync(persons);

                await streamWriter.FlushAsync();
            }

            memoryStream.Position = 0;

            return memoryStream;
        }

        public async Task<MemoryStream> GetPersonsCSVConfiguration()
        {
            CsvConfiguration configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ",",
                HasHeaderRecord = true,
                NewLine = Environment.NewLine
            };
            MemoryStream memoryStream = new MemoryStream();
            var persons = await GetAllPersons();
            using StreamWriter streamWriter = new StreamWriter(memoryStream, leaveOpen: true);
            using CsvWriter csvWriter = new CsvWriter(streamWriter, configuration);
            {
                //csvWriter.WriteHeader<PersonResponse>();
                //PersonName, Email, DateOfBirth, Country
                csvWriter.WriteField(nameof(PersonResponse.PersonName));
                csvWriter.WriteField(nameof(PersonResponse.Email));
                csvWriter.WriteField(nameof(PersonResponse.DateOfBirth));
                csvWriter.WriteField(nameof(PersonResponse.Country));

                await csvWriter.NextRecordAsync();                

                foreach (var person in persons)
                {
                    csvWriter.WriteField(person.PersonName);
                    csvWriter.WriteField(person.Email);
                    csvWriter.WriteField(person.DateOfBirth.HasValue ? person.DateOfBirth.Value.ToString("dd MM yyyy") : string.Empty);
                    csvWriter.WriteField(person.Country);
                    await csvWriter.NextRecordAsync();
                }

                //await csvWriter.WriteRecordsAsync(persons);

                await streamWriter.FlushAsync();
            }

            memoryStream.Position = 0;

            return memoryStream;
        }

        public async Task<MemoryStream> GetPersonsExcel()
        {
            MemoryStream memoryStream = new MemoryStream();
            using (ExcelPackage excelPackage = new ExcelPackage(memoryStream))
            {
                ExcelWorksheet workSheet = excelPackage.Workbook.Worksheets.Add("Persons");
                workSheet.Cells["A1"].Value = "Person Name";
                workSheet.Cells["B1"].Value = "Email";
                workSheet.Cells["C1"].Value = "Date Of Birth";
                workSheet.Cells["D1"].Value = "Country";

                var persons = await GetAllPersons();
                int row = 2;

                foreach (var person in persons)
                {
                    workSheet.Cells[row, 1].Value = person.PersonName;
                    workSheet.Cells[row, 2].Value = person.Email;
                    workSheet.Cells[row, 3].Value = person.DateOfBirth.HasValue ? person.DateOfBirth.Value.ToString("dd MM yyyy") : string.Empty;
                    workSheet.Cells[row, 4].Value = person.Country;

                    row++;
                }

                workSheet.Cells.AutoFitColumns();
                await excelPackage.SaveAsync();
            }

            memoryStream.Position = 0;
            return memoryStream;
        }
    }
}

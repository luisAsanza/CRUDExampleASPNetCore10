using BenchmarkDotNet.Attributes;
using Entities;
using Moq;
using RepositoryContracts;
using ServiceContracts;
using ServiceContracts.Enums;
using Services;
using System.Linq.Expressions;

namespace CRUDTests.Benchmarks
{
    [SimpleJob]
    public class PersonsServiceBenchmark
    {
        private List<Person> _testPersons;
        private const int PersonCount = 1000;

        [GlobalSetup]
        public void Setup()
        {
            // Generate test data - 1000 persons with diverse data
            _testPersons = new List<Person>(PersonCount);
            var random = new Random(42);
            var firstNames = new[] { "John", "Jane", "Michael", "Sarah", "David", "Emma", "Robert", "Lisa", "James", "Mary" };
            var lastNames = new[] { "Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Rodriguez", "Martinez" };
            var domains = new[] { "gmail.com", "yahoo.com", "outlook.com", "test.com", "example.com" };
            var genders = new[] { "Male", "Female", "Other" };
            var addresses = new[] { "123 Main St", "456 Oak Ave", "789 Pine Rd", "321 Elm St", "654 Maple Dr" };
            var today = DateTime.Today;

            for (int i = 0; i < PersonCount; i++)
            {
                var firstName = firstNames[random.Next(firstNames.Length)];
                var lastName = lastNames[random.Next(lastNames.Length)];
                var email = $"{firstName.ToLower()}{i}@{domains[random.Next(domains.Length)]}";

                _testPersons.Add(new Person
                {
                    PersonId = Guid.NewGuid(),
                    PersonName = $"{firstName} {lastName} {i}",
                    Email = email,
                    DateOfBirth = DateOnly.FromDateTime(today.AddYears(-random.Next(18, 65))),
                    Gender = genders[random.Next(genders.Length)],
                    CountryId = Guid.NewGuid(),
                    Address = addresses[random.Next(addresses.Length)],
                    ReceiveNewsLetters = random.Next(2) == 0
                });
            }
        }

        private Mock<IPersonsRepository> CreateRepositoryMock()
        {
            var repoMock = new Mock<IPersonsRepository>();

            repoMock
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(_testPersons);

            repoMock
                .Setup(r => r.GetFilteredPersonsAsync(It.IsAny<Expression<Func<Person, bool>>>()))
                .ReturnsAsync((Expression<Func<Person, bool>> predicate) =>
                    _testPersons.AsQueryable().Where(predicate).ToList());

            return repoMock;
        }

        [Benchmark]
        public async Task GetFilteredPersons_EmptySearch()
        {
            var repoMock = CreateRepositoryMock();
            var service = new PersonService(repoMock.Object);
            await service.GetFilteredPersons(PersonSearchOptions.PersonName, "");
        }

        [Benchmark]
        public async Task GetFilteredPersons_ByPersonName()
        {
            var repoMock = CreateRepositoryMock();
            var service = new PersonService(repoMock.Object);
            await service.GetFilteredPersons(PersonSearchOptions.PersonName, "John");
        }

        [Benchmark]
        public async Task GetFilteredPersons_ByEmail()
        {
            var repoMock = CreateRepositoryMock();
            var service = new PersonService(repoMock.Object);
            await service.GetFilteredPersons(PersonSearchOptions.Email, "gmail.com");
        }

        [Benchmark]
        public async Task GetFilteredPersons_ByGender()
        {
            var repoMock = CreateRepositoryMock();
            var service = new PersonService(repoMock.Object);
            await service.GetFilteredPersons(PersonSearchOptions.Gender, "Male");
        }

        [Benchmark]
        public async Task GetFilteredPersons_ByAddress()
        {
            var repoMock = CreateRepositoryMock();
            var service = new PersonService(repoMock.Object);
            await service.GetFilteredPersons(PersonSearchOptions.Address, "Main");
        }

        [Benchmark]
        public async Task GetFilteredPersons_ByAge()
        {
            var repoMock = CreateRepositoryMock();
            var service = new PersonService(repoMock.Object);
            await service.GetFilteredPersons(PersonSearchOptions.Age, "30");
        }

        [Benchmark]
        public async Task GetFilteredPersons_ByDateOfBirth()
        {
            var repoMock = CreateRepositoryMock();
            var service = new PersonService(repoMock.Object);

            var dateString = _testPersons[0].DateOfBirth.HasValue
                ? _testPersons[0].DateOfBirth.Value.ToString("dd MM yyyy")
                : "01 01 1990";

            await service.GetFilteredPersons(PersonSearchOptions.DateOfBirth, dateString);
        }

        [Benchmark]
        public async Task GetFilteredPersons_ByReceiveNewsLetter()
        {
            var repoMock = CreateRepositoryMock();
            var service = new PersonService(repoMock.Object);
            await service.GetFilteredPersons(PersonSearchOptions.ReceiveNewsLetter, "true");
        }
    }
}
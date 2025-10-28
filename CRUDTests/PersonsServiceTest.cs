using ServiceContracts;
using Entities;
using ServiceContracts.DTO;
using Services;
using ServiceContracts.Enums;
using Xunit.Abstractions;
using Microsoft.EntityFrameworkCore;
using AutoFixture;
using CRUDTests.AutoFixtureBuilder;
using FluentAssertions;

namespace CRUDTests
{
    public class PersonsServiceTest
    {
        //private fields
        private readonly IPersonService _personService;
        private readonly ICountriesService _countriesService;
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly IFixture _fixture;

        //constructor
        public PersonsServiceTest(ITestOutputHelper testOutputHelper)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            var dbContext = new ApplicationDbContext(options);

            _countriesService = new CountriesService(dbContext);
            _personService = new PersonService(_countriesService, dbContext);
            _testOutputHelper = testOutputHelper;
            _fixture = new Fixture();

            //Show AutoFixture how to create a DateOnly
            _fixture.Customize<DateOnly>(c => c.FromFactory(() => DateOnly.FromDateTime(_fixture.Create<DateTime>())));

            //Configure Email with proper value
            _fixture.Customizations.Add(new EmailForPropertyNamedEmailBuilder());
        }

        #region AddPerson

        //When we supply null value as PersonAddRequest, it should throw ArgumentNullException
        [Fact]
        public async Task AddPerson_NullPerson()
        {
            //Arrange
            PersonAddRequest? personAddRequest = null;

            //Act

            var action = async () =>
            {
                await _personService.AddPerson(personAddRequest);
            };

            await action.Should().ThrowAsync<ArgumentNullException>();
        }


        //When we supply null value as PersonName, it should throw ArgumentException
        [Fact]
        public async Task AddPerson_PersonNameIsNull()
        {
            //Arrange
            PersonAddRequest? personAddRequest = _fixture.Build<PersonAddRequest>()
                .With(t => t.PersonName, null as string).Create(); ;

            //Act
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await _personService.AddPerson(personAddRequest);
            });
        }

        //When we supply proper person details, it should insert the person into the persons list; and it should return an object of PersonResponse, which includes with the newly generated person id
        [Fact]
        public async Task AddPerson_ProperPersonDetails()
        {
            //Arrange
            //PersonAddRequest? personAddRequest = new PersonAddRequest() { PersonName = "Person name...", Email = "person@example.com", Address = "sample address", CountryId = Guid.NewGuid(), Gender = GenderOptions.Male, DateOfBirth = DateOnly.Parse("2000-01-01"), ReceiveNewsLetters = true };
            var personAddRequest = _fixture.Create<PersonAddRequest>();

            //Act
            PersonResponse person_response_from_add = await _personService.AddPerson(personAddRequest);

            List<PersonResponse> persons_list = await _personService.GetAllPersons();

            //Assert
            person_response_from_add.PersonId.Should().NotBeEmpty();
            persons_list.Should().Contain(person_response_from_add);
        }

        #endregion


        #region GetPersonByPersonID

        //If we supply null as PersonID, it should return null as PersonResponse
        [Fact]
        public async Task GetPersonByPersonID_NullPersonID()
        {
            //Arrange
            Guid? personID = null;

            //Act
            PersonResponse? person_response_from_get = await _personService.GetPerson(personID);

            //Assert
            person_response_from_get.Should().BeNull();
        }


        //If we supply a valid person id, it should return the valid person details as PersonResponse object
        [Fact]
        public async Task GetPersonByPersonID_WithPersonID()
        {
            //Arange
            CountryAddRequest country_request = _fixture.Create<CountryAddRequest>();
            CountryResponse country_response = await _countriesService.AddCountry(country_request);

            PersonAddRequest person_request = _fixture.Build<PersonAddRequest>()
                .With(f => f.CountryId, country_response.CountryId).Create();
            PersonResponse person_response_from_add = await _personService.AddPerson(person_request);
            PersonResponse? person_response_from_get = await _personService.GetPerson(person_response_from_add.PersonId);

            //Assert
            person_response_from_get.Should().BeEquivalentTo(person_response_from_add);
        }

        #endregion


        #region GetAllPersons

        //The GetAllPersons() should return an empty list by default
        [Fact]
        public async Task GetAllPersons_EmptyList()
        {
            //Act
            List<PersonResponse> persons_from_get = await _personService.GetAllPersons();

            //Assert
            persons_from_get.Should().BeEmpty();
        }


        //First, we will add few persons; and then when we call GetAllPersons(), it should return the same persons that were added
        [Fact]
        public async Task GetAllPersons_AddFewPersons()
        {
            //Arrange
            var countries = _fixture.CreateMany<CountryAddRequest>(2);
            CountryResponse country_response_1 = await _countriesService.AddCountry(countries.ElementAt(0));
            CountryResponse country_response_2 = await _countriesService.AddCountry(countries.ElementAt(1));

            List<PersonAddRequest> person_requests = new();

            person_requests.Add(_fixture.Build<PersonAddRequest>()
                .With(t => t.CountryId, country_response_1.CountryId).Create());
            person_requests.Add(_fixture.Build<PersonAddRequest>()
                .With(t => t.CountryId, country_response_1.CountryId).Create());
            person_requests.Add(_fixture.Build<PersonAddRequest>()
                .With(t => t.CountryId, country_response_2.CountryId).Create());


            List<PersonResponse> person_response_list_from_add = new ();

            foreach (PersonAddRequest person_request in person_requests)
            {
                PersonResponse person_response = await _personService.AddPerson(person_request);
                person_response_list_from_add.Add(person_response);
            }

            //Act
            List<PersonResponse> persons_list_from_get = await _personService.GetAllPersons();

            //Assert
            persons_list_from_get.Should().BeEquivalentTo(person_response_list_from_add);
        }
        #endregion


        #region GetFilteredPersons

        //If the search text is empty and search by is "PersonName", it should return all persons
        [Fact]
        public async Task GetFilteredPersons_EmptySearchText()
        {
            //Arrange
            var countryRequestList = _fixture.CreateMany<CountryAddRequest>(2).ToArray();
            var countryResponseList = new List<CountryResponse>() {
                await _countriesService.AddCountry(countryRequestList[0]),
                await _countriesService.AddCountry(countryRequestList[1])
            };
            var allowedCountryIds = countryResponseList.Select(c => c.CountryId).ToArray();
            var random = new Random();

            var persons = _fixture.Build<PersonAddRequest>()
                .Do(x => x.CountryId = allowedCountryIds[random.Next(allowedCountryIds.Length)])
                .CreateMany(3);

            List<PersonResponse> person_response_list_from_add = new List<PersonResponse>();

            foreach (PersonAddRequest person_request in persons)
            {
                PersonResponse person_response = await _personService.AddPerson(person_request);
                person_response_list_from_add.Add(person_response);
            }

            //print person_response_list_from_add
            _testOutputHelper.WriteLine("Expected:");
            foreach (PersonResponse person_response_from_add in person_response_list_from_add)
            {
                _testOutputHelper.WriteLine(person_response_from_add.ToString());
            }

            //Act
            List<PersonResponse> persons_list_from_search = await _personService.GetFilteredPersons(PersonSearchOptions.PersonName, "");

            //print persons_list_from_get
            _testOutputHelper.WriteLine("Actual:");
            foreach (PersonResponse person_response_from_get in persons_list_from_search)
            {
                _testOutputHelper.WriteLine(person_response_from_get.ToString());
            }

            //Assert
            person_response_list_from_add.Should().BeEquivalentTo(person_response_list_from_add);
        }


        //First we will add few persons; and then we will search based on person name with some search string. It should return the matching persons
        [Fact]
        public async Task GetFilteredPersons_SearchByPersonName()
        {
            //Arrange
            var countryAddRequests = _fixture.CreateMany<CountryAddRequest>(2).ToList();
            var countryAddResponses = new List<CountryResponse>()
            {
                await _countriesService.AddCountry(countryAddRequests[0]),
                await _countriesService.AddCountry(countryAddRequests[1])
            };

            var random = new Random();

            var personAddRequests = _fixture.Build<PersonAddRequest>()
                .Do(x => x.CountryId = countryAddResponses[random.Next(0,1)].CountryId)
                .CreateMany(3);

            var personResponseTasks = personAddRequests.Select(t => _personService.AddPerson(t)).ToList();
            var personResponses = await Task.WhenAll(personResponseTasks);

            //Act
            List<PersonResponse> filteredPersons = await _personService.GetFilteredPersons(PersonSearchOptions.PersonName, "ma");

            //Assert
            filteredPersons.Should().OnlyContain(p => p.PersonName != null && p.PersonName.Contains("ma", StringComparison.OrdinalIgnoreCase));
        }

        #endregion


        #region GetSortedPersons

        //When we sort based on PersonName in DESC, it should return persons list in descending on PersonName
        [Fact]
        public async Task GetSortedPersons()
        {
            //Arrange
            var countryAddRequests = _fixture.CreateMany<CountryAddRequest>(3).ToList();
            var countryAddResponses = new List<CountryResponse>();

            foreach (var countryAddRequest in countryAddRequests)
            {
                CountryResponse countryAddResponse = await _countriesService.AddCountry(countryAddRequest);
                countryAddResponses.Add(countryAddResponse);
            }

            var random = new Random();

            var personAddRequests = _fixture.Build<PersonAddRequest>()
                .Do(x => x.CountryId = countryAddResponses[random.Next(countryAddResponses.Count)].CountryId)
                .CreateMany(10);

            var personResponseTasks = personAddRequests.Select(t => _personService.AddPerson(t)).ToList();
            var personResponses = await Task.WhenAll(personResponseTasks);

            //Act
            var sortedPersons = await _personService.GetSortedPersons(personResponses.ToList(), PersonSearchOptions.PersonName, SortOrderOptions.DESC);

            //Assert
            sortedPersons.Should().BeInDescendingOrder(p => p.PersonName);

        }
        #endregion


        #region UpdatePerson

        //When we supply null as PersonUpdateRequest, it should throw ArgumentNullException
        [Fact]
        public async Task UpdatePerson_NullPerson()
        {
            //Arrange
            PersonUpdateRequest? person_update_request = null;

            //Act
            Func<Task> action = async () =>
            {
                await _personService.UpdatePerson(person_update_request);
            };

            //Assert
            await action.Should().ThrowAsync<ArgumentNullException>();
        }


        //When we supply invalid person id, it should throw ArgumentException
        [Fact]
        public async Task UpdatePerson_InvalidPersonID()
        {
            //Arrange
            PersonUpdateRequest? person_update_request = new PersonUpdateRequest() { PersonId = Guid.NewGuid() };

            //Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                //Act
                await _personService.UpdatePerson(person_update_request);
            });
        }


        //When PersonName is null, it should throw ArgumentException
        [Fact]
        public async Task UpdatePerson_PersonNameIsNull()
        {
            //Arrange
            CountryAddRequest country_add_request = new CountryAddRequest() { CountryName = "UK" };
            CountryResponse country_response_from_add = await _countriesService.AddCountry(country_add_request);

            PersonAddRequest person_add_request = new PersonAddRequest() { PersonName = "John", CountryId = country_response_from_add.CountryId, Email = "john@example.com", Address = "address...", Gender = GenderOptions.Male };

            PersonResponse person_response_from_add = await _personService.AddPerson(person_add_request);

            PersonUpdateRequest person_update_request = new PersonUpdateRequest()
            {
                PersonId = person_response_from_add.PersonId,
                PersonName = null,
                CountryId = country_response_from_add.CountryId,
                Email = ""
            };


            //Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                //Act
                await _personService.UpdatePerson(person_update_request);
            });

        }


        //First, add a new person and try to update the person name and email
        [Fact]
        public async Task UpdatePerson_PersonFullDetailsUpdation()
        {
            //Arrange
            CountryAddRequest country_add_request = new CountryAddRequest() { CountryName = "UK" };
            CountryResponse country_response_from_add = await _countriesService.AddCountry(country_add_request);

            PersonAddRequest person_add_request = new PersonAddRequest() { PersonName = "John", CountryId = country_response_from_add.CountryId, Address = "Abc road", DateOfBirth = DateOnly.Parse("2000-01-01"), Email = "abc@example.com", Gender = GenderOptions.Male, ReceiveNewsLetters = true };

            PersonResponse person_response_from_add = await _personService.AddPerson(person_add_request);

            PersonUpdateRequest person_update_request = new PersonUpdateRequest()
            {
                PersonId = person_response_from_add.PersonId,
                CountryId = country_response_from_add.CountryId,
                Address = "Abc road",
                DateOfBirth = DateOnly.Parse("2000-01-01"),
                PersonName = person_response_from_add.PersonName,
                Email = person_response_from_add.Email
            };

            //Act
            PersonResponse person_response_from_update = await _personService.UpdatePerson(person_update_request);

            PersonResponse? person_response_from_get = await _personService.GetPerson(person_response_from_update.PersonId);

            //Assert
            Assert.Equal(person_response_from_get, person_response_from_update);

        }

        #endregion


        #region DeletePerson

        //If you supply an valid PersonID, it should return true
        [Fact]
        public async Task DeletePerson_ValidPersonID()
        {
            //Arrange
            CountryAddRequest country_add_request = new CountryAddRequest() { CountryName = "USA" };
            CountryResponse country_response_from_add = await _countriesService.AddCountry(country_add_request);

            PersonAddRequest person_add_request = new PersonAddRequest() { PersonName = "Jones", Address = "address", CountryId = country_response_from_add.CountryId, DateOfBirth = DateOnly.Parse("2010-01-01"), Email = "jones@example.com", Gender = GenderOptions.Male, ReceiveNewsLetters = true };

            PersonResponse person_response_from_add = await _personService.AddPerson(person_add_request);


            //Act
            bool isDeleted = await _personService.DeletePerson(person_response_from_add.PersonId);

            //Assert
            Assert.True(isDeleted);
        }


        //If you supply an invalid PersonID, it should return false
        [Fact]
        public async Task DeletePerson_InvalidPersonID()
        {
            //Act
            bool isDeleted = await _personService.DeletePerson(Guid.NewGuid());

            //Assert
            Assert.False(isDeleted);
        }

        #endregion
    }
}

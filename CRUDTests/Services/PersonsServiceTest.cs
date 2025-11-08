using ServiceContracts;
using Entities;
using ServiceContracts.DTO;
using Services;
using ServiceContracts.Enums;
using Xunit.Abstractions;
using AutoFixture;
using CRUDTests.AutoFixtureBuilder;
using FluentAssertions;
using Moq;
using RepositoryContracts;
using System.Linq.Expressions;

namespace CRUDTests.Services
{
    public class PersonsServiceTest
    {
        //private fields
        private readonly IPersonService _personService;
        private readonly IFixture _fixture;
        private readonly Mock<IPersonsRepository> _personsRepositoryMock;

        //constructor
        public PersonsServiceTest(ITestOutputHelper testOutputHelper)
        {
            _personsRepositoryMock = new Mock<IPersonsRepository>();
            _personService = new PersonService(_personsRepositoryMock.Object);

            //Create AutoFixture object
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
            var personAddRequest = _fixture.Create<PersonAddRequest>();
            var personAdded = new Person()
            {
                PersonId = Guid.NewGuid(),
                PersonName = personAddRequest.PersonName,
                Email = personAddRequest.Email,
                DateOfBirth = personAddRequest.DateOfBirth,
                Address = personAddRequest.Address,
                CountryId = personAddRequest.CountryId,
                Gender = personAddRequest.Gender.ToString(),
                ReceiveNewsLetters = personAddRequest.ReceiveNewsLetters
            };

            _personsRepositoryMock.Setup(t => t.AddAsync(It.IsAny<Person>()))
            .ReturnsAsync(personAdded);

            //Act
            PersonResponse person_response_from_add = await _personService.AddPerson(personAddRequest);

            //Assert
            person_response_from_add.PersonId.Should().Be(personAdded.PersonId);
            _personsRepositoryMock.Verify(r => r.AddAsync(
                It.Is<Person>(p => p.PersonName == personAdded.PersonName &&
                p.Email == personAdded.Email &&
                p.DateOfBirth == personAdded.DateOfBirth)), Times.Once
                );
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
            var person = _fixture.Build<Person>().Without(c => c.Country).Create();
            _personsRepositoryMock.Setup(t => t.GetAsync(It.Is<Guid>(id => id == person.PersonId)))
                .ReturnsAsync(person);

            PersonResponse? actual = await _personService.GetPerson(person.PersonId);

            //Assert
            actual.Should().BeEquivalentTo(person.ToPersonResponse());
        }

        #endregion


        #region GetAllPersons

        //The GetAllPersons() should return an empty list by default
        [Fact]
        public async Task GetAllPersons_EmptyList_ReturnsEmptylist()
        {
            //Arrange
            _personsRepositoryMock.Setup(t => t.GetAllAsync()).ReturnsAsync(new List<Person>());

            //Act
            List<PersonResponse> persons_from_get = await _personService.GetAllPersons();

            //Assert
            persons_from_get.Should().BeEmpty();
        }


        //First, we will add few persons; and then when we call GetAllPersons(), it should return the same persons that were added
        [Fact]
        public async Task GetAllPersons_AddTenPersons_Return10Person()
        {
            //Arrange
            var persons = _fixture.Build<Person>().Without(p => p.Country).CreateMany(10).ToList();
            _personsRepositoryMock.Setup(t => t.GetAllAsync()).ReturnsAsync(persons);

            //Act
            List<PersonResponse> actual = await _personService.GetAllPersons();

            //Assert
            actual.Should().BeEquivalentTo(persons.Select(p => p.ToPersonResponse()));
            _personsRepositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
        }
        #endregion


        #region GetFilteredPersons

        //If the search text is empty and search by is "PersonName", it should return all persons
        [Fact]
        public async Task GetFilteredPersons_EmptySearchText()
        {
            //Arrange
            var seed = _fixture.Build<Person>().Without(p => p.Country).CreateMany<Person>(30).ToList();
            _personsRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(seed);
            _personsRepositoryMock.Setup(r => r.GetFilteredPersonsAsync(It.IsAny<Expression<Func<Person, bool>>>()))
                .ReturnsAsync((Expression<Func<Person, bool>> predicate) => seed.AsQueryable().Where(predicate).ToList());

            //Act
            List<PersonResponse> actual = await _personService.GetFilteredPersons(PersonSearchOptions.PersonName, "");

            //Assert
            actual.Should().BeEquivalentTo(seed.Select(t => t.ToPersonResponse()));
        }


        //First we will add few persons; and then we will search based on person name with some search string. It should return the matching persons
        [Fact]
        public async Task GetFilteredPersons_SearchByPersonName()
        {
            //Arrange
            var seed = _fixture.Build<Person>().Without(p => p.Country).CreateMany<Person>(30).ToList();
            var searchText = "SEARCHTEXT";
            seed[5].PersonName += searchText;
            seed[10].PersonName += searchText;
            seed[15].PersonName += searchText;
            seed[20].PersonName += searchText;

            _personsRepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(seed);
            _personsRepositoryMock.Setup(r => r.GetFilteredPersonsAsync(It.IsAny<Expression<Func<Person, bool>>>()))
                .ReturnsAsync((Expression<Func<Person, bool>> predicate) => seed.AsQueryable().Where(predicate).ToList());

            //Act
            List<PersonResponse> actual = await _personService.GetFilteredPersons(PersonSearchOptions.PersonName, searchText);

            //Assert
            actual.Should().BeEquivalentTo(new List<PersonResponse> { seed[5].ToPersonResponse(), seed[10].ToPersonResponse(), seed[15].ToPersonResponse(), seed[20].ToPersonResponse() });
        }

        #endregion


        #region GetSortedPersons

        //When we sort based on PersonName in DESC, it should return persons list in descending on PersonName
        [Fact]
        public async Task GetSortedPersons()
        {
            //Arrange
            var personResponses = _fixture.Build<PersonResponse>().CreateMany<PersonResponse>(20);

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
        public async Task UpdatePerson_WithPersonNameNull_ArgumentExceptionThrown()
        {
            //Arrange
            var personUpdateRequest = _fixture.Build<PersonUpdateRequest>()
                .With(t => t.PersonName, null as string)
                .Create();

            //Act
            var act = () => _personService.UpdatePerson(personUpdateRequest);

            //Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }


        //First, add a new person and try to update the person name and email
        [Fact]
        public async Task UpdatePerson_WhenValidRequest_UpdatesEntityAndReturnsMappedResponse()
        {
            //Arrange
            var personId = Guid.NewGuid();
            var personUpdateRequest = _fixture.Build<PersonUpdateRequest>()
                .With(t => t.PersonId, personId)
                .With(t => t.PersonName, "Updated Name")
                .With(t => t.Email, "test@test.com")
                .With(t => t.DateOfBirth, DateOnly.FromDateTime(DateTime.Now.AddYears(-30)))
                .Create();
            var personFromDb = _fixture.Build<Person>()
                .Without(t => t.Country)
                .With(t => t.PersonId, personId)
                .Create();
            Person? updatedEntity = null;
            _personsRepositoryMock.Setup(r => r.GetAsync(personId))
                .ReturnsAsync(personFromDb);
            _personsRepositoryMock.Setup(r => r.UpdateAsync(It.Is<Person>(t => t.PersonId == personId)))
                .Callback<Person>(p => updatedEntity = p)
                .ReturnsAsync((Person p) => p);

            //Act
            PersonResponse actual = await _personService.UpdatePerson(personUpdateRequest);

            //Assert
            updatedEntity.Should().NotBeNull();
            updatedEntity.PersonId.Should().Be(personId);
            updatedEntity.PersonName.Should().Be(personUpdateRequest.PersonName);
            updatedEntity.Email.Should().Be(personUpdateRequest.Email);
            updatedEntity.DateOfBirth.Should().Be(personUpdateRequest.DateOfBirth);

            actual.Should().NotBeNull();
            actual.PersonId.Should().Be(personId);
            actual.PersonName.Should().Be(updatedEntity.PersonName);
            actual.Email.Should().Be(updatedEntity.Email);
            actual.DateOfBirth.Should().Be(updatedEntity.DateOfBirth);

            _personsRepositoryMock.Verify(r => r.GetAsync(personId), Times.Once);
            _personsRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Person>()), Times.Once);
        }

        #endregion


        #region DeletePerson

        //If you supply an valid PersonID, it should return true
        [Fact]
        public async Task DeletePerson_ValidPersonID_PersonIsDeleted()
        {
            //Arrange
            var personToDelete = _fixture.Build<Person>()
                .Without(t => t.Country)
                .Create();
            _personsRepositoryMock
                .Setup(t => t.GetAsync(It.Is<Guid>(id => id == personToDelete.PersonId)))
                .ReturnsAsync(personToDelete);
            _personsRepositoryMock
                .Setup(t => t.DeleteAsync(It.Is<Guid>(id => id == personToDelete.PersonId)))
                .ReturnsAsync(true);

            //Act
            bool isDeleted = await _personService.DeletePerson(personToDelete.PersonId);

            //Assert
            isDeleted.Should().BeTrue();
            _personsRepositoryMock.Verify(
                r => r.DeleteAsync(It.Is<Guid>(id => id == personToDelete.PersonId)
                ), Times.Once());
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

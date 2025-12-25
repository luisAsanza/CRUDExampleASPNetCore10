using AutoFixture;
using Entities;
using FluentAssertions;
using Moq;
using RepositoryContracts;
using ServiceContracts;
using ServiceContracts.DTO;
using Services;
using System.Linq.Expressions;
using Xunit.Abstractions;
using Microsoft.Extensions.Logging.Abstractions;

namespace CRUDTests.Services
{
    public class CountriesServiceTest
    {
        private readonly ICountriesService _countryService;
        private readonly ITestOutputHelper _testOutputHelper;
        private readonly Mock<ICountriesRepository> _countriesRepositoryMock;
        private readonly Mock<ICacheService> _cacheServiceMock;
        private readonly IFixture _fixture;

        public CountriesServiceTest(ITestOutputHelper testOutputHelper)
        {
            _countriesRepositoryMock = new Mock<ICountriesRepository>();
            _cacheServiceMock = new Mock<ICacheService>();
            var logger = NullLogger<CountriesService>.Instance;
            _countryService = new CountriesService(_countriesRepositoryMock.Object, _cacheServiceMock.Object, logger);
            _testOutputHelper = testOutputHelper;
            _fixture = new Fixture();
            _fixture.Customize<DateOnly>(c => c.FromFactory(() => DateOnly.FromDateTime(_fixture.Create<DateTime>())));
        }

        #region AddCountry

        //When request object is null then throw Argument null Exception
        [Fact]
        public async Task AddCountry_RequestObjectIsNull_ArgumentNullException()
        {
            //Arrange
            CountryAddRequest? request = null;

            //Act
            var act = () => _countryService.AddCountry(request);

            //Assert
            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        //When CountryName is null, throw argument Exception
        [Fact]
        public async Task AddCountry_CountryNameIsNull_ArgumentException()
        {
            //Arrange
            CountryAddRequest? request = new CountryAddRequest() { CountryName = null };

            //Act
            var act = () => _countryService.AddCountry(request);

            //Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        //When CountryName is duplicated, throw Argument Exception
        [Fact]
        public async Task AddCountry_CountryNameDuplicated_ArgumentException()
        {
            //Arrange
            var countryName = "Country001";
            var countryAddRequest = _fixture
                .Build<CountryAddRequest>()
                .With(t => t.CountryName, countryName)
                .Create();
            _countriesRepositoryMock.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Country, bool>>>()))
                .ReturnsAsync(true);

            //Act
            var act = () => _countryService.AddCountry(countryAddRequest);

            //Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        //When proper name is suplied, it should insert the country to the existing list of countries
        [Fact]
        public async Task AddCountry_CountryNotDuplicated_CountryAdded()
        {
            //Arrange
            var countryName = "Country002";
            Country? addedCountry = null;
            var countryAddRequest = _fixture.Build<CountryAddRequest>()
                .With(t => t.CountryName, countryName)
                .Create();
            _countriesRepositoryMock
                .Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Country, bool>>>()))
                .ReturnsAsync(false);
            _countriesRepositoryMock
                .Setup(r => r.AddAsync(It.Is<Country>(c => c.CountryName == countryName)))
                .Callback<Country>(c =>
                {
                    if(c.CountryId == Guid.Empty)
                        c.CountryId = Guid.NewGuid();

                    addedCountry = c;
                })
                .ReturnsAsync((Country country) => country);
            _cacheServiceMock.Setup(c => c.Remove(It.IsAny<string>()));

            //Act
            var current = await _countryService.AddCountry(countryAddRequest);

            //Assert
            current.Should().NotBeNull();
            current.CountryId.Should().NotBe(Guid.Empty);
            current.CountryName.Should().Be(countryName);
            addedCountry.Should().NotBeNull();
            addedCountry.CountryName.Should().Be(countryName);
            addedCountry.CountryId.Should().NotBe(Guid.Empty);
            _countriesRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Country>()), Times.Once);
            _countriesRepositoryMock.Verify(r => r.AnyAsync(It.IsAny<Expression<Func<Country, bool>>>()), Times.Once);
            _cacheServiceMock.Verify(c => c.Remove("countries_all"), Times.Once);
        }

        #endregion

        #region GetAllCountries

        [Fact]
        public async Task GetAllCountries_ListIsEmpty_EmptyListIsReturned()
        {
            //Arrange
            var countries = new List<Country>();
            _countriesRepositoryMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(countries);
            // Setup cache to return false (cache miss)
            _cacheServiceMock.Setup(c => c.TryGetValue(It.IsAny<string>(), out It.Ref<List<CountryResponse>?>.IsAny))
                .Returns(false);

            //Act
            var actual = await _countryService.GetAllCountries();

            //Assert
            actual.Should().NotBeNull();
            actual.Should().BeEmpty();
            _countriesRepositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
            _countriesRepositoryMock.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task GetAllCountries_Add3Countries_3ItemsListReturned()
        {
            //Arrange
            var countries = _fixture.Build<Country>()
                .Without(p => p.Persons)
                .CreateMany(3)
                .ToList();
            _countriesRepositoryMock.Setup(r => r.GetAllAsync())
                .ReturnsAsync(countries);
            // Setup cache to return false (cache miss)
            _cacheServiceMock.Setup(c => c.TryGetValue(It.IsAny<string>(), out It.Ref<List<CountryResponse>?>.IsAny))
                .Returns(false);

            //Act
            var actual = await _countryService.GetAllCountries();

            //Assert
            actual.Should().NotBeNull();
            actual.Should().HaveCount(3);
            actual.Select(t => t.CountryId).Should().BeEquivalentTo(countries.Select(t => t.CountryId));
            actual.Select(t => t.CountryName).Should().BeEquivalentTo(countries.Select(t => t.CountryName));
            _countriesRepositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
            _countriesRepositoryMock.VerifyNoOtherCalls();
        }

        #endregion

        #region Get Country By ID (guid)

        [Fact]
        public async Task GetCountry_GivenCountryIdIsNull_ReturnNull()
        {
            //Arrange
            Guid? countryId = null;

            //Act
            CountryResponse? actual = await _countryService.GetCountry(countryId);

            //Assert
            actual.Should().BeNull();
        }

        [Fact]
        public async Task GetCountry_CountryDoesNotExist_ReturnNull()
        {
            //Arrange
            Guid? countryId = Guid.NewGuid();
            _countriesRepositoryMock.Setup(r => r.GetAsync(It.IsAny<Guid>()))
                .ReturnsAsync((Country?)null);

            //Act
            CountryResponse? actual = await _countryService.GetCountry(countryId);

            //Assert
            actual.Should().BeNull();
        }

        [Fact]
        public async Task GetCountry_CountryIdIsValid_ReturnCountry()
        {
            //Arrange
            var countryId = Guid.NewGuid();
            var countryName = "Country001";
            var country = _fixture.Build<Country>()
                .Without(c => c.Persons)
                .With(c => c.CountryId, countryId)
                .With(c => c.CountryName, countryName)
                .Create();
            _countriesRepositoryMock.Setup(r => r.GetAsync(country.CountryId))
                .ReturnsAsync(country);

            //Act
            CountryResponse? actual = await _countryService.GetCountry(countryId);

            //Assert
            actual.Should().NotBeNull();
            actual.CountryId.Should().Be(countryId);
            actual.CountryName.Should().Be(countryName);
        }

        #endregion
    }
}

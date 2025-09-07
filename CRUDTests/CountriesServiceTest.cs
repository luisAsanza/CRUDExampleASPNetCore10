using ServiceContracts;
using ServiceContracts.DTO;
using Services;
using Xunit.Abstractions;

namespace CRUDTests
{
    public class CountriesServiceTest
    {
        private readonly ICountriesService _countryService;
        private readonly ITestOutputHelper _testOutputHelper;

        public CountriesServiceTest(ITestOutputHelper testOutputHelper)
        {
            _countryService = new CountriesService();
            _testOutputHelper = testOutputHelper;
        }

        #region AddCountry

        //When request object is null then throw Argument null Exception
        [Fact]
        public void AddCountry_RequestObjectIsNull_ArgumentNullException()
        {
            //Arrange
            CountryAddRequest? request = null;

            //Assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                //Act
                _countryService.AddCountry(request);
            });
        }

        //When CountryName is null, throw argument Exception
        [Fact]
        public void AddCountry_CountryNameIsNull_ArgumentException()
        {
            //Arrange
            CountryAddRequest? request = new CountryAddRequest() { CountryName = null };

            //Assert
            Assert.Throws<ArgumentException>(() =>
            {
                //Act
                _countryService.AddCountry(request);
            });
        }

        //When CountryName is duplicated, throw Argument Exception
        [Fact]
        public void AddCountry_CountryNameDuplicated_ArgumentException()
        {
            //Arrange
            CountryAddRequest request1 = new CountryAddRequest() { CountryName = "Argentina" };
            CountryAddRequest request2 = new CountryAddRequest() { CountryName = "Argentina" };

            //Assert
            Assert.Throws<ArgumentException>(() =>
            {
                //Act
                _countryService.AddCountry(request1);
                _countryService.AddCountry(request2);
            });
        }

        //When proper name is suplied, it should insert the country to the existing list of countries
        [Fact]
        public void AddCountry_CountryNotDuplicated_CountryAdded()
        {
            //Arrange
            CountryAddRequest request = new CountryAddRequest() { CountryName = "Ecuador" };

            //Act
            var current = _countryService.AddCountry(request);

            //Assert
            Assert.True(current.CountryID != Guid.Empty);
        }

        #endregion

        #region GetAllCountries

        [Fact]
        public void GetAllCountries_ListIsEmpty_EmptyListIsReturned()
        {
            //Arrange

            //Act
            IEnumerable<CountryResponse> actual = _countryService.GetAllCountries();

            //Assert
            Assert.Empty(actual);
        }

        [Fact]
        public void GetAllCountries_Add3Countries_3ItemsListReturned()
        {
            //Arrange
            string countryName1 = "USA", countryName2 = "Canada", countryName3 = "Mexico";

            CountryAddRequest request1 = new CountryAddRequest() { CountryName = countryName1 };
            CountryAddRequest request2 = new CountryAddRequest() { CountryName = countryName2 };
            CountryAddRequest request3 = new CountryAddRequest() { CountryName = countryName3 };

            //Act
            _countryService.AddCountry(request1);
            _countryService.AddCountry(request2);
            _countryService.AddCountry(request3);

            //Assert
            Assert.Collection<CountryResponse>(_countryService.GetAllCountries(),
                item => Assert.Equal(countryName1, item.CountryName),
                item => Assert.Equal(countryName2, item.CountryName),
                item => Assert.Equal(countryName3, item.CountryName)
                );
        }

        [Fact]
        public void GetAllCountries_AddOneCountry_GuidIsNotEmpty()
        {
            //Arrange
            CountryAddRequest request = new CountryAddRequest() { CountryName = "Japan" };

            //Act
            CountryResponse response = _countryService.AddCountry(request);
            IEnumerable<CountryResponse> allCountries = _countryService.GetAllCountries();

            //Assert
            Assert.True(response.CountryID != Guid.Empty);
            Assert.Contains<CountryResponse>(allCountries, 
                item => item.CountryName == "Japan" && item.CountryID != Guid.Empty
                );
        }

        #endregion

        #region Get Country By ID (guid)

        [Fact]
        public void GetCountry_GivenCountryIDIsNull_ReturnNull()
        {
            //Arrange
            Guid? countryId = null;

            //Act
            CountryResponse? actual = _countryService.GetCountry(countryId);

            //Assert
            Assert.Null(actual);
        }

        [Fact]
        public void GetCountry_CountryDoesNotExist_ReturnNull()
        {
            //Arrange
            Guid? countryId = Guid.NewGuid();

            //Act
            CountryResponse? actual = _countryService.GetCountry(countryId);

            //Assert
            Assert.Null(actual);
        }

        [Fact]
        public void GetCountry_CountryIdIsValid_ReturnCountry()
        {
            //Arrange
            var countryName = "USA";
            var countryResponse = _countryService.AddCountry(new CountryAddRequest() { CountryName = countryName });

            //Act
            CountryResponse? actual = _countryService.GetCountry(countryResponse.CountryID);

            _testOutputHelper.WriteLine(actual?.ToString());

            //Assert
            Assert.NotNull(actual);
            Assert.NotEqual(Guid.Empty, actual.CountryID);
            Assert.Equal(countryName, actual.CountryName);
        }

        #endregion
    }
}

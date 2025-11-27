using BenchmarkDotNet.Attributes;
using Entities;
using Moq;
using RepositoryContracts;
using ServiceContracts;
using ServiceContracts.DTO;
using Services;

namespace BenchmarkSuite1
{
    [SimpleJob(warmupCount: 3, launchCount: 1)]
    [MemoryDiagnoser]
    public class CountriesCacheBenchmark
    {
        private ICountriesService _countriesServiceWithCache;
        private ICountriesService _countriesServiceWithoutCache;
        private Mock<ICountriesRepository> _repositoryMock;
        private Mock<ICacheService> _cacheMock;
        private List<Country> _testCountries;

        [GlobalSetup]
        public void Setup()
        {
            // Generate test data - 50 countries
            _testCountries = new List<Country>();
            for (int i = 0; i < 50; i++)
            {
                _testCountries.Add(new Country
                {
                    CountryId = Guid.NewGuid(),
                    CountryName = $"Country_{i:D3}"
                });
            }

            // Setup repository mock to return test countries
            _repositoryMock = new Mock<ICountriesRepository>();
            _repositoryMock
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(_testCountries);

            // Service WITHOUT cache - cache always misses
            var noCacheMock = new Mock<ICacheService>();
            noCacheMock
                .Setup(c => c.TryGetValue(It.IsAny<string>(), out It.Ref<List<CountryResponse>?>.IsAny))
                .Returns(false);
            _countriesServiceWithoutCache = new CountriesService(_repositoryMock.Object, noCacheMock.Object);

            // Service WITH cache - setup for cache hits after first call
            _cacheMock = new Mock<ICacheService>();
            var cachedCountries = _testCountries.Select(c => new CountryResponse { CountryId = c.CountryId, CountryName = c.CountryName }).ToList();
            
            // First call: cache miss, subsequent calls: cache hit
            var callCount = 0;
            _cacheMock
                .Setup(c => c.TryGetValue(It.IsAny<string>(), out It.Ref<List<CountryResponse>?>.IsAny))
                .Returns((string key, out List<CountryResponse>? value) =>
                {
                    callCount++;
                    if (callCount == 1)
                    {
                        value = null;
                        return false;
                    }
                    value = cachedCountries;
                    return true;
                });

            _countriesServiceWithCache = new CountriesService(_repositoryMock.Object, _cacheMock.Object);
        }

        [Benchmark]
        public async Task GetAllCountries_WithoutCache()
        {
            // Reset call count for each iteration by creating fresh service
            var noCacheMock = new Mock<ICacheService>();
            noCacheMock
                .Setup(c => c.TryGetValue(It.IsAny<string>(), out It.Ref<List<CountryResponse>?>.IsAny))
                .Returns(false);
            var service = new CountriesService(_repositoryMock.Object, noCacheMock.Object);

            // Simulate multiple calls - all hit database
            for (int i = 0; i < 5; i++)
            {
                await service.GetAllCountries();
            }
        }

        [Benchmark]
        public async Task GetAllCountries_WithCache()
        {
            // First call hits database, subsequent calls hit cache
            var cacheMock = new Mock<ICacheService>();
            var cachedCountries = _testCountries.Select(c => new CountryResponse { CountryId = c.CountryId, CountryName = c.CountryName }).ToList();
            var callCount = 0;

            cacheMock
                .Setup(c => c.TryGetValue(It.IsAny<string>(), out It.Ref<List<CountryResponse>?>.IsAny))
                .Returns((string key, out List<CountryResponse>? value) =>
                {
                    callCount++;
                    if (callCount == 1)
                    {
                        value = null;
                        return false;
                    }
                    value = cachedCountries;
                    return true;
                });

            cacheMock
                .Setup(c => c.Set(It.IsAny<string>(), It.IsAny<List<CountryResponse>>(), It.IsAny<int?>()))
                .Callback<string, List<CountryResponse>, int?>((key, value, minutes) => { });

            var service = new CountriesService(_repositoryMock.Object, cacheMock.Object);

            // Simulate multiple calls - first hits database, rest hit cache
            for (int i = 0; i < 5; i++)
            {
                await service.GetAllCountries();
            }
        }
    }
}
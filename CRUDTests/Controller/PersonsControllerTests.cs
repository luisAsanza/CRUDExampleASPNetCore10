using AutoFixture;
using CRUDExample.Controllers;
using CRUDTests.TestDoubles;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace CRUDTests.Controller
{
    public class PersonsControllerTests
    {
        private readonly Fixture _fixture;
        private readonly Mock<IPersonService> _personsServiceMock;
        private readonly Mock<ICountriesService> _countriesServiceMock;
        private readonly Mock<IConfiguration> _configurationMock;
        private readonly PersonsController _controller;

        public PersonsControllerTests()
        {
            _fixture = new Fixture();
            _fixture.Customize<DateOnly>(c => c.FromFactory(() => DateOnly.FromDateTime(_fixture.Create<DateTime>())));
            _personsServiceMock = new Mock<IPersonService>();
            _countriesServiceMock = new Mock<ICountriesService>();
            _configurationMock = new Mock<IConfiguration>();
            
            _controller = new PersonsController(_personsServiceMock.Object,
                _countriesServiceMock.Object,
                _configurationMock.Object,
                NullLogger<PersonsController>.Instance,
                NullDiagnosticContext.Instance);
        }

        [Fact]
        public async Task Index_WithPersons_ReturnsAViewResult()
        {
            // Arrange
            PersonSearchOptions searchBy = PersonSearchOptions.PersonName;
            string? search = null;
            PersonSearchOptions? sortBy = PersonSearchOptions.PersonName;
            SortOrderOptions sortOrder = SortOrderOptions.ASC;

            var filtered = _fixture.Build<PersonResponse>()
                .With(p => p.PersonName, _fixture.Create<string>()) //Avoid autofixture creating null values here
                .CreateMany(20).ToList();
            _personsServiceMock.Setup(service => service.GetFilteredPersons(searchBy, search))
                .ReturnsAsync(filtered);

            List<PersonResponse>? capturedForSort = null;
            var sorted = filtered.OrderBy(p => p.PersonName).ToList();
            _personsServiceMock.Setup(service => service.GetSortedPersons(
                It.IsAny<List<PersonResponse>>(), sortBy, sortOrder))
                .Callback<List<PersonResponse>, PersonSearchOptions?, SortOrderOptions>((lpr, _, _) => capturedForSort = lpr)
                .ReturnsAsync(sorted);

            // Act
            var result = await _controller.Index(searchBy, search, sortBy, sortOrder);

            // Assert
            result.Should().NotBeNull();
            ViewResult view = result.Should().BeOfType<ViewResult>().Subject;
            view.Model.Should().BeOfType(typeof(List<PersonResponse>)).And.BeSameAs(sorted);
            view.Model.As<List<PersonResponse>>().Count.Should().Be(filtered.Count);
            view.Model.As<List<PersonResponse>>().Should().BeInAscendingOrder(p => p.PersonName);
            capturedForSort.Should().BeSameAs(filtered);

            ((PersonSearchOptions)_controller.ViewBag.CurrentSearchBy).Should().Be(searchBy);
            ((string?)_controller.ViewBag.CurrentSearch).Should().Be(search);
            ((PersonSearchOptions?)_controller.ViewBag.CurrentSortBy).Should().Be(sortBy);
            ((SortOrderOptions)_controller.ViewBag.CurrentSortOrder).Should().Be(sortOrder);

            var searchFields = ((Dictionary<PersonSearchOptions, string>)_controller.ViewBag.SearchFields);
            searchFields.Should().NotBeNull();
            searchFields.Should().HaveCount(8).And.ContainKey(PersonSearchOptions.PersonName)
                .And.ContainKey(PersonSearchOptions.Email);

            _personsServiceMock.Verify(r => r.GetFilteredPersons(searchBy, search), Times.Once);
            _personsServiceMock.Verify(r => r.GetSortedPersons(It.IsAny<List<PersonResponse>>(), 
                sortBy, sortOrder), Times.Once);
            _personsServiceMock.VerifyNoOtherCalls();
        }
    }
}
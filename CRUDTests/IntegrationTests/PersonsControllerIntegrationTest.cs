using AngleSharp;
using AngleSharp.Dom;
using CRUDTests.IntegrationTests.WebAppFactory;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace CRUDTests.IntegrationTests
{
    /// <summary>
    /// TODO: Integration tests must live in a separate test project.
    /// </summary>
    public class PersonsControllerIntegrationTest : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly IBrowsingContext _browsingContext;

        public PersonsControllerIntegrationTest(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            //AngleSharp setup
            _browsingContext = BrowsingContext.New(Configuration.Default);
        }

        [Theory]
        [InlineData("/Persons/Index")]
        [InlineData("/Persons/Create")]
        [InlineData("/")]
        public async Task Index_ToReturnView(string requestUri)
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            HttpResponseMessage response = await client.GetAsync(requestUri);

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            response.Content.Headers.ContentType?.ToString().Should().Be("text/html; charset=utf-8");

            //AngleSharp to parse HTML content
            var content = await response.Content.ReadAsStringAsync();
            var document = await _browsingContext.OpenAsync(r => r.Content(content));
        }
    }
}

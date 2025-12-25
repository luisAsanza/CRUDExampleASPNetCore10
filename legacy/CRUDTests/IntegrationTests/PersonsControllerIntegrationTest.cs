using AngleSharp;
using AngleSharp.Html.Dom;
using CRUDTests.IntegrationTests.WebAppFactory;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using RazorPagesProject.Tests.Helpers;

namespace CRUDTests.IntegrationTests
{
    /// <summary>
    /// TODO: Integration tests must live in a separate test project.
    /// </summary>
    public class PersonsControllerIntegrationTest : IClassFixture<CustomWebApplicationFactory>
    {
        private readonly CustomWebApplicationFactory _factory;
        private readonly HttpClient _client;
        private readonly IBrowsingContext _browsingContext;

        //Hardcoded CountryIds from SeedPersonsDbForTestingAsync
        private static readonly Guid _countryId1 = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        private static readonly Guid _countryId2 = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");

        //Hardcode PersonIds from SeedPersonsDbForTestingAsync
        private static readonly Guid _personId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
        private static readonly Guid _personId2 = Guid.Parse("22222222-2222-2222-2222-222222222222");
        private static readonly Guid _personId3 = Guid.Parse("33333333-3333-3333-3333-333333333333");

        public PersonsControllerIntegrationTest(CustomWebApplicationFactory factory)
        {
            _factory = factory;
            _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
            {
                //To prevent the HttpClient from following redirects.
                AllowAutoRedirect = false, //This permits tests to check the result of the app's first response
                HandleCookies = true
            });

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

            // Act
            HttpResponseMessage response = await _client.GetAsync(requestUri);
            var responseContent = await response.Content.ReadAsStringAsync();

            // Assert
            response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            response.Content.Headers.ContentType?.ToString().Should().Be("text/html; charset=utf-8");
        }

        [Fact]
        public async Task Post_EditPerson_ReturnsRedirectToRoot()
        {
            // Arrange
            var defaultPage = await _client.GetAsync($"/Persons/Edit/{_personId1}");
            var defaultPageContent = await defaultPage.Content.ReadAsStringAsync();
            var documentDefaultpage = await _browsingContext.OpenAsync(r => r.Content(defaultPageContent));
            const string expectedLocation = "/persons/index";

            // Act
            var response = await _client.SendAsync(
                (IHtmlFormElement)documentDefaultpage.QuerySelector("form[id='EditPersonForm']"),
                (IHtmlButtonElement)documentDefaultpage.QuerySelector("button[id='UpdatePersonBtn']"));

            // Assert
            defaultPage.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            defaultPage.Content.Headers.ContentType?.ToString().Should().Be("text/html; charset=utf-8");

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.Redirect);
            response.Content.Headers.ContentType?.ToString().Should().Be("text/html; charset=utf-8");
            response.Headers.Location?.OriginalString.Should().Be(expectedLocation);
        }

        [Fact]
        public async Task Post_EditPerson_AntiForgeryTokenFails()
        {
            // Arrange
            var defaultPage = await _client.GetAsync($"/Persons/Edit/{_personId1}");
            var defaultPageContent = await defaultPage.Content.ReadAsStringAsync();
            var documentDefaultpage = await _browsingContext.OpenAsync(r => r.Content(defaultPageContent));

            var submitButton = documentDefaultpage.QuerySelector("button[id='UpdatePersonBtn']") as IHtmlButtonElement;
            submitButton.Should().NotBeNull("The Update Person's page must contain the Edit button.");

            var form = documentDefaultpage.QuerySelector("form[id='EditPersonForm']") as IHtmlFormElement;
            form.Should().NotBeNull("The Update Person's page must contain the form.");
            //Remove Antiforgery Token hidden input
            form.QuerySelector("input[name='__RequestVerificationToken']")?.Remove();
            form.QuerySelector("input[name='__RequestVerificationToken']").Should().BeNull();

            // Act
            var response = await _client.SendAsync(form, submitButton);

            // Assert
            defaultPage.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
            defaultPage.Content.Headers.ContentType?.ToString().Should().Be("text/html; charset=utf-8");

            response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);
        }
    }
}

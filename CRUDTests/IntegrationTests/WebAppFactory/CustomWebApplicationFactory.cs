using Entities;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Data.Common;


namespace CRUDTests.IntegrationTests.WebAppFactory
{
    public class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        //Create a unique DB name so tests don't leak data across runs
        private readonly string _dbName = $"PersonsDbForTesting_{Guid.NewGuid()}";

        private static readonly Guid _countryId1 = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
        private static readonly Guid _countryId2 = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb");
        private const string _testingEnv = "Testing";

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // 1) Remove the app's real Db Context objects registration
                services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
                services.RemoveAll<ApplicationDbContext>();
                services.RemoveAll<IDbContextFactory<ApplicationDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();

                // 2) Replace SQL with InMemory and Seed.
                //services.AddDbContext<ApplicationDbContext>(options =>
                //{
                //    options.UseInMemoryDatabase(_dbName);
                //    options.UseAsyncSeeding(SeedPersonsDbForTestingAsync);
                //    options.UseSeeding((DbContext ctx, bool storeOpPerformed) =>
                //    SeedPersonsDbForTestingAsync(ctx, storeOpPerformed, CancellationToken.None)
                //        .GetAwaiter().GetResult());
                //});

                // 2) Create one shared, open connection instance
                var keepAliveConnection = new SqliteConnection("Data Source=:memory:");
                keepAliveConnection.Open();

                // 3) Register the same connection instance.
                services.AddSingleton<DbConnection>(keepAliveConnection);

                // 4) Hook up DbContext to the connection
                services.AddDbContext<ApplicationDbContext>((sp, options) =>
                {
                    options.UseSqlite(sp.GetRequiredService<DbConnection>());
                    options.UseAsyncSeeding(SeedPersonsDbForTestingAsync);
                });

                // 5) This section of code creates a second Service Provider, which means a second set
                //    of singletons. Nevertheless, this works because keepAliveConnection is created outside
                //    of the DbContext registration, so the new sp points to the same connection instance.
                //    Instead of creating a second sp, we could also use a factory pattern to create the DbContext.
                //using var sp = services.BuildServiceProvider();
                //using var scope = sp.CreateScope();
                //var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                //db.Database.EnsureCreated();

                services.AddHostedService<EnsureDbCreatedHostedService>();

            });

            builder.UseEnvironment(_testingEnv);
        }        

        /// <summary>
        /// Seeds the test database with sample country and person data if no such data exists and a store operation has
        /// been performed.
        /// </summary>
        /// <remarks>This method is intended for use in testing scenarios to ensure that required country
        /// and person records exist in the database. No data is added if the relevant tables already contain entries or
        /// if <paramref name="storeOpPerformed"/> is <see langword="false"/>.</remarks>
        /// <param name="context">The database context to use for seeding data. Must be an instance of ApplicationDbContext.</param>
        /// <param name="storeOpPerformed">Indicates whether a store operation has already been performed. Data is seeded only if this value is <see
        /// langword="true"/>.</param>
        /// <param name="ct">A cancellation token that can be used to cancel the asynchronous operation.</param>
        /// <returns>A task that represents the asynchronous seeding operation.</returns>
        private async Task SeedPersonsDbForTestingAsync(DbContext context, bool storeOpPerformed, CancellationToken ct)
        {
            if (!storeOpPerformed)
            {
                return;
            }

            var db = (ApplicationDbContext)context;

            //Countries
            bool anyCountry = await db.Countries.AnyAsync(ct);
            if (!anyCountry)
            {
                db.Countries.AddRange(
                    new Country { CountryId = _countryId1, CountryName = "Argentina" },
                    new Country { CountryId = _countryId2, CountryName = "USA" }
                    );

                await db.SaveChangesAsync(ct);
            }

            //Persons
            bool anyPerson = await db.Persons.AnyAsync(ct);
            if (!anyPerson)
            {
                db.Persons.AddRange(
                    new Person
                    {
                        PersonName = "Alicia",
                        Email = "alicia@test.com",
                        CountryId = _countryId1,
                        DateOfBirth = new DateOnly(1970, 1, 1),
                        Address = "Address 1",
                        Gender = "Female",
                        ReceiveNewsLetters = true,
                        TIN = null
                    },
                    new Person
                    {
                        PersonName = "Carlos",
                        Email = "carlos@test.com",
                        CountryId = _countryId1,
                        DateOfBirth = new DateOnly(1980, 1, 1),
                        Address = "Address 2",
                        Gender = "Male",
                        ReceiveNewsLetters = true,
                        TIN = null
                    },
                    new Person
                    {
                        PersonName = "Luis",
                        Email = "luis@test.com",
                        CountryId = _countryId2,
                        DateOfBirth = new DateOnly(1990, 1, 1),
                        Address = "Address 3",
                        Gender = "Male",
                        ReceiveNewsLetters = true,
                        TIN = null
                    }
                    );

                await db.SaveChangesAsync(ct);
            }

        }
    }

    public sealed class EnsureDbCreatedHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        public EnsureDbCreatedHostedService( IServiceProvider serviceProvider ) => _serviceProvider = serviceProvider;

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await db.Database.EnsureCreatedAsync(cancellationToken);
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }
}

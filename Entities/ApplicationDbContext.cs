using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entities
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Person> Persons { get; set; }
        public DbSet<Country> Countries { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Country>().ToTable("Countries").HasKey(t => t.CountryId);

            if (Database.IsSqlServer())
            {
                modelBuilder.Entity<Person>().ToTable("Persons", t =>
                {
                    t.HasCheckConstraint("CHK_TIN", "len([TaxIdentificationNumber]) = 8");
                });
            }
            else if (Database.IsSqlite())
            {
                modelBuilder.Entity<Person>().ToTable("Persons", t =>
                {
                    t.HasCheckConstraint("CHK_TIN", "length([TaxIdentificationNumber]) = 8");
                });
            }

            //Seed Countries and Persons data
            //(var countries, var persons) = SeedData();

            //foreach (var country in countries)
            //{
            //    modelBuilder.Entity<Country>().HasData(country);
            //}
            //foreach (var person in persons)
            //{
            //    modelBuilder.Entity<Person>().HasData(person);
            //}

            //Fluent API
            modelBuilder.Entity<Person>().Property(t => t.TIN)
                .HasColumnName("TaxIdentificationNumber")
                .HasColumnType("varchar(8)")
                .HasDefaultValue("ABC12345");

            //modelBuilder.Entity<Person>().HasIndex(t => t.TIN).IsUnique();

            //Table Relationships
            modelBuilder.Entity<Person>()
                .HasOne<Country>(p => p.Country)
                .WithMany(t => t.Persons)
                .HasForeignKey(u => u.CountryId);
        }

        public IQueryable<Person> sp_GetAllPersons()
        {
            return Persons.FromSqlRaw("EXECUTE [dbo].[GetAllPersons]");
        }

        /// <summary>
        /// Exact data as in the JSON files. 
        /// NOTE: Two Person rows reference CountryIds that DO NOT exist in the countries list,
        /// which will violate FKs on SQLite (and any relational DB enforcing FK constraints).
        /// </summary>
        private static (Country[] Countries, Person[] Persons) SeedData()
        {
            var countries = new[]
            {
            new Country { CountryId = Guid.Parse("ef684ba0-0ad5-41d1-8223-4122c149f9da"), CountryName = "Argentina" },
            new Country { CountryId = Guid.Parse("ad914740-7d42-4374-83a0-6b939e2f1f05"), CountryName = "Brazil" },
            new Country { CountryId = Guid.Parse("08852737-942a-48e2-8622-6654af313a09"), CountryName = "China" }
        };

            var persons = new[]
            {
            new Person
            {
                PersonId = Guid.Parse("9403db58-adc5-494b-a455-b075aa7cfc46"),
                PersonName = "Alice Johnson",
                Email = "alice.johnson@example.com",
                DateOfBirth = new DateOnly(1962, 11, 25),
                Gender = "Other",
                CountryId = Guid.Parse("08852737-942a-48e2-8622-6654af313a09"),
                Address = "913 Main St, Cityville",
                ReceiveNewsLetters = true
            },
            new Person
            {
                PersonId = Guid.Parse("cd6f6dfa-71cf-4f74-ab7d-5359ac17bbea"),
                PersonName = "Bob Smith",
                Email = "bob.smith@example.com",
                DateOfBirth = new DateOnly(2004, 3, 5),
                Gender = "Male",
                CountryId = Guid.Parse("ad914740-7d42-4374-83a0-6b939e2f1f05"),
                Address = "332 Main St, Cityville",
                ReceiveNewsLetters = true
            },
            new Person
            {
                PersonId = Guid.Parse("210780c5-945e-4a1c-9287-f5dee8e67e53"),
                PersonName = "Carlos Rivera",
                Email = "carlos.rivera@example.com",
                DateOfBirth = new DateOnly(2005, 8, 29),
                Gender = "Female",
                CountryId = Guid.Parse("ad914740-7d42-4374-83a0-6b939e2f1f05"),
                Address = "688 Main St, Cityville",
                ReceiveNewsLetters = true
            },
        };

            return (countries, persons);
        }
    }
}

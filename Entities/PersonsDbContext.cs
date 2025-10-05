using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entities
{
    public class PersonsDbContext : DbContext
    {
        public DbSet<Person> Persons { get; set; }
        public DbSet<Country> Countries { get; set; }

        public PersonsDbContext(DbContextOptions options) : base(options)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Country>().ToTable("Countries");
            modelBuilder.Entity<Person>().ToTable("Persons");

            //Seed Country data
            string countriesJson = System.IO.File.ReadAllText("CountriesSeedData.json");
            var countries = System.Text.Json.JsonSerializer.Deserialize<List<Country>>(countriesJson) ?? new List<Country>();

            foreach (var country in countries)
            {
                modelBuilder.Entity<Country>().HasData(country);
            }

            //Seed Persons data
            string personsJson = System.IO.File.ReadAllText("PersonsSeedData.json");
            var persons = System.Text.Json.JsonSerializer.Deserialize<List<Person>>(personsJson) ?? new List<Person>();

            foreach(var person in persons)
            {
                modelBuilder.Entity<Person>().HasData(person);
            }
        }

        public IQueryable<Person> sp_GetAllPersons()
        {
            return Persons.FromSqlRaw("EXECUTE [dbo].[GetAllPersons]");
        }
    }
}

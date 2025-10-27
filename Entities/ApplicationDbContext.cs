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

            modelBuilder.Entity<Person>().ToTable("Persons", t =>
            {
                t.HasCheckConstraint("CHK_TIN", "len([TaxIdentificationNumber]) = 8");
            });

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
    }
}

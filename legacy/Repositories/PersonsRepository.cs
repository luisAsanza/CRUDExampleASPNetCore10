using Castle.Core.Logging;
using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RepositoryContracts;
using System.Linq.Expressions;

namespace Repositories
{
    public class PersonsRepository : IPersonsRepository
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<PersonsRepository> _logger;

        public PersonsRepository(ApplicationDbContext db, ILogger<PersonsRepository> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<Person> AddAsync(Person person)
        {
            _logger.LogInformation("AddAsync on PersonsRepository");

            _db.Persons.Add(person);
            await _db.SaveChangesAsync();

            return person;
        }

        public async Task<bool> DeleteAsync(Guid personId)
        {
            _db.Persons.RemoveRange(_db.Persons.Where(t => t.PersonId == personId));
            int rowsDeleted = await _db.SaveChangesAsync();

            return rowsDeleted > 0;
        }

        public async Task<Person?> GetAsync(Guid personId)
        {
            return await _db.Persons.Include(t => t.Country).FirstOrDefaultAsync(p => p.PersonId == personId);
        }

        public async Task<List<Person>> GetAllAsync()
        {
            return await _db.Persons.Include(p => p.Country).AsNoTracking().ToListAsync();
        }

        public async Task<List<Person>> GetFilteredPersonsAsync(Expression<Func<Person, bool>> predicate)
        {
            return await _db.Persons.Where(predicate).Include(p => p.Country).AsNoTracking().ToListAsync();
        }

        public async Task<Person> UpdateAsync(Person person)
        {
            var matchingPerson = await _db.Persons.FirstOrDefaultAsync(t => t.PersonId == person.PersonId);

            if (matchingPerson == null)
                return person;

            //Update the fields
            matchingPerson.PersonName = person.PersonName;
            matchingPerson.Email = person.Email;
            matchingPerson.DateOfBirth = person.DateOfBirth;
            matchingPerson.Gender = person.Gender;
            matchingPerson.CountryId = person.CountryId;
            matchingPerson.Address = person.Address;
            matchingPerson.ReceiveNewsLetters = person.ReceiveNewsLetters;
            matchingPerson.TIN = person.TIN;

            await _db.SaveChangesAsync();
            return matchingPerson;
        }
    }
}

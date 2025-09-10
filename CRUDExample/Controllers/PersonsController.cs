using Microsoft.AspNetCore.Mvc;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace CRUDExample.Controllers
{
    public class PersonsController : Controller
    {
        private readonly IPersonService _personService;
        private readonly ICountriesService _countriesService;

        public PersonsController(IPersonService personService, ICountriesService countriesService)
        {
            _personService = personService;
            _countriesService = countriesService;
        }

        [Route("persons/index")]
        [Route("/")]
        public IActionResult Index(PersonSearchOptions searchBy, string? search,
            PersonSearchOptions? sortBy, SortOrderOptions sortOrder)
        {
            ViewBag.SearchFields = new Dictionary<PersonSearchOptions, string>()
            {
                { PersonSearchOptions.PersonName, "Person Name" },
                { PersonSearchOptions.Email, "Email" },
                { PersonSearchOptions.DateOfBirth, "Date Of Birth" },
                { PersonSearchOptions.Age, "Age" },
                { PersonSearchOptions.Gender, "Gender" },
                { PersonSearchOptions.Country, "Country" },
                { PersonSearchOptions.Address, "Address" },
                { PersonSearchOptions.ReceiveNewsLetter, "Receive News Letter" }
            };

            //Get Persons
            List<PersonResponse> persons = _personService.GetFilteredPersons(searchBy, search);

            //Sort List
            persons = _personService.GetSortedPersons(persons, sortBy, sortOrder);

            //Persist current values of searchBy, search, sortBy, sortOrder
            ViewBag.CurrentSearchBy = searchBy;
            ViewBag.CurrentSearch = search;
            ViewBag.CurrentSortBy = sortBy;
            ViewBag.CurrentSortOrder = sortOrder;

            return View(persons);
        }
    }
}

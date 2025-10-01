using Microsoft.AspNetCore.Mvc;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace CRUDExample.Controllers
{
    [Route("[controller]")]
    public class PersonsController : Controller
    {
        private readonly IPersonService _personService;
        private readonly ICountriesService _countriesService;

        public PersonsController(IPersonService personService, ICountriesService countriesService)
        {
            _personService = personService;
            _countriesService = countriesService;
        }

        [Route("index")]
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

        [Route("create")]
        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Countries = _countriesService.GetAllCountries();

            return View();
        }

        [Route("create")]
        [HttpPost]
        public IActionResult Create(PersonAddRequest personAddRequest)
        {
            var countries = _countriesService.GetAllCountries();

            if (!ModelState.IsValid)
            {
                ViewBag.Countries = countries;
                ViewBag.Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return View();
            }

            PersonResponse personResponse = _personService.AddPerson(personAddRequest);

            return RedirectToAction("Index");
        }
    }
}

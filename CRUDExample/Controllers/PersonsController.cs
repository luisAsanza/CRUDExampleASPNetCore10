using Microsoft.AspNetCore.Mvc;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Microsoft.AspNetCore.Mvc.Rendering;

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
            var countries = _countriesService.GetAllCountries();
            ViewBag.Countries = countries.Select(countries => new SelectListItem()
            {
                Text = countries.CountryName,
                Value = countries.CountryId.ToString()
            }).ToList();


            return View(new PersonAddRequest());
        }

        [Route("create")]
        [HttpPost]
        public IActionResult Create(PersonAddRequest personAddRequest)
        {

            if (!ModelState.IsValid)
            {
                var countries = _countriesService.GetAllCountries();
                ViewBag.Countries = countries.Select(countries => new SelectListItem()
                {
                    Text = countries.CountryName,
                    Value = countries.CountryId.ToString()
                }).ToList();
                ViewBag.Errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return View();
            }

            PersonResponse personResponse = _personService.AddPerson(personAddRequest);

            return RedirectToAction("Index");
        }

        [HttpGet]
        [Route("[action]/{personId}")] //Eg: /persons/edit/1
        public IActionResult Edit(Guid personId)
        {
            PersonResponse? personResponse = _personService.GetPerson(personId);

            if (personResponse is null) {
                return RedirectToAction("Index");
            }

            var personUpdateRequest = PersonResponseToPersonUpdateRequest(personResponse);

            var countries = _countriesService.GetAllCountries();
            ViewBag.Countries = countries.Select(countries => new SelectListItem()
            {
                Text = countries.CountryName,
                Value = countries.CountryId.ToString()
            }).ToList();

            return View(personUpdateRequest);
        }

        [HttpPost]
        [Route("[action]/{personId}")]
        public IActionResult Edit(PersonUpdateRequest request)
        {
            PersonResponse? personResponse = _personService.GetPerson(request.PersonId);

            if (personResponse is null)
            {
                return RedirectToAction("Index");
            }

            if (!ModelState.IsValid) {
                var countries = _countriesService.GetAllCountries();
                ViewBag.Countries = countries.Select(countries => new SelectListItem()
                {
                    Text = countries.CountryName,
                    Value = countries.CountryId.ToString()
                }).ToList();
                return View(request);
            }

            _personService.UpdateResponse(request);

            return RedirectToAction("Index");
        }

        [HttpGet]
        [Route("[action]/{personId}")]
        public IActionResult Delete(Guid personId)
        {
            PersonResponse? personResponse = _personService.GetPerson(personId);
            if (personResponse is null)
            {
                return RedirectToAction("Index");
            }

            var personUpdateRequest = PersonResponseToPersonUpdateRequest(personResponse);

            return View(personUpdateRequest);
        }

        [HttpPost]
        [Route("[action]/{personId}")]
        public IActionResult Delete(PersonUpdateRequest request)
        {
            PersonResponse? personResponse = _personService.GetPerson(request.PersonId);
            if (personResponse is null)
            {
                return RedirectToAction("Index");
            }
            _personService.DeletePerson(request.PersonId);
            return RedirectToAction("Index");
        }

        #region Private Methods

        private static PersonUpdateRequest PersonResponseToPersonUpdateRequest(PersonResponse personResponse)
        {
            return new PersonUpdateRequest()
            {
                PersonId = personResponse.PersonId,
                PersonName = personResponse.PersonName,
                Email = personResponse.Email,
                DateOfBirth = personResponse.DateOfBirth,
                Gender = Enum.TryParse<GenderOptions>(personResponse.Gender, out GenderOptions result) ? result : null,
                CountryId = personResponse.CountryId,
                Address = personResponse.Address,
                ReceiveNewsLetters = personResponse.ReceiveNewsLetters
            };
        }

        #endregion

        
    }
}

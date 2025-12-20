using CRUDExample.Filters.ActionFilters;
using CRUDExample.Filters.ResourceFilters;
using CRUDExample.Filters.ResultFilters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Rotativa.AspNetCore;
using Serilog;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using ServiceContracts.ReportGenerator;

namespace CRUDExample.Controllers
{
    [Route("[controller]")]
    //[TypeFilter(typeof(HandleExceptionFilter))]
    [ResponseHeaderFilterFactory]
    public class PersonsController : Controller
    {
        private readonly IPersonService _personService;
        private readonly ICountriesService _countriesService;
        private readonly IFactoryReportGenerator _factoryReportGenerator;
        private readonly IConfiguration _configuration;
        private readonly ILogger<PersonsController> _logger;
        private readonly IDiagnosticContext _diagnosticContext;

        public PersonsController(IPersonService personService,
            ICountriesService countriesService, IFactoryReportGenerator factoryReportGenerator,
            IConfiguration configuration,
            ILogger<PersonsController> logger, IDiagnosticContext diagnosticContext)
        {
            _personService = personService;
            _countriesService = countriesService;
            _factoryReportGenerator = factoryReportGenerator;
            _configuration = configuration;
            _logger = logger;
            _diagnosticContext = diagnosticContext;
        }

        [Route("index")]
        [Route("/")]
        [TypeFilter(typeof(PersonsListActionFilter))]
        [TypeFilter(typeof(ResponseHeaderActionFilter), Arguments = new object[] { "X-Custom-Key", "X-Custom-Value", 1 })]
        [TypeFilter(typeof(PersonsListResultFilter))]
        public async Task<IActionResult> Index(PersonSearchOptions searchBy, string? search,
            PersonSearchOptions? sortBy, SortOrderOptions sortOrder)
        {
            _logger.LogInformation("Index Action method of PersonsController");
            _logger.LogDebug($"searchBy: {searchBy}, search: {search}");

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
            List<PersonResponse> persons = await _personService.GetFilteredPersons(searchBy, search);
            //Enrich Http request with persons data using Serilog
            _diagnosticContext.Set("Persons", persons);

            //Sort List
            persons = await _personService.GetSortedPersons(persons, sortBy, sortOrder);

            return View(persons);
        }

        [Route("create")]
        [HttpGet]
        [TypeFilter(typeof(FeatureDisabledResourceFilter))]
        public async Task<IActionResult> Create()
        {
            var countries = await _countriesService.GetAllCountries();
            ViewBag.Countries = countries.Select(countries => new SelectListItem()
            {
                Text = countries.CountryName,
                Value = countries.CountryId.ToString()
            }).ToList();

            return View(new PersonAddRequest());
        }

        [Route("create")]
        [HttpPost]
        [TypeFilter(typeof(PersonsCreateAndEditActionFilter))]        
        public async Task<IActionResult> Create(PersonAddRequest personAddRequest)
        {
            PersonResponse personResponse = await _personService.AddPerson(personAddRequest);

            return RedirectToAction("Index");
        }

        [HttpGet]
        [Route("[action]/{personId}")] //Eg: /persons/edit/1
        public async Task<IActionResult> Edit(Guid personId)
        {
            PersonResponse? personResponse = await _personService.GetPerson(personId);

            if (personResponse is null) {
                return RedirectToAction("Index");
            }

            var personUpdateRequest = PersonResponseToPersonUpdateRequest(personResponse);

            var countries = await _countriesService.GetAllCountries();
            ViewBag.Countries = countries.Select(countries => new SelectListItem()
            {
                Text = countries.CountryName,
                Value = countries.CountryId.ToString()
            }).ToList();

            return View(personUpdateRequest);
        }

        [HttpPost]
        [Route("[action]/{personId}")]
        [ValidateAntiForgeryToken]
        [TypeFilter(typeof(PersonsCreateAndEditActionFilter))]
        public async Task<IActionResult> Edit(PersonUpdateRequest request)
        {
            PersonResponse? personResponse = await _personService.GetPerson(request.PersonId);

            if (personResponse is null)
            {
                return RedirectToAction("Index");
            }

            await _personService.UpdatePerson(request);

            return RedirectToAction("Index");
        }

        [HttpGet]
        [Route("[action]/{personId}")]
        public async Task<IActionResult> Delete(Guid personId)
        {
            PersonResponse? personResponse = await _personService.GetPerson(personId);
            if (personResponse is null)
            {
                return RedirectToAction("Index");
            }

            var personUpdateRequest = PersonResponseToPersonUpdateRequest(personResponse);

            return View(personUpdateRequest);
        }

        [HttpPost]
        [Route("[action]/{personId}")]
        public async Task<IActionResult> Delete(PersonUpdateRequest request)
        {
            PersonResponse? personResponse = await _personService.GetPerson(request.PersonId);
            if (personResponse is null)
            {
                return RedirectToAction("Index");
            }
            await _personService.DeletePerson(request.PersonId);
            return RedirectToAction("Index");
        }

        [Route("[action]")]
        public async Task<IActionResult> PersonsPDF()
        {
            var persons = await _personService.GetAllPersons();

            return new ViewAsPdf(persons)
            {
                PageMargins = new Rotativa.AspNetCore.Options.Margins()
                {
                    Top = 20,
                    Bottom = 20,
                    Right = 20,
                    Left = 20
                },
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Landscape
            };
        }

        [Route("[action]")]
        public async Task<IActionResult> PersonsCSV()
        {
            const ReportOptions reportOption = ReportOptions.CSV;
            //const ReportOptions reportOption = ReportOptions.CSVConfigured;
            IStrategyReportGenerator reportGenerator = _factoryReportGenerator.GetStrategy(reportOption);
            var allPersons = await _personService.GetAllPersons();
            MemoryStream memoryStream = await reportGenerator.GenerateAllPersonsReport(allPersons);

            //Use application/octet-stream when we want browser to download the file
            //return File(memoryStream, "application/octet-stream", "Persons.csv");

            //Use text/csv to be more specific, sometimes the browser will try to open it in Excel
            //but most of the time will be downloaded
            return File(memoryStream, "text/csv", "Persons.csv");
        }

        [Route("[action]")]
        public async Task<IActionResult> PersonsExcel()
        {
            const ReportOptions reportOption = ReportOptions.Excel;
            IStrategyReportGenerator reportGenerator = _factoryReportGenerator.GetStrategy(reportOption);
            var allPersons = await _personService.GetAllPersons();
            MemoryStream memoryStream = await reportGenerator.GenerateAllPersonsReport(allPersons);

            return File(memoryStream, "application/vnd.openxmlformats-officedocument", "Persons.xlsx");
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

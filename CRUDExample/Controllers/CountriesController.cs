using Microsoft.AspNetCore.Mvc;
using ServiceContracts;

namespace CRUDExample.Controllers
{
    [Route("[controller]")]
    public class CountriesController : Controller
    {
        private readonly ICountriesService _countriesService;
        public CountriesController(ICountriesService countriesService)
        {
            _countriesService = countriesService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return View();
        }

        [Route("UploadFromExcel")]
        [HttpPost]
        public async Task<IActionResult> UploadFromExcel(IFormFile formFile)
        {
            var countriesAddedCount = await _countriesService.UploadCountriesFromExcelFile(formFile);
            return View("Index", countriesAddedCount);
        }
    }
}

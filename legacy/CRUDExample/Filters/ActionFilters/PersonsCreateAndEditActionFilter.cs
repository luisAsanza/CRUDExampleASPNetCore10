using CRUDExample.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Rendering;
using ServiceContracts;

namespace CRUDExample.Filters.ActionFilters
{
    public class PersonsCreateAndEditActionFilter : IAsyncActionFilter
    {
        private readonly ICountriesService _countriesService;
        private readonly ILogger<PersonsCreateAndEditActionFilter> _logger;

        public PersonsCreateAndEditActionFilter(ICountriesService countriesService, 
            ILogger<PersonsCreateAndEditActionFilter> logger)
        {
            _countriesService = countriesService;
            _logger = logger;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            _logger.LogInformation("PersonsCreateAndEditActionFilter.OnActionExecutionAsync");

            if(context.Controller is PersonsController personsController &&
                !context.ModelState.IsValid)
            {
                _logger.LogWarning("ModelState is invalid. Retrieving countries list for ViewBag");

                var countries = await _countriesService.GetAllCountries();
                
                personsController.ViewBag.Countries = countries.Select(country =>
                new SelectListItem()
                {
                    Text = country.CountryName,
                    Value = country.CountryId.ToString()
                }).ToList();

                var errors = personsController
                    .ModelState.Values.SelectMany(t => t.Errors).Select(e => e.ErrorMessage).ToList();
                
                _logger.LogDebug("Validation errors count: {ErrorCount}", errors.Count);
                personsController.ViewBag.Errors = errors;
                
                context.Result = personsController.View();
            }
            else
            {
                _logger.LogDebug("ModelState is valid or controller is not PersonsController. Proceeding to action");
                await next();
            }
        }
    }
}

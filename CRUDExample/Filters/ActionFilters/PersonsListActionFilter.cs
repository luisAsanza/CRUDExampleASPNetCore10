using CRUDExample.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using ServiceContracts.Enums;

namespace CRUDExample.Filters.ActionFilters
{
    /// <summary>
    /// Action filter for PersonsController that manages search and sort parameters.
    /// Persists filter state between action execution phases via HttpContext.Items.
    /// </summary>
    public class PersonsListActionFilter : IActionFilter
    {
        private readonly static object SearchByKey = new();
        private readonly static object SearchKey = new();
        private readonly ILogger<PersonsListActionFilter> _logger;

        public PersonsListActionFilter(ILogger<PersonsListActionFilter> logger)
        {
            _logger = logger;
        }

        public void OnActionExecuting(ActionExecutingContext context)
        {
            _logger.LogInformation("PersonsListActionFilter.OnActionExecuting");

            LogActionArguments(context.ActionArguments);
            PersistFilterParameters(context);
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            _logger.LogInformation("PersonsListActionFilter.OnActionExecuted");

            if (context.Controller is not PersonsController personsController)
            {
                _logger.LogWarning("PersonsListActionFilter applied to non-PersonsController");
                return;
            }

            SetViewBagCurrentSearchBy(personsController, context);
            SetViewBagCurrentSearch(personsController, context);
            SetViewBagCurrentSortBy(personsController, context);
            SetViewBagCurrentSortOrder(personsController, context);
        }

        private void LogActionArguments(IDictionary<string, object?> actionArguments)
        {
            if (!_logger.IsEnabled(LogLevel.Debug))
                return;

            foreach (var item in actionArguments)
            {
                _logger.LogDebug("Action Argument: {Key} = {Value}", item.Key, item.Value);
            }
        }

        private void PersistFilterParameters(ActionExecutingContext context)
        {
            if (context.ActionArguments.TryGetValue("searchBy", out var searchBy))
            {
                context.HttpContext.Items[SearchByKey] = searchBy;
            }

            if (context.ActionArguments.TryGetValue("search", out var search))
            {
                context.HttpContext.Items[SearchKey] = search;
            }
        }

            private void SetViewBagCurrentSearchBy(PersonsController personsController, ActionExecutedContext context)
        {
            var searchByValue = context.HttpContext.Items[SearchByKey]?.ToString();
            var searchBy = TryParseEnum<PersonSearchOptions>(searchByValue, ignoreCase: true) 
                ?? PersonSearchOptions.PersonName;
            
            personsController.ViewBag.CurrentSearchBy = searchBy;
        }

        private void SetViewBagCurrentSearch(PersonsController personsController, ActionExecutedContext context)
        {
            personsController.ViewBag.CurrentSearch = context.HttpContext.Items[SearchKey];
        }

        private void SetViewBagCurrentSortBy(PersonsController personsController, ActionExecutedContext context)
        {
            var sortByValue = context.HttpContext.Request.Query["sortBy"].ToString();
            var sortBy = TryParseEnum<PersonSearchOptions>(sortByValue, ignoreCase: true);
            
            personsController.ViewBag.CurrentSortBy = sortBy;
        }

        private void SetViewBagCurrentSortOrder(PersonsController personsController, ActionExecutedContext context)
        {
            var sortOrderValue = context.HttpContext.Request.Query["sortOrder"].ToString();
            var sortOrder = TryParseEnum<SortOrderOptions>(sortOrderValue, ignoreCase: true) 
                ?? SortOrderOptions.ASC;
            
            personsController.ViewBag.CurrentSortOrder = sortOrder;
        }

        private static T? TryParseEnum<T>(string? value, bool ignoreCase = false) where T : struct, Enum
        {
            if (string.IsNullOrWhiteSpace(value))
                return null;

            return Enum.TryParse<T>(value, ignoreCase, out var result) ? result : null;
        }
    }
}

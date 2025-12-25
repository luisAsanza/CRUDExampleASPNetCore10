using Microsoft.AspNetCore.Mvc.Filters;

namespace CRUDExample.Filters.ResultFilters
{
    /// <summary>
    /// Implements an always-run result filter for actions related to persons.
    /// </summary>
    /// <remarks>This filter executes for every result in the request pipeline, regardless of whether other
    /// filters have short-circuited the pipeline. Use this class to apply logic that must always run after actions
    /// involving persons, such as logging or auditing. Implements the <see cref="IAlwaysRunResultFilter"/>
    /// interface.</remarks>
    public class PersonsAlwaysRunResultFilter : IAlwaysRunResultFilter
    {
        public void OnResultExecuted(ResultExecutedContext context)
        {
            throw new NotImplementedException();
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            throw new NotImplementedException();
        }
    }
}

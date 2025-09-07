using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace ServiceContracts
{
    public interface IPersonService
    {
        PersonResponse AddPerson(PersonAddRequest request);
        List<PersonResponse> GetAllPersons();
        PersonResponse? GetPerson(Guid? personId);
        List<PersonResponse> GetFilteredPersons(string searchBy, string? searchString);
        List<PersonResponse> GetSortedPersons(List<PersonResponse> allPersons, string sortBy, SortOrderEnum sortOrder);
        PersonResponse UpdateResponse(PersonUpdateRequest? personUpdateRequest);
        bool DeletePerson(Guid? personId);
    }
}

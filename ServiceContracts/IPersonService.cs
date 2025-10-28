using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace ServiceContracts
{
    public interface IPersonService
    {
        Task<PersonResponse> AddPerson(PersonAddRequest request);
        Task<List<PersonResponse>> GetAllPersons();
        Task<PersonResponse?> GetPerson(Guid? personId);
        Task<List<PersonResponse>> GetFilteredPersons(PersonSearchOptions searchBy, string? searchString);
        Task<List<PersonResponse>> GetSortedPersons(List<PersonResponse> allPersons, PersonSearchOptions? sortBy, SortOrderOptions sortOrder);
        Task<PersonResponse> UpdatePerson(PersonUpdateRequest? personUpdateRequest);
        Task<bool> DeletePerson(Guid? personId);
        Task<MemoryStream> GetPersonsCSV();
        Task<MemoryStream> GetPersonsCSVConfiguration();
        Task<MemoryStream> GetPersonsExcel();
    }
}

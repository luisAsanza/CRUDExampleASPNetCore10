using ServiceContracts.DTO;

namespace ServiceContracts.ReportGenerator
{
    public interface IStrategyReportGenerator
    {
        /// <summary>
        /// Generates a report containing information about all persons and returns it as a memory stream.
        /// </summary>
        /// <remarks>The report format and contents depend on the implementation. The returned stream is
        /// suitable for reading or saving to a file. This method executes asynchronously and may perform I/O
        /// operations.</remarks>
        /// <returns>A <see cref="MemoryStream"/> containing the generated report data for all persons. The stream is positioned
        /// at the beginning and must be disposed by the caller.</returns>
        Task<MemoryStream> GenerateAllPersonsReport(List<PersonResponse> persons);
    }
}

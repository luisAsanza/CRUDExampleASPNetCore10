using CsvHelper;
using ServiceContracts.DTO;
using ServiceContracts.ReportGenerator;
using System.Globalization;

namespace Services.ReportGenerator
{
    public class CsvStrategyReportGenerator : IStrategyReportGenerator
    {
        public async Task<MemoryStream> GenerateAllPersonsReport(List<PersonResponse> persons)
        {
            MemoryStream memoryStream = new MemoryStream();
            using StreamWriter streamWriter = new StreamWriter(memoryStream, leaveOpen: true);
            using CsvWriter csvWriter = new CsvWriter(streamWriter, CultureInfo.InvariantCulture);
            {
                csvWriter.WriteHeader<PersonResponse>();
                await csvWriter.NextRecordAsync();

                await csvWriter.WriteRecordsAsync(persons);

                await streamWriter.FlushAsync();
            }

            memoryStream.Position = 0;

            return memoryStream;
        }
    }
}

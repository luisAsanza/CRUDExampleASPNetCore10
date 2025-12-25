using CsvHelper;
using CsvHelper.Configuration;
using ServiceContracts.DTO;
using ServiceContracts.ReportGenerator;
using System.Globalization;

namespace Services.ReportGenerator
{
    public class CsvConfiguredStrategyReportGenerator : IStrategyReportGenerator
    {
        public async Task<MemoryStream> GenerateAllPersonsReport(List<PersonResponse> persons)
        {
            CsvConfiguration configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ",",
                HasHeaderRecord = true,
                NewLine = Environment.NewLine
            };
            MemoryStream memoryStream = new MemoryStream();
            using StreamWriter streamWriter = new StreamWriter(memoryStream, leaveOpen: true);
            using CsvWriter csvWriter = new CsvWriter(streamWriter, configuration);
            {
                //csvWriter.WriteHeader<PersonResponse>();
                //PersonName, Email, DateOfBirth, Country
                csvWriter.WriteField(nameof(PersonResponse.PersonName));
                csvWriter.WriteField(nameof(PersonResponse.Email));
                csvWriter.WriteField(nameof(PersonResponse.DateOfBirth));
                csvWriter.WriteField(nameof(PersonResponse.Country));

                await csvWriter.NextRecordAsync();

                foreach (var person in persons)
                {
                    csvWriter.WriteField(person.PersonName);
                    csvWriter.WriteField(person.Email);
                    csvWriter.WriteField(person.DateOfBirth.HasValue ? person.DateOfBirth.Value.ToString("dd MM yyyy") : string.Empty);
                    csvWriter.WriteField(person.Country);
                    await csvWriter.NextRecordAsync();
                }

                //await csvWriter.WriteRecordsAsync(persons);

                await streamWriter.FlushAsync();
            }

            memoryStream.Position = 0;

            return memoryStream;
        }
    }
}

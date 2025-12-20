using OfficeOpenXml;
using ServiceContracts.DTO;
using ServiceContracts.ReportGenerator;

namespace Services.ReportGenerator
{
    public class ExcelStrategyReportGenerator : IStrategyReportGenerator
    {
        public async Task<MemoryStream> GenerateAllPersonsReport(List<PersonResponse> persons)
        {
            MemoryStream memoryStream = new MemoryStream();
            using (ExcelPackage excelPackage = new ExcelPackage(memoryStream))
            {
                ExcelWorksheet workSheet = excelPackage.Workbook.Worksheets.Add("Persons");
                workSheet.Cells["A1"].Value = "Person Name";
                workSheet.Cells["B1"].Value = "Email";
                workSheet.Cells["C1"].Value = "Date Of Birth";
                workSheet.Cells["D1"].Value = "Country";

                int row = 2;

                foreach (var person in persons)
                {
                    workSheet.Cells[row, 1].Value = person.PersonName;
                    workSheet.Cells[row, 2].Value = person.Email;
                    workSheet.Cells[row, 3].Value = person.DateOfBirth.HasValue ? person.DateOfBirth.Value.ToString("dd MM yyyy") : string.Empty;
                    workSheet.Cells[row, 4].Value = person.Country;

                    row++;
                }

                workSheet.Cells.AutoFitColumns();
                await excelPackage.SaveAsync();
            }

            memoryStream.Position = 0;
            return memoryStream;
        }
    }
}

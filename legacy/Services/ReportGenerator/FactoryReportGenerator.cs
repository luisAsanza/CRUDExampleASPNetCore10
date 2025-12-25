using Microsoft.Extensions.DependencyInjection;
using ServiceContracts.Enums;
using ServiceContracts.ReportGenerator;

namespace Services.ReportGenerator
{
    public class FactoryReportGenerator : IFactoryReportGenerator
    {
        private readonly IServiceProvider _serviceProvider;
        public FactoryReportGenerator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }
        public IStrategyReportGenerator GetStrategy(ReportOptions option)
        {
            switch (option)
            {
                case ReportOptions.Excel:
                    return _serviceProvider.GetRequiredService<ExcelStrategyReportGenerator>();
                case ReportOptions.CSV:
                    return _serviceProvider.GetRequiredService<CsvStrategyReportGenerator>();
                case ReportOptions.CSVConfigured:
                    return _serviceProvider.GetRequiredService<CsvConfiguredStrategyReportGenerator>();
                default:
                    throw new NotImplementedException($"The report option '{option}' is not implemented.");
            }
        }
    }
}

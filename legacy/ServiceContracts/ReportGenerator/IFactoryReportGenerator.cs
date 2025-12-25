using ServiceContracts.Enums;

namespace ServiceContracts.ReportGenerator
{
    /// <summary>
    /// Defines a contract for generating strategy-specific report generators based on provided report options.
    /// </summary>
    public interface IFactoryReportGenerator
    {
        IStrategyReportGenerator GetStrategy(ReportOptions option);
    }
}

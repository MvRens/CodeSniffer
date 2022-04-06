using CodeSniffer.Core.Sniffer;

namespace CodeSniffer.Repository.Reports
{
    public interface IReportRepository
    {
        ValueTask<IReadOnlyDictionary<string, CsReportResult>> GetDefinitionsStatus();
    }
}

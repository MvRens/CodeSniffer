using CodeSniffer.Core.Sniffer;

namespace CodeSniffer.Repository.Reports
{
    public interface IReportRepository
    {
        ValueTask<string> Store(string definitionId, ICsReport report);

        ValueTask<IReadOnlyDictionary<string, CsReportResult>> GetDefinitionsStatus();
    }
}

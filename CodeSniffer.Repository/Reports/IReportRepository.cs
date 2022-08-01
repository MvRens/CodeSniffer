using CodeSniffer.Core.Sniffer;

namespace CodeSniffer.Repository.Reports
{
    public interface IReportRepository
    {
        ValueTask<string> Store(ICsScanReport report);
        ValueTask<IReadOnlyList<ICsScanReport>> GetActiveReports();
    }


    public interface ICsScanReport
    {
        public string DefinitionId { get; }
        public string SourceId { get; }
        public string RevisionId { get; }
        public string RevisionName { get; }
        public string Branch { get; }
        public IReadOnlyList<ICsScanReportCheck> Checks { get; }
    }


    public interface ICsScanReportCheck
    {
        public Guid PluginId { get; }
        public string Name { get; }
        public ICsReport Report { get; }
    }
}

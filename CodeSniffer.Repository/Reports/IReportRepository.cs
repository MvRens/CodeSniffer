using CodeSniffer.Core.Sniffer;

namespace CodeSniffer.Repository.Reports
{
    public interface IReportRepository
    {
        ValueTask<string> Store(ICsScanReport report);
        ValueTask<IReadOnlyDictionary<string, CsReportResult>> GetDefinitionsStatus();
    }


    public interface ICsScanReport
    {
        public string DefinitionId { get; }
        public string SourceId { get; }
        public string RevisionId { get; }
        public string RevisionName { get; }
        public IReadOnlyList<ICsScanReportCheck> Checks { get; }
    }


    public interface ICsScanReportCheck
    {
        public Guid PluginId { get; }
        public string Name { get; }
        public ICsReport Report { get; }
    }
}

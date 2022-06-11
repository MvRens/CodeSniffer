using CodeSniffer.Core.Sniffer;

namespace CodeSniffer.Repository.Reports
{
    public interface IReportRepository
    {
        ValueTask<string> Store(ICsJobReport report);
        ValueTask<IReadOnlyDictionary<string, CsReportResult>> GetDefinitionsStatus();
    }


    public interface ICsJobReport
    {
        public string DefinitionId { get; }
        public IReadOnlyList<ICsSourceReport> Sources { get; }
    }


    public interface ICsSourceReport
    {
        public Guid PluginId { get; }
        public string Name { get; }
        public IReadOnlyList<ICsCheckReport> Checks { get; }
    }


    public interface ICsCheckReport
    {
        public Guid PluginId { get; }
        public string Name { get; }
        public ICsReport Report { get; }
    }
}

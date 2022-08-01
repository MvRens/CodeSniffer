using System.Collections.Generic;
using CodeSniffer.Core.Sniffer;

namespace CodeSniffer.API.Report
{
    public class ScanReportViewModel
    {
        public string DefinitionId { get; }
        public string SourceId { get; }
        public string RevisionId { get; }
        public string RevisionName { get; }
        public string Branch { get; }
        public IReadOnlyList<ScanReportCheckViewModel> Checks { get; }


        public ScanReportViewModel(string definitionId, string sourceId, string revisionId, string revisionName, string branch, IReadOnlyList<ScanReportCheckViewModel> checks)
        {
            DefinitionId = definitionId;
            SourceId = sourceId;
            RevisionId = revisionId;
            RevisionName = revisionName;
            Branch = branch;
            Checks = checks;
        }
    }


    public class ScanReportCheckViewModel
    {
        public string Name { get; }
        public IReadOnlyDictionary<string, string>? Configuration { get; }
        public IReadOnlyList<ScanReportAssetViewModel> Assets { get; }


        public ScanReportCheckViewModel(string name, IReadOnlyDictionary<string, string>? configuration, IReadOnlyList<ScanReportAssetViewModel> assets)
        {
            Name = name;
            Configuration = configuration;
            Assets = assets;
        }
    }


    public class ScanReportAssetViewModel
    {
        public string Id { get; }
        public string Name { get; }
        public CsReportResult Result { get; }
        public string? Summary { get; }
        public IReadOnlyDictionary<string, string>? Properties { get; }
        public string? Output { get; }


        public ScanReportAssetViewModel(string id, string name, CsReportResult result, string? summary, IReadOnlyDictionary<string, string>? properties, string? output)
        {
            Id = id;
            Name = name;
            Result = result;
            Summary = summary;
            Properties = properties;
            Output = output;
        }
    }
}

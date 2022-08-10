using System.Collections.Generic;

namespace CodeSniffer.API.Report
{
    public class ReportsViewModel
    {
        public IReadOnlyList<ReportLookupViewModel> Definitions { get; }
        public IReadOnlyList<ReportLookupViewModel> Sources { get; }
        public IReadOnlyList<ReportViewModel> Reports { get; }
        public ReportResult Result { get; }


        public ReportsViewModel(IReadOnlyList<ReportLookupViewModel> definitions, IReadOnlyList<ReportLookupViewModel> sources, IReadOnlyList<ReportViewModel> reports, ReportResult result)
        {
            Definitions = definitions;
            Sources = sources;
            Reports = reports;
            Result = result;
        }
    }


    public class ReportLookupViewModel
    {
        public string Id { get; }
        public string Name { get; }


        public ReportLookupViewModel(string id, string name)
        {
            Id = id;
            Name = name;
        }
    }
    


    public class ReportViewModel
    {
        public int DefinitionIndex { get; }
        public int SourceIndex { get; }
        public string RevisionId { get; }
        public string RevisionName { get; }
        public string Branch { get; }
        public IReadOnlyList<ReportCheckViewModel> Checks { get; }
        public ReportResult Result { get; }


        public ReportViewModel(int definitionIndex, int sourceIndex, string revisionId, string revisionName, string branch, IReadOnlyList<ReportCheckViewModel> checks, ReportResult result)
        {
            DefinitionIndex = definitionIndex;
            SourceIndex = sourceIndex;
            RevisionId = revisionId;
            RevisionName = revisionName;
            Branch = branch;
            Checks = checks;
            Result = result;
        }
    }


    public class ReportCheckViewModel
    {
        public string Name { get; }
        public IReadOnlyDictionary<string, string>? Configuration { get; }
        public IReadOnlyList<ReportAssetViewModel> Assets { get; }
        public ReportResult Result { get; }


        public ReportCheckViewModel(string name, IReadOnlyDictionary<string, string>? configuration, IReadOnlyList<ReportAssetViewModel> assets, ReportResult result)
        {
            Name = name;
            Configuration = configuration;
            Assets = assets;
            Result = result;
        }
    }


    public class ReportAssetViewModel
    {
        public string Id { get; }
        public string Name { get; }
        public ReportResult Result { get; }
        public string? Summary { get; }
        public IReadOnlyDictionary<string, string>? Properties { get; }
        public string? Output { get; }


        public ReportAssetViewModel(string id, string name, ReportResult result, string? summary, IReadOnlyDictionary<string, string>? properties, string? output)
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

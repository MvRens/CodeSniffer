using System.Collections.Generic;

namespace CodeSniffer.API.Report
{
    public class DashboardViewModel
    {
        public IReadOnlyList<DashboardSourceViewModel> Sources { get; }
        public ReportResult Result { get; }


        public DashboardViewModel(IReadOnlyList<DashboardSourceViewModel> sources, ReportResult result)
        {
            Sources = sources;
            Result = result;
        }
    }


    public class DashboardSourceViewModel
    {
        public string Id { get; }
        public string Name { get; }
        public IReadOnlyList<DashboardBranchViewModel> Branches { get; }
        public ReportResult Result { get; }


        public DashboardSourceViewModel(string id, string name, IReadOnlyList<DashboardBranchViewModel> branches, ReportResult result)
        {
            Id = id;
            Name = name;
            Branches = branches;
            Result = result;
        }
    }
    


    public class DashboardBranchViewModel
    {
        public string Name { get; }
        public IReadOnlyList<DashboardFailedDefinitionViewModel> FailedDefinitions { get; }
        public ReportResult Result { get; }


        public DashboardBranchViewModel(string name, IReadOnlyList<DashboardFailedDefinitionViewModel> failedDefinitions, ReportResult result)
        {
            Name = name;
            FailedDefinitions = failedDefinitions;
            Result = result;
        }
    }


    public class DashboardFailedDefinitionViewModel
    {
        public string Name { get; }
        public IReadOnlyList<string> Summaries { get; }
        public ReportResult Result { get; }


        public DashboardFailedDefinitionViewModel(string name, IReadOnlyList<string> summaries, ReportResult result)
        {
            Name = name;
            Summaries = summaries;
            Result = result;
        }
    }
}

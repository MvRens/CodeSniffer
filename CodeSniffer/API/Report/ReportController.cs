using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeSniffer.Auth;
using CodeSniffer.Core.Sniffer;
using CodeSniffer.Core.Source;
using CodeSniffer.Repository.Checks;
using CodeSniffer.Repository.Reports;
using CodeSniffer.Repository.Source;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeSniffer.API.Report
{
    [Route("/api/reports")]
    public class ReportController : ControllerBase
    {
        private readonly IReportRepository reportRepository;
        private readonly ISourceRepository sourceRepository;
        private readonly IDefinitionRepository definitionRepository;


        public ReportController(IReportRepository reportRepository, ISourceRepository sourceRepository, IDefinitionRepository definitionRepository)
        {
            this.reportRepository = reportRepository;
            this.sourceRepository = sourceRepository;
            this.definitionRepository = definitionRepository;
        }


        [HttpGet("dashboard")]
        [Authorize(Policy = CsPolicyNames.Developers)]
        public async ValueTask<DashboardViewModel> Dashboard()
        {
            // TODO more optimized version of the repository call? doesn't matter for LiteDB as the entire object is read anyways, but could make a difference if another storage backend is implemented
            var reports = await reportRepository.GetActiveReports();
            if (reports.Count == 0)
                return new DashboardViewModel(Array.Empty<DashboardSourceViewModel>(), ReportResult.Success);


            var sources = (await sourceRepository.ListSources()).ToDictionary(s => s.Id, s => s.Name);
            var definitions = (await definitionRepository.List()).ToDictionary(s => s.Id, s => s.Name);

            var reportsBySource = reports
                .GroupBy(r => r.SourceId)
                .Select(sourceGroup =>
                {
                    var branches = sourceGroup
                        .GroupBy(s => s.Branch)
                        .Select(branchGroup =>
                        {
                            var failedDefinitions = branchGroup
                                .SelectMany(g => g.Checks.Select(c => new DefinitionCheck(g.DefinitionId, c)))
                                .Select(c =>
                                {
                                    var assets = c.Check.Report.Assets.ToList();

                                    return new DashboardFailedDefinitionViewModel(
                                        definitions.TryGetValue(c.DefinitionId, out var definitionName) ? definitionName : c.DefinitionId,
                                        assets
                                            .Where(a => !string.IsNullOrEmpty(a.Summary))
                                            .Select(a => a.Summary!)
                                            .Distinct()
                                            .ToList(),
                                        MapResult(assets.Count > 0 ? assets.Max(a => a.Result) : CsReportResult.Success));
                                })
                                .Where(c => c.Result != ReportResult.Success)
                                .OrderBy(c => c.Name, StringComparer.InvariantCultureIgnoreCase)
                                .ToList();

                            return new DashboardBranchViewModel(branchGroup.Key, failedDefinitions, failedDefinitions.Count > 0 ? failedDefinitions.Max(c => c.Result) : ReportResult.Success);
                        })
                        .OrderBy(b => b.Name, BranchComparer.Default)
                        .ToList();

                    return new DashboardSourceViewModel(
                        sourceGroup.Key,
                        sources.TryGetValue(sourceGroup.Key, out var sourceName) ? sourceName : sourceGroup.Key,
                        branches,
                        branches.Max(b => b.Result)
                    );
                })
                .OrderBy(s => s.Name, StringComparer.InvariantCultureIgnoreCase)
                .ToList();

            return new DashboardViewModel(reportsBySource, reportsBySource.Max(r => r.Result));
        }


        [HttpGet]
        [Authorize(Policy = CsPolicyNames.Developers)]
        public async ValueTask<ActionResult<ReportsViewModel>> SourceBranchReport(string source, string branch)
        {
            var reports = await reportRepository.GetSourceBranchReports(source, branch);
            if (reports.Count == 0)
                return NotFound();

            var sources = (await sourceRepository.ListSources()).ToDictionary(s => s.Id, s => s.Name);
            var definitions = (await definitionRepository.List()).ToDictionary(s => s.Id, s => s.Name);

            var definitionsMap = new Dictionary<string, int>();
            var definitionsLookup = new List<ReportLookupViewModel>();
            var sourcesMap = new Dictionary<string, int>();
            var sourcesLookup = new List<ReportLookupViewModel>();


            static int GetLookupIndex(IDictionary<string, int> map, ICollection<ReportLookupViewModel> lookup, string id, IReadOnlyDictionary<string, string> nameLookup)
            {
                if (map.TryGetValue(id, out var index))
                    return index;

                index = lookup.Count;
                lookup.Add(new ReportLookupViewModel(id, nameLookup.TryGetValue(id, out var name) ? name : id));
                map.Add(id, index);

                return index;
            }


            var mappedReports = reports
                .Select(r =>
                {
                    var checks = MapChecks(r.Checks);

                    return new ReportViewModel(
                        GetLookupIndex(definitionsMap, definitionsLookup, r.DefinitionId, definitions),
                        GetLookupIndex(sourcesMap, sourcesLookup, r.SourceId, sources),
                        r.RevisionId,
                        r.RevisionName,
                        r.Branch,
                        checks,
                        checks.Count > 0 ? checks.Max(c => c.Result) : ReportResult.Success
                    );
                })
                .ToList();

            return new ReportsViewModel(definitionsLookup, sourcesLookup, mappedReports,
                mappedReports.Count > 0 ? mappedReports.Max(r => r.Result) : ReportResult.Success);
        }


        private static IReadOnlyList<ReportCheckViewModel> MapChecks(IEnumerable<ICsScanReportCheck> checks)
        {
            return checks
                .Select(c =>
                {
                    var assets = MapAssets(c.Report.Assets);

                    return new ReportCheckViewModel(
                        c.Name,
                        c.Report.Configuration,
                        assets,
                        assets.Count > 0 ? assets.Max(a => a.Result) : ReportResult.Success
                    );
                })
                .ToList();
        }


        private static IReadOnlyList<ReportAssetViewModel> MapAssets(IEnumerable<ICsReportAsset> assets)
        {
            return assets
                .Select(a => new ReportAssetViewModel(
                    a.Id,
                    a.Name,
                    MapResult(a.Result),
                    a.Summary,
                    a.Properties,
                    a.Output
                ))
                .ToList();
        }


        private static ReportResult MapResult(CsReportResult result)
        {
            return result switch
            {
                CsReportResult.Success => ReportResult.Success,
                CsReportResult.Warning => ReportResult.Warning,
                CsReportResult.Critical => ReportResult.Critical,
                CsReportResult.Error => ReportResult.Error,
                _ => throw new ArgumentOutOfRangeException(nameof(result), result, null)
            };
        }


        private readonly struct DefinitionCheck
        {
            public readonly string DefinitionId;
            public readonly ICsScanReportCheck Check;


            public DefinitionCheck(string definitionId, ICsScanReportCheck check)
            {
                DefinitionId = definitionId;
                Check = check;
            }
        }
    }
}

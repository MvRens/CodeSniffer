using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeSniffer.Auth;
using CodeSniffer.Repository.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeSniffer.API.Report
{
    [Route("/api/reports")]
    public class ReportController : ControllerBase
    {
        private readonly IReportRepository reportRepository;

        
        public ReportController(IReportRepository reportRepository)
        {
            this.reportRepository = reportRepository;
        }


        [HttpGet("active")]
        [Authorize(Policy = CsPolicyNames.Developers)]
        public async ValueTask<IReadOnlyList<ScanReportViewModel>> List()
        {
            var definitions = await reportRepository.GetActiveReports();
            return definitions
                .Select(r => new ScanReportViewModel(
                    r.DefinitionId,
                    r.SourceId,
                    r.RevisionId,
                    r.RevisionName,
                    r.Branch,
                    r.Checks
                        .Select(c => new ScanReportCheckViewModel(
                            c.Name,
                            c.Report.Configuration,
                            c.Report.Assets
                                .Select(a => new ScanReportAssetViewModel(
                                    a.Id,
                                    a.Name,
                                    a.Result,
                                    a.Summary,
                                    a.Properties,
                                    a.Output
                                ))
                                .ToList()
                        ))
                        .ToList()
                ))
                .ToList();
        }
    }
}

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


        [HttpGet("definitionstatus")]
        [Authorize(Policy = CsPolicyNames.Developers)]
        public async ValueTask<IReadOnlyDictionary<string, string>> List()
        {
            var definitions = await reportRepository.GetDefinitionsStatus();
            return definitions.ToDictionary(p => p.Key, p => p.Value.ToString());
        }
    }
}

using System.Threading.Tasks;
using CodeSniffer.Repository.Reports;

namespace CodeSniffer.Facade
{
    public class ReportFacade : IReportFacade
    {
        private readonly IReportRepository reportRepository;


        public ReportFacade(IReportRepository reportRepository)
        {
            this.reportRepository = reportRepository;
        }


        public async ValueTask StoreReport(ICsScanReport report)
        {
            await reportRepository.Store(report);

            // TODO MUSTHAVE check if the state changed, if so mark it for notification
        }
    }
}

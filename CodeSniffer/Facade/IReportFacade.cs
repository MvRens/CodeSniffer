using System.Threading.Tasks;
using CodeSniffer.Repository.Reports;

namespace CodeSniffer.Facade
{
    public interface IReportFacade
    {
        ValueTask StoreReport(ICsScanReport report);
    }
}

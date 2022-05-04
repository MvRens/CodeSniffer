using System.Threading.Tasks;
using CodeSniffer.Core.Sniffer;

namespace CodeSniffer.Facade
{
    public interface IJobResultHandler
    {
        ValueTask StoreJobResult(ICsJobResult jobResult);
    }
}

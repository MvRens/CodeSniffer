using System.Threading.Tasks;
using CodeSniffer.Core.Sniffer;

namespace CodeSniffer.Facade
{
    public class JobResultHandler : IJobResultHandler
    {
        public ValueTask StoreJobResult(ICsJobResult jobResult)
        {
            // TODO store in repository
            // TODO check if the state changed, if so mark it for notification

            return default;
        }
    }
}

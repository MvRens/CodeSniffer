using System.Threading;
using System.Threading.Tasks;
using CodeSniffer.Core.Sniffer;

namespace CodeSniffer.Sniffer
{
    public interface IJobRunner
    {
        ValueTask<ICsJobResult> Execute(string definitionId, string workingCopyPath, ICsScanContext context, CancellationToken cancellationToken);
    }
}

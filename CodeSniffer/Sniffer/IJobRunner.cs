using System.Threading.Tasks;

namespace CodeSniffer.Sniffer
{
    public interface IJobRunner
    {
        ValueTask Execute(string definitionId, string workingCopyPath);
    }
}

using System.Threading.Tasks;
using CodeSniffer.Repository.Checks;

namespace CodeSniffer.Facade
{
    public interface IDefinitionFacade
    {
        ValueTask Initialize();
        ValueTask<string> Insert(CsDefinition newDefinition, string author);
        ValueTask Update(string id, CsDefinition newDefinition, string author);
        ValueTask Remove(string id, string author);
    }
}

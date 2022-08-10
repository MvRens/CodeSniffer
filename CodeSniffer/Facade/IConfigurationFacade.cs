using System.Threading.Tasks;
using CodeSniffer.Repository.Checks;
using CodeSniffer.Repository.Source;

namespace CodeSniffer.Facade
{
    public interface IConfigurationFacade
    {
        ValueTask Initialize();

        ValueTask<string> InsertSource(CsSource newSource, string author);
        ValueTask UpdateSource(string id, CsSource newSource, string author);
        ValueTask RemoveSource(string id, string author);

        ValueTask<string> InsertSourceGroup(CsSourceGroup newSourceGroup, string author);
        ValueTask UpdateSourceGroup(string id, CsSourceGroup newSourceGroup, string author);
        ValueTask RemoveSourceGroup(string id, string author);

        ValueTask<CsStoredDefinition> InsertDefinition(CsDefinition newDefinition, string author);
        ValueTask<CsStoredDefinition> UpdateDefinition(string id, CsDefinition newDefinition, string author);
        ValueTask RemoveDefinition(string id, string author);

    }
}

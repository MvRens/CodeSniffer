using System.Threading.Tasks;
using CodeSniffer.Repository.Checks;
using CodeSniffer.Repository.Source;
using CodeSniffer.Sniffer;

namespace CodeSniffer.Facade
{
    public class ConfigurationFacade : IConfigurationFacade
    {
        private readonly ISourceRepository sourceRepository;
        private readonly IDefinitionRepository definitionRepository;
        private readonly IRepositoryMonitor repositoryMonitor;


        public ConfigurationFacade(ISourceRepository sourceRepository, IDefinitionRepository definitionRepository, IRepositoryMonitor repositoryMonitor)
        {
            this.sourceRepository = sourceRepository;
            this.definitionRepository = definitionRepository;
            this.repositoryMonitor = repositoryMonitor;
        }


        public async ValueTask Initialize()
        {
            var sources = await sourceRepository.GetAllSources();
            var sourceGroups = await sourceRepository.GetAllSourceGroups();
            var definitions = await definitionRepository.GetAllDetails();

            repositoryMonitor.Initialize(sources, sourceGroups, definitions);
        }


        public async ValueTask<string> InsertSource(CsSource newSource, string author)
        {
            var id = await sourceRepository.InsertSource(newSource, author);
            repositoryMonitor.SourceChanged(id, newSource);
            return id;
        }


        public async ValueTask UpdateSource(string id, CsSource newSource, string author)
        {
            await sourceRepository.UpdateSource(id, newSource, author);
            repositoryMonitor.SourceChanged(id, newSource);
        }


        public async ValueTask RemoveSource(string id, string author)
        {
            await sourceRepository.RemoveSource(id, author);
            repositoryMonitor.SourceRemoved(id);
        }



        public async ValueTask<string> InsertSourceGroup(CsSourceGroup newSourceGroup, string author)
        {
            var id = await sourceRepository.InsertSourceGroup(newSourceGroup, author);
            repositoryMonitor.SourceGroupChanged(id, newSourceGroup);
            return id;
        }


        public async ValueTask UpdateSourceGroup(string id, CsSourceGroup newSourceGroup, string author)
        {
            await sourceRepository.UpdateSourceGroup(id, newSourceGroup, author);
            repositoryMonitor.SourceGroupChanged(id, newSourceGroup);
        }


        public async ValueTask RemoveSourceGroup(string id, string author)
        {
            await sourceRepository.RemoveSourceGroup(id, author);
            repositoryMonitor.SourceGroupRemoved(id);
        }



        public async ValueTask<string> InsertDefinition(CsDefinition newDefinition, string author)
        {
            var id = await definitionRepository.Insert(newDefinition, author);
            repositoryMonitor.DefinitionChanged(id, newDefinition);
            return id;
        }


        public async ValueTask UpdateDefinition(string id, CsDefinition newDefinition, string author)
        {
            await definitionRepository.Update(id, newDefinition, author);
            repositoryMonitor.DefinitionChanged(id, newDefinition);
        }


        public async ValueTask RemoveDefinition(string id, string author)
        {
            await definitionRepository.Remove(id, author);
            repositoryMonitor.DefinitionRemoved(id);
        }
    }
}

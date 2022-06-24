using System.Threading.Tasks;
using CodeSniffer.Repository.Checks;
using CodeSniffer.Sniffer;

namespace CodeSniffer.Facade
{
    public class DefinitionFacade : IDefinitionFacade
    {
        private readonly IDefinitionRepository definitionRepository;
        private readonly IRepositoryMonitor repositoryMonitor;


        public DefinitionFacade(IDefinitionRepository definitionRepository, IRepositoryMonitor repositoryMonitor)
        {
            this.definitionRepository = definitionRepository;
            this.repositoryMonitor = repositoryMonitor;
        }


        public async ValueTask Initialize()
        {
            var definitions = await definitionRepository.GetAllDetails();
            repositoryMonitor.Initialize(definitions);
        }


        public async ValueTask<string> Insert(CsDefinition newDefinition, string author)
        {
            var id = await definitionRepository.Insert(newDefinition, author);
            repositoryMonitor.DefinitionChanged(id, newDefinition);
            return id;
        }


        public async ValueTask Update(string id, CsDefinition newDefinition, string author)
        {
            await definitionRepository.Update(id, newDefinition, author);
            repositoryMonitor.DefinitionChanged(id, newDefinition);
        }


        public async ValueTask Remove(string id, string author)
        {
            await definitionRepository.Remove(id, author);
            repositoryMonitor.DefinitionRemoved(id);
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeSniffer.Auth;
using CodeSniffer.Repository.Checks;
using CodeSniffer.ViewModel.Checks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeSniffer.Controller
{
    [Route("/definitions")]
    public class DefinitionController : ControllerBase
    {
        private readonly IDefinitionRepository definitionRepository;


        public DefinitionController(IDefinitionRepository definitionRepository)
        {
            this.definitionRepository = definitionRepository;
        }


        [HttpGet]
        [Authorize(Policy = CsPolicyNames.Developers)]
        public async ValueTask<IEnumerable<ListDefinitionViewModel>> List()
        {
            var definitions = await definitionRepository.List();
            var viewModels = definitions.Select(d => new ListDefinitionViewModel(d.Id, d.Name));

            return viewModels;
        }
    }
}

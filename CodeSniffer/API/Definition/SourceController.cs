using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeSniffer.Auth;
using CodeSniffer.Facade;
using CodeSniffer.Repository.Source;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeSniffer.API.Definition
{
    [Route("/api/source")]
    public class SourceController : ControllerBase
    {
        private readonly IConfigurationFacade configurationFacade;
        private readonly ISourceRepository sourceRepository;


        public SourceController(IConfigurationFacade configurationFacade, ISourceRepository sourceRepository)
        {
            this.configurationFacade = configurationFacade;
            this.sourceRepository = sourceRepository;
        }


        [HttpGet]
        [Authorize(Policy = CsPolicyNames.Developers)]
        public async ValueTask<IEnumerable<ListSourceViewModel>> List()
        {
            var definitions = await sourceRepository.ListSources();
            var viewModels = definitions.Select(d => new ListSourceViewModel(d.Id, d.Name));

            return viewModels;
        }


        [HttpGet("/groups")]
        [Authorize(Policy = CsPolicyNames.Developers)]
        public async ValueTask<IEnumerable<ListSourceGroupViewModel>> ListGroups()
        {
            var definitions = await sourceRepository.ListSourceGroups();
            var viewModels = definitions.Select(d => new ListSourceGroupViewModel(d.Id, d.Name));

            return viewModels;
        }


        /*
        [HttpGet("plugins")]
        [Authorize(Policy = CsPolicyNames.Developers)]
        public PluginsViewModel Plugins()
        {
            var sourcePlugins = new List<PluginViewModel>();
            var checkPlugins = new List<PluginViewModel>();

            
            // Move to a helper method when needed in another controller method
            var cultures = Request.GetTypedHeaders().AcceptLanguage
                .OrderByDescending(l => l.Quality ?? 1)
                .Select(l =>
                {
                    try
                    {
                        return CultureInfo.GetCultureInfo(l.Value.ToString());
                    }
                    catch (CultureNotFoundException)
                    {
                        return null;
                    }
                })
                .Where(l => l != null)
                .Cast<CultureInfo>()
                .Distinct()
                .ToList();


            foreach (var pluginInfo in pluginManager)
            {
                var pluginViewModel = new PluginViewModel(pluginInfo.Id, pluginInfo.Plugin.Name,
                    pluginInfo.Plugin.DefaultOptions?.ToJsonString(DefaultOptionsSerializerOptions),
                    pluginInfo.Plugin is ICsPluginHelp pluginHelp ? pluginHelp.GetOptionsHelpHtml(cultures) : null);

                // ReSharper disable once ConvertIfStatementToSwitchStatement - not the same! a plugin could implement both.
                if (pluginInfo.Plugin is ICsSourceCodeRepositoryPlugin)
                    sourcePlugins.Add(pluginViewModel);

                if (pluginInfo.Plugin is ICsSnifferPlugin)
                    checkPlugins.Add(pluginViewModel);
            }

            return new PluginsViewModel(
                sourcePlugins.ToArray(),
                checkPlugins.ToArray()
            );
        }


        [HttpGet("{id}")]
        [Authorize(Policy = CsPolicyNames.Developers)]
        public async ValueTask<ActionResult<DefinitionViewModel>> GetDetails(string id)
        {
            try
            {
                var details = await definitionRepository.GetDetails(id);

                return Ok(new DefinitionViewModel
                {
                    Name = details.Name,
                    SourceGroupId = details.SourceGroupId,
                    Checks = details.Checks.Select(c => new DefinitionCheckViewModel
                    {
                        Name = c.Name,
                        PluginId = c.PluginId,
                        Configuration = c.Configuration.ToJsonString()
                    }).ToArray()
                });
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
        }


        [HttpPost]
        [Authorize(Policy = CsPolicyNames.Developers)]
        public async ValueTask<ActionResult<string>> InsertDetails([FromBody] DefinitionViewModel viewModel)
        {
            var definition = ViewModelToDefinition(viewModel);
            var id = await definitionFacade.Insert(definition, GetAuthor());
            return Ok(id);
        }


        [HttpPut("{id}")]
        [Authorize(Policy = CsPolicyNames.Developers)]
        public async ValueTask<ActionResult> UpdateDetails(string id, [FromBody] DefinitionViewModel viewModel)
        {
            var definition = ViewModelToDefinition(viewModel);
            
            try
            {
                await definitionFacade.Update(id, definition, GetAuthor());
                return NoContent();
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
        }


        [HttpDelete("{id}")]
        [Authorize(Policy = CsPolicyNames.Developers)]
        public async ValueTask<ActionResult> Delete(string id)
        {
            try
            {
                await definitionFacade.Remove(id, GetAuthor());
                return NoContent();
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
        }


        private string GetAuthor()
        {
            var usernameClaim = Request.HttpContext.User.FindFirst(ClaimTypes.Name);
            if (usernameClaim == null)
                throw new UnauthorizedAccessException();

            return usernameClaim.Value;
        }


        private static CsDefinition ViewModelToDefinition(DefinitionViewModel viewModel)
        {
            return new CsDefinition(
                viewModel.Name!, 
                viewModel.SourceGroupId!,
                viewModel.Checks?.Select(c => new CsDefinitionCheck(c.Name!, c.PluginId!.Value, ParseConfiguration(c.Configuration))).ToArray() ?? Array.Empty<CsDefinitionCheck>());
        }


        private static JsonObject ParseConfiguration(string? configuration)
        {
            if (configuration == null)
                return new JsonObject();

            return JsonNode.Parse(configuration) as JsonObject ?? new JsonObject();
        }
        */
    }
}

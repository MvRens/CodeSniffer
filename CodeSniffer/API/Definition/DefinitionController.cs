using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using CodeSniffer.Auth;
using CodeSniffer.Core.Plugin;
using CodeSniffer.Core.Sniffer;
using CodeSniffer.Facade;
using CodeSniffer.Plugins;
using CodeSniffer.Repository.Checks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeSniffer.API.Definition
{
    [Route("/api/definitions")]
    public class DefinitionController : ControllerBase
    {
        private readonly IConfigurationFacade configurationFacade;
        private readonly IDefinitionRepository definitionRepository;
        private readonly IPluginManager pluginManager;


        public DefinitionController(IConfigurationFacade configurationFacade, IDefinitionRepository definitionRepository, IPluginManager pluginManager)
        {
            this.configurationFacade = configurationFacade;
            this.definitionRepository = definitionRepository;
            this.pluginManager = pluginManager;
        }


        [HttpGet]
        [Authorize(Policy = CsPolicyNames.Developers)]
        public async ValueTask<IEnumerable<ListDefinitionViewModel>> List()
        {
            var definitions = await definitionRepository.List();
            var viewModels = definitions.Select(d => new ListDefinitionViewModel(d.Id, d.Name));

            return viewModels;
        }


        [HttpGet("plugins")]
        [Authorize(Policy = CsPolicyNames.Developers)]
        public async IAsyncEnumerable<PluginViewModel> Plugins()
        {
            await foreach (var pluginInfo in pluginManager.ByType<ICsSnifferPlugin>())
            { 
                await using var pluginLock = await pluginInfo.Acquire();

                yield return new PluginViewModel(pluginInfo.Id, pluginInfo.Name,
                    pluginLock.Plugin.DefaultOptions?.ToDisplayJsonString(),
                    pluginLock.Plugin is ICsPluginHelp pluginHelp
                        ? pluginHelp.GetOptionsHelpHtml(Request.Cultures())
                        : null);
            }
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
                        Configuration = c.Configuration.ToDisplayJsonString()
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
            var id = await configurationFacade.InsertDefinition(definition, Request.Author());
            return Ok(id);
        }


        [HttpPut("{id}")]
        [Authorize(Policy = CsPolicyNames.Developers)]
        public async ValueTask<ActionResult> UpdateDetails(string id, [FromBody] DefinitionViewModel viewModel)
        {
            var definition = ViewModelToDefinition(viewModel);
            
            try
            {
                await configurationFacade.UpdateDefinition(id, definition, Request.Author());
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
                await configurationFacade.RemoveDefinition(id, Request.Author());
                return NoContent();
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
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
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using CodeSniffer.Auth;
using CodeSniffer.Core.Plugin;
using CodeSniffer.Core.Source;
using CodeSniffer.Facade;
using CodeSniffer.Plugins;
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
        private readonly IPluginManager pluginManager;


        public SourceController(IConfigurationFacade configurationFacade, ISourceRepository sourceRepository, IPluginManager pluginManager)
        {
            this.configurationFacade = configurationFacade;
            this.sourceRepository = sourceRepository;
            this.pluginManager = pluginManager;
        }


        [HttpGet]
        [Authorize(Policy = CsPolicyNames.Developers)]
        public async ValueTask<IEnumerable<ListSourceViewModel>> List()
        {
            var sources = await sourceRepository.ListSources();
            var viewModels = sources.Select(d => new ListSourceViewModel(d.Id, d.Name));

            return viewModels;
        }


        [HttpGet("{id}")]
        [Authorize(Policy = CsPolicyNames.Developers)]
        public async ValueTask<ActionResult<SourceViewModel>> GetDetails(string id)
        {
            try
            {
                var details = await sourceRepository.GetSourceDetails(id);

                return Ok(new SourceViewModel
                {
                    Name = details.Name,
                    PluginId = details.PluginId.ToString(),
                    Configuration = details.Configuration.ToDisplayJsonString()
                });
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
        }


        [HttpPost]
        [Authorize(Policy = CsPolicyNames.Developers)]
        public async ValueTask<ActionResult<string>> InsertDetails([FromBody] SourceViewModel viewModel)
        {
            var source = ViewModelToSource(viewModel);
            var id = await configurationFacade.InsertSource(source, Request.Author());
            return Ok(id);
        }


        [HttpPut("{id}")]
        [Authorize(Policy = CsPolicyNames.Developers)]
        public async ValueTask<ActionResult> UpdateDetails(string id, [FromBody] SourceViewModel viewModel)
        {
            var source = ViewModelToSource(viewModel);

            try
            {
                await configurationFacade.UpdateSource(id, source, Request.Author());
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
            // TODO check if linked to a group
            
            try
            {
                await configurationFacade.RemoveSource(id, Request.Author());
                return NoContent();
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
        }


        [HttpGet("groups")]
        [Authorize(Policy = CsPolicyNames.Developers)]
        public async ValueTask<IEnumerable<ListSourceGroupViewModel>> ListGroups()
        {
            var sourceGroups = await sourceRepository.ListSourceGroups();
            var viewModels = sourceGroups.Select(d => new ListSourceGroupViewModel(d.Id, d.Name));

            return viewModels;
        }


        [HttpGet("group/{id}")]
        [Authorize(Policy = CsPolicyNames.Developers)]
        public async ValueTask<ActionResult<SourceGroupViewModel>> GetGroupDetails(string id)
        {
            try
            {
                var details = await sourceRepository.GetSourceGroupDetails(id);

                return Ok(new SourceGroupViewModel
                {
                    Name = details.Name,
                    SourceIds = details.SourceIds.ToArray()
                });
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
        }


        [HttpPost("group")]
        [Authorize(Policy = CsPolicyNames.Developers)]
        public async ValueTask<ActionResult<string>> InsertGroupDetails([FromBody] SourceGroupViewModel viewModel)
        {
            var sourceGroup = ViewModelToSourceGroup(viewModel);
            var id = await configurationFacade.InsertSourceGroup(sourceGroup, Request.Author());
            return Ok(id);
        }


        [HttpPut("group/{id}")]
        [Authorize(Policy = CsPolicyNames.Developers)]
        public async ValueTask<ActionResult> UpdateGroupDetails(string id, [FromBody] SourceGroupViewModel viewModel)
        {
            var sourceGroup = ViewModelToSourceGroup(viewModel);

            try
            {
                await configurationFacade.UpdateSourceGroup(id, sourceGroup, Request.Author());
                return NoContent();
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
        }


        [HttpDelete("group/{id}")]
        [Authorize(Policy = CsPolicyNames.Developers)]
        public async ValueTask<ActionResult> DeleteGroup(string id)
        {
            // TODO check if linked to a definition

            try
            {
                await configurationFacade.RemoveSourceGroup(id, Request.Author());
                return NoContent();
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
        }


        [HttpGet("plugins")]
        [Authorize(Policy = CsPolicyNames.Developers)]
        public IEnumerable<PluginViewModel> Plugins()
        {
            return pluginManager
                .ByType<ICsSourceCodeRepositoryPlugin>()
                .Select(pluginInfo => new PluginViewModel(pluginInfo.Id, pluginInfo.Plugin.Name,
                    pluginInfo.Plugin.DefaultOptions?.ToDisplayJsonString(),
                    pluginInfo.Plugin is ICsPluginHelp pluginHelp ? pluginHelp.GetOptionsHelpHtml(Request.Cultures()) : null)
                )
                .ToArray();
        }


        private static CsSource ViewModelToSource(SourceViewModel viewModel)
        {
            return new CsSource(
                viewModel.Name!,
                Guid.Parse(viewModel.PluginId!),
                ParseConfiguration(viewModel.Configuration));
        }


        private static CsSourceGroup ViewModelToSourceGroup(SourceGroupViewModel viewModel)
        {
            return new CsSourceGroup(
                viewModel.Name!,
                viewModel.SourceIds!);
        }


        private static JsonObject ParseConfiguration(string? configuration)
        {
            if (configuration == null)
                return new JsonObject();

            return JsonNode.Parse(configuration) as JsonObject ?? new JsonObject();
        }
    }
}

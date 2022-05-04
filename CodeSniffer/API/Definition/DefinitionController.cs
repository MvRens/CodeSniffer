using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using CodeSniffer.Auth;
using CodeSniffer.Core.Sniffer;
using CodeSniffer.Core.Source;
using CodeSniffer.Plugins;
using CodeSniffer.Repository.Checks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CodeSniffer.API.Definition
{
    [Route("/api/definitions")]
    public class DefinitionController : ControllerBase
    {
        private readonly IDefinitionRepository definitionRepository;
        private readonly IPluginManager pluginManager;

        private static readonly JsonSerializerOptions DefaultOptionsSerializerOptions = new()
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.Never,
            WriteIndented = true
        };


        public DefinitionController(IDefinitionRepository definitionRepository, IPluginManager pluginManager)
        {
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
        public PluginsViewModel Plugins()
        {
            var sourcePlugins = new List<PluginViewModel>();
            var checkPlugins = new List<PluginViewModel>();

            foreach (var pluginInfo in pluginManager)
            {
                var pluginViewModel = new PluginViewModel(pluginInfo.Id, pluginInfo.Plugin.Name, pluginInfo.Plugin.DefaultOptions?.ToJsonString(DefaultOptionsSerializerOptions));

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


        [HttpGet("/:id")]
        [Authorize(Policy = CsPolicyNames.Developers)]
        public async ValueTask<ActionResult<DefinitionViewModel>> GetDetails(string id)
        {
            try
            {
                var details = await definitionRepository.GetDetails(id);

                return Ok(new DefinitionViewModel
                {
                    Name = details.Name,
                    Checks = details.Checks.Select(c => new DefinitionCheckViewModel
                    {
                        Name = c.Name,
                        PluginName = c.PluginName,
                        Configuration = c.Configuration.ToJsonString()
                    }).ToArray(),
                    Sources = details.Checks.Select(s => new DefinitionSourceViewModel
                    {
                        Name = s.Name,
                        PluginName = s.PluginName,
                        Configuration = s.Configuration.ToJsonString()
                    }).ToArray()
                });
            }
            catch (InvalidOperationException)
            {
                return NotFound();
            }
        }


        // TODO save
    }
}

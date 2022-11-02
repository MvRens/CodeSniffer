using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CodeSniffer.Auth;
using CodeSniffer.Plugins;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace CodeSniffer.API.Admin
{
    [Route("/api/plugins")]
    [Authorize(Policy = CsPolicyNames.Admins)]
    public class PluginController : ControllerBase
    {
        private readonly ILogger logger;
        private readonly IPluginManager pluginManager;


        public PluginController(ILogger logger, IPluginManager pluginManager)
        {
            this.logger = logger;
            this.pluginManager = pluginManager;
        }


        [HttpGet]
        public IEnumerable<PluginContainerViewModel> List()
        {
            return pluginManager
                .GroupBy(p => p.ContainerId)
                .Select(g => new PluginContainerViewModel(g.Key, g
                    .Select(p => new PluginViewModel(p.Id, p.Name))
                    .OrderBy(p => p.Name)
                    .ToArray()))
                .OrderBy(c => c.Plugins[0].Name);
        }


        [HttpPost("upload")]
        public async Task<IActionResult> Upload(IFormFile plugin)
        {
            try
            {
                await pluginManager.Update(plugin.OpenReadStream());
                return Ok();
            }
            catch (Exception e)
            {
                logger.Error("Error while uploading plugin {filename}: {message}", plugin.FileName, e.Message);
                return BadRequest(e.Message);
            }
        }


        // TODO remove
    }
}

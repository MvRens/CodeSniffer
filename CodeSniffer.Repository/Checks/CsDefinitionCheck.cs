﻿using System.Text.Json.Nodes;

namespace CodeSniffer.Repository.Checks
{
    public class CsDefinitionCheck
    {
        public string Name { get; }
        public string PluginId { get; }
        public JsonObject Configuration { get; }


        public CsDefinitionCheck(string name, string pluginId, JsonObject configuration)
        {
            Name = name;
            PluginId = pluginId;
            Configuration = configuration;
        }
    }
}

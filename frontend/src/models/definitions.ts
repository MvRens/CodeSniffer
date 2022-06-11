export interface ListDefinitionViewModel
{
  id: string;
  name: string;
}


export interface DefinitionViewModel
{
  name: string | null;
  sources: Array<DefinitionSourceViewModel>;
  checks: Array<DefinitionCheckViewModel>;
}


export interface DefinitionSourceViewModel
{
  name: string | null;
  pluginId: string | null;
  configuration: string | null;
}


export interface DefinitionCheckViewModel
{
  name: string | null;
  pluginId: string | null;
  configuration: string | null;
}


export interface PluginsViewModel
{
  sourcePlugins: PluginViewModel[];
  checkPlugins: PluginViewModel[];
}


export interface PluginViewModel
{
  id: string;
  name: string;
  defaultOptions: string | null;
  optionsHelp: string | null;
}
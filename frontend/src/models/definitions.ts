export interface ListDefinitionViewModel
{
  id: string;
  name: string;
}


export interface DefinitionViewModel
{
  name?: string;
  sources: Array<DefinitionSourceViewModel>;
  checks: Array<DefinitionCheckViewModel>;
}


export interface DefinitionSourceViewModel
{
  name?: string;
  pluginId?: string;
  configuration?: string;
}


export interface DefinitionCheckViewModel
{
  name?: string;
  pluginId?: string;
  configuration?: string;
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
  defaultOptions?: string;
  optionsHelp?: string;
}
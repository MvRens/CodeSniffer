export interface ListDefinitionAPIModel
{
  id: string;
  name: string;
}


export interface DefinitionAPIModel
{
  name: string;
  sourceGroupId: string;
  checks: Array<DefinitionCheckAPIModel>;
}


export interface DefinitionCheckAPIModel
{
  name: string;
  pluginId: string;
  configuration?: string;
}


export interface PluginAPIModel
{
  id: string;
  name: string;
  defaultOptions?: string;
  optionsHelp?: string;
}
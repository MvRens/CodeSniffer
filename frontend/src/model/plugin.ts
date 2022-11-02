export interface ListPluginContainerAPIModel
{
  id: string;
  plugins: ListPluginAPIModel[]
}

export interface ListPluginAPIModel
{
  id: string;
  name: string;
}
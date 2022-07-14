export interface ListSourceAPIModel
{
    id: string;
    name: string;
}


export interface ListSourceGroupAPIModel
{
  id: string;
  name: string;
}


export interface SourceAPIModel
{
  name: string;
  pluginId: string;
  configuration?: string;
}


export interface SourceGroupAPIModel
{
  name: string;
  sourceIds: Array<string>;
}

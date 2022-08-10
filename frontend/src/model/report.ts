export enum ReportResult
{
  Success = 0,
  Warning = 1,
  Critical = 2,
  Error = 3
}


export interface DashboardAPIModel
{
  sources: DashboardSourceAPIModel[];
  result: ReportResult;
}


export interface DashboardSourceAPIModel
{
  id: string;
  name: string;
  branches: DashboardBranchAPIModel[];
  result: ReportResult;
}



export interface DashboardBranchAPIModel
{
  name: string;
  failedDefinitions: DashboardFailedDefinitionAPIModel[];
  result: ReportResult;
}


export interface DashboardFailedDefinitionAPIModel
{
  name: string;
  summaries: string[];
  result: ReportResult;
}



export interface ReportsAPIModel
{
  definitions: ReportLookupAPIModel[];
  sources: ReportLookupAPIModel[];
  reports: ReportAPIModel[];
  result: ReportResult;
}


export interface ReportLookupAPIModel
{
  id: string;
  name: string;
}



export interface ReportAPIModel
{
  definitionIndex: number;
  sourceIndex: number;
  revisionId: string;
  revisionName: string;
  branch: string;
  checks: ReportCheckAPIModel[];
  result: ReportResult;
}


export interface ReportCheckAPIModel
{
  name: string;
  configuration?: any;
  assets: ReportAssetAPIModel[];
  result: ReportResult;
}


export interface ReportAssetAPIModel
{
  id: string;
  name: string;
  result: ReportResult;
  summary?: string;
  properties?: any;
  output?: string;
}
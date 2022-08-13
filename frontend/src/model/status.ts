export enum JobTypeAPIModel
{
    CheckRevisions = 0,
    Scan,
    CleanBranches
}


export enum JobStatusTypeAPIModel
{
    Running = 0,
    Success,
    Warning,
    Error
}


export interface JobStatusAPIModel
{
    name: string;
    jobType: JobTypeAPIModel;

    progress?: number;
    maxProgress?: number;

    log: string[];
    status: JobStatusTypeAPIModel;

    startTime: string;
    finishTime?: string;
}
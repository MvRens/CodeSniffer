using System;
using System.Collections.Generic;

namespace CodeSniffer.API.Status
{
    public enum JobTypeViewModel
    {
        CheckRevisions = 0,
        Scan,
        CleanBranches
    }


    public enum JobStatusTypeViewModel
    {
        Running = 0,
        Success,
        Warning,
        Error
    }


    public class JobStatusViewModel
    {
        public string Name { get; }
        public JobTypeViewModel JobType { get; }

        public int? Progress { get; }
        public int? MaxProgress { get; }

        public IReadOnlyList<string> Log { get; }
        public JobStatusTypeViewModel Status { get; }

        public DateTime StartTime { get; }
        public DateTime? FinishTime { get; }


        public JobStatusViewModel(string name, JobTypeViewModel jobType, int? progress, int? maxProgress, IReadOnlyList<string> log, JobStatusTypeViewModel status, DateTime startTime, DateTime? finishTime)
        {
            Name = name;
            JobType = jobType;
            Progress = progress;
            MaxProgress = maxProgress;
            Log = log;
            Status = status;
            StartTime = startTime;
            FinishTime = finishTime;
        }
    }
}

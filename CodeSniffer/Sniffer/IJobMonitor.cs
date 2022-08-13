using System;
using System.Collections.Generic;
using Serilog;

namespace CodeSniffer.Sniffer
{
    public interface IJobMonitor : IEnumerable<IJobStatus>
    {
        IRunningJob Start(ILogger logger, JobType jobType, string name);
    }


    public enum JobType
    {
        CheckRevisions,
        Scan,
        CleanBranches
    }


    public enum JobStatus
    {
        Running,
        Success,
        Warning,
        Error
    }


    public interface IJobStatus
    {
        string Name { get; }
        JobType JobType { get; }

        int? Progress { get; }
        int? MaxProgress { get; }

        IReadOnlyList<string> Log { get; }
        JobStatus Status { get; }

        DateTime StartTime { get; }
        DateTime? FinishTime { get; }
    }


    public interface IRunningJob : IDisposable
    {
        ILogger Logger { get; }

        void SetProgress(int progress, int maxProgress);
        void SetStatus(JobStatus status);
    }
}

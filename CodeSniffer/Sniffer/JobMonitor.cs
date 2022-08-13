using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace CodeSniffer.Sniffer
{
    public class JobMonitor : IJobMonitor, IDisposable
    {
        private readonly ConcurrentDictionary<RunningJob, bool> runningJobs = new();
        private readonly ConcurrentStack<RunningJob> finishedJobs = new();
        private readonly Timer cleanupTimer;

        private static readonly TimeSpan CleanupInterval = TimeSpan.FromMinutes(1);
        private static readonly TimeSpan CleanupMaxAge = TimeSpan.FromMinutes(15);


        public JobMonitor()
        {
            cleanupTimer = new Timer(Cleanup, null, CleanupInterval, CleanupInterval);
        }


        public void Dispose()
        {
            GC.SuppressFinalize(this);
            cleanupTimer.Dispose();
        }


        public IRunningJob Start(ILogger logger, JobType jobType, string name)
        {
            var runningJob = new RunningJob(this, logger, jobType, name);
            runningJobs.TryAdd(runningJob, true);

            return runningJob;
        }

        
        public IEnumerator<IJobStatus> GetEnumerator()
        {
            return runningJobs.Keys.Concat(finishedJobs).GetEnumerator();
        }

        
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        private void FinishJob(RunningJob runningJob)
        {
            if (runningJobs.TryRemove(runningJob, out _))
                finishedJobs.Push(runningJob);
        }


        private void Cleanup(object? state)
        {
            var treshold = DateTime.UtcNow.Subtract(CleanupMaxAge);

            while (finishedJobs.TryPeek(out var oldestJob) && oldestJob.FinishTime < treshold)
                finishedJobs.TryPop(out _);
        }


        private class RunningJob : IRunningJob, IJobStatus
        {
            private readonly JobMonitor jobMonitor;
            public DateTime StartTime { get; }
            public DateTime? FinishTime { get; private set; }

            public ILogger Logger { get; }

            public JobType JobType { get; }
            public string Name { get; }

            public int? Progress { get; private set; }
            public int? MaxProgress { get; private set; }

            public IReadOnlyList<string> Log => jobSink.Capture();
            public JobStatus Status { get; private set; } = JobStatus.Running;

            private readonly StringListSink jobSink = new();


            public RunningJob(JobMonitor jobMonitor, ILogger logger, JobType jobType, string name)
            {
                this.jobMonitor = jobMonitor;

                StartTime = DateTime.UtcNow;
                JobType = jobType;
                Name = name;

                Logger = new LoggerConfiguration()
                    .MinimumLevel.Debug()
                    .WriteTo.Sink(jobSink)
                    .WriteTo.Logger(logger)
                    .CreateLogger();
            }


            public void Dispose()
            {
                GC.SuppressFinalize(this);

                if (Status == JobStatus.Running)
                    SetStatus(JobStatus.Success);

                FinishTime = DateTime.UtcNow;
                jobMonitor.FinishJob(this);
            }


            public void SetProgress(int progress, int maxProgress)
            {
                Progress = progress;
                MaxProgress = maxProgress;
            }


            public void SetStatus(JobStatus status)
            {
                Status = status;
            }
        }


        private class StringListSink : ILogEventSink
        {
            private readonly List<string> output = new();
            private readonly object outputLock = new();


            public void Emit(LogEvent logEvent)
            {
                var message = logEvent.RenderMessage();

                lock (outputLock)
                {
                    output.Add(message);
                }
            }


            public IReadOnlyList<string> Capture()
            {
                lock (outputLock)
                {
                    return output.ToList();
                }
            }
        }
    }
}

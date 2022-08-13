using System;
using System.Collections.Generic;
using System.Linq;
using CodeSniffer.Sniffer;
using Microsoft.AspNetCore.Mvc;

namespace CodeSniffer.API.Status
{
    [Route("/api/status")]
    public class StatusController : ControllerBase
    {
        private readonly IJobMonitor jobMonitor;


        public StatusController(IJobMonitor jobMonitor)
        {
            this.jobMonitor = jobMonitor;
        }


        [HttpGet("jobs")]
        public IEnumerable<JobStatusViewModel> Jobs()
        {
            return jobMonitor
                .Select(j => new JobStatusViewModel(
                    j.Name,
                    MapJobType(j.JobType),
                    j.Progress,
                    j.MaxProgress,
                    j.Log,
                    MapJobStatus(j.Status),
                    j.StartTime,
                    j.FinishTime
                ));
        }


        private static JobTypeViewModel MapJobType(JobType jobType)
        {
            return jobType switch
            {
                JobType.CheckRevisions => JobTypeViewModel.CheckRevisions,
                JobType.Scan => JobTypeViewModel.Scan,
                JobType.CleanBranches => JobTypeViewModel.CleanBranches,
                _ => throw new ArgumentOutOfRangeException(nameof(jobType), jobType, null)
            };
        }


        private static JobStatusTypeViewModel MapJobStatus(JobStatus status)
        {
            return status switch
            {
                JobStatus.Running => JobStatusTypeViewModel.Running,
                JobStatus.Success => JobStatusTypeViewModel.Success,
                JobStatus.Warning => JobStatusTypeViewModel.Warning,
                JobStatus.Error => JobStatusTypeViewModel.Error,
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, null)
            };
        }
    }
}

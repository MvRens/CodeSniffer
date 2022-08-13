<template>
  <div class="container">
    <h4>{{ t('title') }}</h4>

    <div v-if="isLoading">
      {{ t('loading') }}
    </div>

    <div v-else>
      <div class="lastRefresh">
        {{ t('lastRefresh', { refreshInterval, lastRefresh: formatDateTime(lastRefresh!) }, refreshInterval )}}
      </div>

      <template v-for="group in groupedJobs">
        <h5 class="group">{{ group.group }}</h5>
        <div v-for="(job, i) in group.jobs" :key="i">        
          <div class="jobName">
            <span class="startTime">{{ getStartText(job) }}</span>
            {{ job.name }}
          </div>
          <div class="finishTime" v-if="!!job.finishTime">{{ getFinishText(job) }}</div>

          <!-- TODO show job type -->
          <!-- TODO show status (if finished) -->
          <!-- TODO progress bar -->
          <!-- TODO collapsible log -->

          <div class="log">
            <template v-for="line in job.log">{{ line }}<br /></template>
          </div>
        </div>
        <div v-if="group.jobs.length === 0" class="nojobs">
          {{ group.noJobsText }}
        </div>
      </template>
    </div>
  </div>
</template>

<i18n lang="yaml">
en:
  title: "Jobs"
  loading: "Loading..."

  lastRefresh: "This list automatically refreshes every {refreshInterval} second. Last refreshed at {lastRefresh} | This list automatically refreshes every {refreshInterval} seconds. Last refreshed at {lastRefresh}"

  running: "Running"
  finished: "Finished"

  noRunningJobs: "No jobs are currently running"
  noFinishedJobs: "No jobs have recently finished"

  startTime: "{startTime}"
  finishTime: "Finished at {finishTime}, ran for {duration} second | Finished at {finishTime}, ran for {duration} seconds"

  notifications:
    loadJobsFailed: "Failed to load jobs status: {message}"
</i18n>

<script lang="ts" setup>
import { ref, computed, onMounted, onUnmounted } from 'vue';
import axios from 'axios';
import { useI18n } from 'vue-i18n';
import { DateTime } from 'luxon';
import { JobStatusAPIModel, JobTypeAPIModel, JobStatusTypeAPIModel } from '@/model/status';

import { useNotifications } from '@/lib/notifications';

const { t } = useI18n();
const notifications = useNotifications();


interface JobGroupViewModel
{
  group: string;
  noJobsText: string;
  jobs: JobStatusViewModel[];
}


interface JobStatusViewModel
{
  name: string;
  jobType: JobTypeAPIModel;

  progress?: number;
  maxProgress?: number;

  log: string[];
  status: JobStatusTypeAPIModel;

  startTime: DateTime;
  finishTime?: DateTime;
}


const jobs = ref<JobStatusViewModel[]>();
const lastRefresh = ref<DateTime>();

const refreshInterval = 10;

const isLoading = computed(() => 
{
  return jobs.value === undefined;
});


let refreshTimer: number;

onMounted(() =>
{
    refreshJobs();
    refreshTimer = setInterval(refreshJobs, refreshInterval * 1000);
});


onUnmounted(() =>
{
  clearInterval(refreshTimer);
});

async function refreshJobs()
{
  try
  {
    const response = await axios.get<JobStatusAPIModel[]>('/api/status/jobs');
    jobs.value = response.data.map(j => {
      const job: JobStatusViewModel = {
        name: j.name,
        jobType: j.jobType,
        log: j.log,
        status: j.status,
        startTime: DateTime.fromISO(j.startTime),
        finishTime: !!j.finishTime ? DateTime.fromISO(j.finishTime) : undefined
      };

      return job;
    });

    lastRefresh.value = DateTime.local();
  }
  catch (e)
  {
    notifications.alert(t('notifications.loadJobsFailed', { message: (e as Error).message }));
  }
}


const groupedJobs = computed<JobGroupViewModel[]>(() =>
{
  if (jobs.value === undefined)
    return [];

  const groups: JobGroupViewModel[] = [
    {
      group: t('running'),
      noJobsText: t('noRunningJobs'),
      jobs: jobs.value
        .filter(j => j.status === JobStatusTypeAPIModel.Running)
        .sort((a, b) => b.startTime.toMillis() - a.startTime.toMillis())
    },
    {
      group: t('finished'),
      noJobsText: t('noFinishedJobs'),
      jobs: jobs.value
        .filter(j => j.status !== JobStatusTypeAPIModel.Running)
        .sort((a, b) => b.finishTime!.toMillis() - a.finishTime!.toMillis())
    }
  ];

  return groups;
});


function formatDateTime(value: DateTime)
{
  if (value.hasSame(DateTime.local(), 'day'))
    return value.toLocaleString(DateTime.TIME_WITH_SECONDS);

  return value.toLocaleString(DateTime.DATETIME_SHORT_WITH_SECONDS);
}


function getStartText(job: JobStatusViewModel)
{
  return t('startTime', { startTime: formatDateTime(job.startTime) });
}

function getFinishText(job: JobStatusViewModel)
{
  if (!job.finishTime)
    return '';

  const duration = Math.round(job.finishTime!.diff(job.startTime).as('seconds'));
  return t('finishTime', { finishTime: formatDateTime(job.finishTime!), duration: duration }, duration);
}
</script>

<style lang="scss" scoped>
.lastRefresh
{
  font-size: 80%;
  margin-bottom: 2em;
}

.log
{
  font-size: 90%;
  font-family: monospace;
  margin-bottom: 1em;
}

.group
{
  color: gray;
}

.nojobs
{
  color: gray;
  margin-bottom: 1em;
}


.finishTime
{
  font-size: 80%;
}


.jobName
{
  font-weight: bold;
}

.startTime
{
  font-weight: normal;
}
</style>

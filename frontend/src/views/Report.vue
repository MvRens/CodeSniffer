<template>
  <div class="container">
    <h6 class="suptitle">{{ t('title') }}</h6>
    <h4>{{ sourceName }} - {{ branch }}</h4>
          
    <div v-if="isLoading">
      {{ t('loading') }}
    </div>

    <div v-else>
      <div class="revision">
        Revision {{ revisionId }}
      </div>

      <div class="definition" v-for="report in reports!.reports" :key="report.reportKey">
        <ReportResultLine :result="report.result">{{ report.definitionName }}</ReportResultLine>
        <div class="check" v-for="check in report.checks" :key="check.name">
          <ReportResultLine :result="check.result">{{ check.name }}</ReportResultLine>

          <!-- {{ check.configuration }} -->

          <div class="asset" v-for="asset in check.assets" :key="asset.id">
            <ReportResultLine :result="asset.result">{{ asset.name }}</ReportResultLine>

            <div class="info">
              <div v-if="!!asset.output" class="output">{{ asset.output }}</div>
              <div v-if="!!asset.summary" class="summary">{{ asset.summary }}</div>
              <PropertiesGrid :properties="asset.properties" />
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<i18n lang="yaml">
en:
  title: "Report"
  loading: "Loading..."

  notifications:
    loadReportsFailed: "Failed to load report: {message}"
</i18n>

<script lang="ts" setup>
import { ref, computed, onMounted } from 'vue';
import axios from 'axios';
import { useI18n } from 'vue-i18n';
import ReportResultLine from '@/components/ReportResultLine.vue';
import PropertiesGrid from '@/components/PropertiesGrid.vue';
import { ReportsAPIModel, ReportCheckAPIModel, ReportResult } from '@/model/report';

import { useNotifications } from '@/lib/notifications';

const { t } = useI18n();
const notifications = useNotifications();

const props = defineProps({
  sourceId: {
    type: String,
    required: true
  },
  branchName: {
    type: String,
    required: true
  }    
});


interface ReportsViewModel
{
  reports: ReportViewModel[];
  result: ReportResult;
}


interface ReportViewModel
{
  reportKey: string;
  definitionId: string;
  definitionName: string;
  sourceId: string;
  sourceName: string;
  revisionId: string;
  revisionName: string;
  branch: string;
  checks: ReportCheckAPIModel[];
  result: ReportResult;
}


const reports = ref<ReportsViewModel>();

const isLoading = computed(() => 
{
  return reports.value === undefined;
});


const sourceName = computed<string>(() =>
{
  if (reports.value === undefined || reports.value.reports.length == 0)
    return '';

  return reports.value.reports[0].sourceName;
});


const branch = computed<string>(() =>
{
  if (reports.value === undefined || reports.value.reports.length == 0)
    return '';

  return reports.value.reports[0].branch;
});


const revisionId = computed<string>(() =>
{
  if (reports.value === undefined || reports.value.reports.length == 0)
    return '';

  return reports.value.reports[0].revisionId;
});


onMounted(() =>
{
    loadReports();
});


async function loadReports()
{
  try
  {
    const response = await axios.get<ReportsAPIModel>(`/api/reports?source=${encodeURIComponent(props.sourceId)}&branch=${encodeURIComponent(props.branchName)}`);
    reports.value = {
      reports: response.data.reports
        .map(r => 
        {
          const definition = response.data.definitions[r.definitionIndex];
          const source = response.data.sources[r.sourceIndex];

          const report: ReportViewModel = {
            reportKey: definition.id + '.' + source.id + '.' + r.branch,
            definitionId: definition.id,
            definitionName: definition.name,
            sourceId: source.id,
            sourceName: source.name,
            revisionId: r.revisionId,
            revisionName: r.revisionName,
            branch: r.branch,
            checks: r.checks,
            result: r.result
          };

          return report;
        })
        .sort((a, b) => a.definitionName.localeCompare(b.definitionName)),
      result: response.data.result
    };
  }
  catch (e)
  {
    notifications.alert(t('notifications.loadReportsFailed', { message: (e as Error).message }));
  }
}
</script>

<style lang="scss" scoped>
.revision
{
  margin-top: 0;
  margin-bottom: 2em;
  font-style: italic;
}

.definition
{
  > .result
  {
    font-size: 120%;
    font-weight: bold;
    border-bottom: solid 1px #c0c0c0;
  }
}


.check
{
  margin-top: .5em;
}


.asset
{
  margin-left: 2em;
  margin-top: .5em;

  .info
  {
    margin-left: 2em;
    margin-bottom: 1em;

    .summary
    {
      margin-bottom: 1em;
    }
  }
}
</style>

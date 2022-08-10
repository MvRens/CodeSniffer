<template>
  <div class="container">
    <h4>{{ t('title') }}</h4>

    <div v-if="isLoading">
      {{ t('loading') }}
    </div>

    <div v-else>
      <div v-for="source in dashboard!.sources" :key="source.name">
        <ReportResultLine :result="source.result" class="source" @click="toggleExpanded(source)">
          {{ source.name }}
          <font-awesome-icon :icon="source.expanded ? 'fa-angle-up' : 'fa-angle-down'" class="expandicon" :class="{ expanded: source.expanded }" />
        </ReportResultLine>

        <div v-if="source.expanded" v-for="branch in source.branches" :key="branch.name">
          <ReportResultLine :result="branch.result" class="branch">
            {{ branch.name }} 
            <router-link class="reportlink" :to="{ name: 'SourceBranchReport', params: { sourceId: source.id, branchName: branch.name } }">{{ t('sourceBranchReport') }}</router-link>
          </ReportResultLine>

          <div v-for="failedDefinition in branch.failedDefinitions" :key="failedDefinition.name">
            <ReportResultLine :result="failedDefinition.result" class="failedDefinition">
              {{ failedDefinition.name }}
            </ReportResultLine>
            
            <ul v-if="failedDefinition.summaries.length > 0" class="summaries">
              <li v-for="summary in failedDefinition.summaries" :key="summary">{{ summary }}</li>
            </ul>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<i18n lang="yaml">
en:
  title: "CodeSniffer Dashboard"
  loading: "Loading..."

  sourceBranchReport: "view report"

  notifications:
    loadDashboardFailed: "Failed to load dashboard: {message}"
</i18n>

<script lang="ts" setup>
import { ref, computed, onMounted, Ref, toRaw } from 'vue';
import axios from 'axios';
import { useI18n } from 'vue-i18n';
import ReportResultLine from '@/components/ReportResultLine.vue';
import { DashboardAPIModel, DashboardBranchAPIModel, ReportResult } from '@/model/report';

import { useNotifications } from '@/lib/notifications';

const { t } = useI18n();
const notifications = useNotifications();


interface DashboardViewModel
{
  sources: DashboardSourceViewModel[];
  result: ReportResult;
}


interface DashboardSourceViewModel
{
  id: string;
  name: string;
  branches: DashboardBranchAPIModel[];
  result: ReportResult;
  expanded: Ref<boolean>;
}


const dashboard = ref<DashboardViewModel>();

const isLoading = computed(() => 
{
  return dashboard.value === undefined;
});


onMounted(() =>
{
    loadDashboard();
});


async function loadDashboard()
{
  try
  {
    const response = await axios.get<DashboardAPIModel>('/api/reports/dashboard');
    dashboard.value = {
      result: response.data.result,
      sources: response.data.sources.map(s => 
      {
        const source: DashboardSourceViewModel = {
          id: s.id,
          name: s.name,
          branches: s.branches,
          result: s.result,
          expanded: ref(s.result !== ReportResult.Success)
        };

        return source;
      })
    }
  }
  catch (e)
  {
    notifications.alert(t('notifications.loadDashboardFailed', { message: (e as Error).message }));
  }
}


function toggleExpanded(source: DashboardSourceViewModel)
{
  // Some proxy magic happens otherwise which TypeScript can't handle because the
  // DashboardSourceViewModel is supposed to contain a Ref<> not a boolean
  var rawSource = toRaw(source);
  rawSource.expanded.value = !rawSource.expanded.value;
}
</script>

<style lang="scss" scoped>
.source
{
  font-size: 110%;
  font-weight: bold;
  border-bottom-style: solid;
  border-bottom-width: 1px;
  padding: .25em;
  margin-top: 1em;
  display: flex;
  align-items: center;
  
  &.success
  {
    border-color: green;
    background-color: rgb(195, 224, 195);
  }

  &.warning
  {
    border-color: orange;
    background-color: rgb(255, 238, 208);
  }

  &.critical
  {
    border-color: maroon;
    background-color: rgb(250, 234, 234);
  }

  &.error
  {
    border-color: red;
    background-color: rgb(250, 193, 193);
  }

  .expandicon
  {
    margin-left: auto;
    margin-right: .5em;    
  }
}

.branch
{
  margin-left: 1em;
  margin-top: 1em;
  font-weight: bold;
}


.failedDefinition
{
  margin-left: 2em;
}

.summaries
{
  margin: 0;
  margin-left: 3em;

  li
  {
    margin: 0;
  }
}


.reportlink
{
  margin-left: 2em;
}
</style>

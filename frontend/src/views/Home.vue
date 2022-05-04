<template>
  <div class="container">
    <h4>{{ t('title') }}</h4>
    <h5 class="section">{{ t('definitions') }}</h5>

    <div v-if="isLoading">
      {{ t('loading') }}
    </div>

    <div v-else>
      <div class="toolbar">
        <router-link :to="{ name: 'CreateDefinition' }" class="button button-primary" :class="{ disabled: isLoading }">{{ t('definitiontoolbar.create') }}</router-link>
      </div>

      <div v-if="definitions !== null && definitions.length === 0">
        {{ t('nodefinitions' )}}
      </div>      
    </div>
  </div>
</template>

<i18n lang="yaml">
en:
  title: "CodeSniffer Dashboard"
  definitions: "Definitions"
  loading: "Loading..."
  nodefinitions: "No definitions created yet."
  definitiontoolbar:
    create: "Create"
  notifications:
    loadDefinitionsFailed: "Failed to load definitions list: {message}"
    loadStatusMapFailed: "Failed to load definition status: {message}"
</i18n>

<script lang="ts" setup>
import { ref, computed, onMounted } from 'vue';
import axios from 'axios';
import { useI18n } from 'vue-i18n';

import { useNotifications } from '@/lib/notifications';
import { ListDefinitionViewModel } from '../models/definitions';
import { DefinitionStatusMap } from '../models/reports';

const { t } = useI18n();
const notifications = useNotifications();

const definitions = ref(null as ListDefinitionViewModel[] | null);
const statusMap = ref(null as DefinitionStatusMap | null);


const isLoading = computed(() => 
{
  return definitions === null || statusMap === null;
});


onMounted(() =>
{
    loadDefinitions();
    loadStatusMap();
});


async function loadDefinitions()
{
  try
  {
    const response = await axios.get('/api/definitions');
    definitions.value = response.data;
  }
  catch (e)
  {
    notifications.alert(t('notifications.loadDefinitionsFailed', { message: (e as Error).message }));
  }
}


async function loadStatusMap()
{
  try
  {
    const response = await axios.get('/api/reports/definitionstatus');
    statusMap.value = response.data;
  }
  catch (e)
  {
    notifications.alert(t('notifications.loadStatusMapFailed', { message: (e as Error).message }));
  }
}
</script>

<style lang="scss" scoped>
</style>

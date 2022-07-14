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

      <div v-if="definitions?.length === 0">
        {{ t('nodefinitions' )}}
      </div>

      <div class="definitions">
        <template v-for="definition in definitions" :key="definition.id">
          {{ definition.name }}
          <div class="buttons">
            <router-link :to="{ name: 'EditDefinition', params: { id: definition.id } }" class="button">{{ t('edit') }}</router-link>
            <button v-if="confirmDeleteDefinitionId !== definition.id" @click="confirmDeleteDefinition(definition.id)" class="button">{{ t('delete') }}</button>
            <button v-if="confirmDeleteDefinitionId === definition.id" @click="deleteDefinition(definition.id)" class="button">{{ t('delete') }}</button>            
            <button v-if="confirmDeleteDefinitionId === definition.id" @click="cancelDeleteDefinition()" class="button">{{ t('cancelDelete') }}</button>            
          </div>
        </template>
      </div>
    </div>


    <h5 class="section">{{ t('sourceGroups') }}</h5>

    <div v-if="isLoading">
      {{ t('loading') }}
    </div>

    <div v-else>
      <div class="toolbar">
        <router-link :to="{ name: 'CreateSourceGroup' }" class="button button-primary" :class="{ disabled: isLoading }">{{ t('sourcegrouptoolbar.create') }}</router-link>
      </div>

      <div v-if="sourceGroups?.length === 0">
        {{ t('nosourceGroups' )}}
      </div>

      <div class="sourcegroups">
        <template v-for="sourdeGroup in sourceGroups" :key="sourdeGroup.id">
          {{ sourdeGroup.name }}
          <div class="buttons">
            <router-link :to="{ name: 'EditSourceGroup', params: { id: sourdeGroup.id } }" class="button">{{ t('edit') }}</router-link>
            <button v-if="confirmDeleteSourceGroupId !== sourdeGroup.id" @click="confirmDeleteSourceGroup(sourdeGroup.id)" class="button">{{ t('delete') }}</button>
            <button v-if="confirmDeleteSourceGroupId === sourdeGroup.id" @click="deleteSourceGroup(sourdeGroup.id)" class="button">{{ t('delete') }}</button>            
            <button v-if="confirmDeleteSourceGroupId === sourdeGroup.id" @click="cancelDeleteSourceGroup()" class="button">{{ t('cancelDelete') }}</button>            
          </div>
        </template>
      </div>
    </div>    
  </div>
</template>

<i18n lang="yaml">
en:
  title: "CodeSniffer Dashboard"
  loading: "Loading..."
  definitions: "Check definitions"
  nodefinitions: "No definitions created yet."
  definitiontoolbar:
    create: "Create"

  sourceGroups: "Source groups"
  nosourceGroups: "No source groups created yet."
  sourcegrouptoolbar:
    create: "Create"

  edit: "Edit"
  delete: "Delete"
  cancelDelete: "Cancel"
  notifications:
    loadDefinitionsFailed: "Failed to load definitions list: {message}"
    loadStatusMapFailed: "Failed to load definition status: {message}"
    loadSourceGroupsFailed: "Failed to load source groups: {message}"
</i18n>

<script lang="ts" setup>
import { ref, computed, onMounted } from 'vue';
import axios from 'axios';
import { useI18n } from 'vue-i18n';

import { useNotifications } from '@/lib/notifications';
import { ListDefinitionAPIModel } from '../model/definition';
import { DefinitionStatusMap } from '../model/report';
import { ListSourceGroupAPIModel } from '@/model/source';

const { t } = useI18n();
const notifications = useNotifications();

const definitions = ref<ListDefinitionAPIModel[]>();
const statusMap = ref<DefinitionStatusMap>();
const sourceGroups = ref<ListSourceGroupAPIModel[]>();

const isLoading = computed(() => 
{
  return definitions.value === undefined || statusMap.value === undefined || sourceGroups.value === undefined;
});


onMounted(() =>
{
    loadDefinitions();
    loadStatusMap();
    loadSourceGroups();
});


async function loadDefinitions()
{
  try
  {
    const response = await axios.get<ListDefinitionAPIModel[]>('/api/definitions');
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
    const response = await axios.get<DefinitionStatusMap>('/api/reports/definitionstatus');
    statusMap.value = response.data;
  }
  catch (e)
  {
    notifications.alert(t('notifications.loadStatusMapFailed', { message: (e as Error).message }));
  }
}


async function loadSourceGroups()
{
  try
  {
    const response = await axios.get<ListDefinitionAPIModel[]>('/api/source/groups');
    sourceGroups.value = response.data;
  }
  catch (e)
  {
    notifications.alert(t('notifications.loadSourceGroupsFailed', { message: (e as Error).message }));
  }
}


const confirmDeleteDefinitionId = ref<string>();

function confirmDeleteDefinition(id: string)
{
  confirmDeleteDefinitionId.value = id;
}


function cancelDeleteDefinition()
{
  confirmDeleteDefinitionId.value = undefined;
}


async function deleteDefinition(id: string)
{
  if (definitions.value === undefined)
    return;

  const response = await axios.delete(`/api/definitions/${encodeURIComponent(id)}`);

  const definitionIndex = definitions.value.findIndex(d => d.id === id);
  if (definitionIndex > -1)
    definitions.value.splice(definitionIndex, 1);

  confirmDeleteDefinitionId.value = undefined;
}


const confirmDeleteSourceGroupId = ref<string>();

function confirmDeleteSourceGroup(id: string)
{
  confirmDeleteSourceGroupId.value = id;
}


function cancelDeleteSourceGroup()
{
  confirmDeleteSourceGroupId.value = undefined;
}


async function deleteSourceGroup(id: string)
{
  if (sourceGroups.value === undefined)
    return;

  const response = await axios.delete(`/api/source/group/${encodeURIComponent(id)}`);

  const SourceGroupIndex = sourceGroups.value.findIndex(d => d.id === id);
  if (SourceGroupIndex > -1)
    sourceGroups.value.splice(SourceGroupIndex, 1);

  confirmDeleteSourceGroupId.value = undefined;
}
</script>

<style lang="scss" scoped>
.section
{
  margin-top: 1em;
}

.definitions, .sourcegroups
{
  display: grid;
  grid-template-columns: 1fr auto;
  align-items: center;

  .buttons
  {
    text-align: right;
  }
}
</style>

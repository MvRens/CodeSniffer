<template>
  <div class="container">
    <h4>{{ t('title') }}</h4>
    <h5 class="section">{{ t('sources') }}</h5>

    <div v-if="isLoading">
      {{ t('loading') }}
    </div>

    <div v-else>
      <div class="toolbar">
        <router-link :to="{ name: 'CreateSource' }" class="button button-primary" :class="{ disabled: isLoading }">{{ t('sourcestoolbar.create') }}</router-link>
      </div>

      <div v-if="sources?.length === 0">
        {{ t('nosources' )}}
      </div>

      <div class="sources">
        <template v-for="sourde in sources" :key="sourde.id">
          {{ sourde.name }}
          <div class="buttons">
            <router-link :to="{ name: 'EditSource', params: { id: sourde.id } }" class="button">{{ t('edit') }}</router-link>
            <button v-if="confirmDeleteSourceId !== sourde.id" @click="confirmDeleteSource(sourde.id)" class="button">{{ t('delete') }}</button>
            <button v-if="confirmDeleteSourceId === sourde.id" @click="deleteSource(sourde.id)" class="button button-danger">{{ t('delete') }}</button>            
            <button v-if="confirmDeleteSourceId === sourde.id" @click="cancelDeleteSource()" class="button">{{ t('cancelDelete') }}</button>            
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
        <router-link :to="{ name: 'CreateSourceGroup' }" class="button button-primary" :class="{ disabled: isLoading }">{{ t('sourceGroupstoolbar.create') }}</router-link>
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
            <button v-if="confirmDeleteSourceGroupId === sourdeGroup.id" @click="deleteSourceGroup(sourdeGroup.id)" class="button button-danger">{{ t('delete') }}</button>            
            <button v-if="confirmDeleteSourceGroupId === sourdeGroup.id" @click="cancelDeleteSourceGroup()" class="button">{{ t('cancelDelete') }}</button>            
          </div>
        </template>
      </div>
    </div>        
  </div>
</template>

<i18n lang="yaml">
en:
  title: "Sources"
  loading: "Loading..."
  sources: "Repositories"
  nosources: "No sources added yet."
  sourcestoolbar:
    create: "Add"

  sourceGroups: "Groups"
  nosourceGroups: "No groups created yet."
  sourceGroupstoolbar:
    create: "Create"

  edit: "Edit"
  delete: "Delete"
  cancelDelete: "Cancel"
  notifications:
    loadSourcesFailed: "Failed to load sources: {message}"
    loadSourceGroupsFailed: "Failed to load source groups: {message}"
</i18n>

<script lang="ts" setup>
import { ref, computed, onMounted } from 'vue';
import axios from 'axios';
import { useI18n } from 'vue-i18n';

import { useNotifications } from '@/lib/notifications';
import { ListSourceAPIModel, ListSourceGroupAPIModel } from '@/model/source';

const { t } = useI18n();
const notifications = useNotifications();

const sources = ref<ListSourceAPIModel[]>();
const sourceGroups = ref<ListSourceGroupAPIModel[]>();

const isLoading = computed(() => 
{
  return sources.value === undefined || sourceGroups.value === undefined;
});


onMounted(() =>
{
  loadSources();
  loadSourceGroups();
});


async function loadSources()
{
  try
  {
    const response = await axios.get<ListSourceAPIModel[]>('/api/source');
    sources.value = response.data;
  }
  catch (e)
  {
    notifications.alert(t('notifications.loadSourcesFailed', { message: (e as Error).message }));
  }
}


async function loadSourceGroups()
{
  try
  {
    const response = await axios.get<ListSourceGroupAPIModel[]>('/api/source/groups');
    sourceGroups.value = response.data;
  }
  catch (e)
  {
    notifications.alert(t('notifications.loadSourceGroupsFailed', { message: (e as Error).message }));
  }
}


const confirmDeleteSourceId = ref<string>();

function confirmDeleteSource(id: string)
{
  confirmDeleteSourceId.value = id;
}


function cancelDeleteSource()
{
  confirmDeleteSourceId.value = undefined;
}


async function deleteSource(id: string)
{
  if (sources.value === undefined)
    return;

  const response = await axios.delete(`/api/source//${encodeURIComponent(id)}`);

  const SourceIndex = sources.value.findIndex(d => d.id === id);
  if (SourceIndex > -1)
    sources.value.splice(SourceIndex, 1);

  confirmDeleteSourceId.value = undefined;
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
.sources, .sourcegroups
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

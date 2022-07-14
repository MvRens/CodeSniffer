<template>
  <div class="container">
    <h4>{{ t('title') }}</h4>

    <div v-if="isLoading">
      {{ t('loading') }}
    </div>

    <div v-else>
      <div class="toolbar">
        <router-link :to="{ name: 'CreateDefinition' }" class="button button-primary" :class="{ disabled: isLoading }">{{ t('toolbar.create') }}</router-link>
      </div>

      <div v-if="definitions?.length === 0">
        {{ t('nodefinitions' )}}
      </div>

      <div class="definitions">
        <template v-for="definition in definitions" :key="definition.id">
          {{ definition.name }}
          <div class="buttons">
            <router-link :to="{ name: 'EditDefinition', params: { id: definition.id } }" class="button">{{ t('edit') }}</router-link>
            <button v-if="confirmDeleteId !== definition.id" @click="confirmDeleteDefinition(definition.id)" class="button">{{ t('delete') }}</button>
            <button v-if="confirmDeleteId === definition.id" @click="deleteDefinition(definition.id)" class="button button-danger">{{ t('delete') }}</button>            
            <button v-if="confirmDeleteId === definition.id" @click="cancelDeleteDefinition()" class="button">{{ t('cancelDelete') }}</button>            
          </div>
        </template>
      </div>
    </div>
  </div>
</template>

<i18n lang="yaml">
en:
  title: "Scan jobs"
  loading: "Loading..."
  nodefinitions: "No definitions created yet."
  toolbar:
    create: "Create"

  edit: "Edit"
  delete: "Delete"
  cancelDelete: "Cancel"
  notifications:
    loadDefinitionsFailed: "Failed to load definitions list: {message}"
</i18n>

<script lang="ts" setup>
import { ref, computed, onMounted } from 'vue';
import axios from 'axios';
import { useI18n } from 'vue-i18n';

import { useNotifications } from '@/lib/notifications';
import { ListDefinitionAPIModel } from '../model/definition';

const { t } = useI18n();
const notifications = useNotifications();

const definitions = ref<ListDefinitionAPIModel[]>();
const isLoading = computed(() => 
{
  return definitions.value === undefined;
});


onMounted(() =>
{
    loadDefinitions();
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


const confirmDeleteId = ref<string>();

function confirmDeleteDefinition(id: string)
{
  confirmDeleteId.value = id;
}


function cancelDeleteDefinition()
{
  confirmDeleteId.value = undefined;
}


async function deleteDefinition(id: string)
{
  if (definitions.value === undefined)
    return;

  const response = await axios.delete(`/api/definitions/${encodeURIComponent(id)}`);

  const definitionIndex = definitions.value.findIndex(d => d.id === id);
  if (definitionIndex > -1)
    definitions.value.splice(definitionIndex, 1);

  confirmDeleteId.value = undefined;
}
</script>

<style lang="scss" scoped>
.section
{
  margin-top: 1em;
}

.definitions
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

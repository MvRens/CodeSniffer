<template>
  <div class="container">
    <h4>{{ !!id ? t('title.edit', { originalName }) : t('title.create') }}</h4>

    <div v-if="loading">
      {{ t('loading') }}
    </div>

    <form v-else class="definition u-full-width" @submit.prevent="saveDefinition">    
      <div class="toolbar">
        <input type="submit" class="button button-primary" :class="{ disabled: saving }" :value="t('toolbar.submit')" />
        <router-link :to="{ name: 'Home' }" class="button">{{ t('toolbar.cancel') }}</router-link>
      </div>

      <div class="properties">
        <label for="name">{{ t('name') }}</label>
        <input class="u-full-width" type="text" id="name" v-model="name" :class="{ required: !name }" />

        <label for="sourceGroup">{{ t('sourceGroup') }}</label>
        <select class="u-full-width" id="sourceGroup" :class="{ required: !sourceGroupId }" v-model="sourceGroupId">
          <option disabled :value="undefined">{{ t('sourceGroupSelect') }}</option>
          <option v-for="option in sourceGroups" :key="option.id" :value="option.id">{{ option.name }}</option>
        </select>
      </div>
    </form>

    <template v-if="!loading">
      <h5 class="section">{{ t('checks') }}</h5>
      <div class="checks">
        <div v-if="checks.length === 0">
          {{ t('nochecks' )}}
        </div>

        <template v-for="(check, i) in checks" :key="i">
          <div v-if="editingCheck === i" class="check editing">
            <form @submit.prevent="closeCheck">
              <label for="checkName">{{ t('checkName') }}</label>
              <input class="u-full-width" type="text" id="checkName" v-model="check.name" :class="{ required: !check.name }" />

              <label for="checkPlugin">{{ t('pluginName') }}</label>            
              <select class="u-full-width" id="checkPlugin" :class="{ required: !check.pluginId }" v-model="check.pluginId">
                <option disabled :value="undefined">{{ t('pluginSelect') }}</option>
                <option v-for="option in checkPluginOptions" :key="option.value" :value="option.value">{{ option.label }}</option>
              </select>

              <label for="checkConfiguration">{{ t('configuration') }}</label>
              <textarea class="u-full-width configuration" type="text" id="checkConfiguration" v-model="check.configuration" />

              <div v-if="!!checkOptionsHelp" v-html="checkOptionsHelp" class="help"></div>

              <input type="submit" class="button button-primary" :value="t('toolbar.close')" />
            </form>
          </div>

          <template v-else>
            <div class="name">{{ check.name || t('noname') }}</div>
            <div class="plugin">{{ getOptionsLabel(checkPluginOptions, check.pluginId) }}</div>
            <div class="buttons">
              <button @click="editCheck(i)" class="button">{{ t('edit') }}</button>
              <button @click="deleteCheck(i)" class="button">{{ t('delete') }}</button>
            </div>
          </template>
        </template>

        <div class="toolbar">
          <button @click="addCheck" class="button button-primary" :class="{ disabled: saving }">{{ t('toolbar.addcheck') }}</button>
        </div>
      </div>
    </template>
  </div>
</template>

<i18n lang="yaml">
en:
  title:
    create: "Create definition"
    edit: "Edit definition: {originalName}"

  loading: "Loading..."
  toolbar:
    submit: "Save"
    cancel: "Cancel"
    addsource: "Add source"
    addcheck: "Add check"
    close: "Close"

  name: "Name"
  sourceGroup: "Source group"
  sourceGroupSelect: "<no source group selected>"
  checks: "Checks"
  nochecks: "No checks added yet."
  checkName: "Name"
  checkSelect: "Select a check type..."
  pluginName: "Type"
  pluginSelect: "Select a source type..."
  edit: "Edit"
  delete: "Remove"
  noname: "<unnamed>"
  noplugin: "<no plugin selected>"
  configuration: "Configuration"
  notifications: 
    saveSuccess: "The definition has been saved"
    loadDefinitionFailed: "Failed to load definition: {message}"
    saveDefinitionFailed: "Failed to save definition: {message}"
</i18n>

<script lang="ts" setup>
import { ref, watchEffect, onMounted, reactive, computed } from 'vue';
import axios from 'axios';
import { useI18n } from 'vue-i18n';
import { useNotifications } from '@/lib/notifications';
import { DefinitionCheckAPIModel, DefinitionAPIModel, PluginsAPIModel, PluginAPIModel } from '@/model/definition';
import { ListSourceAPIModel, ListSourceGroupAPIModel } from '@/model/source';
import router from '@/router';


interface PluginSelectOption
{
  label: string;
  value: string;
  defaultOptions?: string;
  optionsHelp?: string;
}


interface DefinitionCheckViewModel
{
  name?: string;
  pluginId?: string;
  configuration?: string;
}



const props = defineProps({
  id: String
});

const { t } = useI18n();
const notifications = useNotifications();


const loading = ref(true);
const saving = ref(false);

const originalName = ref<string>();
const name = ref<string>();
const sourceGroupId = ref<string>();
const checks = reactive([] as Array<DefinitionCheckViewModel>)

const editingSource = ref<number>();
const editingCheck = ref<number>();

const sourceGroups = reactive([] as Array<ListSourceGroupAPIModel>);
const sourcePluginOptions = reactive([] as Array<PluginSelectOption>);
const checkPluginOptions = reactive([] as Array<PluginSelectOption>);

onMounted(async () =>
{
  try
  {
    await loadPlugins();
    await loadSourceGroups();

    if (!!props.id)
      await loadDefinition(props.id);

    loading.value = false;
  }
  catch (e)
  {
    notifications.alert(t('notifications.loadDefinitionFailed', { message: (e as Error).message }));
  }
});


async function loadDefinition(id: string)
{
  const response = await axios.get<DefinitionAPIModel>(`/api/definitions/${encodeURIComponent(id)}`);

  name.value = response.data.name;
  originalName.value = name.value;
  sourceGroupId.value = response.data.sourceGroupId;

  Object.assign(checks, response.data.checks);
}


async function saveDefinition()
{
  // TODO validate input


  const definition: DefinitionAPIModel = {
    name: name.value!,
    sourceGroupId: sourceGroupId.value!,
    checks: checks.map(c => 
    {
      const check: DefinitionCheckAPIModel = {
        name: c.name!,
        pluginId: c.pluginId!,
        configuration: c.configuration
      };

      return check;
    })
  };

  try
  {
    const response = !!props.id 
      ? await axios.put(`/api/definitions/${encodeURIComponent(props.id)}`, definition)
      : await axios.post('/api/definitions', definition);

    notifications.info(t('notifications.saveSuccess'));
    router.push({ name: 'Home' });
  }
  catch (e)
  {
    notifications.alert(t('notifications.saveDefinitionFailed', { message: (e as Error).message }));
  }
}


async function loadPlugins()
{
  const response = await axios.get<PluginsAPIModel>('/api/definitions/plugins');


  const convertViewModel = (plugin: PluginAPIModel): PluginSelectOption =>
  {
    return {
      label: plugin.name,
      value: plugin.id,
      defaultOptions: plugin.defaultOptions,
      optionsHelp: plugin.optionsHelp
    }
  };

  Object.assign(sourcePluginOptions, response.data.sourcePlugins.map(convertViewModel));
  Object.assign(checkPluginOptions, response.data.checkPlugins.map(convertViewModel));
}


async function loadSourceGroups()
{
  const response = await axios.get<ListSourceGroupAPIModel[]>('/api/source/groups');

  Object.assign(sourceGroups, response.data);
}


function getOptionsLabel(options: Array<PluginSelectOption>, value?: string)
{
  if (value === undefined)
    return t('noplugin');

  const option = options.find(o => o.value === value);
  if (!!option)
    return option.label;

  return t('noplugin');
}



function addCheck()
{
  editingCheck.value = checks.push(watchCheck({})) - 1;
}

function editCheck(index: number)
{
  editingCheck.value = index;
}

function deleteCheck(index: number)
{
  if (index === editingCheck.value)
    editingCheck.value = undefined;

  if (index < 0 || index >= checks.length)
    return;

  checks.splice(index, 1);
}

function closeCheck()
{
  editingCheck.value = undefined;
}

function watchCheck(check: DefinitionCheckViewModel): DefinitionCheckViewModel
{
  const reactiveCheck = reactive(check);
  let lastPluginId = reactiveCheck.pluginId;

  watchEffect(() => 
  {
    if (reactiveCheck.pluginId !== undefined && reactiveCheck.pluginId !== lastPluginId)
    {
      lastPluginId = reactiveCheck.pluginId;

      const option = checkPluginOptions.find(o => o.value === reactiveCheck.pluginId);
      if (!!option)
        reactiveCheck.configuration = option.defaultOptions; 
    }
  });

  return reactiveCheck;
}


const checkOptionsHelp = computed<string | undefined>(() =>
{
  if (editingCheck.value === undefined)
    return undefined;

  const check = checks[editingCheck.value];
  if (check.pluginId === undefined)
    return undefined;

  const plugin = checkPluginOptions.find(p => p.value === check.pluginId);
  return plugin?.optionsHelp;
});
</script>

<style lang="scss" scoped>
.properties
{
  margin-top: 1em;
}

.properties, .sources, .checks
{
  margin-bottom: 3em;

  .toolbar
  {
    margin-top: 1em;
  }
}

.editing
{
  background-color: #f0f0f0;
  border: solid 1px #c0c0c0;
  padding: 1em;
  margin-bottom: 1em;

  form
  {
    margin: 0;
  }
}

.checks, .sources
{
  display: grid;
  grid-template-columns: 1fr 1fr auto;

  .editing, .toolbar
  {
    grid-column: 1 / span 3;
  }
}


.configuration
{
  height: 20em;
  resize: vertical;
  font-family: 'Courier New', Courier, monospace;
}


.help
{
  border: solid 1px #c0c0c0;
  background-color: #f0f0f0;

  padding: .5em;
  margin-bottom: 1em;

  height: 10em;
  overflow-y: auto;
  resize: vertical;


  // The generated HTML help from the plugin does not contain the scope
  // specifiers, so v-deep is required to indicate they should match
  :deep()
  {
    .help-summary
    {
      display: block;
    }

    .help-configuration
    {
      display: grid;
      grid-template-columns: auto 1fr;
    }

    .help-option
    {
      display: contents;


      .help-option-key
      {
        margin-right: 2em;
        margin-top: 1em;
      }


      &.required .help-option-key
      {
        font-weight: bold;

        &::after
        {
          content: " (required)";
          font-weight: normal;
        }
      }


      .help-option-summary
      {
        grid-column: 2;
        margin-top: 1em;
      }


      .help-option-description
      {
        grid-column: 2;
      }
    }
  }
}
</style>

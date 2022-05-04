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
      </div>
    </form>

    <h5 class="section">{{ t('sources') }}</h5>
    <div class="sources">
      <div v-if="sources.length === 0">
        {{ t('nosources' )}}
      </div>

      <div v-for="(source, i) in sources" :key="i">
        <div v-if="editingSource === i" class="row editing">
          <form class="u-full-width" @submit.prevent="closeSource">
            <label for="sourceName">{{ t('sourceName') }}</label>
            <input class="u-full-width" type="text" id="sourceName" v-model="source.name" :class="{ required: !source.name }" />

            <label for="sourcePlugin">{{ t('pluginName') }}</label>
            <select class="u-full-width" id="sourcePlugin" :class="{ required: !source.pluginName }" v-model="source.pluginName">
              <option disabled :value="null">{{ t('pluginSelect') }}</option>
              <option v-for="option in sourcePluginOptions" :key="option.value" :value="option.value">{{ option.label }}</option>
            </select>

            <label for="sourceConfiguration">{{ t('configuration') }}</label>
            <textarea class="u-full-width configuration" type="text" id="sourceConfiguration" v-model="source.configuration" />

            <input type="submit" class="button button-primary" :value="t('toolbar.close')" />
          </form>
        </div>

        <div v-else class="row item">
          <div class="five columns">{{ source.name || t('noname') }}</div>
          <div class="five columns">{{ getOptionsLabel(sourcePluginOptions, source.pluginName) }}</div>
          <div class="two columns buttons"><button @click="editSource(i)" class="button">{{ t('edit') }}</button></div>
        </div>
      </div>

      <div class="toolbar">
        <button @click="addSource" class="button button-primary">{{ t('toolbar.addsource') }}</button>
      </div>
    </div>

    <h5 class="section">{{ t('checks') }}</h5>
    <div class="checks">
      <div v-if="checks.length === 0">
        {{ t('nochecks' )}}
      </div>

      <div v-for="(check, i) in checks" :key="i">
        <div v-if="editingCheck === i" class="row editing">
          <form class="u-full-width" @submit.prevent="closeCheck">
            <label for="checkName">{{ t('checkName') }}</label>
            <input class="u-full-width" type="text" id="checkName" v-model="check.name" :class="{ required: !check.name }" />

            <label for="checkPlugin">{{ t('pluginName') }}</label>            
            <select class="u-full-width" id="checkPlugin" :class="{ required: !check.pluginName }" v-model="check.pluginName">
              <option disabled :value="null">{{ t('pluginSelect') }}</option>
              <option v-for="option in checkPluginOptions" :key="option.value" :value="option.value">{{ option.label }}</option>
            </select>

            <label for="checkConfiguration">{{ t('configuration') }}</label>
            <textarea class="u-full-width configuration" type="text" id="checkConfiguration" v-model="check.configuration" />

            <input type="submit" class="button button-primary" :value="t('toolbar.close')" />
          </form>
        </div>

        <div v-else class="row item">
          <div class="five columns">{{ check.name || t('noname') }}</div>
          <div class="five columns">{{ getOptionsLabel(checkPluginOptions, check.pluginName) }}</div>
          <div class="two columns buttons"><button @click="editCheck(i)" class="button">{{ t('edit') }}</button></div>
        </div>
      </div>

      <div class="toolbar">
        <button @click="addCheck" class="button button-primary" :class="{ disabled: saving }">{{ t('toolbar.addcheck') }}</button>
      </div>
    </div>
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
  sources: "Sources"
  nosources: "No sources added yet."
  sourceName: "Name"
  checks: "Checks"
  nochecks: "No checks added yet."
  checkName: "Name"
  checkSelect: "Select a check type..."
  pluginName: "Type"
  pluginSelect: "Select a source type..."
  edit: "Edit"
  noname: "<unnamed>"
  noplugin: "<no plugin selected>"
  configuration: "Configuration"
  notifications: 
    loadDefinitionFailed: "Failed to load definition: {message}"
</i18n>

<script lang="ts" setup>
import { ref, watchEffect, onMounted, reactive } from 'vue';
import axios from 'axios';
import { useI18n } from 'vue-i18n';
import { useNotifications } from '@/lib/notifications';
import { DefinitionCheckViewModel, DefinitionSourceViewModel, PluginsViewModel, PluginViewModel } from '@/models/definitions';


interface PluginSelectOption
{
  label: string;
  value: string;
  defaultOptions?: string;
}


const props = defineProps({
  id: String
});

const { t } = useI18n();
const notifications = useNotifications();


const loading = ref(true);
const saving = ref(false);

const originalName = ref(null as string | null);
const name = ref(null as string | null);
const sources = reactive([] as Array<DefinitionSourceViewModel>)
const checks = reactive([] as Array<DefinitionCheckViewModel>)

const editingSource = ref(null as number | null);
const editingCheck = ref(null as number | null);

const sourcePluginOptions = reactive([] as Array<PluginSelectOption>);
const checkPluginOptions = reactive([] as Array<PluginSelectOption>);

onMounted(async () =>
{
  try
  {
    if (!!props.id)
      await loadDefinition(props.id);

    await loadPlugins();

    loading.value = false;
  }
  catch (e)
  {
    notifications.alert(t('notifications.loadDefinitionFailed', { message: (e as Error).message }));
  }
});


async function loadDefinition(id: string)
{
  // TODO

  name.value = "Test";
  originalName.value = "Test";

  Object.assign(sources, []);
  Object.assign(checks, []);
}


async function saveDefinition()
{
  notifications.info('Hier had uw definitie opgeslagen kunnen worden');
}


async function loadPlugins()
{
  const response = await axios.get<PluginsViewModel>('/api/definitions/plugins');


  const convertViewModel = (plugin: PluginViewModel): PluginSelectOption =>
  {
    return {
      label: plugin.name,
      value: plugin.id,
      defaultOptions: plugin.defaultOptions
    }
  };

  Object.assign(sourcePluginOptions, response.data.sourcePlugins.map(convertViewModel));
  Object.assign(checkPluginOptions, response.data.checkPlugins.map(convertViewModel));
}


function getOptionsLabel(options: Array<PluginSelectOption>, value: string | null)
{
  if (value === null)
    return t('noplugin');

  const option = options.find(o => o.value === value);
  if (!!option)
    return option.label;

  return t('noplugin');
}

function addSource()
{
  editingSource.value = sources.push(watchSource({
    name: null,
    pluginName: null,
    configuration: null
  })) - 1;
}

function editSource(index: number)
{
  editingSource.value = index;
}

function closeSource()
{
  editingSource.value = null;
}

function watchSource(source: DefinitionSourceViewModel): DefinitionSourceViewModel
{
  const reactiveSource = reactive(source);
  let lastPluginName = reactiveSource.pluginName;

  watchEffect(() => 
  {
    if (reactiveSource.pluginName !== null && reactiveSource.pluginName !== lastPluginName)
    {
      lastPluginName = reactiveSource.pluginName;

      const option = sourcePluginOptions.find(o => o.value === reactiveSource.pluginName);
      if (!!option)
        reactiveSource.configuration = option.defaultOptions || null; 
    }
  });

  return reactiveSource;
}


function addCheck()
{
  editingCheck.value = checks.push(watchCheck({
    name: null,
    pluginName: null,
    configuration: null
  })) - 1;
}

function editCheck(index: number)
{
  editingCheck.value = index;
}

function closeCheck()
{
  editingCheck.value = null;
}

function watchCheck(check: DefinitionCheckViewModel): DefinitionCheckViewModel
{
  const reactiveCheck = reactive(check);
  let lastPluginName = reactiveCheck.pluginName;

  watchEffect(() => 
  {
    if (reactiveCheck.pluginName !== null && reactiveCheck.pluginName !== lastPluginName)
    {
      lastPluginName = reactiveCheck.pluginName;

      const option = checkPluginOptions.find(o => o.value === reactiveCheck.pluginName);
      if (!!option)
        reactiveCheck.configuration = option.defaultOptions || null; 
    }
  });

  return reactiveCheck;
}
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

.buttons
{
  text-align: right;
}


.configuration
{
  height: 20em;
  resize: vertical;
  font-family: 'Courier New', Courier, monospace;
}
</style>

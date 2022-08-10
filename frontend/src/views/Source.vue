<template>
  <div class="container">
    <h6 class="suptitle" v-if="!!props.id">{{ t('title.edit' )}}</h6>
    <h4>{{ !!id ? originalName : t('title.create') }}</h4>

    <div v-if="loading">
      {{ t('loading') }}
    </div>

    <form v-else class="source u-full-width" @submit.prevent="saveSource">    
      <div class="toolbar">
        <input type="submit" class="button button-primary" :class="{ disabled: saving }" :value="t('toolbar.submit')" />
        <router-link :to="{ name: 'ListSources' }" class="button">{{ t('toolbar.cancel') }}</router-link>
      </div>

      <div class="properties">
        <label for="name">{{ t('name') }}</label>
        <input class="u-full-width" type="text" id="name" v-model="name" :class="{ required: !name }" />

        <label for="sourcePlugin">{{ t('pluginName') }}</label>
        <select class="u-full-width" id="sourcePlugin" :class="{ required: !pluginId }" v-model="pluginId">
          <option disabled :value="undefined">{{ t('pluginSelect') }}</option>
          <option v-for="option in sourcePluginOptions" :key="option.value" :value="option.value">{{ option.label }}</option>
        </select>

        <label for="sourceConfiguration">{{ t('configuration') }}</label>
        <textarea class="u-full-width configuration" type="text" id="sourceConfiguration" v-model="configuration" />

        <div v-if="!!sourceOptionsHelp" v-html="sourceOptionsHelp" class="plugin-help"></div>
      </div>
    </form>
  </div>
</template>


<i18n lang="yaml">
en:
  title:
    create: "Add source"
    edit: "Edit source"

  loading: "Loading..."
  toolbar:
    submit: "Save"
    cancel: "Cancel"
    addsource: "Add source"
    close: "Close"

  name: "Name"
  sources: "Sources"
  nosources: "No sources added yet."
  sourceName: "Name"
  pluginName: "Type"
  pluginSelect: "Select a source type..."
  edit: "Edit"
  delete: "Remove"
  noname: "<unnamed>"
  noplugin: "<no plugin selected>"
  configuration: "Configuration"
  notifications: 
    saveSuccess: "The source has been saved"
    loadSourceFailed: "Failed to load source: {message}"
    saveSourceFailed: "Failed to save source: {message}"
</i18n>


<script lang="ts" setup>
import { ref, watchEffect, onMounted, reactive, computed } from 'vue';
import axios from 'axios';
import { useI18n } from 'vue-i18n';
import { useNotifications } from '@/lib/notifications';
import { SourceAPIModel } from '@/model/source';
import router from '@/router';
import { PluginAPIModel } from '@/model/definition';


interface PluginSelectOption
{
  label: string;
  value: string;
  defaultOptions?: string;
  optionsHelp?: string;
}


interface SourceViewModel
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
const pluginId = ref<string>();
const configuration = ref<string>();

const editingSource = ref<number>();

const sourcePluginOptions = reactive([] as Array<PluginSelectOption>);


onMounted(async () =>
{
  try
  {
    await loadPlugins();

    if (!!props.id)
      await loadSource(props.id);

    loading.value = false;
  }
  catch (e)
  {
    notifications.alert(t('notifications.loadSourceFailed', { message: (e as Error).message }));
  }
});


function getOptionsLabel(options: Array<PluginSelectOption>, value?: string)
{
  if (value === undefined)
    return t('noplugin');

  const option = options.find(o => o.value === value);
  if (!!option)
    return option.label;

  return t('noplugin');
}


async function loadSource(id: string)
{
  const response = await axios.get<SourceAPIModel>(`/api/source/${encodeURIComponent(id)}`);

  name.value = response.data.name;
  originalName.value = name.value;

  pluginId.value = response.data.pluginId;  
  configuration.value = response.data.configuration;

  lastPluginId = pluginId.value;
}


async function saveSource()
{
  // TODO validate input


  const source: SourceAPIModel = {
    name: name.value!,
    pluginId: pluginId.value!,
    configuration: configuration.value
  };

  try
  {
    const response = !!props.id 
      ? await axios.put(`/api/source/${encodeURIComponent(props.id)}`, source)
      : await axios.post('/api/source', source);

    notifications.info(t('notifications.saveSuccess'));
    router.push({ name: 'ListSources' });
  }
  catch (e)
  {
    notifications.alert(t('notifications.saveSourceFailed', { message: (e as Error).message }));
  }
}


async function loadPlugins()
{
  const response = await axios.get<PluginAPIModel[]>('/api/source/plugins');


  const convertViewModel = (plugin: PluginAPIModel): PluginSelectOption =>
  {
    return {
      label: plugin.name,
      value: plugin.id,
      defaultOptions: plugin.defaultOptions,
      optionsHelp: plugin.optionsHelp
    }
  };

  Object.assign(sourcePluginOptions, response.data.map(convertViewModel));
}


let lastPluginId = pluginId.value;

watchEffect(() =>
{
  if (pluginId.value !== undefined && pluginId.value !== lastPluginId)
  {
    lastPluginId = pluginId.value;

    const option = sourcePluginOptions.find(o => o.value === pluginId.value);
    if (!!option)
      configuration.value = option.defaultOptions; 
  }
});


const sourceOptionsHelp = computed<string | undefined>(() =>
{
  if (pluginId.value === undefined)
    return undefined;

  const plugin = sourcePluginOptions.find(p => p.value === pluginId.value);
  return plugin?.optionsHelp;
});
</script>

<style lang="scss" scoped>
.configuration
{
  height: 20em;
  resize: vertical;
  font-family: 'Courier New', Courier, monospace;
}
</style>
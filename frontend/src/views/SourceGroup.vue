<template>
  <div class="container">
    <h6 class="suptitle" v-if="!!props.id">{{ t('title.edit' )}}</h6>
    <h4>{{ !!id ? originalName : t('title.create') }}</h4>

    <div v-if="loading">
      {{ t('loading') }}
    </div>

    <form v-else class="sourceGroup u-full-width" @submit.prevent="saveSourceGroup">    
      <div class="toolbar">
        <input type="submit" class="button button-primary" :class="{ disabled: saving }" :value="t('toolbar.submit')" />
        <router-link :to="{ name: 'ListSources' }" class="button">{{ t('toolbar.cancel') }}</router-link>
      </div>

      <div class="properties">
        <label for="name">{{ t('name') }}</label>
        <input class="u-full-width" type="text" id="name" v-model="name" :class="{ required: !name }" />
      </div>
    </form>

    <template v-if="!loading">
      <h5 class="section">{{ t('sources') }}</h5>
      <div class="sources">
        <input type="text" v-model="sourceFilter" :placeholder="t('filterPlaceholder')" class="u-full-width" />

        <div v-if="sources.length === 0">
          {{ t('nosources' )}}
        </div>
        
        <template v-for="(source, i) in filteredSources" :key="i">
          <div class="source">
            <Checkbox v-model="source.selected" :label="source.name" />
          </div>
        </template>
      </div>
    </template>
  </div>
</template>


<i18n lang="yaml">
en:
  title:
    create: "Create source group"
    edit: "Edit source group"

  loading: "Loading..."
  toolbar:
    submit: "Save"
    cancel: "Cancel"
    addsource: "Add source"
    close: "Close"

  name: "Name"
  sources: "Sources"
  nosources: "No sources added yet."
  filterPlaceholder: "Filter sources..."
  notifications: 
    saveSuccess: "The source group has been saved"
    loadSourceGroupFailed: "Failed to load source group: {message}"
    saveSourceGroupFailed: "Failed to save source group: {message}"
</i18n>


<script lang="ts" setup>
import { Ref, ref, onMounted, reactive, computed } from 'vue';
import axios from 'axios';
import { useI18n } from 'vue-i18n';
import { useNotifications } from '@/lib/notifications';
import { ListSourceAPIModel, SourceGroupAPIModel } from '@/model/source';
import router from '@/router';
import { localeContains } from '@/lib/strutils';
import Checkbox from '@/components/Checkbox.vue';



interface SourceViewModel
{
  id: string;
  name: string;
  selected: Ref<boolean>;
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
const sources = reactive([] as Array<SourceViewModel>);

const sourceFilter = ref<string>('');


const orderedSources = computed(() =>
{
  return sources.slice().sort((a, b) => a.name.localeCompare(b.name));
});


const filteredSources = computed(() =>
{
  if (!sourceFilter.value)
    return orderedSources.value;

  return orderedSources.value.filter(s => localeContains(s.name, sourceFilter.value));
});


onMounted(async () =>
{
  try
  {
    await loadSources();

    if (!!props.id)
      await loadSourceGroup(props.id);

    loading.value = false;
  }
  catch (e)
  {
    notifications.alert(t('notifications.loadSourceGroupFailed', { message: (e as Error).message }));
  }
});


async function loadSourceGroup(id: string)
{
  const response = await axios.get<SourceGroupAPIModel>(`/api/source/group/${encodeURIComponent(id)}`);

  name.value = response.data.name;
  originalName.value = name.value;

  response.data.sourceIds.forEach(s =>
  {
    const source = sources.find(fs => fs.id === s);
    if (source === undefined)
      return;

    source.selected = true;
  });
}


async function saveSourceGroup()
{
  // TODO validate input


  const sourceGroup: SourceGroupAPIModel = {
    name: name.value!,
    sourceIds: sources.filter(c => c.selected).map(c => c.id)
  };

  try
  {
    const response = !!props.id 
      ? await axios.put(`/api/source/group/${encodeURIComponent(props.id)}`, sourceGroup)
      : await axios.post('/api/source/group', sourceGroup);

    notifications.info(t('notifications.saveSuccess'));
    router.push({ name: 'ListSources' });
  }
  catch (e)
  {
    notifications.alert(t('notifications.saveSourceGroupFailed', { message: (e as Error).message }));
  }
}


async function loadSources()
{
  const response = await axios.get<ListSourceAPIModel[]>('/api/source');
  Object.assign(sources, response.data.map(s => 
  {
    const source: SourceViewModel = {
      id: s.id,
      name: s.name,
      selected: ref(false)
    };

    return source;
  }));
}
</script>

<style lang="scss" scoped>
.sources
{
  .source
  {
    display: block;
    margin-bottom: .75em;
  }
}
</style>
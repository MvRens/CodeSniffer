<template>
    <!--
      <h5 class="section">{{ t('sources') }}</h5>
      <div class="sources">
        <div v-if="sources.length === 0">
          {{ t('nosources' )}}
        </div>

        <template v-for="(source, i) in sources" :key="i">
          <div v-if="editingSource === i" class="source editing">
            <form @submit.prevent="closeSource">
              <label for="sourceName">{{ t('sourceName') }}</label>
              <input class="u-full-width" type="text" id="sourceName" v-model="source.name" :class="{ required: !source.name }" />

              <label for="sourcePlugin">{{ t('pluginName') }}</label>
              <select class="u-full-width" id="sourcePlugin" :class="{ required: !source.pluginId }" v-model="source.pluginId">
                <option disabled :value="undefined">{{ t('pluginSelect') }}</option>
                <option v-for="option in sourcePluginOptions" :key="option.value" :value="option.value">{{ option.label }}</option>
              </select>

              <label for="sourceConfiguration">{{ t('configuration') }}</label>
              <textarea class="u-full-width configuration" type="text" id="sourceConfiguration" v-model="source.configuration" />

              <div v-if="!!sourceOptionsHelp" v-html="sourceOptionsHelp" class="help"></div>

              <input type="submit" class="button button-primary" :value="t('toolbar.close')" />
            </form>
          </div>

          <template v-else>
            <div class="name">{{ source.name || t('noname') }}</div>
            <div class="plugin">{{ getOptionsLabel(sourcePluginOptions, source.pluginId) }}</div>
            <div class="buttons">
              <button @click="editSource(i)" class="button">{{ t('edit') }}</button>
              <button @click="deleteSource(i)" class="button">{{ t('delete') }}</button>
            </div>
          </template>
        </template>

        <div class="toolbar">
          <button @click="addSource" class="button button-primary">{{ t('toolbar.addsource') }}</button>
        </div>
    </div>
  -->
</template>


<i18n lang="yaml">
en:
  sources: "Sources"
  nosources: "No sources added yet."
  sourceName: "Name"
</i18n>


<script lang="ts">
/*
function addSource()
{
  editingSource.value = sources.push(watchSource({})) - 1;
}

function editSource(index: number)
{
  editingSource.value = index;
}

function deleteSource(index: number)
{
  if (index === editingSource.value)
    editingSource.value = undefined;

  if (index < 0 || index >= sources.length)
    return;

  sources.splice(index, 1);
}

function closeSource()
{
  editingSource.value = undefined;
}

function watchSource(source: DefinitionSourceViewModel): DefinitionSourceViewModel
{
  const reactiveSource = reactive(source);
  let lastPluginId = reactiveSource.pluginId;

  watchEffect(() => 
  {
    if (reactiveSource.pluginId !== undefined && reactiveSource.pluginId !== lastPluginId)
    {
      lastPluginId = reactiveSource.pluginId;

      const option = sourcePluginOptions.find(o => o.value === reactiveSource.pluginId);
      if (!!option)
        reactiveSource.configuration = option.defaultOptions; 
    }
  });

  return reactiveSource;
}


const sourceOptionsHelp = computed<string | undefined>(() =>
{
  if (editingSource.value === undefined)
    return undefined;

  const source = sources[editingSource.value];
  if (source.pluginId === undefined)
    return undefined;

  const plugin = sourcePluginOptions.find(p => p.value === source.pluginId);
  return plugin?.optionsHelp;
});

*/
</script>
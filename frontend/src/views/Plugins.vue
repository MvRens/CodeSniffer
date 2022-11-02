<template>
  <div class="container">
    <h4>{{ t('title') }}</h4>

    <div v-if="isLoading">
      {{ t('loading') }}
    </div>

    <div v-else>
      <div v-if="plugins?.length === 0">
        {{ t('noplugins' )}}
      </div>

      <div class="plugins">
        <template v-for="container in plugins" :key="container.id">
          <div class="plugin">
            <span class="id">{{ container.id }}</span>
            <ul class="plugins">
              <li v-for="plugin in container.plugins" :key="plugin.id">{{ plugin.name }}</li>
            </ul>
          </div>
          <div class="buttons">
            <button v-if="confirmDeleteId !== container.id" @click="confirmDeletePlugin(container.id)" class="button">{{ t('delete') }}</button>
            <button v-if="confirmDeleteId === container.id" @click="deletePlugin(container.id)" class="button button-danger">{{ t('delete') }}</button>            
            <button v-if="confirmDeleteId === container.id" @click="cancelDeletePlugin()" class="button">{{ t('cancelDelete') }}</button>            
          </div>
        </template>
      </div>

      <h4>{{ t('upload.title' )}}</h4>
      <form class="upload" @submit.prevent="uploadPlugins">
        <input type="file" ref="uploadFileElement" class="u-full-width" accept=".zip" multiple />
        <input type="submit" class="button" :value="t('upload.submit')" />
      </form>

      <FileUploadProgress v-for="(uploadFile, i) in uploadFiles" 
        :key="i" 
        :filename="uploadFile.filename" 
        :filesize="uploadFile.filesize" 
        :progress="uploadFile.progress" 
        :done="uploadFile.done"
        :error="uploadFile.error" />
  </div>
  </div>
</template>

<i18n lang="yaml">
en:
  title: "Plugins"
  loading: "Loading..."
  noplugins: "No plugins installed yet."

  delete: "Delete"
  cancelDelete: "Cancel"

  upload:
    title: "Upload plugin"
    submit: "Upload"

  notifications:
    loadPluginsFailed: "Failed to load plugins list: {message}"
</i18n>

<script lang="ts" setup>
import { ref, computed, onMounted, Ref } from 'vue';
import axios from 'axios';
import { useI18n } from 'vue-i18n';

import { useNotifications } from '@/lib/notifications';
import { ListPluginContainerAPIModel, ListPluginAPIModel } from '../model/plugin';
import FileUploadProgress from '@/components/FileUploadProgress.vue';

const { t } = useI18n();
const notifications = useNotifications();



const plugins = ref<ListPluginContainerAPIModel[]>();
const isLoading = computed(() => 
{
  return plugins.value === undefined;
});


interface IUploadFile
{
  filename: string;
  filesize: number;
  progress: Ref<number>;
  done: Ref<boolean>;
  error: Ref<boolean>;
}

const uploadFileElement = ref<HTMLInputElement>();
const uploadFiles = ref<IUploadFile[]>([]);

onMounted(() =>
{
    loadPlugins();
});


async function loadPlugins()
{
  try
  {
    const response = await axios.get<ListPluginContainerAPIModel[]>('/api/plugins');
    plugins.value = response.data;
  }
  catch (e)
  {
    notifications.alert(t('notifications.loadPluginsFailed', { message: (e as Error).message }));
  }
}


const confirmDeleteId = ref<string>();

function confirmDeletePlugin(id: string)
{
  confirmDeleteId.value = id;
}


function cancelDeletePlugin()
{
  confirmDeleteId.value = undefined;
}


async function deletePlugin(id: string)
{
  if (plugins.value === undefined)
    return;

  const response = await axios.delete(`/api/plugins/${encodeURIComponent(id)}`);

  const pluginIndex = plugins.value.findIndex(d => d.id === id);
  if (pluginIndex > -1)
    plugins.value.splice(pluginIndex, 1);

  confirmDeleteId.value = undefined;
}


async function uploadPlugins()
{
  const files = uploadFileElement.value?.files;
  if (!files || files.length == 0)
    return;

  const newUploadFiles: IUploadFile[] = [];
  const tasks: Promise<void>[] = [];

  for (let i = 0; i < files.length; i++)
  {
    const file = files[i];
    const uploadFile: IUploadFile = {
      filename: file.name,
      filesize: file.size,
      progress: ref(0),
      done: ref(false),
      error: ref(false)
    };

    newUploadFiles.push(uploadFile);
    tasks.push(uploadPlugin(file, uploadFile));
  }

  uploadFiles.value = newUploadFiles as any;
  await Promise.all(tasks);

  await loadPlugins();
}

async function uploadPlugin(file: File, uploadFile: IUploadFile)
{
  var formData = new FormData();
  formData.append("plugin", file);

  try
  {
    await axios.post('/api/plugins/upload', formData, {
      headers: {
        'Content-Type': 'multipart/form-data'
      },
      onUploadProgress: (event: ProgressEvent) =>
      {
        uploadFile.progress.value = (event.loaded / event.total) * 100;
      }
    });

    uploadFile.done.value = true;
  }
  catch
  {
    uploadFile.error.value = true;
  }
}
</script>

<style lang="scss" scoped>
.section
{
  margin-top: 1em;
}

.plugins
{
  display: grid;
  grid-template-columns: 1fr auto;
  align-items: center;
  margin-bottom: 3rem;

  .buttons
  {
    text-align: right;
  }

  .plugin
  {
    .id
    {
      color: silver;
    }
  }
}
</style>

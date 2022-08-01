<template>
  <div class="container">
    <h4>{{ t('title') }}</h4>

    <div v-if="isLoading">
      {{ t('loading') }}
    </div>

    <div v-else>
      <div class="toolbar">
        <router-link :to="{ name: 'CreateUser' }" class="button button-primary" :class="{ disabled: isLoading }">{{ t('toolbar.create') }}</router-link>
      </div>

      <div v-if="users?.length === 0">
        {{ t('nousers' )}}
      </div>

      <div class="users">
        <template v-for="user in users" :key="user.id">
          {{ user.displayName }} ({{ user.username }})
          <div class="buttons">
            <router-link :to="{ name: 'EditUser', params: { id: user.id } }" class="button">{{ t('edit') }}</router-link>
            <button v-if="confirmDeleteId !== user.id" @click="confirmDeleteUser(user.id)" class="button">{{ t('delete') }}</button>
            <button v-if="confirmDeleteId === user.id" @click="deleteUser(user.id)" class="button button-danger">{{ t('delete') }}</button>            
            <button v-if="confirmDeleteId === user.id" @click="cancelDeleteUser()" class="button">{{ t('cancelDelete') }}</button>            
          </div>
        </template>
      </div>
    </div>
  </div>
</template>

<i18n lang="yaml">
en:
  title: "Users"
  loading: "Loading..."
  nousers: "No users created yet."
  toolbar:
    create: "Add user"

  edit: "Edit"
  delete: "Delete"
  cancelDelete: "Cancel"
  notifications:
    loadUsersFailed: "Failed to load users list: {message}"
</i18n>

<script lang="ts" setup>
import { ref, computed, onMounted } from 'vue';
import axios from 'axios';
import { useI18n } from 'vue-i18n';

import { useNotifications } from '@/lib/notifications';
import { ListUserAPIModel } from '../model/user';

const { t } = useI18n();
const notifications = useNotifications();

const users = ref<ListUserAPIModel[]>();
const isLoading = computed(() => 
{
  return users.value === undefined;
});


onMounted(() =>
{
    loadUsers();
});


async function loadUsers()
{
  try
  {
    const response = await axios.get<ListUserAPIModel[]>('/api/users');
    users.value = response.data;
  }
  catch (e)
  {
    notifications.alert(t('notifications.loadUsersFailed', { message: (e as Error).message }));
  }
}


const confirmDeleteId = ref<string>();

function confirmDeleteUser(id: string)
{
  confirmDeleteId.value = id;
}


function cancelDeleteUser()
{
  confirmDeleteId.value = undefined;
}


async function deleteUser(id: string)
{
  if (users.value === undefined)
    return;

  const response = await axios.delete(`/api/users/${encodeURIComponent(id)}`);

  const userIndex = users.value.findIndex(d => d.id === id);
  if (userIndex > -1)
    users.value.splice(userIndex, 1);

  confirmDeleteId.value = undefined;
}
</script>

<style lang="scss" scoped>
.section
{
  margin-top: 1em;
}

.users
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

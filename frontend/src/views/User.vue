<template>
  <div class="container">
    <h6 class="suptitle" v-if="!!props.id">{{ t('title.edit' )}}</h6>
    <h4>{{ !!id ? originalUsername : t('title.create') }}</h4>

    <div v-if="loading">
      {{ t('loading') }}
    </div>

    <form v-else class="user u-full-width" @submit.prevent="saveUser">    
      <div class="toolbar">
        <input type="submit" class="button button-primary" :class="{ disabled: saving }" :value="t('toolbar.submit')" />
        <router-link :to="{ name: 'ListUsers' }" class="button">{{ t('toolbar.cancel') }}</router-link>
      </div>

      <div class="properties">
        <label for="username">{{ t('username') }}</label>
        <input class="u-full-width" type="text" id="username" v-model="username" :class="{ required: !username }" />

        <label for="displayName">{{ t('displayName') }}</label>
        <input class="u-full-width" type="text" id="displayName" v-model="displayName" :class="{ required: !displayName }" />

        <label for="email">{{ t('email') }}</label>
        <input class="u-full-width" type="text" id="email" v-model="email" :class="{ required: !email }" />

        <label for="password">{{ t(isNew ? 'password' : 'newPassword') }}</label>
        <input class="u-full-width" type="password" id="password" v-model="password" :class="{ required: isNew && !password }" :placeholder="isNew ? '' : t('newPasswordPlaceholder')" />

        <label for="role">{{ t('role') }}</label>
        <select class="u-full-width" id="role" :class="{ required: !role }" v-model="role">
          <option disabled :value="undefined">{{ t('roleSelect') }}</option>
          <option v-for="option in roleOptions" :key="option.value" :value="option.value">{{ option.label }}</option>
        </select>

        <Checkbox v-model="userNotifications" :label="t('userNotifications')" />

        <!--
        <label for="role">{{ t('role') }}</label>
        <select class="u-full-width" id="role" :class="{ required: !roleId }" v-model="roleId">
          <option disabled :value="undefined">{{ t('roleSelect') }}</option>
          <option v-for="option in roles" :key="option.id" :value="option.id">{{ option.name }}</option>
        </select>
        -->
      </div>
    </form>
  </div>
</template>

<i18n lang="yaml">
en:
  title:
    create: "Add user"
    edit: "Edit user"

  loading: "Loading..."
  toolbar:
    submit: "Save"
    cancel: "Cancel"
    addcheck: "Add check"
    close: "Close"

  username: "Username"
  displayName: "Display name"
  email: "E-mail"
  password: "Password"
  newPassword: "New password"
  newPasswordPlaceholder: "<keep current password>"
  role: "Role"
  roleSelect: "Select a role..."
  userNotifications: "Receive notifications"
  notifications: 
    saveSuccess: "The user has been saved"
    loadUserFailed: "Failed to load user: {message}"
    saveUserFailed: "Failed to save user: {message}"
</i18n>

<script lang="ts" setup>
import { ref, onMounted, reactive, computed } from 'vue';
import axios from 'axios';
import { useI18n } from 'vue-i18n';
import { useNotifications } from '@/lib/notifications';
import { UserAPIModel, BaseUserAPIModel, InsertUserAPIModel, UpdateUserAPIModel, RoleAPIModel } from '@/model/user';
import router from '@/router';
import Checkbox from '@/components/Checkbox.vue';


interface RoleSelectOption
{
  label: string;
  value: string;
}


const props = defineProps({
  id: String
});

const { t } = useI18n();
const notifications = useNotifications();


const loading = ref(true);
const saving = ref(false);

const originalUsername = ref<string>();
const username = ref<string>();
const displayName = ref<string>();
const email = ref<string>();
const role = ref<string>();
const userNotifications = ref<boolean>();
const password = ref<string>();

const roleOptions = reactive([] as Array<RoleSelectOption>);


const isNew = computed<boolean>(() =>
{
  return !props.id;
});


onMounted(async () =>
{
  try
  {
    await loadRoles();
    
    if (!!props.id)
      await loadUser(props.id);

    loading.value = false;
  }
  catch (e)
  {
    notifications.alert(t('notifications.loadUserFailed', { message: (e as Error).message }));
  }
});


async function loadUser(id: string)
{
  const response = await axios.get<UserAPIModel>(`/api/users/${encodeURIComponent(id)}`);

  username.value = response.data.username;
  originalUsername.value = username.value;
  displayName.value = response.data.displayName;
  email.value = response.data.email;
  role.value = response.data.role;
  userNotifications.value = response.data.notifications;
}


async function saveUser()
{
  // TODO validate input

  const user: BaseUserAPIModel = {
    username: username.value!,
    displayName: displayName.value!,
    email: email.value!,
    role: role.value!,
    notifications: userNotifications.value!
  };

  if (!!props.id)
    (user as UpdateUserAPIModel).newPassword = password.value;
  else
    (user as InsertUserAPIModel).password = password.value!;

  try
  {
    const response = !!props.id 
      ? await axios.put(`/api/users/${encodeURIComponent(props.id)}`, user)
      : await axios.post('/api/users', user);

    notifications.info(t('notifications.saveSuccess'));
    router.push({ name: 'ListUsers' });
  }
  catch (e)
  {
    notifications.alert(t('notifications.saveUserFailed', { message: (e as Error).message }));
  }
}


async function loadRoles()
{
  const response = await axios.get<RoleAPIModel[]>('/api/users/roles');


  const convertViewModel = (role: RoleAPIModel): RoleSelectOption =>
  {
    return {
      label: role.name,
      value: role.id,
    }
  };

  Object.assign(roleOptions, response.data.map(convertViewModel));
}
</script>

<style lang="scss" scoped>
.properties
{
  margin-top: 1em;
  margin-bottom: 3em;

  .toolbar
  {
    margin-top: 1em;
  }
}
</style>

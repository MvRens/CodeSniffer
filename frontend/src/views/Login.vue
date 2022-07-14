<template>
  <div class="login-container">
    <form class="login u-full-width" @submit.prevent="submitLogin">
      <h4>{{ t('title') }}</h4>
      <label for="username">{{ t('username') }}</label>
      <input class="u-full-width" type="text" id="username" v-model="username" />

      <label for="password">{{ t('password') }}</label>
      <input class="u-full-width" type="password" id="password" v-model="password" />

      <div class="buttons">
        <input class="button-primary" type="submit" :value="t('submit')" />
      </div>

      <div class="message">
        {{ message }}
      </div>
    </form>
  </div>
</template>

<i18n lang="yaml">
en:
  title: "CodeSniffer Login"
  username: "Username"
  password: "Password"
  submit: "Log in"
  messages:
    missingFields: "Please enter your username and password."
    forbidden: "Your username and/or password are incorrect."
    internalServerError: "An unknown error occured. Please try again later."
</i18n>

<script lang="ts" setup>
import { ref, onUnmounted } from 'vue';
import axios from 'axios';
import { useLogin } from '../store/login';
import router from '@/router';
import { useI18n } from 'vue-i18n';


const loginStore = useLogin();
const { t } = useI18n();

const username = ref<string>();
const password = ref<string>();
const message = ref<string>();
let messageTimer = undefined as number | undefined;


onUnmounted(() =>
{
  if (messageTimer !== undefined)
  {
    clearTimeout(messageTimer);
    messageTimer = undefined;
  }
});


function showMessage(key: string)
{
  message.value = t(key);
  if (messageTimer !== undefined)
    clearTimeout(messageTimer);

  messageTimer = setTimeout(() =>
  {
    message.value = undefined;

    clearTimeout(messageTimer!);
    messageTimer = undefined;
  }, 5000);
}


async function submitLogin()
{
  if (!username.value || !password.value)
  {
    showMessage('messages.missingFields');
    return;
  }

  const loginResponse = await axios.post('/api/login', {
    username: username.value,
    password: password.value
  }, {
    validateStatus: () => true
  });

  if (loginResponse.status != 200)
  {
    showMessage(loginResponse.status === 403
      ? "messages.forbidden"
      : "messages.internalServerError");

    return;
  }

  loginStore.login(loginResponse.data.token);
  router.push({ name: 'Home' });
}
</script>

<style lang="scss" scoped>
.login-container
{
  height: 100vh;
  display: flex;
  justify-content: center;
  align-items: center;
}

.login
{
  max-width: 30em;

  h4
  {
    text-align: center;
  }

  .buttons
  {
    text-align: center;
  }
}

.message
{
  text-align: center;
  min-height: 3em;
}
</style>

<template>
  <div class="container">
    {{ t('bye') }}
  </div>
</template>

<i18n lang="yaml">
en:
  bye: "Bye!"
</i18n>

<script lang="ts" setup>
import { onMounted } from 'vue';
import { useI18n } from 'vue-i18n';
import { useLogin } from '@/store/login';
import router from '@/router';

const { t } = useI18n();
const login = useLogin();

onMounted(() =>
{
  login.logout();
  router.push({ name: "Login" });
});
</script>
<template>
  <div class="container">
    <h4>{{ t('title') }}</h4>

    <div v-if="isLoading">
      {{ t('loading') }}
    </div>

    <div v-else>
    </div>
  </div>
</template>

<i18n lang="yaml">
en:
  title: "CodeSniffer Dashboard"
  loading: "Loading..."
</i18n>

<script lang="ts" setup>
import { computed, onMounted } from 'vue';
import { useI18n } from 'vue-i18n';

import { useNotifications } from '@/lib/notifications';

const { t } = useI18n();
const notifications = useNotifications();

const isLoading = computed(() => 
{
  return true;
});


onMounted(() =>
{
});
</script>

<style lang="scss" scoped>
</style>

<template>
  <div v-if="!!properties" class="properties">
    <template v-for="key in Object.keys(properties)" :key="key">
      <div class="key">{{ key }}</div>
      <div class="value">{{ properties[key] }}</div>
    </template>
  </div>
</template>


<script lang="ts" setup>
import { defineProps } from 'vue';

defineProps({
  properties: {
    type: Object
  }
});
</script>


<style lang="scss" scoped>
.properties
{
  display: grid;
  grid-template-columns: auto 1fr;

  .key
  {
    color: #808080;
    margin-right: 2em;
  }
}
</style>
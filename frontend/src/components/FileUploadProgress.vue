<template>
  <div class="file">
    <span class="filename">{{ filename }}</span>
    <div class="progress-container">
      <div class="progress" :class="{ done, error }" :style="'width: ' + progress + '%'"></div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { defineProps } from 'vue';

const props = defineProps({
  filename: String,
  filesize: Number,
  progress: Number,
  done: Boolean,
  error: Boolean
});
</script>

<style lang="scss" scoped>
.file
{
  margin-bottom: 1rem;
}

.filename
{
  font-weight: bold;
}

.progress-container
{
  width: 200px;
  height: 3px;
  background-color: #f0f0f0;

  .progress
  {
    height: 3px;
    background-color: #33C3F0;

    &.done
    {
      background-color: yellowgreen;
    }

    &.error
    {
      background-color: red;
    }
  }
}
</style>
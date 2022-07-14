<template>
  <svg height="24" width="24" viewbox="0,0,24,24" class="checkbox" :class="{ readonly, checked: modelValue }" @click="toggle">
    <defs>
      <clipPath id="outline">
        <circle cx="12" cy="12" r="11"/>
      </clipPath>
    </defs>
    <circle cx="12" cy="12" r="11" class="outline" clip-path="url(#outline)" />
    <polyline points="6,12,10,16,17,8" class="check" />
  </svg>
  <span v-if="!!label" @click="toggle" class="checkbox-label" :class="{ readonly }">{{ label }}</span>
</template>

<script lang="ts" setup>
import { defineEmits } from 'vue';

const props = defineProps({
  modelValue: Boolean,
  label: String,
  readonly: {
    type: Boolean,
    default: false
  }
});

const emit = defineEmits(['update:modelValue']);

function toggle() 
{
  if (props.readonly)
    return;

  emit("update:modelValue", !props.modelValue);
};
</script>


<style lang="scss">
.checkbox
{
  display: inline-block;
  
  .outline
  {
    fill: none; 
    stroke: #d6e0e6; 
    stroke-width: 4;
    transition: all 350ms cubic-bezier(0, 0.89, 0.44, 1);
  }
  
  .check
  {
    fill: none;
    stroke: white;
    stroke-width: 2;
    stroke-linecap: round;
    stroke-linejoin: round;
    transform-origin: center center;
    transform: scale(0);
    opacity: 0;
    transition: all 350ms cubic-bezier(0, 0.89, 0.44, 1);
  }

  &.checked
  {
    .outline
    {
      stroke: #33C3F0;
      stroke-width: 24;
    }

    .check
    {
      transform: scale(1);
      opacity: 1;
    }
  }


  &.checked.readonly
  {
    .background
    {
      fill: lightgray;
      stroke: none;
    }
  }
}


.checkbox-label
{
  margin-left: .75em;
}


.checkbox, .checkbox-label
{
  cursor: pointer;
  user-select: none;
  vertical-align: middle;

  &.readonly
  {
    cursor: default;
  }
}
</style>

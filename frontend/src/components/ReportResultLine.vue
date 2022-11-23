<template>
  <div class="result" :class="resultClass(result)">
    <font-awesome-icon :icon="resultIcon(result)" class="resulticon" />
    <slot></slot>
  </div>          
</template>


<script lang="ts" setup>
import { ReportResult } from '@/model/report';

defineProps({
  result: {
    type: Number,
    required: true
  }
});


function resultClass(result: ReportResult)
{
  switch (result)
  {
    case ReportResult.Skipped: return 'skipped';
    case ReportResult.Warning: return 'warning';
    case ReportResult.Critical: return 'critical';
    case ReportResult.Error: return 'error';
    default: return 'success';
  }
}


function resultIcon(result: ReportResult)
{
  switch (result)
  {
    case ReportResult.Skipped: return 'fa-solid fa-ban';
    case ReportResult.Warning: return 'fa-solid fa-triangle-exclamation';
    case ReportResult.Critical: return 'fa-solid fa-rectangle-xmark';
    case ReportResult.Error: return 'fa-solid fa-bug';
    default: return 'fa-solid fa-circle-check';
  }
}
</script>


<style lang="scss" scoped>
.resulticon
{
  margin-left: .25em;
  margin-right: .5em;
}

.skipped .resulticon
{
  color: grey;
}

.success .resulticon
{
  color: green;
}

.warning .resulticon
{
  color: orange;
}

.critical .resulticon
{
  color: maroon;
}

.error .resulticon
{
  color: red;
}
</style>
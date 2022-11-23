<template>
  <div class="configurationEditor u-full-width" ref="editor"></div>
</template>

<script lang="ts" setup>
import { ref, onMounted } from 'vue';
import JSONEditor from 'jsoneditor';

const props = defineProps({
  modelValue: String
});

const emit = defineEmits(['update:modelValue']);

const editor = ref<HTMLDivElement>();
let jsonEditor: JSONEditor;


onMounted(() =>
{
  if (editor.value === undefined)
    throw Error("Missing editor ref in template");

  jsonEditor = new JSONEditor(editor.value, {
    mode: 'code',
    mainMenuBar: false,
    onChangeText: jsonString => { emit('update:modelValue', jsonString); }
  });

  if (!!props.modelValue)
    jsonEditor.setText(props.modelValue);
});
</script>


<style lang="scss">
@import 'jsoneditor/dist/jsoneditor.min.css';

.configurationEditor
{
  height: 20em;
  min-height: 15em;
  resize: vertical;
  overflow: hidden;

  .jsoneditor
  {
    border-color: #D1D1D1;
  }
}
</style>
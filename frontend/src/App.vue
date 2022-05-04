<template>
  <router-view />
</template>


<style lang="scss">
@font-face {
  font-family: 'Raleway';
  font-style: normal;
  font-weight: 400;
  src: local(''),
       url('../assets/fonts/raleway-v27-latin-regular.woff2') format('woff2'), /* Chrome 26+, Opera 23+, Firefox 39+ */
       url('../assets/fonts/raleway-v27-latin-regular.woff') format('woff'); /* Chrome 6+, Firefox 3.6+, IE 9+, Safari 5.1+ */
}

@import "skeleton-css/css/normalize";
@import "skeleton-css/css/skeleton";
@import "awesome-notifications/dist/style.css";


.button.disabled, .button.disabled:hover, input[type=submit].disabled
{
  color: #c0c0c0;
  background-color: #f0f0f0;
  border-color: #f0f0f0;
  pointer-events: none;
}

.button
{
  background-color: white;
}

.toolbar
{
  .button
  {
    margin-right: .5em;
  }
}

.section
{
  border-bottom: solid 1px #e0e0e0;
}


input.required, input.required:focus, select.required, select.required:focus
{
  border-color: red;
}
</style>

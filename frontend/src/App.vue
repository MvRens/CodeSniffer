<template>
  <div v-if="showMenu" class="menu">
    <div class="container">
      <router-link :to="{ name: 'Home' }">{{ t('menu.home') }}</router-link>
      <router-link :to="{ name: 'Users' }" v-if="isAdmin">{{ t('menu.users') }}</router-link>
      <router-link :to="{ name: 'Logout' }" class="logout">{{ t('menu.logout') }}</router-link>
    </div>
  </div>
  <router-view />
</template>


<i18n lang="yaml">
en:
  menu:
    home: "Dashboard"
    users: "Users"
    logout: "Logout"
</i18n>

<script lang="ts" setup>
import { computed } from 'vue';
import router from '@/router';
import { useI18n } from 'vue-i18n';
import { useLogin } from "@/store/login";

const { t } = useI18n();
const login = useLogin();

const showMenu = computed(() => !router.currentRoute.value.meta || !router.currentRoute.value.meta.hideMenu);
const isAdmin = computed(() => login.isAdmin());
</script>

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


.menu
{
  background: black;
  margin-bottom: 1em;

  .container
  {
    display: flex;
  }

  a
  {
    display: inline-block;
    color: white;
    padding: .5em;
    text-decoration: none;

    &:hover
    {
      background-color: #404040;
    }

    &.logout
    {
      margin-left: auto;
    }
  }
}


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

import { createApp } from 'vue';
import { createI18n } from 'vue-i18n';
import axios from 'axios';

import App from './App.vue';
import router from './router';
import { useLogin } from './store/login';


const i18n = createI18n({
  legacy: false,
  locale: navigator.language,
  fallbackLocale: 'en',
  missingWarn: false,
  fallbackWarn: false
});


const app = createApp(App)
  .use(router)
  .use(i18n)
  .mount('#app');


const login = useLogin();

axios.interceptors.request.use(config =>
{
  if (login.loggedIn())
  {
    if (!config.headers)
      config.headers = {};
            
    config.headers['Authorization'] = 'Bearer ' + login.bearerToken.value;
  }

  return config;
});
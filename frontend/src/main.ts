import { createApp } from 'vue';
import { createI18n } from 'vue-i18n';
import axios from 'axios';

import { library } from '@fortawesome/fontawesome-svg-core';
import { FontAwesomeIcon } from '@fortawesome/vue-fontawesome';
import 
{ 
  faCircleCheck,
  faTriangleExclamation, 
  faRectangleXmark, 
  faBug,
  faAngleDown,
  faAngleUp
} from '@fortawesome/free-solid-svg-icons'

import App from './App.vue';
import router from './router';
import { useLogin } from './store/login';


library.add(
  faCircleCheck, 
  faTriangleExclamation, 
  faRectangleXmark, 
  faBug,
  faAngleDown,
  faAngleUp
);


const i18n = createI18n({
  legacy: false,
  locale: navigator.language,
  fallbackLocale: 'en',
  missingWarn: false,
  fallbackWarn: false
});


const app = createApp(App)
  .component('font-awesome-icon', FontAwesomeIcon)
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
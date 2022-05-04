import { createRouter, createWebHistory, RouteRecordRaw } from 'vue-router';
import { useLogin } from '../store/login';

import Home from '../views/Home.vue';
import Login from '../views/Login.vue';
import Definition from '../views/Definition.vue';

const routes: Array<RouteRecordRaw> = [
  {
    path: '/',
    name: 'Home',
    component: Home
  },
  {
    path: '/login',
    name: 'Login',
    component: Login
  },

  {
    path: '/definitions/create',
    name: 'CreateDefinition',
    component: Definition
  },
  {
    path: '/definitions/edit/:id',
    name: 'EditDefinition',
    component: Definition,
    props: true
  }
];

const router = createRouter({
  history: createWebHistory('/'),
  routes,
});



const login = useLogin();

router.beforeEach(to =>
{
  if (to.name === 'Login' || to.meta.allowAnonymous)
    return true;

  if (login.loggedIn())
    return true;
  
  return { name: 'Login' };
});


export default router;

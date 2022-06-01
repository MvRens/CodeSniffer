import { createRouter, createWebHistory, RouteRecordRaw } from 'vue-router';
import { useLogin } from '../store/login';

import Home from '../views/Home.vue';
import Login from '../views/Login.vue';
import Logout from '../views/Logout.vue';
import Definition from '../views/Definition.vue';
import Users from '../views/Users.vue';

const routes: Array<RouteRecordRaw> = [
  {
    path: '/',
    name: 'Home',
    component: Home
  },
  {
    path: '/login',
    name: 'Login',
    component: Login,
    meta: {
      hideMenu: true
    }
  },
  {
    path: '/logout',
    name: 'Logout',
    component: Logout,
    meta: {
      hideMenu: true
    }
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
  },

  {
    path: '/users',
    name: 'Users',
    component: Users
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

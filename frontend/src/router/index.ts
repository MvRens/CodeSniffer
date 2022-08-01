import { createRouter, createWebHistory, RouteRecordRaw } from 'vue-router';
import { useLogin } from '../store/login';

import EmptyRoute from '../views/EmptyRoute.vue';
import Dashboard from '../views/Dashboard.vue';
import Login from '../views/Login.vue';
import Logout from '../views/Logout.vue';
import Definitions from '../views/Definitions.vue';
import Definition from '../views/Definition.vue';
import Sources from '../views/Sources.vue';
import Source from '../views/Source.vue';
import SourceGroup from '../views/SourceGroup.vue';
import Users from '../views/Users.vue';
import User from '../views/User.vue';

const routes: Array<RouteRecordRaw> = [
  {
    path: '/',
    name: 'Dashboard',
    component: Dashboard
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
    path: '/definition',
    name: 'Definitions',
    component: EmptyRoute,
    redirect: { name: 'ListDefinitions' },
    children: [
      {
        path: '',
        name: 'ListDefinitions',
        component: Definitions
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
    ]
  },

  {
    path: '/source',
    name: 'Sources',
    component: EmptyRoute,
    redirect: { name: 'ListSources' },
    children: [
      {
        path: '',
        name: 'ListSources',
        component: Sources
      },      
      {
        path: '/source/create',
        name: 'CreateSource',
        component: Source
      },
      {
        path: '/source/edit/:id',
        name: 'EditSource',
        component: Source,
        props: true
      },
      {
        path: '/source/group/create',
        name: 'CreateSourceGroup',
        component: SourceGroup
      },
      {
        path: '/source/group/edit/:id',
        name: 'EditSourceGroup',
        component: SourceGroup,
        props: true
      }

    ]
  },

  {
    path: '/users',
    name: 'Users',
    component: EmptyRoute,
    redirect: { name: 'ListUsers' },
    children: [
      {
        path: '',
        name: 'ListUsers',
        component: Users
      },      
      {
        path: '/users/create',
        name: 'CreateUser',
        component: User
      },
      {
        path: '/users/edit/:id',
        name: 'EditUser',
        component: User,
        props: true
      }
    ]
  },];

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

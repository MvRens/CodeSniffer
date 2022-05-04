import { Ref, ref } from 'vue';
import Cookies from 'js-cookie';

const bearerToken = ref<string | null>(null);

const cookieName = 'CodeSniffer.Token';
const cookie = Cookies.get(cookieName);
if (cookie !== undefined)
  bearerToken.value = cookie;


export function useLogin(): Login
{
  return {
    loggedIn,
    login,
    logout,

    bearerToken
  };
}


export interface Login
{
  loggedIn(): boolean;

  login(token: string): void;
  logout(): void;

  bearerToken: Ref<string | null>;
}


function loggedIn(): boolean
{
  return bearerToken.value !== null;
}


function login(token: string)
{
  bearerToken.value = token;
  Cookies.set(cookieName, token);
}


function logout()
{
  bearerToken.value = null;
  Cookies.remove(cookieName);
}
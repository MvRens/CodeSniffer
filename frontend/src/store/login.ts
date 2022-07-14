import { Ref, ref } from 'vue';
import Cookies from 'js-cookie';
import * as jose from 'jose'

const bearerToken = ref<string>();

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
    isAdmin,

    bearerToken
  };
}


export interface Login
{
  loggedIn(): boolean;

  login(token: string): void;
  logout(): void;

  isAdmin(): boolean;

  bearerToken: Ref<string | undefined>;
}


function loggedIn(): boolean
{
  return bearerToken.value !== undefined;
}


function login(token: string)
{
  bearerToken.value = token;
  Cookies.set(cookieName, token);
}


function logout()
{
  bearerToken.value = undefined;
  Cookies.remove(cookieName);
}


function decodePayload(): jose.JWTPayload | undefined
{
  if (!loggedIn())
    return undefined;

  return jose.decodeJwt(bearerToken.value!);
}


function isAdmin(): boolean
{
  const payload = decodePayload();
  if (payload === undefined)
    return false;

  const role = (payload as any).role;
  return role === "admin";
}
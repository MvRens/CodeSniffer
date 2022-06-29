import { Ref, ref } from 'vue';
import Cookies from 'js-cookie';
import * as jose from 'jose'

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


function decodePayload(): jose.JWTPayload | null
{
  if (!loggedIn())
    return null;

  return jose.decodeJwt(bearerToken.value!);
}


function isAdmin(): boolean
{
  const payload = decodePayload();
  if (payload === null)
    return false;

  const role = (payload as any).role;
  return role === "admin";
}
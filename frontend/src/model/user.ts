export interface ListUserAPIModel
{
  id: string;
  username: string;
  displayName: string;
}


export interface UserAPIModel
{
  username: string;
  displayName: string;
  email: string;
  role: string;
  notifications: boolean;
}


export interface BaseUserAPIModel
{
  username: string;
  displayName: string;
  email: string;
  role: string;
  notifications: boolean;
}


export interface InsertUserAPIModel extends BaseUserAPIModel
{
  password: string;
}


export interface UpdateUserAPIModel extends BaseUserAPIModel
{
  newPassword?: string;
}


export interface RoleAPIModel
{
  id: string;
  name: string;
}
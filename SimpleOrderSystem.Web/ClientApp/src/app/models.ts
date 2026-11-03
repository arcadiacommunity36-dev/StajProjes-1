export interface CurrentUser {
  id: number;
  fullName: string;
  email: string;
  roles: string[];
}

export interface LoginResponse {
  token: string;
  expiration: string;
  user: CurrentUser;
}

export interface Role {
  id: number;
  name: string;
  status: boolean;
}

export interface Employee {
  id: number;
  fullName: string;
  phoneNumber: string;
  email: string;
  gender: string;
  roleIds: number[];
  roles: string[];
  status: boolean;
}

export interface Customer {
  id: number;
  name: string;
  code: string;
  address: string;
  phoneNumber: string;
  email: string;
  status: boolean;
}

export interface Color {
  id: number;
  name: string;
  status: boolean;
}

export interface Product {
  id: number;
  name: string;
  colorId: number;
  colorName: string;
  size: string;
  status: boolean;
}

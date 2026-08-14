import { apiClient } from './client';
import { Customer } from '../types/models';

export const customersApi = {
  getAll: () => apiClient.get<Customer[]>('/customers'),
  create: (data: { name: string; email: string; phone?: string; address?: string }) =>
    apiClient.post<Customer>('/customers', data),
};

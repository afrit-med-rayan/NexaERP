import { apiClient } from './client';
import { SalesOrder } from '../types/models';

export const salesApi = {
  getAll: () => apiClient.get<SalesOrder[]>('/sales-orders'),
  getById: (id: string) => apiClient.get<SalesOrder>(`/sales-orders/${id}`),
  create: (data: {
    customerId: string;
    warehouseId: string;
    items: { productId: string; quantity: number }[];
  }) => apiClient.post<SalesOrder>('/sales-orders', data),
  cancel: (id: string) => apiClient.post(`/sales-orders/${id}/cancel`),
};

import { apiClient } from './client';
import { Product, Category } from '../types/models';

export const productsApi = {
  getAll: () => apiClient.get<Product[]>('/products'),
  getById: (id: string) => apiClient.get<Product>(`/products/${id}`),
  create: (data: Partial<Product>) => apiClient.post<Product>('/products', data),
  update: (id: string, data: Partial<Product>) => apiClient.put<Product>(`/products/${id}`, data),
  delete: (id: string) => apiClient.delete(`/products/${id}`),
};

export const categoriesApi = {
  getAll: () => apiClient.get<Category[]>('/categories'),
  create: (data: { name: string; description?: string }) => apiClient.post<Category>('/categories', data),
};

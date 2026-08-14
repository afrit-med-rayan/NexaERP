import { apiClient } from './client';
import { InventoryItem, LowStockProduct } from '../types/models';

export const inventoryApi = {
  getAll: () => apiClient.get<InventoryItem[]>('/inventory'),
  getLowStock: () => apiClient.get<LowStockProduct[]>('/inventory/low-stock'),
  adjust: (data: { productId: string; warehouseId: string; delta: number; reason: string }) =>
    apiClient.post('/inventory/adjust', data),
};

export interface DashboardSummary {
  totalRevenueThisMonth: number;
  totalOrdersThisMonth: number;
  lowStockCount: number;
  totalCustomers: number;
}

export interface SalesOverviewPoint {
  date: string;
  revenue: number;
  orderCount: number;
}

export interface Product {
  id: string;
  sku: string;
  name: string;
  description?: string;
  categoryId: string;
  categoryName?: string;
  price: number;
  costPrice: number;
  reorderThreshold: number;
  isActive: boolean;
  createdAt: string;
}

export interface Category {
  id: string;
  name: string;
  description?: string;
}

export interface InventoryItem {
  productId: string;
  productName: string;
  sku: string;
  warehouseId: string;
  warehouseName: string;
  quantity: number;
  reorderThreshold: number;
}

export interface LowStockProduct {
  productId: string;
  productName: string;
  sku: string;
  warehouseId: string;
  warehouseName: string;
  quantity: number;
  reorderThreshold: number;
}

export interface Customer {
  id: string;
  name: string;
  email: string;
  phone?: string;
  address?: string;
  createdAt: string;
}

export interface SalesOrder {
  id: string;
  customerId: string;
  customerName: string;
  orderDate: string;
  status: 'Pending' | 'Completed' | 'Cancelled';
  totalAmount: number;
  createdByName: string;
  items: SalesOrderItem[];
}

export interface SalesOrderItem {
  productId: string;
  productName: string;
  quantity: number;
  unitPrice: number;
}

export interface Warehouse {
  id: string;
  name: string;
  location?: string;
}

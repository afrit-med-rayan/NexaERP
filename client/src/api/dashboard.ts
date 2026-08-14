import { apiClient } from './client';
import { DashboardSummary, SalesOverviewPoint } from '../types/models';

export const dashboardApi = {
  getSummary: () => apiClient.get<DashboardSummary>('/dashboard/summary'),
  getSalesOverview: () => apiClient.get<SalesOverviewPoint[]>('/dashboard/sales-overview'),
};

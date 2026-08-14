import { useEffect, useState } from 'react';
import {
  AreaChart, Area, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer
} from 'recharts';
import { TrendingUp, ShoppingCart, AlertTriangle, Users } from 'lucide-react';
import { dashboardApi } from '../api/dashboard';
import { DashboardSummary, SalesOverviewPoint } from '../types/models';
import { format } from 'date-fns';

interface KpiCardProps {
  icon: React.ComponentType<any>;
  label: string;
  value: string | number;
  color: string;
  bg: string;
}

function KpiCard({ icon: Icon, label, value, color, bg }: KpiCardProps) {
  return (
    <div className="glass-panel animate-fade-in" style={{ padding: '1.5rem', display: 'flex', alignItems: 'center', gap: '1rem' }}>
      <div style={{ width: '48px', height: '48px', borderRadius: 'var(--radius-lg)', backgroundColor: bg, display: 'flex', alignItems: 'center', justifyContent: 'center', flexShrink: 0 }}>
        <Icon size={24} color={color} />
      </div>
      <div>
        <p style={{ fontSize: '0.875rem', color: 'var(--text-secondary)', marginBottom: '0.25rem' }}>{label}</p>
        <p style={{ fontSize: '1.5rem', fontWeight: 700, color: 'var(--text-primary)' }}>{value}</p>
      </div>
    </div>
  );
}

export function Dashboard() {
  const [summary, setSummary] = useState<DashboardSummary | null>(null);
  const [overview, setOverview] = useState<SalesOverviewPoint[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    Promise.all([
      dashboardApi.getSummary(),
      dashboardApi.getSalesOverview(),
    ]).then(([s, o]) => {
      setSummary(s.data);
      // Format dates for display
      const formatted = o.data.map(p => ({
        ...p,
        date: format(new Date(p.date), 'MMM d'),
      }));
      setOverview(formatted);
    }).finally(() => setLoading(false));
  }, []);

  const formatCurrency = (v: number) =>
    new Intl.NumberFormat('en-DZ', { style: 'currency', currency: 'DZD', maximumFractionDigits: 0 }).format(v);

  if (loading) {
    return (
      <div style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', minHeight: '60vh' }}>
        <p style={{ color: 'var(--text-secondary)' }}>Loading dashboard...</p>
      </div>
    );
  }

  return (
    <div className="animate-fade-in" style={{ display: 'flex', flexDirection: 'column', gap: '2rem' }}>
      <div>
        <h1 style={{ fontSize: '1.75rem', fontWeight: 700, marginBottom: '0.25rem' }}>Dashboard</h1>
        <p style={{ color: 'var(--text-secondary)' }}>Business overview at a glance</p>
      </div>

      {/* KPI Cards */}
      <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(220px, 1fr))', gap: '1.25rem' }}>
        <KpiCard
          icon={TrendingUp}
          label="Revenue This Month"
          value={formatCurrency(summary?.totalRevenueThisMonth ?? 0)}
          color="#10b981"
          bg="#d1fae5"
        />
        <KpiCard
          icon={ShoppingCart}
          label="Orders This Month"
          value={summary?.totalOrdersThisMonth ?? 0}
          color="#3b82f6"
          bg="#eff6ff"
        />
        <KpiCard
          icon={AlertTriangle}
          label="Low Stock Alerts"
          value={summary?.lowStockCount ?? 0}
          color="#f59e0b"
          bg="#fef3c7"
        />
        <KpiCard
          icon={Users}
          label="Total Customers"
          value={summary?.totalCustomers ?? 0}
          color="#8b5cf6"
          bg="#ede9fe"
        />
      </div>

      {/* Sales Chart */}
      <div className="glass-panel" style={{ padding: '1.5rem' }}>
        <h2 style={{ fontSize: '1.125rem', fontWeight: 600, marginBottom: '1.5rem' }}>Sales Revenue (Last 30 Days)</h2>
        <ResponsiveContainer width="100%" height={280}>
          <AreaChart data={overview} margin={{ top: 5, right: 10, left: 10, bottom: 5 }}>
            <defs>
              <linearGradient id="colorRevenue" x1="0" y1="0" x2="0" y2="1">
                <stop offset="5%" stopColor="#3b82f6" stopOpacity={0.3} />
                <stop offset="95%" stopColor="#3b82f6" stopOpacity={0} />
              </linearGradient>
            </defs>
            <CartesianGrid strokeDasharray="3 3" stroke="var(--border)" />
            <XAxis dataKey="date" tick={{ fontSize: 12, fill: 'var(--text-tertiary)' }} axisLine={false} tickLine={false} />
            <YAxis tick={{ fontSize: 12, fill: 'var(--text-tertiary)' }} axisLine={false} tickLine={false} tickFormatter={(v) => `${(v/1000).toFixed(0)}k`} />
            <Tooltip
              contentStyle={{ backgroundColor: 'var(--bg-secondary)', border: '1px solid var(--border)', borderRadius: 'var(--radius-md)' }}
              labelStyle={{ color: 'var(--text-primary)', fontWeight: 600 }}
              formatter={(value: number) => [formatCurrency(value), 'Revenue']}
            />
            <Area
              type="monotone"
              dataKey="revenue"
              stroke="#3b82f6"
              strokeWidth={2}
              fill="url(#colorRevenue)"
            />
          </AreaChart>
        </ResponsiveContainer>
      </div>
    </div>
  );
}

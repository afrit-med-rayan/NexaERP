import { useEffect, useState } from 'react';
import { Plus, X, Check, ShoppingCart, XCircle } from 'lucide-react';
import { salesApi } from '../api/sales';
import { customersApi } from '../api/customers';
import { productsApi } from '../api/products';
import { inventoryApi } from '../api/inventory';
import { SalesOrder, Customer, Product, InventoryItem } from '../types/models';
import { useAuthStore } from '../stores/authStore';
import { format } from 'date-fns';

interface OrderLine {
  productId: string;
  quantity: string;
  maxQty: number;
  productName: string;
  unitPrice: number;
}

const STATUS_COLORS: Record<string, { bg: string; color: string }> = {
  Completed: { bg: 'rgba(16,185,129,0.1)', color: 'var(--success)' },
  Pending: { bg: 'rgba(245,158,11,0.1)', color: 'var(--warning)' },
  Cancelled: { bg: 'rgba(239,68,68,0.1)', color: 'var(--danger)' },
};

export function SalesOrders() {
  const { roles } = useAuthStore();
  const canCreate = roles.some(r => ['Admin', 'Manager', 'SalesEmployee'].includes(r));
  const canCancel = roles.some(r => ['Admin', 'Manager'].includes(r));

  const [orders, setOrders] = useState<SalesOrder[]>([]);
  const [customers, setCustomers] = useState<Customer[]>([]);
  const [products, setProducts] = useState<Product[]>([]);
  const [inventory, setInventory] = useState<InventoryItem[]>([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [error, setError] = useState('');
  const [search, setSearch] = useState('');

  // Form state
  const [customerId, setCustomerId] = useState('');
  const [warehouseId, setWarehouseId] = useState('');
  const [lines, setLines] = useState<OrderLine[]>([{ productId: '', quantity: '1', maxQty: 0, productName: '', unitPrice: 0 }]);

  // Warehouses derived from inventory
  const warehouses = [...new Map(inventory.map(i => [i.warehouseId, { id: i.warehouseId, name: i.warehouseName }])).values()];

  const load = () => {
    Promise.all([
      salesApi.getAll(),
      customersApi.getAll(),
      productsApi.getAll(),
      inventoryApi.getAll(),
    ]).then(([o, c, p, inv]) => {
      setOrders(o.data);
      setCustomers(c.data);
      setProducts(p.data.filter(p => p.isActive));
      setInventory(inv.data);
    }).finally(() => setLoading(false));
  };

  useEffect(() => { load(); }, []);

  const getStock = (productId: string) => {
    if (!warehouseId) return 0;
    return inventory.find(i => i.productId === productId && i.warehouseId === warehouseId)?.quantity ?? 0;
  };

  const addLine = () => {
    setLines([...lines, { productId: '', quantity: '1', maxQty: 0, productName: '', unitPrice: 0 }]);
  };

  const removeLine = (idx: number) => {
    setLines(lines.filter((_, i) => i !== idx));
  };

  const updateLine = (idx: number, field: keyof OrderLine, value: string) => {
    const updated = [...lines];
    if (field === 'productId') {
      const prod = products.find(p => p.id === value);
      const stock = getStock(value);
      updated[idx] = { ...updated[idx], productId: value, productName: prod?.name ?? '', unitPrice: prod?.price ?? 0, maxQty: stock };
    } else {
      (updated[idx] as any)[field] = value;
    }
    setLines(updated);
  };

  const orderTotal = lines.reduce((sum, l) => sum + (parseInt(l.quantity) || 0) * l.unitPrice, 0);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    // Stock validation
    for (const line of lines) {
      if (!line.productId) { setError('Please select a product for all lines.'); return; }
      const qty = parseInt(line.quantity);
      if (qty <= 0) { setError('Quantity must be positive.'); return; }
      if (qty > line.maxQty) { setError(`Insufficient stock for "${line.productName}" (available: ${line.maxQty})`); return; }
    }

    try {
      await salesApi.create({
        customerId,
        warehouseId,
        items: lines.map(l => ({ productId: l.productId, quantity: parseInt(l.quantity) })),
      });
      setShowForm(false);
      setLines([{ productId: '', quantity: '1', maxQty: 0, productName: '', unitPrice: 0 }]);
      setCustomerId('');
      setWarehouseId('');
      load();
    } catch (err: any) {
      setError(err.response?.data?.title ?? 'Failed to create order');
    }
  };

  const handleCancel = async (id: string) => {
    if (!confirm('Cancel this order and restore stock?')) return;
    try {
      await salesApi.cancel(id);
      load();
    } catch (err: any) {
      alert(err.response?.data?.title ?? 'Cancellation failed');
    }
  };

  const filtered = orders.filter(o =>
    o.customerName.toLowerCase().includes(search.toLowerCase()) ||
    o.id.toLowerCase().includes(search.toLowerCase())
  );

  return (
    <div className="animate-fade-in" style={{ display: 'flex', flexDirection: 'column', gap: '1.5rem' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <div>
          <h1 style={{ fontSize: '1.75rem', fontWeight: 700 }}>Sales Orders</h1>
          <p style={{ color: 'var(--text-secondary)' }}>{orders.length} total orders</p>
        </div>
        {canCreate && (
          <button className="btn btn-primary" onClick={() => setShowForm(true)}>
            <Plus size={18} /> New Order
          </button>
        )}
      </div>

      <input
        className="input"
        placeholder="Search by customer or order ID..."
        value={search}
        onChange={e => setSearch(e.target.value)}
        style={{ maxWidth: '360px' }}
      />

      {/* New Order Modal */}
      {showForm && (
        <div style={{ position: 'fixed', inset: 0, backgroundColor: 'rgba(0,0,0,0.5)', display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 50, padding: '1rem' }}>
          <div className="glass-panel" style={{ width: '100%', maxWidth: '700px', padding: '2rem', maxHeight: '90vh', overflowY: 'auto' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem' }}>
              <h2 style={{ fontSize: '1.25rem', fontWeight: 600 }}>New Sales Order</h2>
              <button onClick={() => setShowForm(false)} style={{ background: 'none', border: 'none', cursor: 'pointer', color: 'var(--text-tertiary)' }}>
                <X size={20} />
              </button>
            </div>

            {error && (
              <div style={{ padding: '0.75rem', marginBottom: '1rem', color: 'var(--danger)', background: 'rgba(239,68,68,0.1)', borderRadius: 'var(--radius-md)', fontSize: '0.875rem', border: '1px solid rgba(239,68,68,0.2)' }}>
                {error}
              </div>
            )}

            <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '1.25rem' }}>
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
                <div>
                  <label className="label">Customer</label>
                  <select className="input" value={customerId} onChange={e => setCustomerId(e.target.value)} required>
                    <option value="">-- Select Customer --</option>
                    {customers.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
                  </select>
                </div>
                <div>
                  <label className="label">Warehouse</label>
                  <select className="input" value={warehouseId} onChange={e => { setWarehouseId(e.target.value); setLines(l => l.map(ln => ({ ...ln, maxQty: 0 }))); }} required>
                    <option value="">-- Select Warehouse --</option>
                    {warehouses.map(w => <option key={w.id} value={w.id}>{w.name}</option>)}
                  </select>
                </div>
              </div>

              <div>
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '0.75rem' }}>
                  <label className="label" style={{ margin: 0 }}>Order Lines</label>
                  <button type="button" className="btn btn-secondary" style={{ padding: '0.375rem 0.75rem', fontSize: '0.8rem' }} onClick={addLine}>
                    <Plus size={14} /> Add Line
                  </button>
                </div>

                <div style={{ display: 'flex', flexDirection: 'column', gap: '0.625rem' }}>
                  {lines.map((line, idx) => {
                    const stock = warehouseId ? getStock(line.productId) : 0;
                    const qty = parseInt(line.quantity) || 0;
                    const overStock = qty > stock && line.productId;
                    return (
                      <div key={idx} style={{ display: 'grid', gridTemplateColumns: '1fr 100px auto', gap: '0.5rem', alignItems: 'center', padding: '0.75rem', backgroundColor: overStock ? 'rgba(239,68,68,0.05)' : 'var(--bg-tertiary)', borderRadius: 'var(--radius-md)', border: overStock ? '1px solid rgba(239,68,68,0.2)' : '1px solid var(--border)' }}>
                        <div>
                          <select className="input" style={{ padding: '0.5rem 0.625rem' }} value={line.productId} onChange={e => updateLine(idx, 'productId', e.target.value)} required>
                            <option value="">-- Product --</option>
                            {products.map(p => <option key={p.id} value={p.id}>{p.name} — {p.sku}</option>)}
                          </select>
                          {line.productId && warehouseId && (
                            <div style={{ fontSize: '0.75rem', color: overStock ? 'var(--danger)' : 'var(--text-tertiary)', marginTop: '0.25rem' }}>
                              {overStock ? `⚠ Only ${stock} in stock` : `Available: ${stock}`}
                            </div>
                          )}
                        </div>
                        <input
                          type="number" min="1"
                          className="input"
                          style={{ padding: '0.5rem 0.625rem' }}
                          value={line.quantity}
                          onChange={e => updateLine(idx, 'quantity', e.target.value)}
                          required
                        />
                        {lines.length > 1 && (
                          <button type="button" onClick={() => removeLine(idx)} style={{ background: 'none', border: 'none', cursor: 'pointer', color: 'var(--text-tertiary)', padding: '0.25rem' }}>
                            <X size={18} />
                          </button>
                        )}
                      </div>
                    );
                  })}
                </div>
              </div>

              {/* Order Total */}
              <div style={{ padding: '0.875rem 1rem', backgroundColor: 'var(--accent-light)', borderRadius: 'var(--radius-md)', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <span style={{ fontWeight: 600, color: 'var(--text-primary)' }}>Order Total</span>
                <span style={{ fontSize: '1.25rem', fontWeight: 700, color: 'var(--accent-primary)' }}>
                  {new Intl.NumberFormat('en-DZ', { style: 'currency', currency: 'DZD', maximumFractionDigits: 2 }).format(orderTotal)}
                </span>
              </div>

              <div style={{ display: 'flex', gap: '0.75rem', justifyContent: 'flex-end' }}>
                <button type="button" className="btn btn-secondary" onClick={() => setShowForm(false)}>Cancel</button>
                <button type="submit" className="btn btn-primary">
                  <ShoppingCart size={16} /> Place Order
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Orders Table */}
      {loading ? (
        <p style={{ color: 'var(--text-secondary)' }}>Loading orders...</p>
      ) : (
        <div className="glass-panel" style={{ overflow: 'hidden' }}>
          <div style={{ overflowX: 'auto' }}>
            <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.875rem' }}>
              <thead>
                <tr style={{ borderBottom: '1px solid var(--border)', textAlign: 'left' }}>
                  {['Order ID', 'Customer', 'Date', 'Total', 'Status', 'Created By', canCancel ? 'Actions' : ''].filter(Boolean).map(h => (
                    <th key={h} style={{ padding: '0.875rem 1rem', color: 'var(--text-secondary)', fontWeight: 600, whiteSpace: 'nowrap' }}>{h}</th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {filtered.map(o => {
                  const sc = STATUS_COLORS[o.status] ?? STATUS_COLORS.Pending;
                  return (
                    <tr key={o.id} style={{ borderBottom: '1px solid var(--border)' }}
                      onMouseEnter={e => (e.currentTarget.style.backgroundColor = 'var(--bg-tertiary)')}
                      onMouseLeave={e => (e.currentTarget.style.backgroundColor = 'transparent')}>
                      <td style={{ padding: '0.875rem 1rem', fontFamily: 'monospace', color: 'var(--text-tertiary)', fontSize: '0.75rem' }}>{o.id.slice(0, 8)}…</td>
                      <td style={{ padding: '0.875rem 1rem', fontWeight: 600 }}>{o.customerName}</td>
                      <td style={{ padding: '0.875rem 1rem', color: 'var(--text-secondary)' }}>{format(new Date(o.orderDate), 'MMM d, yyyy')}</td>
                      <td style={{ padding: '0.875rem 1rem', fontWeight: 700 }}>
                        {new Intl.NumberFormat('en-DZ', { style: 'currency', currency: 'DZD', maximumFractionDigits: 2 }).format(o.totalAmount)}
                      </td>
                      <td style={{ padding: '0.875rem 1rem' }}>
                        <span style={{ padding: '0.25rem 0.625rem', borderRadius: '9999px', fontSize: '0.75rem', fontWeight: 600, backgroundColor: sc.bg, color: sc.color }}>
                          {o.status}
                        </span>
                      </td>
                      <td style={{ padding: '0.875rem 1rem', color: 'var(--text-secondary)' }}>{o.createdByName}</td>
                      {canCancel && (
                        <td style={{ padding: '0.875rem 1rem' }}>
                          {o.status === 'Completed' && (
                            <button className="btn btn-danger" style={{ padding: '0.375rem 0.75rem', fontSize: '0.8rem' }} onClick={() => handleCancel(o.id)}>
                              <XCircle size={14} /> Cancel
                            </button>
                          )}
                        </td>
                      )}
                    </tr>
                  );
                })}
                {filtered.length === 0 && (
                  <tr><td colSpan={7} style={{ padding: '3rem', textAlign: 'center', color: 'var(--text-tertiary)' }}>No orders found.</td></tr>
                )}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
}

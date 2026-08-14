import { useEffect, useState } from 'react';
import { AlertTriangle, Plus, Check, X } from 'lucide-react';
import { inventoryApi } from '../api/inventory';
import { InventoryItem, LowStockProduct } from '../types/models';
import { useAuthStore } from '../stores/authStore';

export function Inventory() {
  const { roles } = useAuthStore();
  const canAdjust = roles.some(r => ['Admin', 'WarehouseEmployee'].includes(r));

  const [items, setItems] = useState<InventoryItem[]>([]);
  const [lowStock, setLowStock] = useState<LowStockProduct[]>([]);
  const [loading, setLoading] = useState(true);
  const [showAdjust, setShowAdjust] = useState(false);
  const [selectedItem, setSelectedItem] = useState<InventoryItem | null>(null);
  const [delta, setDelta] = useState('');
  const [reason, setReason] = useState('');
  const [error, setError] = useState('');
  const [search, setSearch] = useState('');
  const [tab, setTab] = useState<'all' | 'lowstock'>('all');

  const load = () => {
    Promise.all([inventoryApi.getAll(), inventoryApi.getLowStock()])
      .then(([inv, ls]) => {
        setItems(inv.data);
        setLowStock(ls.data);
      })
      .finally(() => setLoading(false));
  };

  useEffect(() => { load(); }, []);

  const openAdjust = (item: InventoryItem) => {
    setSelectedItem(item);
    setDelta('');
    setReason('');
    setError('');
    setShowAdjust(true);
  };

  const handleAdjust = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedItem) return;
    setError('');
    try {
      await inventoryApi.adjust({
        productId: selectedItem.productId,
        warehouseId: selectedItem.warehouseId,
        delta: parseInt(delta),
        reason,
      });
      setShowAdjust(false);
      load();
    } catch (err: any) {
      setError(err.response?.data?.title ?? 'Adjustment failed');
    }
  };

  const filtered = items.filter(i =>
    i.productName.toLowerCase().includes(search.toLowerCase()) ||
    i.sku.toLowerCase().includes(search.toLowerCase())
  );

  return (
    <div className="animate-fade-in" style={{ display: 'flex', flexDirection: 'column', gap: '1.5rem' }}>
      <div>
        <h1 style={{ fontSize: '1.75rem', fontWeight: 700 }}>Inventory</h1>
        <p style={{ color: 'var(--text-secondary)' }}>Stock levels across all warehouses</p>
      </div>

      {lowStock.length > 0 && (
        <div className="glass-panel" style={{ padding: '1rem 1.25rem', display: 'flex', alignItems: 'center', gap: '0.75rem', border: '1px solid rgba(245,158,11,0.3)', backgroundColor: 'rgba(245,158,11,0.05)' }}>
          <AlertTriangle size={20} color="var(--warning)" />
          <p style={{ color: 'var(--text-primary)', fontWeight: 500 }}>
            <strong>{lowStock.length}</strong> products are below their reorder threshold.
          </p>
        </div>
      )}

      {/* Tabs */}
      <div style={{ display: 'flex', gap: '0.5rem' }}>
        {(['all', 'lowstock'] as const).map(t => (
          <button key={t} onClick={() => setTab(t)} className="btn" style={{
            backgroundColor: tab === t ? 'var(--accent-primary)' : 'var(--bg-secondary)',
            color: tab === t ? 'white' : 'var(--text-secondary)',
            borderColor: tab === t ? 'var(--accent-primary)' : 'var(--border)',
          }}>
            {t === 'all' ? 'All Stock' : `⚠ Low Stock (${lowStock.length})`}
          </button>
        ))}
      </div>

      <input
        className="input"
        placeholder="Search by product name or SKU..."
        value={search}
        onChange={e => setSearch(e.target.value)}
        style={{ maxWidth: '360px' }}
      />

      {/* Adjust Modal */}
      {showAdjust && selectedItem && (
        <div style={{ position: 'fixed', inset: 0, backgroundColor: 'rgba(0,0,0,0.5)', display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 50 }}>
          <div className="glass-panel" style={{ width: '100%', maxWidth: '400px', padding: '2rem' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem' }}>
              <h2 style={{ fontSize: '1.125rem', fontWeight: 600 }}>Adjust Stock</h2>
              <button onClick={() => setShowAdjust(false)} style={{ background: 'none', border: 'none', cursor: 'pointer', color: 'var(--text-tertiary)' }}>
                <X size={20} />
              </button>
            </div>
            <div style={{ marginBottom: '1.25rem', padding: '0.75rem', backgroundColor: 'var(--bg-tertiary)', borderRadius: 'var(--radius-md)', fontSize: '0.875rem' }}>
              <strong>{selectedItem.productName}</strong> · {selectedItem.warehouseName}
              <div style={{ color: 'var(--text-secondary)', marginTop: '0.25rem' }}>Current stock: <strong>{selectedItem.quantity}</strong></div>
            </div>
            {error && (
              <div style={{ padding: '0.75rem', marginBottom: '1rem', color: 'var(--danger)', background: 'rgba(239,68,68,0.1)', borderRadius: 'var(--radius-md)', fontSize: '0.875rem' }}>
                {error}
              </div>
            )}
            <form onSubmit={handleAdjust} style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
              <div>
                <label className="label">Delta (positive = add, negative = remove)</label>
                <input type="number" className="input" value={delta} onChange={e => setDelta(e.target.value)} placeholder="+10 or -5" required />
              </div>
              <div>
                <label className="label">Reason</label>
                <input className="input" value={reason} onChange={e => setReason(e.target.value)} placeholder="e.g. Shipment received" required />
              </div>
              <div style={{ display: 'flex', gap: '0.75rem', justifyContent: 'flex-end' }}>
                <button type="button" className="btn btn-secondary" onClick={() => setShowAdjust(false)}>Cancel</button>
                <button type="submit" className="btn btn-primary"><Check size={16} /> Apply</button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Table */}
      {loading ? (
        <p style={{ color: 'var(--text-secondary)' }}>Loading inventory...</p>
      ) : (
        <div className="glass-panel" style={{ overflow: 'hidden' }}>
          <div style={{ overflowX: 'auto' }}>
            <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.875rem' }}>
              <thead>
                <tr style={{ borderBottom: '1px solid var(--border)', textAlign: 'left' }}>
                  {['SKU', 'Product', 'Warehouse', 'Quantity', 'Threshold', 'Status', canAdjust ? 'Action' : ''].filter(Boolean).map(h => (
                    <th key={h} style={{ padding: '0.875rem 1rem', color: 'var(--text-secondary)', fontWeight: 600 }}>{h}</th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {(tab === 'lowstock' ? lowStock as InventoryItem[] : filtered).map((item, i) => {
                  const isLow = item.quantity <= item.reorderThreshold;
                  return (
                    <tr key={i} style={{ borderBottom: '1px solid var(--border)' }}
                      onMouseEnter={e => (e.currentTarget.style.backgroundColor = 'var(--bg-tertiary)')}
                      onMouseLeave={e => (e.currentTarget.style.backgroundColor = 'transparent')}>
                      <td style={{ padding: '0.875rem 1rem', fontFamily: 'monospace', color: 'var(--accent-primary)', fontWeight: 500 }}>{item.sku}</td>
                      <td style={{ padding: '0.875rem 1rem', fontWeight: 500 }}>{item.productName}</td>
                      <td style={{ padding: '0.875rem 1rem', color: 'var(--text-secondary)' }}>{item.warehouseName}</td>
                      <td style={{ padding: '0.875rem 1rem', fontWeight: 700, color: isLow ? 'var(--danger)' : 'var(--success)' }}>{item.quantity}</td>
                      <td style={{ padding: '0.875rem 1rem', color: 'var(--text-secondary)' }}>{item.reorderThreshold}</td>
                      <td style={{ padding: '0.875rem 1rem' }}>
                        <span style={{ padding: '0.25rem 0.625rem', borderRadius: '9999px', fontSize: '0.75rem', fontWeight: 600, backgroundColor: isLow ? 'rgba(239,68,68,0.1)' : 'rgba(16,185,129,0.1)', color: isLow ? 'var(--danger)' : 'var(--success)' }}>
                          {isLow ? 'Low Stock' : 'OK'}
                        </span>
                      </td>
                      {canAdjust && (
                        <td style={{ padding: '0.875rem 1rem' }}>
                          <button className="btn btn-secondary" style={{ padding: '0.375rem 0.75rem', fontSize: '0.8rem' }} onClick={() => openAdjust(item as InventoryItem)}>
                            <Plus size={14} /> Adjust
                          </button>
                        </td>
                      )}
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
}

import { useEffect, useState } from 'react';
import { Plus, Pencil, Trash2, Check, X } from 'lucide-react';
import { productsApi, categoriesApi } from '../api/products';
import { Product, Category } from '../types/models';
import { useAuthStore } from '../stores/authStore';

interface ProductFormData {
  sku: string;
  name: string;
  description: string;
  categoryId: string;
  price: string;
  costPrice: string;
  reorderThreshold: string;
  isActive: boolean;
}

const emptyForm: ProductFormData = {
  sku: '', name: '', description: '', categoryId: '',
  price: '', costPrice: '', reorderThreshold: '5', isActive: true,
};

export function Products() {
  const { roles } = useAuthStore();
  const canEdit = roles.some(r => ['Admin', 'Manager'].includes(r));
  const canDelete = roles.includes('Admin');

  const [products, setProducts] = useState<Product[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [editId, setEditId] = useState<string | null>(null);
  const [form, setForm] = useState<ProductFormData>(emptyForm);
  const [error, setError] = useState('');
  const [search, setSearch] = useState('');

  const load = () => {
    Promise.all([productsApi.getAll(), categoriesApi.getAll()])
      .then(([p, c]) => {
        setProducts(p.data);
        setCategories(c.data);
      })
      .finally(() => setLoading(false));
  };

  useEffect(() => { load(); }, []);

  const openCreate = () => {
    setForm(emptyForm);
    setEditId(null);
    setError('');
    setShowForm(true);
  };

  const openEdit = (p: Product) => {
    setForm({
      sku: p.sku, name: p.name, description: p.description ?? '',
      categoryId: p.categoryId, price: String(p.price),
      costPrice: String(p.costPrice),
      reorderThreshold: String(p.reorderThreshold),
      isActive: p.isActive,
    });
    setEditId(p.id);
    setError('');
    setShowForm(true);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    try {
      const payload = {
        ...form,
        price: parseFloat(form.price),
        costPrice: parseFloat(form.costPrice),
        reorderThreshold: parseInt(form.reorderThreshold),
      };
      if (editId) {
        await productsApi.update(editId, payload);
      } else {
        await productsApi.create(payload);
      }
      setShowForm(false);
      load();
    } catch (err: any) {
      setError(err.response?.data?.title ?? 'An error occurred');
    }
  };

  const handleDelete = async (id: string) => {
    if (!confirm('Are you sure you want to delete this product?')) return;
    try {
      await productsApi.delete(id);
      load();
    } catch (err: any) {
      alert(err.response?.data?.title ?? 'Delete failed');
    }
  };

  const filtered = products.filter(p =>
    p.name.toLowerCase().includes(search.toLowerCase()) ||
    p.sku.toLowerCase().includes(search.toLowerCase())
  );

  return (
    <div className="animate-fade-in" style={{ display: 'flex', flexDirection: 'column', gap: '1.5rem' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <div>
          <h1 style={{ fontSize: '1.75rem', fontWeight: 700 }}>Products</h1>
          <p style={{ color: 'var(--text-secondary)' }}>{products.length} total products</p>
        </div>
        {canEdit && (
          <button className="btn btn-primary" onClick={openCreate}>
            <Plus size={18} /> New Product
          </button>
        )}
      </div>

      {/* Search */}
      <input
        className="input"
        placeholder="Search by name or SKU..."
        value={search}
        onChange={e => setSearch(e.target.value)}
        style={{ maxWidth: '360px' }}
      />

      {/* Form Modal */}
      {showForm && (
        <div style={{ position: 'fixed', inset: 0, backgroundColor: 'rgba(0,0,0,0.5)', display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 50 }}>
          <div className="glass-panel" style={{ width: '100%', maxWidth: '560px', padding: '2rem', maxHeight: '90vh', overflowY: 'auto' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem' }}>
              <h2 style={{ fontSize: '1.25rem', fontWeight: 600 }}>{editId ? 'Edit Product' : 'New Product'}</h2>
              <button onClick={() => setShowForm(false)} style={{ background: 'none', border: 'none', cursor: 'pointer', color: 'var(--text-tertiary)' }}>
                <X size={20} />
              </button>
            </div>
            {error && (
              <div style={{ padding: '0.75rem', marginBottom: '1rem', backgroundColor: 'rgba(239,68,68,0.1)', color: 'var(--danger)', borderRadius: 'var(--radius-md)', fontSize: '0.875rem', border: '1px solid rgba(239,68,68,0.2)' }}>
                {error}
              </div>
            )}
            <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr', gap: '1rem' }}>
                <div>
                  <label className="label">SKU</label>
                  <input className="input" value={form.sku} onChange={e => setForm({ ...form, sku: e.target.value })} required />
                </div>
                <div>
                  <label className="label">Category</label>
                  <select className="input" value={form.categoryId} onChange={e => setForm({ ...form, categoryId: e.target.value })} required>
                    <option value="">-- Select --</option>
                    {categories.map(c => <option key={c.id} value={c.id}>{c.name}</option>)}
                  </select>
                </div>
              </div>
              <div>
                <label className="label">Name</label>
                <input className="input" value={form.name} onChange={e => setForm({ ...form, name: e.target.value })} required />
              </div>
              <div>
                <label className="label">Description</label>
                <textarea className="input" rows={2} value={form.description} onChange={e => setForm({ ...form, description: e.target.value })} style={{ resize: 'vertical' }} />
              </div>
              <div style={{ display: 'grid', gridTemplateColumns: '1fr 1fr 1fr', gap: '1rem' }}>
                <div>
                  <label className="label">Price (DZD)</label>
                  <input type="number" step="0.01" className="input" value={form.price} onChange={e => setForm({ ...form, price: e.target.value })} required />
                </div>
                <div>
                  <label className="label">Cost Price</label>
                  <input type="number" step="0.01" className="input" value={form.costPrice} onChange={e => setForm({ ...form, costPrice: e.target.value })} required />
                </div>
                <div>
                  <label className="label">Reorder Threshold</label>
                  <input type="number" className="input" value={form.reorderThreshold} onChange={e => setForm({ ...form, reorderThreshold: e.target.value })} required />
                </div>
              </div>
              {editId && (
                <label style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', cursor: 'pointer', fontSize: '0.875rem', fontWeight: 500 }}>
                  <input type="checkbox" checked={form.isActive} onChange={e => setForm({ ...form, isActive: e.target.checked })} />
                  Active (visible in sales orders)
                </label>
              )}
              <div style={{ display: 'flex', gap: '0.75rem', marginTop: '0.5rem', justifyContent: 'flex-end' }}>
                <button type="button" className="btn btn-secondary" onClick={() => setShowForm(false)}>Cancel</button>
                <button type="submit" className="btn btn-primary">
                  <Check size={16} /> {editId ? 'Save Changes' : 'Create Product'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Products Table */}
      {loading ? (
        <p style={{ color: 'var(--text-secondary)' }}>Loading products...</p>
      ) : (
        <div className="glass-panel" style={{ overflow: 'hidden' }}>
          <div style={{ overflowX: 'auto' }}>
            <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.875rem' }}>
              <thead>
                <tr style={{ borderBottom: '1px solid var(--border)', textAlign: 'left' }}>
                  {['SKU', 'Name', 'Category', 'Price', 'Cost', 'Stock Threshold', 'Status', canEdit ? 'Actions' : ''].filter(Boolean).map(h => (
                    <th key={h} style={{ padding: '0.875rem 1rem', color: 'var(--text-secondary)', fontWeight: 600, whiteSpace: 'nowrap' }}>{h}</th>
                  ))}
                </tr>
              </thead>
              <tbody>
                {filtered.map(p => (
                  <tr key={p.id} style={{ borderBottom: '1px solid var(--border)', transition: 'background 0.15s' }}
                    onMouseEnter={e => (e.currentTarget.style.backgroundColor = 'var(--bg-tertiary)')}
                    onMouseLeave={e => (e.currentTarget.style.backgroundColor = 'transparent')}>
                    <td style={{ padding: '0.875rem 1rem', fontFamily: 'monospace', fontWeight: 500, color: 'var(--accent-primary)' }}>{p.sku}</td>
                    <td style={{ padding: '0.875rem 1rem', fontWeight: 500 }}>{p.name}</td>
                    <td style={{ padding: '0.875rem 1rem', color: 'var(--text-secondary)' }}>{p.categoryName}</td>
                    <td style={{ padding: '0.875rem 1rem', fontWeight: 600 }}>{p.price.toFixed(2)}</td>
                    <td style={{ padding: '0.875rem 1rem', color: 'var(--text-secondary)' }}>{p.costPrice.toFixed(2)}</td>
                    <td style={{ padding: '0.875rem 1rem' }}>{p.reorderThreshold}</td>
                    <td style={{ padding: '0.875rem 1rem' }}>
                      <span style={{ padding: '0.25rem 0.625rem', borderRadius: '9999px', fontSize: '0.75rem', fontWeight: 600, backgroundColor: p.isActive ? 'rgba(16,185,129,0.1)' : 'rgba(239,68,68,0.1)', color: p.isActive ? 'var(--success)' : 'var(--danger)' }}>
                        {p.isActive ? 'Active' : 'Inactive'}
                      </span>
                    </td>
                    {canEdit && (
                      <td style={{ padding: '0.875rem 1rem' }}>
                        <div style={{ display: 'flex', gap: '0.5rem' }}>
                          <button className="btn btn-secondary" style={{ padding: '0.375rem 0.625rem' }} onClick={() => openEdit(p)}>
                            <Pencil size={14} />
                          </button>
                          {canDelete && (
                            <button className="btn btn-danger" style={{ padding: '0.375rem 0.625rem' }} onClick={() => handleDelete(p.id)}>
                              <Trash2 size={14} />
                            </button>
                          )}
                        </div>
                      </td>
                    )}
                  </tr>
                ))}
              </tbody>
            </table>
            {filtered.length === 0 && (
              <div style={{ padding: '3rem', textAlign: 'center', color: 'var(--text-tertiary)' }}>No products found.</div>
            )}
          </div>
        </div>
      )}
    </div>
  );
}

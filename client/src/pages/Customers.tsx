import { useEffect, useState } from 'react';
import { Plus, Check, X } from 'lucide-react';
import { customersApi } from '../api/customers';
import { Customer } from '../types/models';
import { useAuthStore } from '../stores/authStore';
import { format } from 'date-fns';

export function Customers() {
  const { roles } = useAuthStore();
  const canCreate = roles.some(r => ['Admin', 'Manager', 'SalesEmployee'].includes(r));

  const [customers, setCustomers] = useState<Customer[]>([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [form, setForm] = useState({ name: '', email: '', phone: '', address: '' });
  const [error, setError] = useState('');
  const [search, setSearch] = useState('');

  const load = () => {
    customersApi.getAll().then(r => setCustomers(r.data)).finally(() => setLoading(false));
  };

  useEffect(() => { load(); }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    try {
      await customersApi.create(form);
      setShowForm(false);
      setForm({ name: '', email: '', phone: '', address: '' });
      load();
    } catch (err: any) {
      setError(err.response?.data?.title ?? 'An error occurred');
    }
  };

  const filtered = customers.filter(c =>
    c.name.toLowerCase().includes(search.toLowerCase()) ||
    c.email.toLowerCase().includes(search.toLowerCase())
  );

  return (
    <div className="animate-fade-in" style={{ display: 'flex', flexDirection: 'column', gap: '1.5rem' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <div>
          <h1 style={{ fontSize: '1.75rem', fontWeight: 700 }}>Customers</h1>
          <p style={{ color: 'var(--text-secondary)' }}>{customers.length} registered customers</p>
        </div>
        {canCreate && (
          <button className="btn btn-primary" onClick={() => setShowForm(true)}>
            <Plus size={18} /> New Customer
          </button>
        )}
      </div>

      <input
        className="input"
        placeholder="Search by name or email..."
        value={search}
        onChange={e => setSearch(e.target.value)}
        style={{ maxWidth: '360px' }}
      />

      {showForm && (
        <div style={{ position: 'fixed', inset: 0, backgroundColor: 'rgba(0,0,0,0.5)', display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 50 }}>
          <div className="glass-panel" style={{ width: '100%', maxWidth: '480px', padding: '2rem' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem' }}>
              <h2 style={{ fontSize: '1.125rem', fontWeight: 600 }}>New Customer</h2>
              <button onClick={() => setShowForm(false)} style={{ background: 'none', border: 'none', cursor: 'pointer', color: 'var(--text-tertiary)' }}>
                <X size={20} />
              </button>
            </div>
            {error && (
              <div style={{ padding: '0.75rem', marginBottom: '1rem', color: 'var(--danger)', background: 'rgba(239,68,68,0.1)', borderRadius: 'var(--radius-md)', fontSize: '0.875rem' }}>
                {error}
              </div>
            )}
            <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
              <div>
                <label className="label">Full Name</label>
                <input className="input" value={form.name} onChange={e => setForm({ ...form, name: e.target.value })} required />
              </div>
              <div>
                <label className="label">Email</label>
                <input type="email" className="input" value={form.email} onChange={e => setForm({ ...form, email: e.target.value })} required />
              </div>
              <div>
                <label className="label">Phone</label>
                <input className="input" value={form.phone} onChange={e => setForm({ ...form, phone: e.target.value })} />
              </div>
              <div>
                <label className="label">Address</label>
                <input className="input" value={form.address} onChange={e => setForm({ ...form, address: e.target.value })} />
              </div>
              <div style={{ display: 'flex', gap: '0.75rem', justifyContent: 'flex-end' }}>
                <button type="button" className="btn btn-secondary" onClick={() => setShowForm(false)}>Cancel</button>
                <button type="submit" className="btn btn-primary"><Check size={16} /> Create</button>
              </div>
            </form>
          </div>
        </div>
      )}

      {loading ? (
        <p style={{ color: 'var(--text-secondary)' }}>Loading...</p>
      ) : (
        <div className="glass-panel" style={{ overflow: 'hidden' }}>
          <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.875rem' }}>
            <thead>
              <tr style={{ borderBottom: '1px solid var(--border)', textAlign: 'left' }}>
                {['Name', 'Email', 'Phone', 'Address', 'Registered'].map(h => (
                  <th key={h} style={{ padding: '0.875rem 1rem', color: 'var(--text-secondary)', fontWeight: 600 }}>{h}</th>
                ))}
              </tr>
            </thead>
            <tbody>
              {filtered.map(c => (
                <tr key={c.id} style={{ borderBottom: '1px solid var(--border)' }}
                  onMouseEnter={e => (e.currentTarget.style.backgroundColor = 'var(--bg-tertiary)')}
                  onMouseLeave={e => (e.currentTarget.style.backgroundColor = 'transparent')}>
                  <td style={{ padding: '0.875rem 1rem', fontWeight: 600 }}>{c.name}</td>
                  <td style={{ padding: '0.875rem 1rem', color: 'var(--accent-primary)' }}>{c.email}</td>
                  <td style={{ padding: '0.875rem 1rem', color: 'var(--text-secondary)' }}>{c.phone ?? '—'}</td>
                  <td style={{ padding: '0.875rem 1rem', color: 'var(--text-secondary)' }}>{c.address ?? '—'}</td>
                  <td style={{ padding: '0.875rem 1rem', color: 'var(--text-tertiary)', fontSize: '0.8rem' }}>
                    {format(new Date(c.createdAt), 'MMM d, yyyy')}
                  </td>
                </tr>
              ))}
              {filtered.length === 0 && (
                <tr><td colSpan={5} style={{ padding: '3rem', textAlign: 'center', color: 'var(--text-tertiary)' }}>No customers found.</td></tr>
              )}
            </tbody>
          </table>
        </div>
      )}
    </div>
  );
}

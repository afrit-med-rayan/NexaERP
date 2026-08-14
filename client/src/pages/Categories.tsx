import { useEffect, useState } from 'react';
import { Plus, Check, X } from 'lucide-react';
import { categoriesApi } from '../api/products';
import { Category } from '../types/models';

export function Categories() {
  const [categories, setCategories] = useState<Category[]>([]);
  const [loading, setLoading] = useState(true);
  const [showForm, setShowForm] = useState(false);
  const [name, setName] = useState('');
  const [desc, setDesc] = useState('');
  const [error, setError] = useState('');

  const load = () => {
    categoriesApi.getAll().then(r => setCategories(r.data)).finally(() => setLoading(false));
  };

  useEffect(() => { load(); }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    try {
      await categoriesApi.create({ name, description: desc });
      setShowForm(false);
      setName('');
      setDesc('');
      load();
    } catch (err: any) {
      setError(err.response?.data?.title ?? 'An error occurred');
    }
  };

  return (
    <div className="animate-fade-in" style={{ display: 'flex', flexDirection: 'column', gap: '1.5rem' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <div>
          <h1 style={{ fontSize: '1.75rem', fontWeight: 700 }}>Categories</h1>
          <p style={{ color: 'var(--text-secondary)' }}>{categories.length} categories</p>
        </div>
        <button className="btn btn-primary" onClick={() => setShowForm(true)}>
          <Plus size={18} /> New Category
        </button>
      </div>

      {showForm && (
        <div style={{ position: 'fixed', inset: 0, backgroundColor: 'rgba(0,0,0,0.5)', display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 50 }}>
          <div className="glass-panel" style={{ width: '100%', maxWidth: '440px', padding: '2rem' }}>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem' }}>
              <h2 style={{ fontSize: '1.125rem', fontWeight: 600 }}>New Category</h2>
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
                <label className="label">Name</label>
                <input className="input" value={name} onChange={e => setName(e.target.value)} required />
              </div>
              <div>
                <label className="label">Description</label>
                <textarea className="input" rows={2} value={desc} onChange={e => setDesc(e.target.value)} style={{ resize: 'vertical' }} />
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
        <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(240px, 1fr))', gap: '1rem' }}>
          {categories.map(c => (
            <div key={c.id} className="glass-panel animate-fade-in" style={{ padding: '1.25rem' }}>
              <h3 style={{ fontWeight: 600, marginBottom: '0.375rem' }}>{c.name}</h3>
              {c.description && <p style={{ fontSize: '0.875rem', color: 'var(--text-secondary)' }}>{c.description}</p>}
            </div>
          ))}
          {categories.length === 0 && (
            <p style={{ color: 'var(--text-tertiary)', gridColumn: '1/-1' }}>No categories yet.</p>
          )}
        </div>
      )}
    </div>
  );
}

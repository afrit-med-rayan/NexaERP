import React, { useState } from 'react';
import { useNavigate, useLocation } from 'react-router-dom';
import { useAuthStore } from '../stores/authStore';
import { apiClient } from '../api/client';
import { AuthResponse } from '../types/auth';

export function Login() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);
  
  const navigate = useNavigate();
  const location = useLocation();
  const { setAuth } = useAuthStore();

  const from = location.state?.from?.pathname || '/dashboard';

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);

    try {
      const { data } = await apiClient.post<AuthResponse>('/auth/login', {
        email,
        password
      });
      
      setAuth(data.token, data.roles);
      navigate(from, { replace: true });
    } catch (err: any) {
      if (err.response?.data?.title) {
        setError(err.response.data.title);
      } else {
        setError('Invalid email or password');
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="app-container" style={{ alignItems: 'center', justifyItems: 'center', background: 'linear-gradient(135deg, var(--bg-tertiary) 0%, var(--bg-primary) 100%)', width: '100%', justifyContent: 'center' }}>
      <div className="glass-panel animate-fade-in" style={{ width: '100%', maxWidth: '400px', padding: '2.5rem' }}>
        <div style={{ textAlign: 'center', marginBottom: '2rem' }}>
          <h1 style={{ fontSize: '1.75rem', fontWeight: 700, color: 'var(--text-primary)', marginBottom: '0.5rem' }}>
            NexaERP
          </h1>
          <p style={{ color: 'var(--text-secondary)' }}>Welcome back. Please login to your account.</p>
        </div>

        {error && (
          <div style={{ padding: '0.75rem', marginBottom: '1.5rem', backgroundColor: 'var(--danger)', color: 'white', borderRadius: 'var(--radius-md)', fontSize: '0.875rem' }}>
            {error}
          </div>
        )}

        <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '1.25rem' }}>
          <div>
            <label className="label" htmlFor="email">Email</label>
            <input 
              id="email"
              type="email" 
              className="input" 
              placeholder="name@example.com"
              value={email}
              onChange={e => setEmail(e.target.value)}
              required
            />
          </div>
          
          <div>
            <label className="label" htmlFor="password">Password</label>
            <input 
              id="password"
              type="password" 
              className="input" 
              placeholder="••••••••"
              value={password}
              onChange={e => setPassword(e.target.value)}
              required
            />
          </div>

          <button 
            type="submit" 
            className="btn btn-primary" 
            disabled={loading}
            style={{ marginTop: '0.5rem', padding: '0.75rem' }}
          >
            {loading ? 'Signing in...' : 'Sign In'}
          </button>
        </form>

        <div style={{ marginTop: '2rem', textAlign: 'center', fontSize: '0.875rem', color: 'var(--text-secondary)' }}>
          <p>Demo accounts (password: Role#2024):</p>
          <ul style={{ listStyle: 'none', marginTop: '0.5rem', display: 'flex', flexDirection: 'column', gap: '0.25rem' }}>
            <li>admin@nexaerp.com</li>
            <li>sales@nexaerp.com</li>
          </ul>
        </div>
      </div>
    </div>
  );
}

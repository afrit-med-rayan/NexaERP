import { useAuthStore } from '../stores/authStore';

export function DashboardPlaceholder() {
  const { user, logout } = useAuthStore();

  return (
    <div style={{ padding: '2rem', width: '100%' }}>
      <div className="glass-panel" style={{ padding: '2rem' }}>
        <h1 style={{ marginBottom: '1rem' }}>Dashboard Placeholder</h1>
        <p>Welcome back, <strong>{user?.fullName}</strong>!</p>
        <p style={{ color: 'var(--text-secondary)', marginBottom: '2rem' }}>Roles: {user?.roles.join(', ')}</p>
        
        <button className="btn btn-secondary" onClick={logout}>
          Sign Out
        </button>
      </div>
    </div>
  );
}

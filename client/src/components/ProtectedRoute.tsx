import { Navigate, useLocation } from 'react-router-dom';
import { useAuthStore } from '../stores/authStore';
import { useEffect, useState } from 'react';
import { apiClient } from '../api/client';
import { UserProfile } from '../types/auth';

interface ProtectedRouteProps {
  children: React.ReactNode;
  allowedRoles?: string[];
}

export function ProtectedRoute({ children, allowedRoles }: ProtectedRouteProps) {
  const { isAuthenticated, roles, setUser, user } = useAuthStore();
  const location = useLocation();
  const [loading, setLoading] = useState(!user && isAuthenticated());

  useEffect(() => {
    let isMounted = true;
    if (isAuthenticated() && !user) {
      // Fetch profile to make sure token is still valid
      apiClient.get<UserProfile>('/auth/me')
        .then(res => {
          if (isMounted) {
            setUser(res.data);
            setLoading(false);
          }
        })
        .catch(() => {
          if (isMounted) {
            // Error interceptor handles the token clearing
            setLoading(false);
          }
        });
    }
    return () => { isMounted = false; };
  }, [isAuthenticated, user, setUser]);

  if (!isAuthenticated()) {
    return <Navigate to="/login" state={{ from: location }} replace />;
  }

  if (loading) {
    return (
      <div className="app-container" style={{ alignItems: 'center', justifyContent: 'center' }}>
        <div style={{ color: 'var(--text-secondary)' }}>Loading profile...</div>
      </div>
    );
  }

  if (allowedRoles && allowedRoles.length > 0) {
    const hasRole = roles.some(r => allowedRoles.includes(r));
    if (!hasRole) {
      return (
        <div className="app-container" style={{ alignItems: 'center', justifyContent: 'center' }}>
          <div className="glass-panel" style={{ padding: '2rem', textAlign: 'center' }}>
            <h2 style={{ color: 'var(--danger)', marginBottom: '1rem' }}>Access Denied</h2>
            <p>You do not have permission to view this page.</p>
          </div>
        </div>
      );
    }
  }

  return <>{children}</>;
}

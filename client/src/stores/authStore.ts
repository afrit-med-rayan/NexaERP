import { create } from 'zustand';
import { UserProfile } from '../types/auth';

interface AuthState {
  token: string | null;
  user: UserProfile | null;
  roles: string[];
  setAuth: (token: string, roles: string[], user?: UserProfile) => void;
  setUser: (user: UserProfile) => void;
  logout: () => void;
  isAuthenticated: () => boolean;
}

export const useAuthStore = create<AuthState>((set, get) => ({
  token: localStorage.getItem('nexa_token'),
  user: null,
  roles: JSON.parse(localStorage.getItem('nexa_roles') || '[]'),
  
  setAuth: (token, roles, user) => {
    localStorage.setItem('nexa_token', token);
    localStorage.setItem('nexa_roles', JSON.stringify(roles));
    set({ token, roles, user });
  },
  
  setUser: (user) => {
    set({ user });
  },

  logout: () => {
    localStorage.removeItem('nexa_token');
    localStorage.removeItem('nexa_roles');
    set({ token: null, user: null, roles: [] });
  },

  isAuthenticated: () => !!get().token
}));

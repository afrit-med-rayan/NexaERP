import axios from 'axios';

// The API url points to the backend
const API_URL = import.meta.env.VITE_API_URL || 'http://localhost:5000/api';

export const apiClient = axios.create({
  baseURL: API_URL,
  headers: {
    'Content-Type': 'application/json',
  },
});

// Intercept requests to add the Authorization header
apiClient.interceptors.request.use((config) => {
  const token = localStorage.getItem('nexa_token');
  if (token && config.headers) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// Intercept responses to handle unauthorized (e.g., expired token)
apiClient.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      // Clear token and maybe redirect to login (handled in auth store)
      localStorage.removeItem('nexa_token');
      // If we are not already on login, reload to force AuthGuard to redirect
      if (window.location.pathname !== '/login' && window.location.pathname !== '/register') {
        window.location.href = '/login';
      }
    }
    return Promise.reject(error);
  }
);

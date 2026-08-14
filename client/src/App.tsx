import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { Login } from './pages/Login';
import { Dashboard } from './pages/Dashboard';
import { Products } from './pages/Products';
import { Categories } from './pages/Categories';
import { ProtectedRoute } from './components/ProtectedRoute';
import { Layout } from './components/Layout';
import { useAuthStore } from './stores/authStore';

function App() {
  const { isAuthenticated } = useAuthStore();

  return (
    <Router>
      <div style={{ display: 'flex', minHeight: '100vh', width: '100%' }}>
        <Routes>
          <Route
            path="/"
            element={<Navigate to={isAuthenticated() ? '/dashboard' : '/login'} replace />}
          />
          <Route path="/login" element={<Login />} />

          {/* Protected layout shell */}
          <Route
            path="/dashboard"
            element={
              <ProtectedRoute>
                <Layout />
              </ProtectedRoute>
            }
          >
            {/* Dashboard index */}
            <Route index element={
              <ProtectedRoute allowedRoles={['Admin', 'Manager']}>
                <Dashboard />
              </ProtectedRoute>
            } />
            {/* Products */}
            <Route path="products" element={
              <ProtectedRoute allowedRoles={['Admin', 'Manager']}>
                <Products />
              </ProtectedRoute>
            } />
            {/* Categories */}
            <Route path="categories" element={
              <ProtectedRoute allowedRoles={['Admin', 'Manager']}>
                <Categories />
              </ProtectedRoute>
            } />
            {/* Inventory */}
            <Route path="inventory" element={
              <ProtectedRoute allowedRoles={['Admin', 'Manager', 'WarehouseEmployee']}>
                <div className="animate-fade-in"><h2>Inventory — coming in 9.5</h2></div>
              </ProtectedRoute>
            } />
            {/* Customers */}
            <Route path="customers" element={
              <ProtectedRoute allowedRoles={['Admin', 'Manager', 'SalesEmployee', 'Accountant']}>
                <div className="animate-fade-in"><h2>Customers — coming in 9.5</h2></div>
              </ProtectedRoute>
            } />
            {/* Sales */}
            <Route path="sales" element={
              <ProtectedRoute allowedRoles={['Admin', 'Manager', 'SalesEmployee', 'Accountant']}>
                <div className="animate-fade-in"><h2>Sales Orders — coming in 9.6</h2></div>
              </ProtectedRoute>
            } />
          </Route>
        </Routes>
      </div>
    </Router>
  );
}

export default App;

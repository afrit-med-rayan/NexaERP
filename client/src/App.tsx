import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { Login } from './pages/Login';
import { DashboardPlaceholder } from './pages/DashboardPlaceholder';
import { ProtectedRoute } from './components/ProtectedRoute';
import { useAuthStore } from './stores/authStore';

function App() {
  const { isAuthenticated } = useAuthStore();

  return (
    <Router>
      <div className="app-container">
        <Routes>
          <Route 
            path="/" 
            element={<Navigate to={isAuthenticated() ? "/dashboard" : "/login"} replace />} 
          />
          <Route path="/login" element={<Login />} />
          
          <Route 
            path="/dashboard" 
            element={
              <ProtectedRoute>
                <DashboardPlaceholder />
              </ProtectedRoute>
            } 
          />
        </Routes>
      </div>
    </Router>
  );
}

export default App;

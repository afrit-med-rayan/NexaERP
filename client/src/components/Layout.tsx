import React from 'react';
import { NavLink, Outlet } from 'react-router-dom';
import { LayoutDashboard, Package, Folders, Boxes, Users, ShoppingCart, LogOut } from 'lucide-react';
import { useAuthStore } from '../stores/authStore';

export function Layout() {
  const { user, roles, logout } = useAuthStore();

  const navItems = [
    { to: '/dashboard', icon: LayoutDashboard, label: 'Dashboard', roles: ['Admin', 'Manager'] },
    { to: '/dashboard/products', icon: Package, label: 'Products', roles: ['Admin', 'Manager'] },
    { to: '/dashboard/categories', icon: Folders, label: 'Categories', roles: ['Admin', 'Manager'] },
    { to: '/dashboard/inventory', icon: Boxes, label: 'Inventory', roles: ['Admin', 'Manager', 'WarehouseEmployee'] },
    { to: '/dashboard/customers', icon: Users, label: 'Customers', roles: ['Admin', 'Manager', 'SalesEmployee', 'Accountant'] },
    { to: '/dashboard/sales', icon: ShoppingCart, label: 'Sales Orders', roles: ['Admin', 'Manager', 'SalesEmployee', 'Accountant'] },
  ];

  const filteredNav = navItems.filter(item => 
    roles.some(role => item.roles.includes(role))
  );

  return (
    <div style={{ display: 'flex', minHeight: '100vh', width: '100%' }}>
      {/* Sidebar */}
      <div style={{ width: '260px', backgroundColor: 'var(--bg-secondary)', borderRight: '1px solid var(--border)', display: 'flex', flexDirection: 'column' }}>
        <div style={{ padding: '1.5rem', borderBottom: '1px solid var(--border)' }}>
          <h1 style={{ fontSize: '1.25rem', fontWeight: 700, color: 'var(--accent-primary)' }}>NexaERP</h1>
        </div>
        
        <nav style={{ flex: 1, padding: '1rem' }}>
          <ul style={{ listStyle: 'none', display: 'flex', flexDirection: 'column', gap: '0.25rem' }}>
            {filteredNav.map(item => (
              <li key={item.to}>
                <NavLink 
                  to={item.to}
                  style={({ isActive }) => ({
                    display: 'flex',
                    alignItems: 'center',
                    gap: '0.75rem',
                    padding: '0.75rem 1rem',
                    borderRadius: 'var(--radius-md)',
                    textDecoration: 'none',
                    color: isActive ? 'var(--accent-primary)' : 'var(--text-secondary)',
                    backgroundColor: isActive ? 'var(--accent-light)' : 'transparent',
                    fontWeight: isActive ? 600 : 500,
                    transition: 'all 0.2s ease'
                  })}
                >
                  <item.icon size={20} />
                  {item.label}
                </NavLink>
              </li>
            ))}
          </ul>
        </nav>

        <div style={{ padding: '1rem', borderTop: '1px solid var(--border)' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '0.75rem', marginBottom: '1rem' }}>
            <div style={{ width: '36px', height: '36px', borderRadius: '50%', backgroundColor: 'var(--accent-light)', color: 'var(--accent-primary)', display: 'flex', alignItems: 'center', justifyContent: 'center', fontWeight: 600 }}>
              {user?.fullName.charAt(0)}
            </div>
            <div style={{ overflow: 'hidden' }}>
              <div style={{ fontSize: '0.875rem', fontWeight: 600, whiteSpace: 'nowrap', textOverflow: 'ellipsis' }}>
                {user?.fullName}
              </div>
              <div style={{ fontSize: '0.75rem', color: 'var(--text-tertiary)', whiteSpace: 'nowrap', textOverflow: 'ellipsis' }}>
                {roles[0]}
              </div>
            </div>
          </div>
          <button 
            onClick={logout}
            style={{ width: '100%', display: 'flex', alignItems: 'center', gap: '0.5rem', padding: '0.75rem', border: 'none', backgroundColor: 'transparent', color: 'var(--text-secondary)', cursor: 'pointer', borderRadius: 'var(--radius-md)' }}
          >
            <LogOut size={18} />
            <span style={{ fontWeight: 500 }}>Sign Out</span>
          </button>
        </div>
      </div>

      {/* Main Content */}
      <div style={{ flex: 1, display: 'flex', flexDirection: 'column', backgroundColor: 'var(--bg-primary)' }}>
        <main style={{ flex: 1, padding: '2rem', overflowY: 'auto' }}>
          <Outlet />
        </main>
      </div>
    </div>
  );
}

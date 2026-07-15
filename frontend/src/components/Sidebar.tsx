import React from 'react';
import { NavLink } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { Role } from '../types';
import { 
  LayoutDashboard, Package, Tag, ShoppingBag, 
  MessageSquare, Settings, Users, Layers, Truck, 
  Store, Cpu, LogOut, ArrowLeft 
} from 'lucide-react';

interface SidebarProps {
  type: 'seller' | 'admin';
}

const Sidebar: React.FC<SidebarProps> = ({ type }) => {
  const { storeId, logout } = useAuth();

  const renderSellerLinks = () => (
    <>
      <NavLink 
        to="/seller/dashboard" 
        end
        className={({ isActive }) => `sidebar-link ${isActive ? 'active' : ''}`}
      >
        <LayoutDashboard size={20} />
        <span>Dashboard</span>
      </NavLink>

      <NavLink 
        to="/seller/products" 
        className={({ isActive }) => `sidebar-link ${isActive ? 'active' : ''}`}
      >
        <Package size={20} />
        <span>My Products</span>
      </NavLink>

      <NavLink 
        to="/seller/offers" 
        className={({ isActive }) => `sidebar-link ${isActive ? 'active' : ''}`}
      >
        <Tag size={20} />
        <span>Offers & Discounts</span>
      </NavLink>

      <NavLink 
        to="/seller/orders" 
        className={({ isActive }) => `sidebar-link ${isActive ? 'active' : ''}`}
      >
        <ShoppingBag size={20} />
        <span>Store Orders</span>
      </NavLink>

      <NavLink 
        to="/messages" 
        className={({ isActive }) => `sidebar-link ${isActive ? 'active' : ''}`}
      >
        <MessageSquare size={20} />
        <span>Buyer Messages</span>
      </NavLink>

      {storeId && (
        <NavLink 
          to="/seller/settings" 
          className={({ isActive }) => `sidebar-link ${isActive ? 'active' : ''}`}
        >
          <Settings size={20} />
          <span>Store Settings</span>
        </NavLink>
      )}
    </>
  );

  const renderAdminLinks = () => (
    <>
      <NavLink 
        to="/admin/dashboard" 
        end
        className={({ isActive }) => `sidebar-link ${isActive ? 'active' : ''}`}
      >
        <LayoutDashboard size={20} />
        <span>Admin Home</span>
      </NavLink>

      <NavLink 
        to="/admin/users" 
        className={({ isActive }) => `sidebar-link ${isActive ? 'active' : ''}`}
      >
        <Users size={20} />
        <span>Users Management</span>
      </NavLink>

      <NavLink 
        to="/admin/stores" 
        className={({ isActive }) => `sidebar-link ${isActive ? 'active' : ''}`}
      >
        <Store size={20} />
        <span>Stores Oversight</span>
      </NavLink>

      <NavLink 
        to="/admin/categories" 
        className={({ isActive }) => `sidebar-link ${isActive ? 'active' : ''}`}
      >
        <Layers size={20} />
        <span>Categories CRUD</span>
      </NavLink>

      <NavLink 
        to="/admin/shipping" 
        className={({ isActive }) => `sidebar-link ${isActive ? 'active' : ''}`}
      >
        <Truck size={20} />
        <span>Shipping Matrix</span>
      </NavLink>

      <NavLink 
        to="/admin/orders" 
        className={({ isActive }) => `sidebar-link ${isActive ? 'active' : ''}`}
      >
        <ShoppingBag size={20} />
        <span>All Orders</span>
      </NavLink>

      <NavLink 
        to="/admin/ai" 
        className={({ isActive }) => `sidebar-link ${isActive ? 'active' : ''}`}
      >
        <Cpu size={20} />
        <span>AI Maintenance</span>
      </NavLink>
    </>
  );

  return (
    <div className="sidebar-container">
      {/* Back to main catalog link */}
      <NavLink 
        to="/" 
        className="sidebar-link" 
        style={{ marginBottom: '1rem', borderBottom: '1px solid rgba(255,255,255,0.1)', paddingBottom: '1rem' }}
      >
        <ArrowLeft size={18} />
        <span>Back to Catalog</span>
      </NavLink>

      <div className="sidebar-logo">
        <span>⚡</span>
        <div style={{ display: 'flex', flexDirection: 'column', lineHeight: 1 }}>
          <span style={{ fontSize: '1.2rem', color: 'var(--primary)' }}>
            {type === 'seller' ? 'Seller Panel' : 'Admin Panel'}
          </span>
          <span style={{ fontSize: '0.6rem', color: 'var(--text-muted)' }}>ElAtaba Marketplace</span>
        </div>
      </div>

      <div className="sidebar-menu">
        {type === 'seller' ? renderSellerLinks() : renderAdminLinks()}
      </div>

      <button 
        className="sidebar-link" 
        style={{ 
          marginTop: 'auto', 
          background: 'none', 
          border: 'none', 
          color: 'rgba(255,255,255,0.6)', 
          textAlign: 'left',
          width: '100%'
        }}
        onClick={logout}
      >
        <LogOut size={20} />
        <span>Logout</span>
      </button>
    </div>
  );
};

export default Sidebar;

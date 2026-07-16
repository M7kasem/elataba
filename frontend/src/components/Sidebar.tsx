import React from 'react';
import { NavLink } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { useLanguage } from '../context/LanguageContext';
import { Role } from '../types';
import { 
  LayoutDashboard, Package, Tag, ShoppingBag, 
  MessageSquare, Settings, Users, Layers, Truck, 
  Store, Cpu, LogOut, ArrowLeft 
} from 'lucide-react';

const copy = {
  ar: {
    backToCatalog: 'الرجوع للموقع الرئيسي',
    logout: 'تسجيل الخروج',
    dashboard: 'لوحة التحكم',
    myProducts: 'المنتجات المعروضة',
    offers: 'العروض والخصومات',
    orders: 'طلبات الزباين',
    messages: 'رسائل المشترين',
    settings: 'إعدادات المحل',
    adminHome: 'الرئيسية للمشرف',
    users: 'إدارة المستخدمين',
    stores: 'إدارة المحلات',
    categories: 'إدارة التصنيفات',
    shipping: 'مصاريف الشحن',
    allOrders: 'كل الطلبيات',
    ai: 'صيانة الذكاء الاصطناعي',
  },
  en: {
    backToCatalog: 'Back to Catalog',
    logout: 'Logout',
    dashboard: 'Dashboard',
    myProducts: 'My Products',
    offers: 'Offers & Discounts',
    orders: 'Store Orders',
    messages: 'Buyer Messages',
    settings: 'Store Settings',
    adminHome: 'Admin Home',
    users: 'Users Management',
    stores: 'Stores Oversight',
    categories: 'Categories CRUD',
    shipping: 'Shipping Matrix',
    allOrders: 'All Orders',
    ai: 'AI Maintenance',
  }
};

interface SidebarProps {
  type: 'seller' | 'admin';
}

const Sidebar: React.FC<SidebarProps> = ({ type }) => {
  const { storeId, logout } = useAuth();
  const { language } = useLanguage();
  const labels = copy[language];

  const renderSellerLinks = () => (
    <>
      <NavLink 
        to="/seller/dashboard" 
        end
        className={({ isActive }) => `sidebar-link ${isActive ? 'active' : ''}`}
      >
        <LayoutDashboard size={20} />
        <span>{labels.dashboard}</span>
      </NavLink>

      <NavLink 
        to="/seller/products" 
        className={({ isActive }) => `sidebar-link ${isActive ? 'active' : ''}`}
      >
        <Package size={20} />
        <span>{labels.myProducts}</span>
      </NavLink>

      <NavLink 
        to="/seller/offers" 
        className={({ isActive }) => `sidebar-link ${isActive ? 'active' : ''}`}
      >
        <Tag size={20} />
        <span>{labels.offers}</span>
      </NavLink>

      <NavLink 
        to="/seller/orders" 
        className={({ isActive }) => `sidebar-link ${isActive ? 'active' : ''}`}
      >
        <ShoppingBag size={20} />
        <span>{labels.orders}</span>
      </NavLink>

      <NavLink 
        to="/messages" 
        className={({ isActive }) => `sidebar-link ${isActive ? 'active' : ''}`}
      >
        <MessageSquare size={20} />
        <span>{labels.messages}</span>
      </NavLink>

      {storeId && (
        <NavLink 
          to="/seller/settings" 
          className={({ isActive }) => `sidebar-link ${isActive ? 'active' : ''}`}
        >
          <Settings size={20} />
          <span>{labels.settings}</span>
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
        <span>{labels.adminHome}</span>
      </NavLink>

      <NavLink 
        to="/admin/users" 
        className={({ isActive }) => `sidebar-link ${isActive ? 'active' : ''}`}
      >
        <Users size={20} />
        <span>{labels.users}</span>
      </NavLink>

      <NavLink 
        to="/admin/stores" 
        className={({ isActive }) => `sidebar-link ${isActive ? 'active' : ''}`}
      >
        <Store size={20} />
        <span>{labels.stores}</span>
      </NavLink>

      <NavLink 
        to="/admin/categories" 
        className={({ isActive }) => `sidebar-link ${isActive ? 'active' : ''}`}
      >
        <Layers size={20} />
        <span>{labels.categories}</span>
      </NavLink>

      <NavLink 
        to="/admin/shipping" 
        className={({ isActive }) => `sidebar-link ${isActive ? 'active' : ''}`}
      >
        <Truck size={20} />
        <span>{labels.shipping}</span>
      </NavLink>

      <NavLink 
        to="/admin/orders" 
        className={({ isActive }) => `sidebar-link ${isActive ? 'active' : ''}`}
      >
        <ShoppingBag size={20} />
        <span>{labels.allOrders}</span>
      </NavLink>

      <NavLink 
        to="/admin/ai" 
        className={({ isActive }) => `sidebar-link ${isActive ? 'active' : ''}`}
      >
        <Cpu size={20} />
        <span>{labels.ai}</span>
      </NavLink>
    </>
  );

  return (
    <div className="sidebar-container">
      <div className="sidebar-menu" style={{ marginTop: '1rem' }}>
        {type === 'seller' ? renderSellerLinks() : renderAdminLinks()}
      </div>

       <button 
        onClick={logout}
        style={{ 
          marginTop: 'auto', 
          background: 'rgba(230, 57, 70, 0.15)', 
          border: '1px solid rgba(230, 57, 70, 0.3)', 
          color: '#e63946', 
          display: 'flex',
          alignItems: 'center',
          gap: '1rem',
          padding: '0.8rem 1rem',
          borderRadius: 'var(--radius-md)',
          fontWeight: 'bold',
          cursor: 'pointer',
          width: '100%',
          transition: 'all 0.2s',
        }}
        onMouseEnter={(e) => {
          e.currentTarget.style.background = 'rgba(230, 57, 70, 0.25)';
          e.currentTarget.style.borderColor = 'rgba(230, 57, 70, 0.5)';
        }}
        onMouseLeave={(e) => {
          e.currentTarget.style.background = 'rgba(230, 57, 70, 0.15)';
          e.currentTarget.style.borderColor = 'rgba(230, 57, 70, 0.3)';
        }}
      >
        <LogOut size={20} />
        <span>{labels.logout}</span>
      </button>
    </div>
  );
};

export default Sidebar;

import React, { useEffect, useState } from 'react';
import { Link, useNavigate, useLocation } from 'react-router-dom';
import { LogOut, MessageSquare, Moon, Package, Search, ShieldAlert, ShoppingCart, Store, Sun, User, Camera } from 'lucide-react';
import { useAuth } from '../context/AuthContext';
import { useCart } from '../context/CartContext';
import { useLanguage } from '../context/LanguageContext';
import { Role } from '../types';

const copy = {
  ar: {
    catalog: 'المنتجات', imageSearch: 'البحث بالصورة', login: 'تسجيل الدخول', register: 'إنشاء حساب',
    store: 'متجري', createStore: 'أنشئ متجرك', admin: 'لوحة التحكم', search: 'ابحث عن المنتجات أو المتاجر...',
  },
  en: {
    catalog: 'Catalog', imageSearch: 'Image Search', login: 'Login', register: 'Register',
    store: 'My Store', createStore: 'Create Store', admin: 'Admin Dashboard', search: 'Search products or stores...',
  },
};

import arabicLogo from '../assets/Arabic_logo_cropped.svg';
import englishLogo from '../assets/English_logo_cropped.svg';

const Navbar: React.FC = () => {
  const { role, storeId, logout, isAuthenticated } = useAuth();
  const { cartItems } = useCart();
  const { language, setLanguage } = useLanguage();
  const navigate = useNavigate();
  const location = useLocation();
  const [searchQuery, setSearchQuery] = useState('');
  const [darkMode, setDarkMode] = useState(() => localStorage.getItem('elAtaba_theme') === 'dark');

  useEffect(() => {
    document.body.classList.toggle('dark-mode', darkMode);
  }, [darkMode]);

  const labels = copy[language];
  const cartCount = cartItems.reduce((total, item) => total + item.quantity, 0);

  const handleSearchSubmit = (event: React.FormEvent) => {
    event.preventDefault();
    if (searchQuery.trim()) navigate(`/?search=${encodeURIComponent(searchQuery.trim())}`);
  };

  const isHome = location.pathname === '/';

  return (
    <nav className={`navbar-container ${isHome ? 'navbar-home' : ''}`} style={isHome ? {
      position: 'absolute',
      top: 0,
      left: 0,
      width: '100%',
      backgroundColor: 'rgb(255 255 255 / 15%)',
      backdropFilter: 'blur(10px)',
      borderBottom: '1px solid rgba(0,0,0,0.05)',
      boxShadow: 'var(--shadow-sm)',
      zIndex: 100
    } : {}}>
      <Link to="/" className="navbar-logo" aria-label="ElAtaba home" style={{ 
        display: 'flex', 
        alignItems: 'center', 
        padding: 0, 
        width: '260px',
        marginLeft: language === 'en' ? '-4rem' : 0,
        marginRight: language === 'ar' ? '-4rem' : 0,
        boxShadow: 'var(--shadow-md)',
        borderBottomRightRadius: language === 'en' ? '1.5rem' : 0,
        borderBottomLeftRadius: language === 'ar' ? '1.5rem' : 0,
        backgroundColor: 'var(--bg-main)', // Using bg-main to blend nicely or white
        overflow: 'hidden'
      }}>
        <img 
          src={language === 'ar' ? arabicLogo : englishLogo} 
          alt="ElAtaba Logo" 
          style={{ width: '100%', height: 'auto', objectFit: 'contain', display: 'block' }}
        />
      </Link>

      <form onSubmit={handleSearchSubmit} className="navbar-search" style={{ position: 'relative' }}>
        <button 
          type="submit" 
          style={{ 
            background: 'none', 
            border: 'none', 
            padding: 0, 
            cursor: 'pointer', 
            display: 'flex', 
            alignItems: 'center', 
            justifyContent: 'center' 
          }}
          title="Search"
        >
          <Search size={18} color="var(--text-muted)" />
        </button>
        <input 
          value={searchQuery} 
          onChange={(event) => setSearchQuery(event.target.value)} 
          placeholder={labels.search} 
          style={{ paddingLeft: '0.5rem', paddingRight: '2rem' }}
        />
        <Link 
          to="/image-search" 
          title={labels.imageSearch}
          style={{ 
            display: 'flex', 
            alignItems: 'center', 
            color: 'var(--text-muted)', 
            cursor: 'pointer',
            padding: '0.2rem'
          }}
        >
          <Camera size={18} />
        </Link>
      </form>

      <div className="navbar-actions">
        <Link to="/products" className="nav-link-item">{labels.catalog}</Link>
        <button className="nav-icon-button language-switch" onClick={() => setLanguage(language === 'ar' ? 'en' : 'ar')} title={language === 'ar' ? 'Switch to English' : 'التبديل إلى العربية'}>
          {language === 'ar' ? 'EN' : 'ع'}
        </button>
        <button className="nav-icon-button" onClick={() => setDarkMode(!darkMode)} title={darkMode ? 'Switch to light mode' : 'Switch to dark mode'}>
          {darkMode ? <Sun size={20} /> : <Moon size={20} />}
        </button>

        <Link to="/cart" className="cart-link" aria-label="Cart">
          <ShoppingCart size={22} />
          {cartCount > 0 && <span className="cart-count">{cartCount}</span>}
        </Link>

        {isAuthenticated ? (
          <>
            <Link to="/messages" title="Messages"><MessageSquare size={20} /></Link>
            {role === Role.Buyer && <Link to="/orders" title="My orders"><Package size={20} /></Link>}
            {(role === Role.Seller || role === Role.StoreManager) && (
              <Link to={storeId ? '/seller/dashboard' : '/seller/create-store'} className="btn btn-sm btn-primary"><Store size={16} />{storeId ? labels.store : labels.createStore}</Link>
            )}
            {role === Role.Admin && <Link to="/admin/dashboard" className="btn btn-sm btn-secondary"><ShieldAlert size={16} />{labels.admin}</Link>}
            <Link to="/profile" title="My profile"><User size={20} /></Link>
            <button className="nav-icon-button logout-button" onClick={logout} title="Logout"><LogOut size={20} /></button>
          </>
        ) : (
          <div className="auth-links">
            <Link to="/login" className="btn btn-sm btn-outline">{labels.login}</Link>
            <Link to="/register" className="btn btn-sm btn-primary">{labels.register}</Link>
          </div>
        )}
      </div>
    </nav>
  );
};

export default Navbar;

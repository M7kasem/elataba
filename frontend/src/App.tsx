import React from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider, useAuth } from './context/AuthContext';
import { CartProvider } from './context/CartContext';
import { ToastProvider } from './context/ToastContext';
import { Role } from './types';

// Components
import Navbar from './components/Navbar';
import FloatingRoleSwitcher from './components/FloatingRoleSwitcher';

// Pages
import Home from './pages/Home';
import ProductDetail from './pages/ProductDetail';
import ImageSearch from './pages/ImageSearch';
import Cart from './pages/Cart';
import Checkout from './pages/Checkout';
import Orders from './pages/Orders';
import OrderDetail from './pages/OrderDetail';
import Login from './pages/Login';
import Register from './pages/Register';
import Profile from './pages/Profile';
import Messages from './pages/Messages';

// Dashboards & setup
import CreateStore from './pages/seller/CreateStore';
import SellerDashboard from './pages/seller/SellerDashboard';
import AdminDashboard from './pages/admin/AdminDashboard';

// 403 Forbidden Component
const Forbidden: React.FC = () => (
  <div className="main-content" style={{ padding: '5rem 2rem', textAlign: 'center' }}>
    <span style={{ fontSize: '4rem' }}>🛑</span>
    <h1 style={{ fontSize: '2rem', marginTop: '1.5rem', marginBottom: '1rem' }}>403 - Not Authorized</h1>
    <p style={{ color: 'var(--text-muted)', marginBottom: '2rem' }}>
      You do not have the required permissions to view this dashboard page.
    </p>
    <Link to="/" className="btn btn-primary" style={{ display: 'inline-block' }}>Back to Home Catalog</Link>
  </div>
);

import { Link } from 'react-router-dom';

// Route Guards
const BuyerRoute: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { isAuthenticated, role, isLoading } = useAuth();
  if (isLoading) return <div style={{ padding: '4rem', textAlign: 'center' }}>Loading session...</div>;
  if (!isAuthenticated) return <Navigate to="/login" replace />;
  if (role !== Role.Buyer) return <Navigate to="/403" replace />;
  return <>{children}</>;
};

const SellerRoute: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { isAuthenticated, role, isLoading } = useAuth();
  if (isLoading) return <div style={{ padding: '4rem', textAlign: 'center' }}>Loading session...</div>;
  if (!isAuthenticated) return <Navigate to="/login" replace />;
  if (role !== Role.Seller && role !== Role.StoreManager) return <Navigate to="/403" replace />;
  return <>{children}</>;
};

const AdminRoute: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { isAuthenticated, role, isLoading } = useAuth();
  if (isLoading) return <div style={{ padding: '4rem', textAlign: 'center' }}>Loading session...</div>;
  if (!isAuthenticated) return <Navigate to="/login" replace />;
  if (role !== Role.Admin) return <Navigate to="/403" replace />;
  return <>{children}</>;
};

const AppContent: React.FC = () => {
  const { role, isAuthenticated } = useAuth();

  return (
    <div className="app-container">
      <Navbar />
      
      <Routes>
        {/* Public Catalog Routes */}
        <Route path="/" element={<Home />} />
        <Route path="/product/:id" element={<ProductDetail />} />
        <Route path="/image-search" element={<ImageSearch />} />
        
        {/* Auth Pages */}
        <Route path="/login" element={<Login />} />
        <Route path="/register" element={<Register />} />
        <Route path="/403" element={<Forbidden />} />

        {/* Buyer Authenticated Routes */}
        <Route path="/cart" element={<Cart />} />
        
        <Route path="/checkout" element={
          <BuyerRoute>
            <Checkout />
          </BuyerRoute>
        } />
        <Route path="/orders" element={
          <BuyerRoute>
            <Orders />
          </BuyerRoute>
        } />
        <Route path="/order/:id" element={
          <BuyerRoute>
            <OrderDetail />
          </BuyerRoute>
        } />

        {/* Shared Authenticated Routes */}
        <Route path="/profile" element={
          <Profile />
        } />
        <Route path="/messages" element={
          <Messages />
        } />

        {/* Seller Dashboards & Gated setup */}
        <Route path="/seller/create-store" element={
          <SellerRoute>
            <CreateStore />
          </SellerRoute>
        } />
        <Route path="/seller/dashboard" element={
          <SellerRoute>
            <SellerDashboard />
          </SellerRoute>
        } />
        <Route path="/seller/products" element={
          <SellerRoute>
            <SellerDashboard />
          </SellerRoute>
        } />
        <Route path="/seller/offers" element={
          <SellerRoute>
            <SellerDashboard />
          </SellerRoute>
        } />
        <Route path="/seller/orders" element={
          <SellerRoute>
            <SellerDashboard />
          </SellerRoute>
        } />
        <Route path="/seller/settings" element={
          <SellerRoute>
            <SellerDashboard />
          </SellerRoute>
        } />

        {/* Admin Dashboard */}
        <Route path="/admin/dashboard" element={
          <AdminRoute>
            <AdminDashboard />
          </AdminRoute>
        } />

        {/* Fallback to Catalog Home */}
        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>

      {/* Floating Role Switcher Widget (Visible only in development and when logged in) */}
      {import.meta.env.DEV && isAuthenticated && <FloatingRoleSwitcher />}

      {/* Simple Footer */}
      <footer style={{ 
        padding: '2rem 4rem', 
        backgroundColor: 'var(--bg-sidebar)', 
        color: 'var(--text-light)', 
        textAlign: 'center',
        fontSize: '0.9rem',
        borderTop: '1px solid var(--border-color)',
        marginTop: 'auto'
      }}>
        <div>&copy; {new Date().getFullYear()} ElAtaba Wholesale Marketplace. All rights reserved.</div>
        <div style={{ color: 'var(--text-muted)', fontSize: '0.75rem', marginTop: '0.5rem' }}>
          Built using React, Vite, and C# ASP.NET Core Backend Services.
        </div>
      </footer>
    </div>
  );
};

import { LanguageProvider } from './context/LanguageContext';

const App: React.FC = () => {
  return (
    <Router>
      <LanguageProvider>
        <AuthProvider>
          <ToastProvider>
            <CartProvider>
              <AppContent />
            </CartProvider>
          </ToastProvider>
        </AuthProvider>
      </LanguageProvider>
    </Router>
  );
};

export default App;


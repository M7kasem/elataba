import React, { Suspense, lazy } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate, useLocation } from 'react-router-dom';
import { AuthProvider, useAuth } from './context/AuthContext';
import { CartProvider } from './context/CartContext';
import { ToastProvider } from './context/ToastContext';
import { Role } from './types';

// Components
import Navbar from './components/Navbar';
import Footer from './components/Footer';
import FloatingRoleSwitcher from './components/FloatingRoleSwitcher';

// Pages
const Home = lazy(() => import('./pages/Home'));
const ProductDetail = lazy(() => import('./pages/ProductDetail'));
const ImageSearch = lazy(() => import('./pages/ImageSearch'));
const Cart = lazy(() => import('./pages/Cart'));
const Checkout = lazy(() => import('./pages/Checkout'));
const Orders = lazy(() => import('./pages/Orders'));
const OrderDetail = lazy(() => import('./pages/OrderDetail'));
const Login = lazy(() => import('./pages/Login'));
const Register = lazy(() => import('./pages/Register'));
const Profile = lazy(() => import('./pages/Profile'));
const Messages = lazy(() => import('./pages/Messages'));

// Dashboards & setup
const CreateStore = lazy(() => import('./pages/seller/CreateStore'));
const SellerDashboard = lazy(() => import('./pages/seller/SellerDashboard'));
const AdminDashboard = lazy(() => import('./pages/admin/AdminDashboard'));

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
  const location = useLocation();

  return (
    <div className="app-container">
      <Navbar />
      
      <Suspense fallback={<div style={{ padding: '5rem', textAlign: 'center' }}>Loading...</div>}>
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
          <Route path="/admin/users" element={
            <AdminRoute>
              <AdminDashboard />
            </AdminRoute>
          } />
          <Route path="/admin/stores" element={
            <AdminRoute>
              <AdminDashboard />
            </AdminRoute>
          } />
          <Route path="/admin/categories" element={
            <AdminRoute>
              <AdminDashboard />
            </AdminRoute>
          } />
          <Route path="/admin/shipping" element={
            <AdminRoute>
              <AdminDashboard />
            </AdminRoute>
          } />
          <Route path="/admin/orders" element={
            <AdminRoute>
              <AdminDashboard />
            </AdminRoute>
          } />
          <Route path="/admin/ai" element={
            <AdminRoute>
              <AdminDashboard />
            </AdminRoute>
          } />

          {/* Fallback to Catalog Home */}
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </Suspense>

      {/* Floating Role Switcher Widget (Visible only in development and when logged in) */}
      {import.meta.env.DEV && isAuthenticated && <FloatingRoleSwitcher />}

      {/* Simple Footer (Global) */}
      <Footer />
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


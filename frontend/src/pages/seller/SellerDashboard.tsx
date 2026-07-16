import React, { useState, useEffect, useCallback } from 'react';
import { useLocation } from 'react-router-dom';
import apiClient from '../../api/client';
import { toCategories, toOffers, toOrders, toProducts } from '../../api/normalizers';
import { useAuth } from '../../context/AuthContext';
import { Product, Order, Offer, Category } from '../../types';
import Sidebar from '../../components/Sidebar';

// Subcomponents
import { SellerStats } from './components/SellerStats';
import { ProductInventory } from './components/ProductInventory';
import { OfferManagement } from './components/OfferManagement';
import { OrderList } from './components/OrderList';
import { StoreSettings } from './components/StoreSettings';
import Footer from '../../components/Footer';

const SellerDashboard: React.FC = () => {
  const { storeId } = useAuth();
  const location = useLocation();

  const [products, setProducts] = useState<Product[]>([]);
  const [orders, setOrders] = useState<Order[]>([]);
  const [offers, setOffers] = useState<Offer[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [loading, setLoading] = useState(true);

  // Fetch data function that can be triggered to refresh the dashboard lists
  const loadDashboardData = useCallback(async () => {
    if (!storeId) return;
    try {
      const [prodRes, ordersRes, offersRes, catsRes] = await Promise.all([
        apiClient.get(`/api/Product?storeId=${storeId}`),
        apiClient.get(`/api/Order?storeId=${storeId}`).catch(() => ({ data: { data: [] } })),
        apiClient.get(`/api/Offer?storeId=${storeId}`).catch(() => ({ data: { data: [] } })),
        apiClient.get('/api/Category/GetAll')
      ]);

      const storeProducts = toProducts(prodRes.data?.data?.data ?? []).filter((p) => p.storeId === storeId);
      setProducts(storeProducts);
      
      const storeOrders = toOrders(ordersRes.data?.data || []).filter((o) => o.storeId === storeId);
      setOrders(storeOrders);
      
      const storeOffers = toOffers(offersRes.data?.data || []).filter((offer) => offer.storeId === storeId);
      setOffers(storeOffers);
      
      const catList = toCategories(catsRes.data?.data || []);
      setCategories(catList);
    } catch (err) {
      console.error('Error fetching dashboard data:', err);
    } finally {
      setLoading(false);
    }
  }, [storeId]);

  useEffect(() => {
    loadDashboardData();
  }, [loadDashboardData]);

  // Determine active view from URL pathname
  const renderActiveView = () => {
    if (!storeId) {
      return <div style={{ padding: '2rem', textAlign: 'center' }}>No Store ID associated with this user.</div>;
    }

    const path = location.pathname;

    switch (path) {
      case '/seller/products':
        return (
          <ProductInventory 
            storeId={storeId} 
            products={products} 
            categories={categories} 
            onRefresh={loadDashboardData} 
          />
        );
      case '/seller/offers':
        return (
          <OfferManagement 
            storeId={storeId} 
            offers={offers} 
            products={products} 
            onRefresh={loadDashboardData} 
          />
        );
      case '/seller/orders':
        return (
          <OrderList 
            storeId={storeId} 
            orders={orders} 
            onRefresh={loadDashboardData} 
          />
        );
      case '/seller/settings':
        return <StoreSettings storeId={storeId} />;
      case '/seller/dashboard':
      default:
        return (
          <SellerStats 
            products={products} 
            orders={orders} 
            offers={offers} 
          />
        );
    }
  };

  if (loading) {
    return (
      <div className="dashboard-layout">
        <div style={{ width: '260px', flexShrink: 0, height: 'calc(100vh - 78px)', position: 'sticky', top: '78px' }}>
          <Sidebar type="seller" />
        </div>
        <div style={{ flex: 1, padding: '3rem', display: 'flex', flexDirection: 'column', gap: '2rem' }}>
          <div className="skeleton" style={{ width: '40%', height: '40px' }} />
          <div className="skeleton" style={{ width: '100%', height: '400px', borderRadius: 'var(--radius-lg)' }} />
        </div>
      </div>
    );
  }

  return (
    <div className="dashboard-layout">
      {/* Navigation Sidebar */}
      <div style={{ width: '260px', flexShrink: 0, height: 'calc(100vh - 78px)', position: 'sticky', top: '78px' }}>
        <Sidebar type="seller" />
      </div>

      {/* Main Panel Content */}
      <div style={{ flex: 1, display: 'flex', flexDirection: 'column', minHeight: 'calc(100vh - 78px)', boxSizing: 'border-box' }}>
        <div className="dashboard-content" style={{ flex: 1, padding: '2rem' }}>
          {renderActiveView()}
        </div>
      </div>
    </div>
  );
};

export default SellerDashboard;

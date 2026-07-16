import React, { useState, useEffect } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useCart } from '../context/CartContext';
import { useAuth } from '../context/AuthContext';
import { useToast } from '../context/ToastContext';
import apiClient from '../api/client';
import { toGovernorates } from '../api/normalizers';
import { Governorate } from '../types';
import { Trash2, ArrowRight, Store, MapPin } from 'lucide-react';

const Cart: React.FC = () => {
  const { cartItems, updateQuantity, removeFromCart, getGroupedCart, getCartTotal } = useCart();
  const { isAuthenticated, user } = useAuth();
  const { showToast } = useToast();
  const navigate = useNavigate();

  const [governorates, setGovernorates] = useState<Governorate[]>([]);
  const [selectedGovId, setSelectedGovId] = useState<number>(user?.governorateId || 1);
  const [shippingRate, setShippingRate] = useState<number>(15); // Default flat rate per store

  useEffect(() => {
    const fetchGovernorates = async () => {
      try {
        const response = await apiClient.get('/api/Governorate');
        const govList = toGovernorates(response.data?.data || []);
        setGovernorates(govList);
        
        // Find selected governorate rate if any
        const activeGov = govList.find((g: Governorate) => g.id === selectedGovId);
        if (activeGov && activeGov.shippingCost) {
          setShippingRate(activeGov.shippingCost);
        }
      } catch (err) {
        console.error('Error loading governorates:', err);
      }
    };
    fetchGovernorates();
  }, [selectedGovId]);

  const handleGovChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const govId = Number(e.target.value);
    setSelectedGovId(govId);
    
    const activeGov = governorates.find((g) => g.id === govId);
    if (activeGov && activeGov.shippingCost) {
      setShippingRate(activeGov.shippingCost);
    }
  };

  const groupedCart = getGroupedCart();
  const uniqueStoresCount = Object.keys(groupedCart).length;
  const totalItemsCost = getCartTotal();
  
  // Total shipping is calculated per store (since orders are split by store)
  const totalShippingEstimate = uniqueStoresCount * shippingRate;
  const grandTotal = totalItemsCost + totalShippingEstimate;

  const handleProceedToCheckout = () => {
    if (cartItems.length === 0) return;
    if (!isAuthenticated) {
      showToast('Please login to complete your order.', 'warning');
      navigate('/login');
      return;
    }
    navigate('/checkout', { state: { selectedGovId, shippingRate } });
  };

  const getAbsoluteImageUrl = (url: string) => {
    if (!url) return 'https://via.placeholder.com/100';
    if (url.startsWith('http://') || url.startsWith('https://') || url.startsWith('data:')) {
      return url;
    }
    return `http://localhost:5191${url.startsWith('/') ? '' : '/'}${url}`;
  };

  if (cartItems.length === 0) {
    return (
      <div className="main-content" style={{ padding: '4rem 2rem', textAlign: 'center' }}>
        <span style={{ fontSize: '4rem' }}>🛒</span>
        <h2 style={{ fontSize: '1.8rem', marginTop: '1.5rem', marginBottom: '1rem' }}>Your shopping cart is empty</h2>
        <p style={{ color: 'var(--text-muted)', marginBottom: '2rem' }}>Add some wholesale products from the catalog to get started.</p>
        <Link to="/" className="btn btn-primary">Go to Catalog</Link>
      </div>
    );
  }

  return (
    <div className="main-content" style={{ padding: '2rem 4rem' }}>
      <h1 style={{ fontSize: '2.2rem', marginBottom: '2rem' }}>Shopping Cart (سلة المشتريات)</h1>

      <div style={{ display: 'flex', gap: '3rem', flexWrap: 'wrap' }}>
        {/* Left Side: Cart Items grouped by store */}
        <div style={{ flex: '1 1 600px', display: 'flex', flexDirection: 'column', gap: '2rem' }}>
          {Object.entries(groupedCart).map(([storeName, items]) => (
            <div key={storeName} className="card" style={{ padding: '1.5rem', border: '1px solid var(--border-color)' }}>
              {/* Store Section Header */}
              <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginBottom: '1rem', borderBottom: '1px solid var(--border-color)', paddingBottom: '0.75rem' }}>
                <Store size={18} color="var(--primary-hover)" />
                <h3 style={{ fontSize: '1.1rem', margin: 0 }}>{storeName}</h3>
              </div>

              {/* Items List */}
              <div style={{ display: 'flex', flexDirection: 'column', gap: '1.25rem' }}>
                {items.map((item) => {
                  const primaryImage = item.product.images?.find((img) => img.isPrimary)?.imageUrl
                    || item.product.images?.[0]?.imageUrl
                    || '';
                  
                  return (
                    <div key={item.product.id} style={{ display: 'flex', alignItems: 'center', gap: '1.5rem', flexWrap: 'wrap' }}>
                      {/* Thumbnail */}
                      <img 
                        src={getAbsoluteImageUrl(primaryImage)} 
                        alt={item.product.name}
                        style={{ width: '80px', height: '80px', borderRadius: 'var(--radius-md)', objectFit: 'cover', backgroundColor: '#f1f3f5' }}
                      />

                      {/* Product details */}
                      <div style={{ flex: 1, minWidth: '200px' }}>
                        <Link to={`/product/${item.product.id}`} style={{ fontWeight: 'bold', fontSize: '1rem' }} className="nav-link-item">
                          {item.product.name}
                        </Link>
                        <div style={{ fontSize: '0.85rem', color: 'var(--text-muted)', marginTop: '0.2rem' }}>
                          Unit Price: <span style={{ color: 'var(--secondary-hover)', fontWeight: 'bold' }}>${item.selectedPrice}</span>
                        </div>
                      </div>

                      {/* Quantity Stepper */}
                      <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                        <button 
                          className="btn btn-outline btn-sm"
                          onClick={() => updateQuantity(item.product.id, item.quantity - 1)}
                          style={{ padding: '0.25rem 0.6rem' }}
                        >
                          -
                        </button>
                        <span style={{ minWidth: '30px', textAlign: 'center', fontWeight: 'bold' }}>{item.quantity}</span>
                        <button 
                          className="btn btn-outline btn-sm"
                          onClick={() => updateQuantity(item.product.id, item.quantity + 1)}
                          style={{ padding: '0.25rem 0.6rem' }}
                        >
                          +
                        </button>
                      </div>

                      {/* Item Subtotal */}
                      <div style={{ minWidth: '80px', textAlign: 'right', fontWeight: 'bold', fontSize: '1.1rem' }}>
                        ${(item.selectedPrice * item.quantity).toFixed(2)}
                      </div>

                      {/* Remove Button */}
                      <button 
                        onClick={() => removeFromCart(item.product.id)}
                        style={{ background: 'none', border: 'none', color: 'var(--color-danger)', cursor: 'pointer' }}
                        title="Remove product"
                      >
                        <Trash2 size={18} />
                      </button>
                    </div>
                  );
                })}
              </div>
            </div>
          ))}
        </div>

        {/* Right Side: Order Summary Panel */}
        <div style={{ flex: '1 1 350px' }}>
          <div className="card" style={{ padding: '2rem', border: '2px solid var(--border-color)', position: 'sticky', top: '100px' }}>
            <h2 style={{ fontSize: '1.4rem', marginBottom: '1.5rem' }}>Order Summary</h2>

            {/* Shipping Estimate Selector */}
            <div className="form-group" style={{ marginBottom: '1.5rem', borderBottom: '1px solid var(--border-color)', paddingBottom: '1.25rem' }}>
              <label className="form-label" style={{ display: 'flex', alignItems: 'center', gap: '0.3rem' }}>
                <MapPin size={16} />
                <span>Estimate Shipping to:</span>
              </label>
              <select 
                className="form-control" 
                value={selectedGovId}
                onChange={handleGovChange}
              >
                {governorates.map((gov) => (
                  <option key={gov.id} value={gov.id}>{gov.name}</option>
                ))}
              </select>
            </div>

            {/* Order Price Details */}
            <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem', marginBottom: '1.5rem' }}>
              <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                <span style={{ color: 'var(--text-muted)' }}>Items Subtotal:</span>
                <span style={{ fontWeight: 'bold' }}>${totalItemsCost.toFixed(2)}</span>
              </div>
              <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                <span style={{ color: 'var(--text-muted)' }}>Stores Split:</span>
                <span style={{ fontWeight: 'bold' }}>{uniqueStoresCount} Store order(s)</span>
              </div>
              <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                <span style={{ color: 'var(--text-muted)' }}>Est. Shipping Total:</span>
                <span style={{ fontWeight: 'bold' }}>${totalShippingEstimate.toFixed(2)}</span>
              </div>
            </div>

            {/* Grand Total */}
            <div style={{ borderTop: '2px solid var(--border-color)', paddingTop: '1.25rem', display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '2rem' }}>
              <span style={{ fontWeight: 800, fontSize: '1.2rem' }}>Grand Total:</span>
              <span style={{ fontWeight: 800, fontSize: '1.6rem', color: 'var(--secondary-hover)' }}>${grandTotal.toFixed(2)}</span>
            </div>

            {/* Checkout Action Button */}
            <button 
              className="btn btn-primary"
              onClick={handleProceedToCheckout}
              style={{ width: '100%', gap: '0.75rem' }}
            >
              <span>Proceed to Checkout</span>
              <ArrowRight size={18} />
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Cart;

import React, { useState, useEffect } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useCart } from '../context/CartContext';
import { useAuth } from '../context/AuthContext';
import { useToast } from '../context/ToastContext';
import { useLanguage } from '../context/LanguageContext';
import apiClient from '../api/client';
import { toGovernorates } from '../api/normalizers';
import { Governorate } from '../types';
import { Trash2, ArrowLeft, ArrowRight, Store, MapPin } from 'lucide-react';

const copy = {
  ar: {
    cartTitle: "سلة المشتريات",
    emptyCart: "سلة المشتريات فارغة",
    emptyCartSub: "أضف بعض منتجات الجملة من الكتالوج للبدء.",
    goCatalog: "الذهاب للكتالوج",
    unitPrice: "سعر الوحدة:",
    removeProduct: "إزالة المنتج",
    orderSummary: "ملخص الطلب",
    estimateShipping: "تقدير الشحن إلى:",
    itemsSubtotal: "المجموع الفرعي للمنتجات:",
    storesSplit: "تقسيم المتاجر:",
    storeOrders: "طلب(أطلبات) متجر",
    estShippingTotal: "إجمالي الشحن المتوقع:",
    grandTotal: "الإجمالي الكلي:",
    checkoutBtn: "إتمام عملية الشراء",
    loginRequired: "يرجى تسجيل الدخول لإكمال طلبك.",
    generalStore: "المتجر العام"
  },
  en: {
    cartTitle: "Shopping Cart (سلة المشتريات)",
    emptyCart: "Your shopping cart is empty",
    emptyCartSub: "Add some wholesale products from the catalog to get started.",
    goCatalog: "Go to Catalog",
    unitPrice: "Unit Price:",
    removeProduct: "Remove product",
    orderSummary: "Order Summary",
    estimateShipping: "Estimate Shipping to:",
    itemsSubtotal: "Items Subtotal:",
    storesSplit: "Stores Split:",
    storeOrders: "Store order(s)",
    estShippingTotal: "Est. Shipping Total:",
    grandTotal: "Grand Total:",
    checkoutBtn: "Proceed to Checkout",
    loginRequired: "Please login to complete your order.",
    generalStore: "General Store"
  }
};

const Cart: React.FC = () => {
  const { cartItems, updateQuantity, removeFromCart, getGroupedCart, getCartTotal } = useCart();
  const { isAuthenticated, user } = useAuth();
  const { showToast } = useToast();
  const { language } = useLanguage();
  const navigate = useNavigate();

  const [governorates, setGovernorates] = useState<Governorate[]>([]);
  const [selectedGovId, setSelectedGovId] = useState<number>(user?.governorateId || 1);
  const [shippingRate, setShippingRate] = useState<number>(15); // Default flat rate per store

  const labels = copy[language as keyof typeof copy];

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
      showToast(labels.loginRequired, 'warning');
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
      <div className="main-content" style={{ padding: '4rem 2rem', textAlign: 'center', direction: language === 'ar' ? 'rtl' : 'ltr' }}>
        <span style={{ fontSize: '4rem' }}>🛒</span>
        <h2 style={{ fontSize: '1.8rem', marginTop: '1.5rem', marginBottom: '1rem' }}>{labels.emptyCart}</h2>
        <p style={{ color: 'var(--text-muted)', marginBottom: '2rem' }}>{labels.emptyCartSub}</p>
        <Link to="/" className="btn btn-primary">{labels.goCatalog}</Link>
      </div>
    );
  }

  return (
    <div className="main-content" style={{ padding: '2rem 4rem', direction: language === 'ar' ? 'rtl' : 'ltr' }}>
      <h1 style={{ fontSize: '2.2rem', marginBottom: '2rem', textAlign: language === 'ar' ? 'right' : 'left' }}>{labels.cartTitle}</h1>

      <div style={{ display: 'flex', gap: '3rem', flexWrap: 'wrap' }}>
        {/* Left Side: Cart Items grouped by store */}
        <div style={{ flex: '1 1 600px', display: 'flex', flexDirection: 'column', gap: '2rem' }}>
          {Object.entries(groupedCart).map(([storeName, items]) => (
            <div key={storeName} className="card" style={{ padding: '1.5rem', border: '1px solid var(--border-color)', textAlign: language === 'ar' ? 'right' : 'left' }}>
              {/* Store Section Header */}
              <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginBottom: '1rem', borderBottom: '1px solid var(--border-color)', paddingBottom: '0.75rem' }}>
                <Store size={18} color="var(--primary-hover)" />
                <h3 style={{ fontSize: '1.1rem', margin: 0 }}>{storeName === 'General Store' ? labels.generalStore : storeName}</h3>
              </div>

              {/* Items List */}
              <div style={{ display: 'flex', flexDirection: 'column', gap: '1.25rem' }}>
                {items.map((item) => {
                  const primaryImage = item.product.images?.find((img) => img.isPrimary)?.imageUrl
                    || item.product.images?.[0]?.imageUrl
                    || '';
                  
                  return (
                    <div key={item.product.id} style={{ display: 'flex', alignItems: 'center', gap: '1.5rem', flexWrap: 'wrap', flexDirection: language === 'ar' ? 'row-reverse' : 'row' }}>
                      {/* Thumbnail */}
                      <img 
                        src={getAbsoluteImageUrl(primaryImage)} 
                        alt={item.product.name}
                        style={{ width: '80px', height: '80px', borderRadius: 'var(--radius-md)', objectFit: 'cover', backgroundColor: '#f1f3f5' }}
                      />

                      {/* Product details */}
                      <div style={{ flex: 1, minWidth: '200px', textAlign: language === 'ar' ? 'right' : 'left' }}>
                        <Link to={`/product/${item.product.id}`} style={{ fontWeight: 'bold', fontSize: '1rem' }} className="nav-link-item">
                          {item.product.name}
                        </Link>
                        <div style={{ fontSize: '0.85rem', color: 'var(--text-muted)', marginTop: '0.2rem' }}>
                          {labels.unitPrice} <span style={{ color: 'var(--secondary-hover)', fontWeight: 'bold' }}>${item.selectedPrice}</span>
                        </div>
                      </div>

                      {/* Quantity Stepper */}
                      <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', direction: 'ltr' }}>
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
                      <div style={{ minWidth: '80px', textAlign: language === 'ar' ? 'left' : 'right', fontWeight: 'bold', fontSize: '1.1rem' }}>
                        ${(item.selectedPrice * item.quantity).toFixed(2)}
                      </div>

                      {/* Remove Button */}
                      <button 
                        onClick={() => removeFromCart(item.product.id)}
                        style={{ background: 'none', border: 'none', color: 'var(--color-danger)', cursor: 'pointer' }}
                        title={labels.removeProduct}
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
          <div className="card" style={{ padding: '2rem', border: '2px solid var(--border-color)', position: 'sticky', top: '100px', textAlign: language === 'ar' ? 'right' : 'left' }}>
            <h2 style={{ fontSize: '1.4rem', marginBottom: '1.5rem' }}>{labels.orderSummary}</h2>

            {/* Shipping Estimate Selector */}
            <div className="form-group" style={{ marginBottom: '1.5rem', borderBottom: '1px solid var(--border-color)', paddingBottom: '1.25rem' }}>
              <label className="form-label" style={{ display: 'flex', alignItems: 'center', gap: '0.3rem', justifyContent: language === 'ar' ? 'flex-start' : 'inherit' }}>
                <MapPin size={16} />
                <span>{labels.estimateShipping}</span>
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
              <div style={{ display: 'flex', justifyContent: 'space-between', flexDirection: language === 'ar' ? 'row-reverse' : 'row' }}>
                <span style={{ color: 'var(--text-muted)' }}>{labels.itemsSubtotal}</span>
                <span style={{ fontWeight: 'bold' }}>${totalItemsCost.toFixed(2)}</span>
              </div>
              <div style={{ display: 'flex', justifyContent: 'space-between', flexDirection: language === 'ar' ? 'row-reverse' : 'row' }}>
                <span style={{ color: 'var(--text-muted)' }}>{labels.storesSplit}</span>
                <span style={{ fontWeight: 'bold' }}>{uniqueStoresCount} {labels.storeOrders}</span>
              </div>
              <div style={{ display: 'flex', justifyContent: 'space-between', flexDirection: language === 'ar' ? 'row-reverse' : 'row' }}>
                <span style={{ color: 'var(--text-muted)' }}>{labels.estShippingTotal}</span>
                <span style={{ fontWeight: 'bold' }}>${totalShippingEstimate.toFixed(2)}</span>
              </div>
            </div>

            {/* Grand Total */}
            <div style={{ borderTop: '2px solid var(--border-color)', paddingTop: '1.25rem', display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '2rem', flexDirection: language === 'ar' ? 'row-reverse' : 'row' }}>
              <span style={{ fontWeight: 800, fontSize: '1.2rem' }}>{labels.grandTotal}</span>
              <span style={{ fontWeight: 800, fontSize: '1.6rem', color: 'var(--secondary-hover)' }}>${grandTotal.toFixed(2)}</span>
            </div>

            {/* Checkout Action Button */}
            <button 
              className="btn btn-primary"
              onClick={handleProceedToCheckout}
              style={{ width: '100%', gap: '0.75rem', display: 'flex', alignItems: 'center', justifyContent: 'center' }}
            >
              <span>{labels.checkoutBtn}</span>
              {language === 'ar' ? <ArrowLeft size={18} /> : <ArrowRight size={18} />}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Cart;

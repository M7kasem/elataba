import React, { useState } from 'react';
import { useLocation, useNavigate, Link } from 'react-router-dom';
import { useCart } from '../context/CartContext';
import { useAuth } from '../context/AuthContext';
import { useToast } from '../context/ToastContext';
import apiClient from '../api/client';
import { PaymentMethod, CheckoutResultDto } from '../types';
import { ShieldCheck, Calendar, MapPin, CreditCard, ChevronRight, CheckCircle } from 'lucide-react';

const Checkout: React.FC = () => {
  const { cartItems, getGroupedCart, getCartTotal, clearCart } = useCart();
  const { userId, user } = useAuth();
  const { showToast } = useToast();
  const location = useLocation();
  const navigate = useNavigate();

  // Load estimated shipping info from Cart page state
  const cartState = location.state as { selectedGovId: number; shippingRate: number } | null;
  const shippingRate = cartState?.shippingRate || 15;

  const [address, setAddress] = useState(user?.shippingAddress || '');
  const [phone, setPhone] = useState(user?.phone || '');
  const [paymentMethod, setPaymentMethod] = useState<PaymentMethod>(PaymentMethod.Cash);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [checkoutSuccess, setCheckoutSuccess] = useState<CheckoutResultDto | null>(null);

  const groupedCart = getGroupedCart();
  const uniqueStoresCount = Object.keys(groupedCart).length;
  const itemsTotal = getCartTotal();
  const shippingTotal = uniqueStoresCount * shippingRate;
  const grandTotal = itemsTotal + shippingTotal;

  const handleCheckoutSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!address.trim()) {
      showToast('Shipping address is required.', 'warning');
      return;
    }
    if (!phone.trim()) {
      showToast('Phone number is required.', 'warning');
      return;
    }

    setIsSubmitting(true);
    try {
      const payload = {
        buyerId: userId || user?.userId || 0,
        carrierId: null, // carrier is auto-assigned by governorate or optional
        shippingAddressSnapshot: address.trim(),
        paymentMethod: paymentMethod,
        items: cartItems.map((item) => ({
          productId: item.product.id,
          quantity: item.quantity,
        })),
      };

      const response = await apiClient.post('/api/Checkout', payload);
      // Response wrapper format: { statusCode, message, data: CheckoutResultDto }
      const checkoutResult = response.data?.data;
      
      if (checkoutResult) {
        setCheckoutSuccess(checkoutResult);
        clearCart(); // Success - clear user's cart
        showToast('Orders created successfully!', 'success');
      } else {
        showToast('Unexpected checkout response format.', 'error');
      }
    } catch (err) {
      console.error('Checkout error:', err);
    } finally {
      setIsSubmitting(false);
    }
  };

  // Success Confirmation Screen
  if (checkoutSuccess) {
    return (
      <div className="main-content" style={{ padding: '4rem 2rem', textAlign: 'center', maxWidth: '600px', margin: '0 auto' }}>
        <div style={{ color: 'var(--color-success)', display: 'inline-block', marginBottom: '1.5rem' }}>
          <CheckCircle size={80} />
        </div>
        <h1 style={{ fontSize: '2rem', marginBottom: '0.5rem' }}>Checkout Completed!</h1>
        <p style={{ color: 'var(--text-muted)', marginBottom: '2rem' }}>
          Your payment was processed successfully, and your cart has been split into individual store orders.
        </p>

        <div className="card" style={{ padding: '1.5rem', textAlign: 'left', marginBottom: '2rem', border: '1px solid var(--border-color)' }}>
          <div style={{ marginBottom: '1rem' }}>
            <span style={{ fontSize: '0.85rem', color: 'var(--text-muted)' }}>Checkout Reference</span>
            <div style={{ fontSize: '1.1rem', fontWeight: 'bold', color: 'var(--secondary-hover)' }}>{checkoutSuccess.checkoutReference}</div>
          </div>
          <div style={{ display: 'flex', justifyContent: 'space-between', borderTop: '1px solid var(--border-color)', paddingTop: '1rem', marginBottom: '1rem' }}>
            <span>Total Paid:</span>
            <strong style={{ fontSize: '1.2rem' }}>${(checkoutSuccess.totalAmount || grandTotal).toFixed(2)}</strong>
          </div>

          <h3 style={{ fontSize: '1rem', marginBottom: '0.75rem', borderTop: '1px solid var(--border-color)', paddingTop: '1rem' }}>
            Generated Store Orders ({checkoutSuccess.orders.length}):
          </h3>
          <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
            {checkoutSuccess.orders.map((order, idx) => (
              <div key={idx} style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.9rem', backgroundColor: 'var(--bg-main)', padding: '0.5rem 0.75rem', borderRadius: 'var(--radius-sm)' }}>
                <span><strong>Order #{order.orderId}</strong> - {order.storeName ?? 'Store order'}</span>
                <span>${order.totalAmount.toFixed(2)}</span>
              </div>
            ))}
          </div>
        </div>

        <div style={{ display: 'flex', gap: '1rem', justifyContent: 'center' }}>
          <Link to="/orders" className="btn btn-primary">Track My Orders</Link>
          <Link to="/" className="btn btn-outline">Back to Catalog</Link>
        </div>
      </div>
    );
  }

  return (
    <div className="main-content" style={{ padding: '2rem 4rem' }}>
      <h1 style={{ fontSize: '2.2rem', marginBottom: '2rem' }}>Checkout (إتمام الطلب)</h1>

      <form onSubmit={handleCheckoutSubmit} style={{ display: 'flex', gap: '3rem', flexWrap: 'wrap' }}>
        {/* Left Side: Forms */}
        <div style={{ flex: '1 1 500px', display: 'flex', flexDirection: 'column', gap: '2rem' }}>
          {/* Shipping Address Section */}
          <div className="card" style={{ padding: '1.5rem' }}>
            <h3 style={{ fontSize: '1.1rem', marginBottom: '1.5rem', display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
              <MapPin size={18} color="var(--primary-hover)" />
              <span>1. Shipping Details</span>
            </h3>

            <div className="form-group">
              <label className="form-label">Recipient Phone Number</label>
              <input
                type="text"
                className="form-control"
                value={phone}
                onChange={(e) => setPhone(e.target.value)}
                placeholder="e.g. 01012345678"
                required
              />
            </div>

            <div className="form-group">
              <label className="form-label">Full Shipping Address</label>
              <textarea
                className="form-control"
                value={address}
                onChange={(e) => setAddress(e.target.value)}
                rows={3}
                placeholder="Governorate, City, Street Name, Building No, Apartment No..."
                required
              />
            </div>
          </div>

          {/* Payment Method Section */}
          <div className="card" style={{ padding: '1.5rem' }}>
            <h3 style={{ fontSize: '1.1rem', marginBottom: '1.5rem', display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
              <CreditCard size={18} color="var(--primary-hover)" />
              <span>2. Payment Method</span>
            </h3>

            <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
              {/* Cash On Delivery */}
              <label 
                style={{
                  display: 'flex',
                  alignItems: 'center',
                  gap: '1rem',
                  padding: '1rem',
                  borderRadius: 'var(--radius-md)',
                  border: paymentMethod === PaymentMethod.Cash ? '2px solid var(--primary)' : '1px solid var(--border-color)',
                  backgroundColor: paymentMethod === PaymentMethod.Cash ? 'rgba(var(--primary-rgb), 0.02)' : 'transparent',
                  cursor: 'pointer'
                }}
              >
                <input
                  type="radio"
                  name="paymentMethod"
                  checked={paymentMethod === PaymentMethod.Cash}
                  onChange={() => setPaymentMethod(PaymentMethod.Cash)}
                  style={{ accentColor: 'var(--primary)' }}
                />
                <div>
                  <strong style={{ display: 'block' }}>Cash on Delivery (الدفع عند الاستلام)</strong>
                  <span style={{ fontSize: '0.8rem', color: 'var(--text-muted)' }}>Pay cash directly to carrier on delivery.</span>
                </div>
              </label>

              {/* Online Payment */}
              <label 
                style={{
                  display: 'flex',
                  alignItems: 'center',
                  gap: '1rem',
                  padding: '1rem',
                  borderRadius: 'var(--radius-md)',
                  border: paymentMethod === PaymentMethod.Online ? '2px solid var(--primary)' : '1px solid var(--border-color)',
                  backgroundColor: paymentMethod === PaymentMethod.Online ? 'rgba(var(--primary-rgb), 0.02)' : 'transparent',
                  cursor: 'pointer'
                }}
              >
                <input
                  type="radio"
                  name="paymentMethod"
                  checked={paymentMethod === PaymentMethod.Online}
                  onChange={() => setPaymentMethod(PaymentMethod.Online)}
                  style={{ accentColor: 'var(--primary)' }}
                />
                <div>
                  <strong style={{ display: 'block' }}>Online Card Payment (الدفع بالبطاقة)</strong>
                  <span style={{ fontSize: '0.8rem', color: 'var(--color-warning)', fontWeight: 'bold' }}>
                    ⚠️ Reserved / Coming Soon (العرض التوضيحي فقط)
                  </span>
                </div>
              </label>
            </div>
          </div>
        </div>

        {/* Right Side: Visual Grouping Summary per Store */}
        <div style={{ flex: '1 1 350px' }}>
          <div className="card" style={{ padding: '2rem', border: '2px solid var(--border-color)', position: 'sticky', top: '100px' }}>
            <h2 style={{ fontSize: '1.4rem', marginBottom: '1.5rem' }}>Order Review</h2>

            {/* Split view info banner */}
            <div style={{ display: 'flex', gap: '0.5rem', backgroundColor: 'rgba(2, 48, 71, 0.05)', padding: '0.75rem', borderRadius: 'var(--radius-sm)', marginBottom: '1.5rem', fontSize: '0.82rem', color: 'var(--text-muted)' }}>
              <ChevronRight size={16} />
              <span>
                Note: Your cart contains items from <strong>{uniqueStoresCount} different store(s)</strong>. It will be split into separate orders.
              </span>
            </div>

            {/* Itemized grouping per store */}
            <div style={{ display: 'flex', flexDirection: 'column', gap: '1.25rem', marginBottom: '1.5rem', borderBottom: '1px solid var(--border-color)', paddingBottom: '1.25rem' }}>
              {Object.entries(groupedCart).map(([storeName, items]) => {
                const storeSubtotal = items.reduce((acc, it) => acc + it.selectedPrice * it.quantity, 0);
                return (
                  <div key={storeName} style={{ fontSize: '0.9rem' }}>
                    <div style={{ display: 'flex', justifySelf: 'space-between', fontWeight: 'bold', color: 'var(--secondary)', marginBottom: '0.3rem' }}>
                      <span>{storeName}</span>
                      <span>${storeSubtotal.toFixed(2)}</span>
                    </div>
                    {items.map((it) => (
                      <div key={it.product.id} style={{ display: 'flex', justifyContent: 'space-between', color: 'var(--text-muted)', fontSize: '0.8rem', paddingLeft: '0.5rem' }}>
                        <span>{it.product.name} (x{it.quantity})</span>
                        <span>${(it.selectedPrice * it.quantity).toFixed(2)}</span>
                      </div>
                    ))}
                    <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.8rem', color: 'var(--text-muted)', paddingLeft: '0.5rem', fontStyle: 'italic', marginTop: '0.1rem' }}>
                      <span>Shipping rate</span>
                      <span>${shippingRate}</span>
                    </div>
                  </div>
                );
              })}
            </div>

            {/* Summary */}
            <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem', marginBottom: '1.5rem' }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.9rem' }}>
                <span style={{ color: 'var(--text-muted)' }}>Products Subtotal:</span>
                <span>${itemsTotal.toFixed(2)}</span>
              </div>
              <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.9rem' }}>
                <span style={{ color: 'var(--text-muted)' }}>Shipping Total:</span>
                <span>${shippingTotal.toFixed(2)}</span>
              </div>
            </div>

            <div style={{ borderTop: '2px solid var(--border-color)', paddingTop: '1.25rem', display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '2rem' }}>
              <span style={{ fontWeight: 800, fontSize: '1.2rem' }}>Grand Total:</span>
              <span style={{ fontWeight: 800, fontSize: '1.6rem', color: 'var(--secondary-hover)' }}>${grandTotal.toFixed(2)}</span>
            </div>

            <button
              type="submit"
              className="btn btn-primary"
              disabled={isSubmitting || cartItems.length === 0}
              style={{ width: '100%', gap: '0.5rem' }}
            >
              {isSubmitting ? (
                <span>Placing Orders...</span>
              ) : (
                <>
                  <ShieldCheck size={18} />
                  <span>Confirm Order (${grandTotal.toFixed(2)})</span>
                </>
              )}
            </button>
          </div>
        </div>
      </form>
    </div>
  );
};

export default Checkout;

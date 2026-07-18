import React, { useState } from 'react';
import { useLocation, useNavigate, Link } from 'react-router-dom';
import { useCart } from '../context/CartContext';
import { useAuth } from '../context/AuthContext';
import { useToast } from '../context/ToastContext';
import { useLanguage } from '../context/LanguageContext';
import apiClient from '../api/client';
import { PaymentMethod, CheckoutResultDto } from '../types';
import { ShieldCheck, Calendar, MapPin, CreditCard, ChevronRight, ChevronLeft, CheckCircle } from 'lucide-react';

const copy = {
  ar: {
    checkoutTitle: "إتمام الطلب",
    shippingDetails: "1. تفاصيل الشحن",
    recipientPhone: "رقم هاتف المستلم",
    fullShippingAddress: "عنوان الشحن بالكامل",
    addressPlaceholder: "المحافظة، المدينة، اسم الشارع، رقم المبنى، رقم الشقة...",
    paymentMethodTitle: "2. طريقة الدفع",
    cashOnDelivery: "الدفع عند الاستلام (Cash on Delivery)",
    cashOnDeliverySub: "ادفع نقداً مباشرة للمندوب عند الاستلام.",
    onlinePayment: "الدفع بالبطاقة أونلاين (Online Card Payment)",
    onlinePaymentSub: "⚠️ محجوز / قريباً (للعرض التوضيحي فقط)",
    orderReview: "مراجعة الطلب",
    splitOrderWarning: "ملاحظة: تحتوي سلتك على منتجات من {count} متاجر مختلفة. سيتم تقسيمها إلى طلبات منفصلة لكل متجر.",
    shippingRate: "تكلفة الشحن",
    productsSubtotal: "المجموع الفرعي للمنتجات:",
    shippingTotal: "إجمالي الشحن:",
    grandTotal: "الإجمالي الكلي:",
    confirmOrder: "تأكيد الطلب",
    placingOrders: "جاري تقديم الطلبات...",
    checkoutSuccessTitle: "اكتمل الدفع بنجاح!",
    checkoutSuccessSub: "تمت معالجة دفعتك بنجاح، وتم تقسيم سلتك إلى طلبات منفصلة لكل متجر.",
    checkoutRef: "مرجع عملية الشراء",
    totalPaid: "إجمالي المدفوع:",
    generatedOrders: "طلبات المتاجر التي تم إنشاؤها ({count}):",
    orderRef: "طلب #",
    trackOrders: "متابعة طلباتي",
    backCatalog: "العودة للكتالوج",
    addressRequired: "العنوان مطلوب.",
    phoneRequired: "رقم الهاتف مطلوب.",
    successMsg: "تم إنشاء الطلبات بنجاح!",
    formatError: "تنسيق استجابة الدفع غير متوقع."
  },
  en: {
    checkoutTitle: "Checkout (إتمام الطلب)",
    shippingDetails: "1. Shipping Details",
    recipientPhone: "Recipient Phone Number",
    fullShippingAddress: "Full Shipping Address",
    addressPlaceholder: "Governorate, City, Street Name, Building No, Apartment No...",
    paymentMethodTitle: "2. Payment Method",
    cashOnDelivery: "Cash on Delivery (الدفع عند الاستلام)",
    cashOnDeliverySub: "Pay cash directly to carrier on delivery.",
    onlinePayment: "Online Card Payment (الدفع بالبطاقة)",
    onlinePaymentSub: "⚠️ Reserved / Coming Soon (العرض التوضيحي فقط)",
    orderReview: "Order Review",
    splitOrderWarning: "Note: Your cart contains items from {count} different store(s). It will be split into separate orders.",
    shippingRate: "Shipping rate",
    productsSubtotal: "Products Subtotal:",
    shippingTotal: "Shipping Total:",
    grandTotal: "Grand Total:",
    confirmOrder: "Confirm Order",
    placingOrders: "Placing Orders...",
    checkoutSuccessTitle: "Checkout Completed!",
    checkoutSuccessSub: "Your payment was processed successfully, and your cart has been split into individual store orders.",
    checkoutRef: "Checkout Reference",
    totalPaid: "Total Paid:",
    generatedOrders: "Generated Store Orders ({count}):",
    orderRef: "Order #",
    trackOrders: "Track My Orders",
    backCatalog: "Back to Catalog",
    addressRequired: "Shipping address is required.",
    phoneRequired: "Phone number is required.",
    successMsg: "Orders created successfully!",
    formatError: "Unexpected checkout response format."
  }
};

const Checkout: React.FC = () => {
  const { cartItems, getGroupedCart, getCartTotal, clearCart } = useCart();
  const { userId, user } = useAuth();
  const { showToast } = useToast();
  const { language } = useLanguage();
  const location = useLocation();
  const navigate = useNavigate();

  const labels = copy[language as keyof typeof copy];

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
      showToast(labels.addressRequired, 'warning');
      return;
    }
    if (!phone.trim()) {
      showToast(labels.phoneRequired, 'warning');
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
      const checkoutResult = response.data?.data;
      
      if (checkoutResult) {
        setCheckoutSuccess(checkoutResult);
        clearCart(); // Success - clear user's cart
        showToast(labels.successMsg, 'success');
      } else {
        showToast(labels.formatError, 'error');
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
      <div className="main-content" style={{ padding: '4rem 2rem', textAlign: 'center', maxWidth: '600px', margin: '0 auto', direction: language === 'ar' ? 'rtl' : 'ltr' }}>
        <div style={{ color: 'var(--color-success)', display: 'inline-block', marginBottom: '1.5rem' }}>
          <CheckCircle size={80} />
        </div>
        <h1 style={{ fontSize: '2rem', marginBottom: '0.5rem' }}>{labels.checkoutSuccessTitle}</h1>
        <p style={{ color: 'var(--text-muted)', marginBottom: '2rem' }}>
          {labels.checkoutSuccessSub}
        </p>

        <div className="card" style={{ padding: '1.5rem', textAlign: language === 'ar' ? 'right' : 'left', marginBottom: '2rem', border: '1px solid var(--border-color)' }}>
          <div style={{ marginBottom: '1rem' }}>
            <span style={{ fontSize: '0.85rem', color: 'var(--text-muted)' }}>{labels.checkoutRef}</span>
            <div style={{ fontSize: '1.1rem', fontWeight: 'bold', color: 'var(--secondary-hover)' }}>{checkoutSuccess.checkoutReference}</div>
          </div>
          <div style={{ display: 'flex', justifyContent: 'space-between', borderTop: '1px solid var(--border-color)', paddingTop: '1rem', marginBottom: '1rem', flexDirection: language === 'ar' ? 'row-reverse' : 'row' }}>
            <span>{labels.totalPaid}</span>
            <strong style={{ fontSize: '1.2rem' }}>${(checkoutSuccess.totalAmount || grandTotal).toFixed(2)}</strong>
          </div>

          <h3 style={{ fontSize: '1rem', marginBottom: '0.75rem', borderTop: '1px solid var(--border-color)', paddingTop: '1rem' }}>
            {labels.generatedOrders.replace('{count}', String(checkoutSuccess.orders.length))}
          </h3>
          <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
            {checkoutSuccess.orders.map((order, idx) => (
              <div key={idx} style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.9rem', backgroundColor: 'var(--bg-main)', padding: '0.5rem 0.75rem', borderRadius: 'var(--radius-sm)', flexDirection: language === 'ar' ? 'row-reverse' : 'row' }}>
                <span><strong>{labels.orderRef}{order.orderId}</strong> - {order.storeName ?? 'Store order'}</span>
                <span>${order.totalAmount.toFixed(2)}</span>
              </div>
            ))}
          </div>
        </div>

        <div style={{ display: 'flex', gap: '1rem', justifyContent: 'center' }}>
          <Link to="/orders" className="btn btn-primary">{labels.trackOrders}</Link>
          <Link to="/" className="btn btn-outline">{labels.backCatalog}</Link>
        </div>
      </div>
    );
  }

  return (
    <div className="main-content" style={{ padding: '2rem 4rem', direction: language === 'ar' ? 'rtl' : 'ltr' }}>
      <h1 style={{ fontSize: '2.2rem', marginBottom: '2rem', textAlign: language === 'ar' ? 'right' : 'left' }}>{labels.checkoutTitle}</h1>

      <form onSubmit={handleCheckoutSubmit} style={{ display: 'flex', gap: '3rem', flexWrap: 'wrap' }}>
        {/* Left Side: Forms */}
        <div style={{ flex: '1 1 500px', display: 'flex', flexDirection: 'column', gap: '2rem' }}>
          {/* Shipping Address Section */}
          <div className="card" style={{ padding: '1.5rem', textAlign: language === 'ar' ? 'right' : 'left' }}>
            <h3 style={{ fontSize: '1.1rem', marginBottom: '1.5rem', display: 'flex', alignItems: 'center', gap: '0.5rem', justifyContent: language === 'ar' ? 'flex-start' : 'inherit' }}>
              <MapPin size={18} color="var(--primary-hover)" />
              <span>{labels.shippingDetails}</span>
            </h3>

            <div className="form-group">
              <label className="form-label">{labels.recipientPhone}</label>
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
              <label className="form-label">{labels.fullShippingAddress}</label>
              <textarea
                className="form-control"
                value={address}
                onChange={(e) => setAddress(e.target.value)}
                rows={3}
                placeholder={labels.addressPlaceholder}
                required
              />
            </div>
          </div>

          {/* Payment Method Section */}
          <div className="card" style={{ padding: '1.5rem', textAlign: language === 'ar' ? 'right' : 'left' }}>
            <h3 style={{ fontSize: '1.1rem', marginBottom: '1.5rem', display: 'flex', alignItems: 'center', gap: '0.5rem', justifyContent: language === 'ar' ? 'flex-start' : 'inherit' }}>
              <CreditCard size={18} color="var(--primary-hover)" />
              <span>{labels.paymentMethodTitle}</span>
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
                  cursor: 'pointer',
                  flexDirection: language === 'ar' ? 'row-reverse' : 'row',
                  textAlign: language === 'ar' ? 'right' : 'left'
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
                  <strong style={{ display: 'block' }}>{labels.cashOnDelivery}</strong>
                  <span style={{ fontSize: '0.8rem', color: 'var(--text-muted)' }}>{labels.cashOnDeliverySub}</span>
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
                  cursor: 'pointer',
                  flexDirection: language === 'ar' ? 'row-reverse' : 'row',
                  textAlign: language === 'ar' ? 'right' : 'left'
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
                  <strong style={{ display: 'block' }}>{labels.onlinePayment}</strong>
                  <span style={{ fontSize: '0.8rem', color: 'var(--text-muted)' }}>{labels.onlinePaymentSub}</span>
                </div>
              </label>
            </div>
          </div>
        </div>

        {/* Right Side: Visual Grouping Summary per Store */}
        <div style={{ flex: '1 1 350px' }}>
          <div className="card" style={{ padding: '2rem', border: '2px solid var(--border-color)', position: 'sticky', top: '100px', textAlign: language === 'ar' ? 'right' : 'left' }}>
            <h2 style={{ fontSize: '1.4rem', marginBottom: '1.5rem' }}>{labels.orderReview}</h2>

            {/* Split view info banner */}
            <div style={{ display: 'flex', gap: '0.5rem', backgroundColor: 'rgba(2, 48, 71, 0.05)', padding: '0.75rem', borderRadius: 'var(--radius-sm)', marginBottom: '1.5rem', fontSize: '0.82rem', color: 'var(--text-muted)', flexDirection: language === 'ar' ? 'row-reverse' : 'row', textAlign: language === 'ar' ? 'right' : 'left' }}>
              {language === 'ar' ? <ChevronLeft size={16} /> : <ChevronRight size={16} />}
              <span>
                {labels.splitOrderWarning.replace('{count}', String(uniqueStoresCount))}
              </span>
            </div>

            {/* Itemized grouping per store */}
            <div style={{ display: 'flex', flexDirection: 'column', gap: '1.25rem', marginBottom: '1.5rem', borderBottom: '1px solid var(--border-color)', paddingBottom: '1.25rem' }}>
              {Object.entries(groupedCart).map(([storeName, items]) => {
                const storeSubtotal = items.reduce((acc, it) => acc + it.selectedPrice * it.quantity, 0);
                return (
                  <div key={storeName} style={{ fontSize: '0.9rem' }}>
                    <div style={{ display: 'flex', justifySelf: 'space-between', fontWeight: 'bold', color: 'var(--secondary)', marginBottom: '0.3rem', flexDirection: language === 'ar' ? 'row-reverse' : 'row' }}>
                      <span>{storeName === 'General Store' ? (language === 'ar' ? 'المتجر العام' : 'General Store') : storeName}</span>
                      <span>${storeSubtotal.toFixed(2)}</span>
                    </div>
                    {items.map((it) => (
                      <div key={it.product.id} style={{ display: 'flex', justifyContent: 'space-between', color: 'var(--text-muted)', fontSize: '0.8rem', paddingLeft: language === 'ar' ? 0 : '0.5rem', paddingRight: language === 'ar' ? '0.5rem' : 0, flexDirection: language === 'ar' ? 'row-reverse' : 'row' }}>
                        <span>{it.product.name} (x{it.quantity})</span>
                        <span>${(it.selectedPrice * it.quantity).toFixed(2)}</span>
                      </div>
                    ))}
                    <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.8rem', color: 'var(--text-muted)', paddingLeft: language === 'ar' ? 0 : '0.5rem', paddingRight: language === 'ar' ? '0.5rem' : 0, fontStyle: 'italic', marginTop: '0.1rem', flexDirection: language === 'ar' ? 'row-reverse' : 'row' }}>
                      <span>{labels.shippingRate}</span>
                      <span>${shippingRate}</span>
                    </div>
                  </div>
                );
              })}
            </div>

            {/* Summary */}
            <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem', marginBottom: '1.5rem' }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.9rem', flexDirection: language === 'ar' ? 'row-reverse' : 'row' }}>
                <span style={{ color: 'var(--text-muted)' }}>{labels.productsSubtotal}</span>
                <span>${itemsTotal.toFixed(2)}</span>
              </div>
              <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.9rem', flexDirection: language === 'ar' ? 'row-reverse' : 'row' }}>
                <span style={{ color: 'var(--text-muted)' }}>{labels.shippingTotal}</span>
                <span>${shippingTotal.toFixed(2)}</span>
              </div>
            </div>

            <div style={{ borderTop: '2px solid var(--border-color)', paddingTop: '1.25rem', display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '2rem', flexDirection: language === 'ar' ? 'row-reverse' : 'row' }}>
              <span style={{ fontWeight: 800, fontSize: '1.2rem' }}>{labels.grandTotal}</span>
              <span style={{ fontWeight: 800, fontSize: '1.6rem', color: 'var(--secondary-hover)' }}>${grandTotal.toFixed(2)}</span>
            </div>

            <button
              type="submit"
              className="btn btn-primary"
              disabled={isSubmitting || cartItems.length === 0}
              style={{ width: '100%', gap: '0.5rem', display: 'flex', alignItems: 'center', justifyContent: 'center' }}
            >
              {isSubmitting ? (
                <span>{labels.placingOrders}</span>
              ) : (
                <>
                  <ShieldCheck size={18} />
                  <span>{labels.confirmOrder} (${grandTotal.toFixed(2)})</span>
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

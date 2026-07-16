import React, { useState } from 'react';
import { Order, OrderStatus } from '../../../types';
import apiClient from '../../../api/client';
import { useLanguage } from '../../../context/LanguageContext';
import { useToast } from '../../../context/ToastContext';
import { ShoppingBag, Edit3, Truck, Calendar, DollarSign, ExternalLink } from 'lucide-react';

const copy = {
  ar: {
    title: 'طلبات الزباين',
    subtitle: 'متابعة الطلبيات الواردة من المشترين وتحديث حالة الشحن والتسليم',
    orderId: 'رقم الطلب',
    buyer: 'الزبون',
    amount: 'المبلغ الإجمالي',
    status: 'حالة الطلب',
    actions: 'تحديث الحالة',
    updateStatus: 'تعديل حالة الطلب',
    statusLabel: 'اختر الحالة الجديدة للطلب:',
    trackingLabel: 'رقم تتبع الشحنة (إن وجد)',
    shippingCostLabel: 'تكلفة الشحن الفلوس ($)',
    save: 'حفظ التحديث',
    cancel: 'إلغاء',
    statusPending: 'قيد الانتظار',
    statusConfirmed: 'تم التأكيد (جاري التجهيز)',
    statusShipped: 'تم الشحن (مع المندوب)',
    statusDelivered: 'تم التسليم للزبون',
    statusCancelled: 'تم إلغاء الطلب',
    statusUnknown: 'غير معروف',
    updateSuccess: 'تم تحديث حالة الطلب بنجاح!',
    noOrders: 'لا توجد أي طلبات شراء مسجلة لمحلك بعد.',
    buyerEmail: 'بريد المشتري:',
    paymentStatus: 'حالة الدفع:',
    paid: 'تم الدفع',
    unpaid: 'لم يدفع بعد',
    itemsOrdered: 'المنتجات المطلوبة:',
    qty: 'الكمية: %{count} قطع',
    loading: 'جاري الحفظ...',
  },
  en: {
    title: 'Customer Orders',
    subtitle: 'Track incoming buyer orders and update their delivery/shipping status',
    orderId: 'Order ID',
    buyer: 'Customer',
    amount: 'Total Amount',
    status: 'Status',
    actions: 'Update Status',
    updateStatus: 'Update Order Status',
    statusLabel: 'Choose new order status:',
    trackingLabel: 'Tracking Number (if any)',
    shippingCostLabel: 'Shipping Cost ($)',
    save: 'Save Changes',
    cancel: 'Cancel',
    statusPending: 'Pending',
    statusConfirmed: 'Confirmed (Processing)',
    statusShipped: 'Shipped (In Transit)',
    statusDelivered: 'Delivered',
    statusCancelled: 'Cancelled',
    statusUnknown: 'Unknown',
    updateSuccess: 'Order status updated successfully!',
    noOrders: 'No orders received for your store yet.',
    buyerEmail: 'Buyer Email:',
    paymentStatus: 'Payment Status:',
    paid: 'Paid',
    unpaid: 'Unpaid',
    itemsOrdered: 'Items Ordered:',
    qty: 'Qty: %{count} pcs',
    loading: 'Saving...',
  }
};

interface OrderListProps {
  storeId: number;
  orders: Order[];
  onRefresh: () => void;
}

export const OrderList: React.FC<OrderListProps> = ({ storeId, orders, onRefresh }) => {
  const { language } = useLanguage();
  const { showToast } = useToast();
  const labels = copy[language];

  const [showModal, setShowModal] = useState(false);
  const [activeOrder, setActiveOrder] = useState<Order | null>(null);
  const [status, setStatus] = useState<OrderStatus>(OrderStatus.Pending);
  const [tracking, setTracking] = useState('');
  const [shippingCost, setShippingCost] = useState<number>(15);
  const [saving, setSaving] = useState(false);

  const handleEditClick = (order: Order) => {
    setActiveOrder(order);
    setStatus(order.orderStatus);
    setTracking(order.trackingNumber || '');
    setShippingCost(order.shippingCost || 15);
    setShowModal(true);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!activeOrder) return;

    setSaving(true);
    try {
      const payload = {
        status: Number(status),
        trackingNumber: tracking.trim() || null,
        carrierId: null, // carrier assigned automatically by backend service
        shippingCost: Number(shippingCost),
        paymentStatus: activeOrder.paymentStatus
      };

      await apiClient.put(`/api/Order/${activeOrder.id}/status`, payload);
      showToast(labels.updateSuccess, 'success');
      setShowModal(false);
      onRefresh();
    } catch (err) {
      console.error('Error updating order status:', err);
    } finally {
      setSaving(false);
    }
  };

  const getOrderStatusLabel = (status: OrderStatus) => {
    switch (status) {
      case OrderStatus.Pending: return labels.statusPending;
      case OrderStatus.Confirmed: return labels.statusConfirmed;
      case OrderStatus.Shipped: return labels.statusShipped;
      case OrderStatus.Delivered: return labels.statusDelivered;
      case OrderStatus.Cancelled: return labels.statusCancelled;
      default: return labels.statusUnknown;
    }
  };

  const getStatusColor = (status: OrderStatus) => {
    switch (status) {
      case OrderStatus.Pending: return '#ff9f1c'; // orange
      case OrderStatus.Confirmed: return '#00b4d8'; // blue
      case OrderStatus.Shipped: return '#fb8500'; // dark orange
      case OrderStatus.Delivered: return '#2ec4b6'; // teal/green
      case OrderStatus.Cancelled: return '#e63946'; // red
      default: return 'var(--text-muted)';
    }
  };

  return (
    <div style={{ padding: '1rem' }}>
      
      {/* Header */}
      <div style={{ marginBottom: '2rem' }}>
        <h1 style={{ fontSize: '2rem', fontWeight: 'bold', color: 'var(--secondary)' }}>{labels.title}</h1>
        <p style={{ color: 'var(--text-muted)' }}>{labels.subtitle}</p>
      </div>

      {/* Orders Table Card */}
      <div className="card" style={{ padding: 0, overflow: 'hidden' }}>
        {orders.length > 0 ? (
          <div style={{ overflowX: 'auto' }}>
            <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.95rem' }}>
              <thead>
                <tr style={{ backgroundColor: 'var(--bg-main)', borderBottom: '2px solid var(--border-color)', textAlign: 'inherit' }}>
                  <th style={{ padding: '1rem', fontWeight: '600' }}>{labels.orderId}</th>
                  <th style={{ padding: '1rem', fontWeight: '600' }}>{labels.buyer}</th>
                  <th style={{ padding: '1rem', fontWeight: '600' }}>{labels.amount}</th>
                  <th style={{ padding: '1rem', fontWeight: '600' }}>{labels.status}</th>
                  <th style={{ padding: '1rem', fontWeight: '600', textAlign: 'center' }}>{labels.actions}</th>
                </tr>
              </thead>
              <tbody>
                {orders.map((o) => (
                  <tr key={o.id} style={{ borderBottom: '1px solid var(--border-color)' }}>
                    <td style={{ padding: '1rem', fontWeight: '700' }}>#{o.id}</td>
                    <td style={{ padding: '1rem' }}>
                      <div style={{ display: 'flex', flexDirection: 'column' }}>
                        <span style={{ fontWeight: '500' }}>{o.buyerEmail || 'مشتري'}</span>
                        {o.shippingAddressSnapshot && (
                          <span style={{ fontSize: '0.8rem', color: 'var(--text-muted)', marginTop: '0.2rem' }}>
                            📍 {o.shippingAddressSnapshot}
                          </span>
                        )}
                      </div>
                    </td>
                    <td style={{ padding: '1rem', fontWeight: '700', color: 'var(--secondary)' }}>${o.totalAmount.toFixed(2)}</td>
                    <td style={{ padding: '1rem' }}>
                      <span style={{ 
                        display: 'inline-block',
                        padding: '0.25rem 0.75rem', 
                        borderRadius: 'var(--radius-pill)', 
                        fontSize: '0.85rem', 
                        fontWeight: '600',
                        backgroundColor: `${getStatusColor(o.orderStatus)}15`,
                        color: getStatusColor(o.orderStatus)
                      }}>
                        {getOrderStatusLabel(o.orderStatus)}
                      </span>
                    </td>
                    <td style={{ padding: '1rem', textAlign: 'center' }}>
                      <button 
                        className="btn btn-outline btn-sm"
                        onClick={() => handleEditClick(o)}
                        style={{ padding: '0.4rem 0.8rem', fontSize: '0.85rem', display: 'inline-flex', gap: '0.3rem' }}
                      >
                        <Edit3 size={14} />
                        <span>{labels.actions}</span>
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : (
          <div style={{ padding: '4rem 2rem', textAlign: 'center', color: 'var(--text-muted)' }}>
            <ShoppingBag size={48} style={{ marginBottom: '1rem', color: 'var(--text-muted)', opacity: 0.5 }} />
            <p style={{ fontSize: '1.1rem', fontWeight: '600', margin: 0 }}>{labels.noOrders}</p>
          </div>
        )}
      </div>

      {/* Edit Status Modal */}
      {showModal && activeOrder && (
        <div style={{
          position: 'fixed',
          top: 0,
          left: 0,
          width: '100%',
          height: '100%',
          backgroundColor: 'rgba(0,0,0,0.5)',
          zIndex: 1000,
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center',
          padding: '1rem'
        }}>
          <div className="card" style={{ width: '100%', maxWidth: '550px', maxHeight: '90vh', overflowY: 'auto', padding: '1.5rem' }}>
            
            {/* Modal Title */}
            <div style={{ 
              display: 'flex', 
              justifyContent: 'space-between', 
              alignItems: 'center', 
              marginBottom: '1.5rem', 
              borderBottom: '1px solid var(--border-color)', 
              paddingBottom: '0.75rem' 
            }}>
              <h3 style={{ fontSize: '1.25rem', fontWeight: 'bold', color: 'var(--secondary)' }}>
                {labels.updateStatus} (#{activeOrder.id})
              </h3>
              <button 
                onClick={() => setShowModal(false)}
                style={{ background: 'none', border: 'none', fontSize: '1.5rem', cursor: 'pointer', color: 'var(--text-muted)' }}
              >
                &times;
              </button>
            </div>

            {/* Order Brief Info */}
            <div style={{
              backgroundColor: 'var(--bg-main)',
              padding: '1rem',
              borderRadius: 'var(--radius-md)',
              marginBottom: '1.5rem',
              fontSize: '0.9rem',
              display: 'flex',
              flexDirection: 'column',
              gap: '0.5rem'
            }}>
              <div><strong>{labels.buyerEmail}</strong> {activeOrder.buyerEmail || 'N/A'}</div>
              <div><strong>{labels.paymentStatus}</strong> {activeOrder.paymentStatus === 1 ? labels.paid : labels.unpaid}</div>
              
              {/* Order Items list */}
              {activeOrder.orderItems && activeOrder.orderItems.length > 0 && (
                <div style={{ borderTop: '1px solid var(--border-color)', paddingTop: '0.5rem', marginTop: '0.2rem' }}>
                  <strong>{labels.itemsOrdered}</strong>
                  <ul style={{ paddingLeft: '1.2rem', paddingRight: '1.2rem', marginTop: '0.25rem' }}>
                    {activeOrder.orderItems.map((item, idx) => (
                      <li key={idx} style={{ color: 'var(--text-main)' }}>
                        {item.productName || `Product #${item.productId}`} - <span style={{ color: 'var(--text-muted)' }}>
                          {labels.qty.replace('{count}', String(item.quantity))}
                        </span>
                      </li>
                    ))}
                  </ul>
                </div>
              )}
            </div>

            {/* Modal Form */}
            <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '1.2rem' }}>
              
              <div className="form-group">
                <label className="form-label">{labels.statusLabel}</label>
                <select 
                  className="form-control"
                  value={status}
                  onChange={(e) => setStatus(Number(e.target.value) as OrderStatus)}
                  style={{ padding: '0.6rem', fontSize: '0.95rem' }}
                >
                  <option value={OrderStatus.Pending}>{labels.statusPending}</option>
                  <option value={OrderStatus.Confirmed}>{labels.statusConfirmed}</option>
                  <option value={OrderStatus.Shipped}>{labels.statusShipped}</option>
                  <option value={OrderStatus.Delivered}>{labels.statusDelivered}</option>
                  <option value={OrderStatus.Cancelled}>{labels.statusCancelled}</option>
                </select>
              </div>

              <div className="form-group">
                <label className="form-label">{labels.trackingLabel}</label>
                <input 
                  type="text" 
                  className="form-control" 
                  value={tracking}
                  onChange={(e) => setTracking(e.target.value)}
                  placeholder="EX123456789EG"
                />
              </div>

              <div className="form-group">
                <label className="form-label">{labels.shippingCostLabel}</label>
                <input 
                  type="number" 
                  className="form-control" 
                  value={shippingCost}
                  onChange={(e) => setShippingCost(Number(e.target.value))}
                  required 
                />
              </div>

              {/* Form Controls */}
              <div style={{ display: 'flex', gap: '1rem', marginTop: '1rem', justifyContent: 'flex-end' }}>
                <button 
                  type="button" 
                  className="btn btn-outline"
                  onClick={() => setShowModal(false)}
                >
                  {labels.cancel}
                </button>
                <button 
                  type="submit" 
                  className="btn btn-primary"
                  disabled={saving}
                >
                  {saving ? labels.loading : labels.save}
                </button>
              </div>

            </form>
          </div>
        </div>
      )}

    </div>
  );
};

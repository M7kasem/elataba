import React, { useState, useEffect } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import apiClient from '../api/client';
import { toOrders } from '../api/normalizers';
import { Order, OrderStatus, PaymentStatus } from '../types';
import { useToast } from '../context/ToastContext';
import { Eye, Calendar, DollarSign, Package } from 'lucide-react';

const Orders: React.FC = () => {
  const [orders, setOrders] = useState<Order[]>([]);
  const [loading, setLoading] = useState(true);
  const [statusFilter, setStatusFilter] = useState<string>('all');
  const { showToast } = useToast();
  const navigate = useNavigate();

  useEffect(() => {
    const fetchOrders = async () => {
      setLoading(true);
      try {
        const response = await apiClient.get('/api/Order');
        // Response shape: { statusCode, message, data: OrderDto[] }
        setOrders(toOrders(response.data?.data || []));
      } catch (err) {
        console.error('Error fetching buyer orders:', err);
        showToast('Failed to load your orders.', 'error');
      } finally {
        setLoading(false);
      }
    };
    fetchOrders();
  }, [showToast]);

  const getStatusBadgeClass = (status: OrderStatus) => {
    switch (status) {
      case OrderStatus.Pending: return 'badge-pending';
      case OrderStatus.Confirmed: return 'badge-confirmed';
      case OrderStatus.Shipped: return 'badge-shipped';
      case OrderStatus.Delivered: return 'badge-delivered';
      case OrderStatus.Cancelled: return 'badge-cancelled';
      default: return '';
    }
  };

  const getStatusLabel = (status: OrderStatus) => {
    switch (status) {
      case OrderStatus.Pending: return 'Pending (قيد الانتظار)';
      case OrderStatus.Confirmed: return 'Confirmed (تم التأكيد)';
      case OrderStatus.Shipped: return 'Shipped (تم الشحن)';
      case OrderStatus.Delivered: return 'Delivered (تم التوصيل)';
      case OrderStatus.Cancelled: return 'Cancelled (ملغي)';
      default: return 'Unknown';
    }
  };

  const getPaymentStatusLabel = (status: PaymentStatus) => {
    switch (status) {
      case PaymentStatus.Pending: return 'Pending';
      case PaymentStatus.Paid: return 'Paid (مدفوع)';
      case PaymentStatus.Failed: return 'Failed';
      default: return 'Unknown';
    }
  };

  const filteredOrders = statusFilter === 'all'
    ? orders
    : orders.filter((o) => o.orderStatus === Number(statusFilter));

  return (
    <div className="main-content" style={{ padding: '2rem 4rem' }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '2rem' }}>
        <h1 style={{ fontSize: '2.2rem', margin: 0 }}>My Orders (طلباتي)</h1>
        
        {/* Status Filter Dropdown */}
        <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
          <span style={{ fontSize: '0.9rem', fontWeight: 600, color: 'var(--text-muted)' }}>Status:</span>
          <select
            className="form-control"
            value={statusFilter}
            onChange={(e) => setStatusFilter(e.target.value)}
            style={{ padding: '0.5rem 1rem', width: '200px' }}
          >
            <option value="all">All Orders (جميع الحالات)</option>
            <option value={String(OrderStatus.Pending)}>Pending</option>
            <option value={String(OrderStatus.Confirmed)}>Confirmed</option>
            <option value={String(OrderStatus.Shipped)}>Shipped</option>
            <option value={String(OrderStatus.Delivered)}>Delivered</option>
            <option value={String(OrderStatus.Cancelled)}>Cancelled</option>
          </select>
        </div>
      </div>

      {loading ? (
        <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
          {Array.from({ length: 4 }).map((_, idx) => (
            <div key={idx} className="skeleton" style={{ width: '100%', height: '80px' }} />
          ))}
        </div>
      ) : filteredOrders.length > 0 ? (
        <div className="card" style={{ padding: 0, overflow: 'hidden' }}>
          <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left' }}>
            <thead>
              <tr style={{ borderBottom: '2px solid var(--border-color)', backgroundColor: 'var(--bg-main)', fontSize: '0.9rem', fontWeight: 'bold' }}>
                <th style={{ padding: '1rem' }}>Order ID</th>
                <th style={{ padding: '1rem' }}>Store Name</th>
                <th style={{ padding: '1rem' }}>Date</th>
                <th style={{ padding: '1rem' }}>Total Amount</th>
                <th style={{ padding: '1rem' }}>Payment Status</th>
                <th style={{ padding: '1rem' }}>Order Status</th>
                <th style={{ padding: '1rem', textAlign: 'center' }}>Actions</th>
              </tr>
            </thead>
            <tbody>
              {filteredOrders.map((order) => (
                <tr key={order.id} style={{ borderBottom: '1px solid var(--border-color)', fontSize: '0.95rem' }}>
                  <td style={{ padding: '1rem', fontWeight: 'bold' }}>#{order.id}</td>
                  <td style={{ padding: '1rem' }}>
                    <div style={{ display: 'flex', alignItems: 'center', gap: '0.4rem' }}>
                      <Package size={16} color="var(--text-muted)" />
                      <span>{order.storeName || `Store #${order.storeId}`}</span>
                    </div>
                  </td>
                  <td style={{ padding: '1rem' }}>
                    <div style={{ display: 'flex', alignItems: 'center', gap: '0.4rem', fontSize: '0.85rem', color: 'var(--text-muted)' }}>
                      <Calendar size={14} />
                      <span>{new Date(order.orderDate).toLocaleDateString()}</span>
                    </div>
                  </td>
                  <td style={{ padding: '1rem', fontWeight: 'bold', color: 'var(--secondary-hover)' }}>
                    ${order.totalAmount.toFixed(2)}
                  </td>
                  <td style={{ padding: '1rem' }}>
                    <span style={{ fontSize: '0.85rem', fontWeight: 600 }}>
                      {getPaymentStatusLabel(order.paymentStatus)}
                    </span>
                  </td>
                  <td style={{ padding: '1rem' }}>
                    <span className={`badge ${getStatusBadgeClass(order.orderStatus)}`}>
                      {getStatusLabel(order.orderStatus)}
                    </span>
                  </td>
                  <td style={{ padding: '1rem', textAlign: 'center' }}>
                    <Link 
                      to={`/order/${order.id}`} 
                      className="btn btn-outline btn-sm"
                      style={{ padding: '0.4rem 0.6rem', display: 'inline-flex', alignItems: 'center', gap: '0.3rem' }}
                    >
                      <Eye size={14} />
                      <span>View</span>
                    </Link>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      ) : (
        <div className="card" style={{ padding: '4rem 2rem', textAlign: 'center' }}>
          <span style={{ fontSize: '3rem' }}>📦</span>
          <h3 style={{ fontSize: '1.4rem', marginTop: '1rem', color: 'var(--text-muted)' }}>No orders found</h3>
          <p style={{ color: 'var(--text-muted)' }}>You haven't placed any orders with this status filter yet.</p>
        </div>
      )}
    </div>
  );
};

export default Orders;

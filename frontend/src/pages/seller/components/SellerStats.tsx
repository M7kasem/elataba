import React from 'react';
import { Product, Order, Offer, OrderStatus } from '../../../types';
import { useLanguage } from '../../../context/LanguageContext';

const copy = {
  ar: {
    title: 'نظرة عامة على المحل',
    subtitle: 'متابعة سريعة لحالة البضاعة والطلبات والعروض في محلك',
    listedProducts: 'المنتجات المعروضة',
    storeOrders: 'طلبات الزباين',
    activeOffers: 'العروض الشغالة',
    recentOrders: 'آخر الطلبيات الواردة',
    orderId: 'رقم الطلب',
    buyer: 'الزبون',
    amount: 'المبلغ الإجمالي',
    status: 'حالة الطلب',
    noOrders: 'لا توجد طلبات لمحلك حتى الآن.',
    statusPending: 'قيد الانتظار',
    statusConfirmed: 'تم التأكيد',
    statusShipped: 'تم الشحن',
    statusDelivered: 'تم التسليم',
    statusCancelled: 'ملغي',
    statusUnknown: 'غير معروف',
  },
  en: {
    title: 'Store Overview',
    subtitle: 'Quick overview of products, orders, and active offers in your store',
    listedProducts: 'Listed Products',
    storeOrders: 'Customer Orders',
    activeOffers: 'Active Offers',
    recentOrders: 'Recent Orders',
    orderId: 'Order ID',
    buyer: 'Customer',
    amount: 'Total Amount',
    status: 'Status',
    noOrders: 'No orders placed for your store yet.',
    statusPending: 'Pending',
    statusConfirmed: 'Confirmed',
    statusShipped: 'Shipped',
    statusDelivered: 'Delivered',
    statusCancelled: 'Cancelled',
    statusUnknown: 'Unknown',
  }
};

interface SellerStatsProps {
  products: Product[];
  orders: Order[];
  offers: Offer[];
}

export const SellerStats: React.FC<SellerStatsProps> = ({ products, orders, offers }) => {
  const { language } = useLanguage();
  const labels = copy[language];

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
      <div style={{ marginBottom: '2rem' }}>
        <h1 style={{ fontSize: '2rem', fontWeight: 'bold', color: 'var(--secondary)' }}>{labels.title}</h1>
        <p style={{ color: 'var(--text-muted)' }}>{labels.subtitle}</p>
      </div>

      {/* Metrics Cards */}
      <div style={{ 
        display: 'grid', 
        gridTemplateColumns: 'repeat(auto-fit, minmax(240px, 1fr))', 
        gap: '1.5rem', 
        marginBottom: '2.5rem' 
      }}>
        
        <div className="card" style={{ padding: '1.5rem', display: 'flex', alignItems: 'center', gap: '1.5rem', borderLeft: '5px solid var(--primary)' }}>
          <div style={{ fontSize: '2.5rem', padding: '0.5rem', background: 'rgba(255, 183, 3, 0.1)', borderRadius: 'var(--radius-md)' }}>📦</div>
          <div>
            <span style={{ fontSize: '0.9rem', color: 'var(--text-muted)', fontWeight: '500' }}>{labels.listedProducts}</span>
            <h3 style={{ fontSize: '2rem', fontWeight: 'bold', margin: '0.2rem 0 0 0' }}>{products.length}</h3>
          </div>
        </div>

        <div className="card" style={{ padding: '1.5rem', display: 'flex', alignItems: 'center', gap: '1.5rem', borderLeft: '5px solid var(--color-success)' }}>
          <div style={{ fontSize: '2.5rem', padding: '0.5rem', background: 'rgba(46, 196, 182, 0.1)', borderRadius: 'var(--radius-md)' }}>🛍️</div>
          <div>
            <span style={{ fontSize: '0.9rem', color: 'var(--text-muted)', fontWeight: '500' }}>{labels.storeOrders}</span>
            <h3 style={{ fontSize: '2rem', fontWeight: 'bold', margin: '0.2rem 0 0 0' }}>{orders.length}</h3>
          </div>
        </div>

        <div className="card" style={{ padding: '1.5rem', display: 'flex', alignItems: 'center', gap: '1.5rem', borderLeft: '5px solid var(--color-warning)' }}>
          <div style={{ fontSize: '2.5rem', padding: '0.5rem', background: 'rgba(255, 159, 28, 0.1)', borderRadius: 'var(--radius-md)' }}>🏷️</div>
          <div>
            <span style={{ fontSize: '0.9rem', color: 'var(--text-muted)', fontWeight: '500' }}>{labels.activeOffers}</span>
            <h3 style={{ fontSize: '2rem', fontWeight: 'bold', margin: '0.2rem 0 0 0' }}>{offers.length}</h3>
          </div>
        </div>

      </div>

      {/* Recent Orders */}
      <div className="card" style={{ padding: '1.5rem' }}>
        <h3 style={{ fontSize: '1.25rem', fontWeight: 'bold', marginBottom: '1.2rem', color: 'var(--secondary)' }}>{labels.recentOrders}</h3>
        {orders.length > 0 ? (
          <div style={{ overflowX: 'auto' }}>
            <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.95rem' }}>
              <thead>
                <tr style={{ borderBottom: '2px solid var(--border-color)', textAlign: 'inherit', backgroundColor: 'var(--bg-main)' }}>
                  <th style={{ padding: '1rem', fontWeight: '600' }}>{labels.orderId}</th>
                  <th style={{ padding: '1rem', fontWeight: '600' }}>{labels.buyer}</th>
                  <th style={{ padding: '1rem', fontWeight: '600' }}>{labels.amount}</th>
                  <th style={{ padding: '1rem', fontWeight: '600' }}>{labels.status}</th>
                </tr>
              </thead>
              <tbody>
                {orders.slice(0, 5).map((o) => (
                  <tr key={o.id} style={{ borderBottom: '1px solid var(--border-color)' }}>
                    <td style={{ padding: '1rem', fontWeight: '700' }}>#{o.id}</td>
                    <td style={{ padding: '1rem', color: 'var(--text-main)' }}>{o.buyerEmail || 'مشتري'}</td>
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
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        ) : (
          <p style={{ color: 'var(--text-muted)', textAlign: 'center', padding: '2rem 0' }}>{labels.noOrders}</p>
        )}
      </div>
    </div>
  );
};

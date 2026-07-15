import React, { useState, useEffect } from 'react';
import { useParams, useNavigate, Link } from 'react-router-dom';
import apiClient from '../api/client';
import { Order, OrderStatus, PaymentStatus, PaymentMethod } from '../types';
import { useAuth } from '../context/AuthContext';
import { useToast } from '../context/ToastContext';
import { ArrowLeft, Calendar, FileText, CheckCircle, Package, Truck, Check, Star } from 'lucide-react';

const OrderDetail: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { userId } = useAuth();
  const { showToast } = useToast();

  const [order, setOrder] = useState<Order | null>(null);
  const [loading, setLoading] = useState(true);

  // Review Form States
  const [showReviewForm, setShowReviewForm] = useState(false);
  const [rating, setRating] = useState<number>(5);
  const [comment, setComment] = useState<string>('');
  const [isSubmittingReview, setIsSubmittingReview] = useState(false);
  const [hasReviewed, setHasReviewed] = useState(false);

  useEffect(() => {
    const fetchOrderDetail = async () => {
      setLoading(true);
      try {
        const response = await apiClient.get(`/api/Order/${id}`);
        const fetchedOrder = response.data?.data;
        if (fetchedOrder) {
          setOrder(fetchedOrder);
          setHasReviewed(fetchedOrder.isReviewed || false);
        } else {
          showToast('Order not found.', 'error');
          navigate('/orders');
        }
      } catch (err) {
        console.error('Error fetching order details:', err);
        showToast('Error loading order details.', 'error');
        navigate('/orders');
      } finally {
        setLoading(false);
      }
    };

    if (id) {
      fetchOrderDetail();
    }
  }, [id, navigate, showToast]);

  const handleReviewSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!order) return;

    if (comment.trim().length < 5) {
      showToast('Comment must be at least 5 characters long.', 'warning');
      return;
    }

    setIsSubmittingReview(true);
    try {
      const payload = {
        orderId: order.id,
        storeId: order.storeId,
        buyerId: userId || 0,
        rating: rating,
        comment: comment.trim()
      };

      await apiClient.post('/api/Review', payload);
      showToast('Review submitted successfully!', 'success');
      setHasReviewed(true);
      setShowReviewForm(false);
    } catch (err) {
      console.error('Error submitting review:', err);
    } finally {
      setIsSubmittingReview(false);
    }
  };

  if (loading) {
    return (
      <div className="main-content" style={{ padding: '4rem' }}>
        <div className="skeleton" style={{ width: '200px', height: '30px', marginBottom: '2rem' }} />
        <div className="skeleton" style={{ width: '100%', height: '400px' }} />
      </div>
    );
  }

  if (!order) return null;

  const isDelivered = order.orderStatus === OrderStatus.Delivered;
  const isCancelled = order.orderStatus === OrderStatus.Cancelled;

  // Status timeline steps
  const steps = [
    { label: 'Pending', status: OrderStatus.Pending },
    { label: 'Confirmed', status: OrderStatus.Confirmed },
    { label: 'Shipped', status: OrderStatus.Shipped },
    { label: 'Delivered', status: OrderStatus.Delivered },
  ];

  const getStepStatusClass = (stepStatus: OrderStatus) => {
    if (isCancelled) return 'timeline-inactive';
    if (order.orderStatus >= stepStatus) return 'timeline-active';
    return 'timeline-inactive';
  };

  const getAbsoluteImageUrl = (url: string) => {
    if (!url) return 'https://via.placeholder.com/100';
    if (url.startsWith('http://') || url.startsWith('https://') || url.startsWith('data:')) {
      return url;
    }
    return `http://localhost:5191${url.startsWith('/') ? '' : '/'}${url}`;
  };

  return (
    <div className="main-content" style={{ padding: '2rem 4rem' }}>
      <button 
        className="btn btn-outline" 
        onClick={() => navigate('/orders')} 
        style={{ marginBottom: '2rem', display: 'inline-flex', alignItems: 'center', gap: '0.5rem' }}
      >
        <ArrowLeft size={16} />
        <span>Back to Orders</span>
      </button>

      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-start', flexWrap: 'wrap', gap: '1rem', marginBottom: '2rem' }}>
        <div>
          <h1 style={{ fontSize: '2rem', margin: 0, display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
            <FileText size={28} />
            <span>Order #{order.id}</span>
          </h1>
          <div style={{ fontSize: '0.9rem', color: 'var(--text-muted)', marginTop: '0.5rem', display: 'flex', alignItems: 'center', gap: '1rem' }}>
            <span style={{ display: 'flex', alignItems: 'center', gap: '0.3rem' }}>
              <Calendar size={14} />
              {new Date(order.orderDate).toLocaleString()}
            </span>
            <span>Store: <strong>{order.storeName || `Store #${order.storeId}`}</strong></span>
          </div>
        </div>

        {/* Purchase gated review button */}
        {isDelivered && !hasReviewed && (
          <button 
            className="btn btn-primary"
            onClick={() => setShowReviewForm(!showReviewForm)}
          >
            Leave a Review (تقييم الطلب)
          </button>
        )}
      </div>

      {/* Review Form Container */}
      {showReviewForm && (
        <div className="card" style={{ padding: '1.5rem', marginBottom: '2rem', border: '2px solid var(--primary)' }}>
          <h3 style={{ fontSize: '1.1rem', marginBottom: '1rem' }}>Leave a Review for this Order</h3>
          <form onSubmit={handleReviewSubmit}>
            <div className="form-group" style={{ marginBottom: '1rem' }}>
              <label className="form-label">Rating (التقييم)</label>
              <div style={{ display: 'flex', gap: '0.5rem' }}>
                {[1, 2, 3, 4, 5].map((star) => (
                  <button
                    key={star}
                    type="button"
                    onClick={() => setRating(star)}
                    style={{ background: 'none', border: 'none', cursor: 'pointer', color: 'var(--color-warning)' }}
                  >
                    <Star size={24} fill={rating >= star ? 'var(--color-warning)' : 'none'} />
                  </button>
                ))}
              </div>
            </div>

            <div className="form-group" style={{ marginBottom: '1rem' }}>
              <label className="form-label">Comment (التعليق)</label>
              <textarea
                className="form-control"
                value={comment}
                onChange={(e) => setComment(e.target.value)}
                placeholder="Share your experience with this seller's products..."
                rows={3}
                required
              />
            </div>

            <div style={{ display: 'flex', gap: '0.75rem' }}>
              <button type="submit" className="btn btn-primary btn-sm" disabled={isSubmittingReview}>
                {isSubmittingReview ? 'Submitting...' : 'Submit Review'}
              </button>
              <button type="button" className="btn btn-outline btn-sm" onClick={() => setShowReviewForm(false)}>
                Cancel
              </button>
            </div>
          </form>
        </div>
      )}

      {/* Tracking Timeline Status Section */}
      <div className="card" style={{ padding: '2rem', marginBottom: '2rem' }}>
        <h3 style={{ fontSize: '1.1rem', marginBottom: '1.5rem' }}>Order Tracking Timeline</h3>
        
        {isCancelled ? (
          <div style={{ color: 'var(--color-danger)', fontWeight: 'bold', display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
            <span>Order Cancelled (الطلب ملغي)</span>
          </div>
        ) : (
          <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', position: 'relative', overflowX: 'auto', padding: '1rem 0' }}>
            {steps.map((step, index) => {
              const activeClass = getStepStatusClass(step.status);
              const isActive = activeClass === 'timeline-active';

              return (
                <div key={index} style={{ display: 'flex', flexDirection: 'column', alignItems: 'center', zIndex: 2, flex: 1, minWidth: '80px' }}>
                  <div style={{
                    width: '32px',
                    height: '32px',
                    borderRadius: '50%',
                    backgroundColor: isActive ? 'var(--primary)' : 'var(--bg-main)',
                    border: '2px solid ' + (isActive ? 'var(--primary)' : 'var(--border-color)'),
                    color: isActive ? '#023047' : 'var(--text-muted)',
                    display: 'flex',
                    alignItems: 'center',
                    justifyContent: 'center',
                    fontWeight: 'bold',
                    marginBottom: '0.5rem'
                  }}>
                    {isActive ? <Check size={16} /> : index + 1}
                  </div>
                  <span style={{ fontSize: '0.85rem', fontWeight: isActive ? 700 : 500 }}>
                    {step.label}
                  </span>
                </div>
              );
            })}
          </div>
        )}
      </div>

      <div style={{ display: 'flex', gap: '2rem', flexWrap: 'wrap' }}>
        {/* Left Panel: Items List */}
        <div style={{ flex: '1 1 500px', display: 'flex', flexDirection: 'column', gap: '1.5rem' }}>
          <div className="card" style={{ padding: '1.5rem' }}>
            <h3 style={{ fontSize: '1.1rem', marginBottom: '1rem', display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
              <Package size={18} color="var(--primary-hover)" />
              <span>Order Items</span>
            </h3>

            <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem' }}>
              {order.orderItems?.map((item) => (
                <div key={item.id} style={{ display: 'flex', alignItems: 'center', gap: '1rem', borderBottom: '1px solid var(--border-color)', paddingBottom: '1rem' }}>
                  <img 
                    src={getAbsoluteImageUrl(item.productImageUrl || '')} 
                    alt={item.productName}
                    style={{ width: '60px', height: '60px', borderRadius: 'var(--radius-md)', objectFit: 'cover', backgroundColor: '#f1f3f5' }}
                  />
                  <div style={{ flex: 1 }}>
                    <Link to={`/product/${item.productId}`} style={{ fontWeight: 'bold', fontSize: '0.95rem' }} className="nav-link-item">
                      {item.productName}
                    </Link>
                    <div style={{ fontSize: '0.8rem', color: 'var(--text-muted)', marginTop: '0.2rem' }}>
                      ${item.unitPrice} &times; {item.quantity}
                    </div>
                  </div>
                  <div style={{ fontWeight: 'bold' }}>
                    ${item.subtotal.toFixed(2)}
                  </div>
                </div>
              ))}
            </div>
          </div>
        </div>

        {/* Right Panel: Delivery & Costs summary */}
        <div style={{ flex: '1 1 300px', display: 'flex', flexDirection: 'column', gap: '1.5rem' }}>
          {/* Shipping snapshot */}
          <div className="card" style={{ padding: '1.5rem' }}>
            <h3 style={{ fontSize: '1.1rem', marginBottom: '1rem', display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
              <Truck size={18} color="var(--primary-hover)" />
              <span>Delivery Details</span>
            </h3>

            <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem', fontSize: '0.9rem' }}>
              <div>
                <span style={{ color: 'var(--text-muted)', display: 'block' }}>Address:</span>
                <strong>{order.shippingAddressSnapshot}</strong>
              </div>
              
              {order.trackingNumber && (
                <div>
                  <span style={{ color: 'var(--text-muted)', display: 'block' }}>Tracking Number:</span>
                  <strong style={{ color: 'var(--color-success)' }}>{order.trackingNumber}</strong>
                </div>
              )}

              {order.carrierName && (
                <div>
                  <span style={{ color: 'var(--text-muted)', display: 'block' }}>Carrier:</span>
                  <strong>{order.carrierName}</strong>
                </div>
              )}
            </div>
          </div>

          {/* Pricing Totals */}
          <div className="card" style={{ padding: '1.5rem' }}>
            <h3 style={{ fontSize: '1.1rem', marginBottom: '1rem' }}>Cost Breakdown</h3>
            
            <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem', fontSize: '0.9rem', marginBottom: '1rem', borderBottom: '1px solid var(--border-color)', paddingBottom: '1rem' }}>
              <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                <span style={{ color: 'var(--text-muted)' }}>Items Subtotal:</span>
                <span>${(order.totalAmount - order.shippingCost).toFixed(2)}</span>
              </div>
              <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                <span style={{ color: 'var(--text-muted)' }}>Shipping Cost:</span>
                <span>${order.shippingCost.toFixed(2)}</span>
              </div>
              <div style={{ display: 'flex', justifyContent: 'space-between' }}>
                <span style={{ color: 'var(--text-muted)' }}>Payment Method:</span>
                <strong>{order.paymentMethod === PaymentMethod.Cash ? 'Cash' : 'Online card'}</strong>
              </div>
            </div>

            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
              <span style={{ fontWeight: 700, fontSize: '1rem' }}>Total Cost:</span>
              <strong style={{ fontSize: '1.4rem', color: 'var(--secondary-hover)' }}>${order.totalAmount.toFixed(2)}</strong>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default OrderDetail;

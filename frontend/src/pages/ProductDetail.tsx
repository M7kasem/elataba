import React, { useState, useEffect } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import apiClient from '../api/client';
import { toProduct } from '../api/normalizers';
import { Product, Role, Review } from '../types';
import { useCart } from '../context/CartContext';
import { useAuth } from '../context/AuthContext';
import { useToast } from '../context/ToastContext';
import { ShoppingCart, Store, ArrowLeft, Star, MessageSquare } from 'lucide-react';

const ProductDetail: React.FC = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { addToCart, getItemPrice } = useCart();
  const { role, isAuthenticated, userId } = useAuth();
  const { showToast } = useToast();

  const [product, setProduct] = useState<Product | null>(null);
  const [reviews, setReviews] = useState<Review[]>([]);
  const [loading, setLoading] = useState(true);
  const [quantity, setQuantity] = useState(1);
  const [selectedImage, setSelectedImage] = useState<string>('');
  const [showAllTiers, setShowAllTiers] = useState(false);

  useEffect(() => {
    const fetchProductDetails = async () => {
      setLoading(true);
      try {
        const [prodRes, reviewsRes] = await Promise.all([
          apiClient.get(`/api/Product/${id}`),
          apiClient.get(`/api/Review/product/${id}`).catch(() => ({ data: { data: [] } }))
        ]);

        const fetchedProduct = prodRes.data?.data ? toProduct(prodRes.data.data) : null;
        if (fetchedProduct) {
          setProduct(fetchedProduct);
          setReviews(reviewsRes.data?.data || []);

          const primaryImg = fetchedProduct.images?.find((img: any) => img.isPrimary)?.imageUrl
            || fetchedProduct.images?.[0]?.imageUrl
            || 'https://via.placeholder.com/600?text=No+Image';
          setSelectedImage(primaryImg);
        } else {
          showToast('Product not found.', 'error');
          navigate('/');
        }
      } catch (err) {
        console.error('Error fetching product details:', err);
        showToast('Error loading product details.', 'error');
        navigate('/');
      } finally {
        setLoading(false);
      }
    };

    if (id) {
      fetchProductDetails();
    }
  }, [id, navigate, showToast]);

  if (loading) {
    return (
      <div className="main-content" style={{ padding: '4rem' }}>
        <div style={{ display: 'flex', gap: '3rem', flexDirection: 'row', flexWrap: 'wrap' }}>
          <div className="skeleton" style={{ width: '450px', height: '450px', flex: '1 1 400px' }} />
          <div style={{ flex: '1 1 400px', display: 'flex', flexDirection: 'column', gap: '1rem' }}>
            <div className="skeleton" style={{ width: '30%', height: '14px' }} />
            <div className="skeleton" style={{ width: '80%', height: '32px' }} />
            <div className="skeleton" style={{ width: '40%', height: '24px', marginTop: '1rem' }} />
            <div className="skeleton" style={{ width: '100%', height: '100px', marginTop: '1rem' }} />
          </div>
        </div>
      </div>
    );
  }

  if (!product) return null;

  const getAbsoluteImageUrl = (url: string) => {
    if (url.startsWith('http://') || url.startsWith('https://') || url.startsWith('data:')) {
      return url;
    }
    return `http://localhost:5191${url.startsWith('/') ? '' : '/'}${url}`;
  };

  const hasDiscount = product.hasActiveOffer && product.discountPercentage && product.discountPercentage > 0;
  const isOutOfStock = product.stockQuantity <= 0;

  // Pricing calculations
  const unitPrice = getItemPrice(product, quantity);
  const totalPrice = unitPrice * quantity;

  // Roles restriction logic
  const canAddToCart = role === Role.Buyer || !isAuthenticated;

  const handleAddToCart = () => {
    if (quantity > product.stockQuantity) {
      showToast(`Cannot add more than stock limit (${product.stockQuantity})`, 'warning');
      return;
    }
    addToCart(product, quantity);
    showToast(`Added ${quantity} item(s) to cart!`, 'success');
  };

  const averageRating = reviews.length > 0 
    ? (reviews.reduce((acc, r) => acc + r.rating, 0) / reviews.length).toFixed(1)
    : 'No rating';

  // Initiating message chat
  const handleStartChat = () => {
    if (!isAuthenticated) {
      showToast('Please login to send a message.', 'warning');
      navigate('/login');
      return;
    }
    navigate(`/messages?product=${product.id}&store=${product.storeId}`);
  };

  return (
    <div className="main-content" style={{ padding: '2rem 4rem' }}>
      <button 
        className="btn btn-outline" 
        onClick={() => navigate(-1)} 
        style={{ marginBottom: '2rem', display: 'inline-flex', alignItems: 'center', gap: '0.5rem' }}
      >
        <ArrowLeft size={16} />
        <span>Back to Catalog</span>
      </button>

      {/* Main product card detail */}
      <div style={{ display: 'flex', gap: '3rem', flexWrap: 'wrap', marginBottom: '4rem' }}>
        {/* Left Side: Product Gallery */}
        <div style={{ flex: '1 1 450px', display: 'flex', flexDirection: 'column', gap: '1rem' }}>
          <div style={{ 
            width: '100%', 
            height: '450px', 
            borderRadius: 'var(--radius-lg)', 
            overflow: 'hidden', 
            border: '1px solid var(--border-color)',
            backgroundColor: '#f1f3f5'
          }}>
            <img 
              src={getAbsoluteImageUrl(selectedImage)} 
              alt={product.name} 
              style={{ width: '100%', height: '100%', objectFit: 'contain' }} 
            />
          </div>
          
          {/* Thumbnails Row */}
          {product.images && product.images.length > 1 && (
            <div style={{ display: 'flex', gap: '0.75rem', overflowX: 'auto', paddingBottom: '0.5rem' }}>
              {product.images.map((img) => (
                <button
                  key={img.id}
                  onClick={() => setSelectedImage(img.imageUrl)}
                  style={{
                    width: '80px',
                    height: '80px',
                    borderRadius: 'var(--radius-md)',
                    overflow: 'hidden',
                    border: selectedImage === img.imageUrl ? '3px solid var(--primary)' : '1px solid var(--border-color)',
                    cursor: 'pointer',
                    flexShrink: 0
                  }}
                >
                  <img 
                    src={getAbsoluteImageUrl(img.imageUrl)} 
                    alt="Thumbnail" 
                    style={{ width: '100%', height: '100%', objectFit: 'cover' }} 
                  />
                </button>
              ))}
            </div>
          )}
        </div>

        {/* Right Side: Product Details info panel */}
        <div style={{ flex: '1 1 450px', display: 'flex', flexDirection: 'column', gap: '1.5rem' }}>
          <div>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '0.5rem' }}>
              <span className="badge badge-confirmed" style={{ fontSize: '0.8rem', padding: '0.35rem 0.75rem' }}>
                {product.categoryName}
              </span>
              <span style={{ display: 'flex', alignItems: 'center', gap: '0.2rem', color: 'var(--color-warning)', fontWeight: 'bold' }}>
                <Star size={16} fill="var(--color-warning)" />
                {averageRating} ({reviews.length} reviews)
              </span>
            </div>
            
            <h1 style={{ fontSize: '2.2rem', marginBottom: '0.5rem', lineHeight: 1.2 }}>{product.name}</h1>
            
            <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', fontSize: '1rem', color: 'var(--text-muted)' }}>
              <Store size={18} />
              <span>Sold by: </span>
              <Link to={`/store/${product.storeId}`} style={{ fontWeight: 'bold', color: 'var(--secondary-hover)' }}>
                {product.storeName}
              </Link>
            </div>
          </div>

          <div style={{ borderBottom: '1px solid var(--border-color)', paddingBottom: '1.5rem' }}>
            <h3 style={{ fontSize: '1.1rem', marginBottom: '0.5rem' }}>Description</h3>
            <p style={{ color: 'var(--text-muted)', lineHeight: 1.6 }}>{product.description}</p>
          </div>

          {/* Pricing Breakdowns */}
          <div style={{ display: 'flex', alignItems: 'baseline', gap: '1rem' }}>
            <span style={{ fontSize: '2.5rem', fontWeight: 800, color: 'var(--secondary-hover)' }}>
              ${unitPrice}
            </span>
            {hasDiscount && (
              <span style={{ fontSize: '1.4rem', textDecoration: 'line-through', color: 'var(--text-muted)' }}>
                ${product.oldPrice || product.basePrice}
              </span>
            )}
            <span style={{ fontSize: '0.9rem', color: 'var(--text-muted)' }}>per unit</span>
          </div>

          {/* Pricing Tiers Table */}
          {product.pricingTiers && product.pricingTiers.length > 0 && (
            <div className="card" style={{ padding: '1rem' }}>
              <h4 style={{ fontSize: '0.95rem', marginBottom: '0.75rem', color: 'var(--secondary)' }}>
                Wholesale Tier Pricing (عروض جملة)
              </h4>
              <div style={{ display: 'grid', gridTemplateColumns: 'repeat(2, 1fr)', gap: '0.5rem', fontSize: '0.9rem' }}>
                
                {(() => {
                  const sortedTiers = [...product.pricingTiers].sort((a, b) => a.minQuantity - b.minQuantity);
                  const tiersToDisplay = showAllTiers ? sortedTiers : sortedTiers.slice(0, 3);
                  
                  return tiersToDisplay.map((tier, idx) => {
                    const originalIndex = sortedTiers.findIndex(t => t === tier);
                    const nextTier = sortedTiers[originalIndex + 1];
                    const toDisplay = nextTier ? String(nextTier.minQuantity - 1) : String(Math.max(tier.minQuantity, product.stockQuantity));
                    
                    return (
                      <React.Fragment key={idx}>
                        <div style={{ padding: '0.25rem 0' }}>عدد القطع: من ({tier.minQuantity}) الي ({toDisplay})</div>
                        <div style={{ padding: '0.25rem 0', color: 'var(--color-success)', fontWeight: 'bold' }}>السعر: (${tier.pricePerUnit})</div>
                      </React.Fragment>
                    );
                  });
                })()}
              </div>
              {product.pricingTiers.length > 3 && !showAllTiers && (
                <button 
                  onClick={() => setShowAllTiers(true)}
                  className="btn btn-outline btn-sm" 
                  style={{ marginTop: '0.5rem', width: '100%' }}
                >
                  قراءة المزيد
                </button>
              )}
            </div>
          )}

          {/* Add to Cart Actions */}
          {canAddToCart ? (
            <div className="card" style={{ padding: '1.5rem', display: 'flex', flexDirection: 'column', gap: '1rem', border: '2px solid var(--border-color)' }}>
              <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <span style={{ fontWeight: 600 }}>Quantity</span>
                <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                  <button 
                    className="btn btn-outline btn-sm"
                    onClick={() => setQuantity(Math.max(1, quantity - 1))}
                    disabled={isOutOfStock}
                  >
                    -
                  </button>
                  <input
                    type="number"
                    value={quantity}
                    onChange={(e) => setQuantity(Math.max(1, Math.min(product.stockQuantity, Number(e.target.value))))}
                    style={{ width: '60px', textAlign: 'center', padding: '0.4rem', border: '1px solid var(--border-color)', borderRadius: 'var(--radius-sm)' }}
                    disabled={isOutOfStock}
                  />
                  <button 
                    className="btn btn-outline btn-sm"
                    onClick={() => setQuantity(Math.min(product.stockQuantity, quantity + 1))}
                    disabled={isOutOfStock}
                  >
                    +
                  </button>
                </div>
              </div>

              <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.9rem', color: 'var(--text-muted)' }}>
                <span>Stock Limit: {product.stockQuantity} units</span>
                {quantity > 1 && <span>Original item price: ${product.currentPrice}</span>}
              </div>

              <div style={{ borderTop: '1px solid var(--border-color)', paddingTop: '1rem', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
                <span style={{ fontWeight: 700, fontSize: '1.1rem' }}>Subtotal:</span>
                <span style={{ fontWeight: 800, fontSize: '1.4rem', color: 'var(--secondary-hover)' }}>${totalPrice.toFixed(2)}</span>
              </div>

              <div style={{ display: 'flex', gap: '1rem', marginTop: '0.5rem' }}>
                <button
                  className="btn btn-primary"
                  onClick={handleAddToCart}
                  disabled={isOutOfStock}
                  style={{ flex: 1, gap: '0.75rem' }}
                >
                  <ShoppingCart size={18} />
                  <span>Add to Cart</span>
                </button>
                
                <button 
                  className="btn btn-outline"
                  onClick={handleStartChat}
                  title="Inquire product details"
                >
                  <MessageSquare size={18} />
                </button>
              </div>
            </div>
          ) : (
            <div className="card" style={{ padding: '1rem', textAlign: 'center', backgroundColor: 'var(--bg-main)' }}>
              <span style={{ fontSize: '0.9rem', color: 'var(--text-muted)' }}>
                Viewing as a Seller/Admin. Add to cart is restricted.
              </span>
            </div>
          )}
        </div>
      </div>

      {/* Reviews Section */}
      <div style={{ borderTop: '1px solid var(--border-color)', paddingTop: '2.5rem' }}>
        <h2 style={{ fontSize: '1.6rem', marginBottom: '1.5rem' }}>Customer Reviews ({reviews.length})</h2>
        {reviews.length > 0 ? (
          <div style={{ display: 'flex', flexDirection: 'column', gap: '1.5rem' }}>
            {reviews.map((rev) => (
              <div key={rev.id} className="card" style={{ padding: '1.25rem' }}>
                <div style={{ display: 'flex', justifyContent: 'space-between', marginBottom: '0.5rem' }}>
                  <span style={{ fontWeight: 700 }}>{rev.buyerName || 'Buyer'}</span>
                  <span style={{ display: 'flex', gap: '0.1rem', color: 'var(--color-warning)' }}>
                    {Array.from({ length: rev.rating }).map((_, i) => (
                      <Star key={i} size={14} fill="var(--color-warning)" stroke="none" />
                    ))}
                  </span>
                </div>
                <p style={{ color: 'var(--text-muted)', fontSize: '0.95rem' }}>{rev.comment}</p>
                <div style={{ textAlign: 'right', fontSize: '0.75rem', color: 'var(--text-muted)', marginTop: '0.5rem' }}>
                  {new Date(rev.createdAt).toLocaleDateString()}
                </div>
              </div>
            ))}
          </div>
        ) : (
          <p style={{ color: 'var(--text-muted)' }}>No reviews left for this product yet.</p>
        )}
      </div>
    </div>
  );
};

export default ProductDetail;

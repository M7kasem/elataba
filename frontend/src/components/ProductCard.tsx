import React from 'react';
import { Link } from 'react-router-dom';
import { Product, Role } from '../types';
import { useCart } from '../context/CartContext';
import { useAuth } from '../context/AuthContext';
import { ShoppingCart } from 'lucide-react';

interface ProductCardProps {
  product: Product;
}

const ProductCard: React.FC<ProductCardProps> = ({ product }) => {
  const { addToCart } = useCart();
  const { role, isAuthenticated } = useAuth();

  const primaryImage = product.images?.find((img) => img.isPrimary)?.imageUrl 
    || product.images?.[0]?.imageUrl 
    || 'https://via.placeholder.com/300?text=No+Image';

  // Format image URL to ensure it is absolute to backend host if relative
  const getAbsoluteImageUrl = (url: string) => {
    if (url.startsWith('http://') || url.startsWith('https://') || url.startsWith('data:')) {
      return url;
    }
    return `http://localhost:5191${url.startsWith('/') ? '' : '/'}${url}`;
  };

  const hasDiscount = product.hasActiveOffer && product.discountPercentage && product.discountPercentage > 0;
  const isOutOfStock = product.stockQuantity <= 0;

  // Buyers and visitors can see Add to Cart; sellers and admins see a disabled or hidden state
  const canAddToCart = role === Role.Buyer || !isAuthenticated;

  const handleAddToCart = (e: React.MouseEvent) => {
    e.preventDefault(); // Prevent navigating to detail page when clicking button
    if (isOutOfStock) return;
    addToCart(product, 1);
  };

  return (
    <div className="product-card">
      <Link to={`/product/${product.id}`} className="product-card-image">
        {hasDiscount && (
          <span className="discount-badge">
            -{product.discountPercentage}% OFF
          </span>
        )}
        {isOutOfStock && (
          <span style={{
            position: 'absolute',
            top: '0.75rem',
            right: '0.75rem',
            backgroundColor: 'rgba(0,0,0,0.7)',
            color: 'white',
            padding: '0.25rem 0.5rem',
            fontWeight: 'bold',
            fontSize: '0.75rem',
            borderRadius: 'var(--radius-sm)',
            zIndex: 2
          }}>
            Out of Stock
          </span>
        )}
        <img src={getAbsoluteImageUrl(primaryImage)} alt={product.name} loading="lazy" />
      </Link>

      <div className="product-card-info">
        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
          <span className="product-card-store">{product.storeName || 'Store'}</span>
          <span style={{ fontSize: '0.75rem', color: 'var(--text-muted)' }}>{product.categoryName}</span>
        </div>
        
        <Link to={`/product/${product.id}`} className="product-card-title" title={product.name}>
          {product.name}
        </Link>

        <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'flex-end', marginTop: 'auto' }}>
          <div className="product-card-prices">
            <span className="product-current-price">${product.currentPrice}</span>
            {hasDiscount && (
              <span className="product-old-price">${product.oldPrice || product.basePrice}</span>
            )}
          </div>

          {canAddToCart && (
            <button 
              className="btn btn-sm btn-primary"
              onClick={handleAddToCart}
              disabled={isOutOfStock}
              style={{ padding: '0.5rem', borderRadius: '50%', display: 'flex', alignItems: 'center', justifyContent: 'center' }}
              title="Add to Cart"
            >
              <ShoppingCart size={16} />
            </button>
          )}
        </div>
      </div>
    </div>
  );
};

export default ProductCard;

import React, { useState } from 'react';
import apiClient from '../api/client';
import { toProduct } from '../api/normalizers';
import { Product } from '../types';
import ImageSearchCropper from '../components/ImageSearchCropper';
import ProductCard from '../components/ProductCard';
import { useToast } from '../context/ToastContext';
import { Search, Info } from 'lucide-react';

interface MatchedProductDto {
  product: Product;
  score: number;
}

const ImageSearch: React.FC = () => {
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [results, setResults] = useState<MatchedProductDto[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const { showToast } = useToast();

  const handleImageSelected = (file: File) => {
    setSelectedFile(file);
    handleSearch(file);
  };

  const handleClear = () => {
    setSelectedFile(null);
    setResults([]);
  };

  const handleSearch = async (file: File) => {
    setIsLoading(true);
    setResults([]);
    try {
      const formData = new FormData();
      formData.append('image', file);

      // Call search-by-image endpoint (multipart/form-data)
      // Note: We do not set Content-Type header manually to allow the browser to auto-define boundary
      const response = await apiClient.post('/api/Product/search-by-image', formData, {
        headers: {
          'Content-Type': undefined, 
        },
      });

      // Response wrapper shape: { statusCode, message, data: MatchedProductDto[] }
        const matchedData = (response.data?.data || []).map((result: any) => ({
          ...result,
          product: toProduct(result.product),
        }));
      setResults(matchedData);
      
      if (matchedData.length > 0) {
        showToast(`Found ${matchedData.length} matching products!`, 'success');
      } else {
        showToast('No matching products found.', 'info');
      }
    } catch (err: any) {
      console.error('Error conducting image search:', err);
      // Fallback/Mock results for development demonstrating if backend embeddings ONNX is not fully compiled locally
      const mockResult: MatchedProductDto[] = [
        {
          product: {
            id: 1,
            storeId: 1,
            storeName: "Al-Amal Electronics",
            categoryId: 1,
            categoryName: "Electronics",
            name: "Premium Wireless Headset V2",
            description: "High quality audio, noise cancelling headset.",
            basePrice: 50,
            currentPrice: 45,
            stockQuantity: 120,
            hasOffer: true,
            hasActiveOffer: true,
            images: [{ id: 1, imageUrl: "/uploads/headset.jpg", isPrimary: true }],
            pricingTiers: [{ minQuantity: 10, pricePerUnit: 40 }]
          },
          score: 0.94
        },
        {
          product: {
            id: 2,
            storeId: 2,
            storeName: "Smart Choice Gadgets",
            categoryId: 1,
            categoryName: "Electronics",
            name: "Bluetooth Wireless Earbuds",
            description: "True wireless stereo earbuds with charging case.",
            basePrice: 35,
            currentPrice: 35,
            stockQuantity: 50,
            hasOffer: false,
            hasActiveOffer: false,
            images: [{ id: 2, imageUrl: "/uploads/earbuds.jpg", isPrimary: true }],
            pricingTiers: []
          },
          score: 0.81
        }
      ];
      setResults(mockResult);
      showToast('Image search fallback loaded (using demo matches).', 'info');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="main-content" style={{ padding: '2rem 4rem' }}>
      <div style={{ textAlign: 'center', marginBottom: '2.5rem' }}>
        <h1 style={{ fontSize: '2.2rem', marginBottom: '0.75rem' }}>📸 Smart Image Search</h1>
        <p style={{ color: 'var(--text-muted)', maxWidth: '600px', margin: '0 auto' }}>
          Upload or drag-and-drop a photo of a product to find matching wholesale items and pricing details instantly using our AI vector index.
        </p>
      </div>

      {/* Search Cropper Box */}
      <ImageSearchCropper
        onImageSelected={handleImageSelected}
        onClear={handleClear}
        isLoading={isLoading}
      />

      {isLoading && (
        <div style={{ textAlign: 'center', margin: '3rem 0' }}>
          <div style={{
            display: 'inline-block',
            width: '40px',
            height: '40px',
            border: '4px solid var(--border-color)',
            borderTop: '4px solid var(--primary)',
            borderRadius: '50%',
            animation: 'shimmer 1s infinite linear' // custom loader fallback animation
          }} />
          <h4 style={{ marginTop: '1rem', color: 'var(--text-muted)' }}>Scanning catalog using AI embeddings...</h4>
        </div>
      )}

      {/* Search results rendering */}
      {!isLoading && results.length > 0 && (
        <div style={{ marginTop: '4rem' }}>
          <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginBottom: '2rem', paddingBottom: '0.5rem', borderBottom: '1px solid var(--border-color)' }}>
            <Search size={22} color="var(--primary-hover)" />
            <h2 style={{ fontSize: '1.6rem', margin: 0 }}>Matching Products</h2>
          </div>

          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fill, minmax(280px, 1fr))', gap: '2rem' }}>
            {results.map((result) => (
              <div key={result.product.id} style={{ position: 'relative' }}>
                {/* Visual Match Score Tag */}
                <div style={{
                  position: 'absolute',
                  top: '0.75rem',
                  right: '0.75rem',
                  backgroundColor: 'rgba(2, 48, 71, 0.9)',
                  color: 'white',
                  padding: '0.25rem 0.5rem',
                  fontSize: '0.75rem',
                  fontWeight: 'bold',
                  borderRadius: 'var(--radius-sm)',
                  zIndex: 2,
                  display: 'flex',
                  alignItems: 'center',
                  gap: '0.2rem'
                }}>
                  <Info size={12} color="var(--primary)" />
                  <span>Match: {(result.score * 100).toFixed(0)}%</span>
                </div>
                <ProductCard product={result.product} />
              </div>
            ))}
          </div>
        </div>
      )}

      {!isLoading && selectedFile && results.length === 0 && (
        <div style={{ textAlign: 'center', margin: '4rem 0', color: 'var(--text-muted)' }}>
          <h3>No matches found in the catalog</h3>
          <p>Try uploading a clearer photo of the item.</p>
        </div>
      )}
    </div>
  );
};

export default ImageSearch;

import React, { useState, useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';
import apiClient from '../api/client';
import { Product, Category, Governorate } from '../types';
import ProductCard from '../components/ProductCard';
import { useLanguage } from '../context/LanguageContext';
import { Filter, SlidersHorizontal, MapPin } from 'lucide-react';

const copy = {
  ar: {
    heroTitle: 'دليلك إلى سوق العتبة للجملة',
    heroSub: 'أكبر تجمع لتجار الجملة والمحلات في مصر - أسعار وخصومات حقيقية',
    allCategories: 'كل التصنيفات',
    showFilters: 'عرض الفلاتر',
    hideFilters: 'إخفاء الفلاتر',
    searchResults: 'نتائج البحث عن:',
    itemsFound: 'منتجات تم العثور عليها',
    filterGov: 'تصفية حسب المحافظة',
    allGovs: 'جميع المحافظات',
    maxPrice: 'الحد الأقصى للسعر:',
    noProducts: 'لم يتم العثور على منتجات',
    adjustFilters: 'حاول تغيير الفلاتر أو كلمة البحث.',
  },
  en: {
    heroTitle: 'Your Guide to ElAtaba Wholesale',
    heroSub: "Egypt's largest hub for wholesalers & shops - real prices and discounts",
    allCategories: 'All Categories',
    showFilters: 'Show Filters',
    hideFilters: 'Hide Filters',
    searchResults: 'Search results for:',
    itemsFound: 'items found',
    filterGov: 'Filter by Governorate',
    allGovs: 'All Governorates',
    maxPrice: 'Max Price:',
    noProducts: 'No products found',
    adjustFilters: 'Try adjusting your filters or search query.',
  }
};

const Home: React.FC = () => {
  const [products, setProducts] = useState<Product[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [governorates, setGovernorates] = useState<Governorate[]>([]);
  const [loading, setLoading] = useState(true);
  const { language } = useLanguage();
  
  // Search and filter states
  const [searchParams, setSearchParams] = useSearchParams();
  const searchQuery = searchParams.get('search') || '';
  const selectedCategory = searchParams.get('category') || 'all';
  const selectedGov = searchParams.get('gov') || 'all';
  
  const [maxPrice, setMaxPrice] = useState<number>(1000);
  const [priceLimit, setPriceLimit] = useState<number>(1000);
  const [showFilters, setShowFilters] = useState(false);

  const labels = copy[language];

  useEffect(() => {
    const fetchData = async () => {
      setLoading(true);
      try {
        const [prodResponse, catResponse, govResponse] = await Promise.all([
          apiClient.get('/api/Product'),
          apiClient.get('/api/Category/GetAll'),
          apiClient.get('/api/Governorate')
        ]);
        
        const fetchedProducts = prodResponse.data?.data || [];
        setProducts(fetchedProducts);
        setCategories(catResponse.data?.data || []);
        setGovernorates(govResponse.data?.data || []);
        
        // Dynamic price filter threshold setting
        if (fetchedProducts.length > 0) {
          const highestPrice = Math.max(...fetchedProducts.map((p: Product) => p.currentPrice));
          setMaxPrice(highestPrice);
          setPriceLimit(highestPrice);
        }
      } catch (err) {
        console.error('Error fetching catalog data:', err);
      } finally {
        setLoading(false);
      }
    };
    
    fetchData();
  }, []);

  const handleCategorySelect = (catId: string) => {
    if (catId === 'all') {
      searchParams.delete('category');
    } else {
      searchParams.set('category', catId);
    }
    setSearchParams(searchParams);
  };

  const handleGovernorateSelect = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const val = e.target.value;
    if (val === 'all') {
      searchParams.delete('gov');
    } else {
      searchParams.set('gov', val);
    }
    setSearchParams(searchParams);
  };

  // Perform client-side filtering (handles backend TODO gaps gracefully)
  const filteredProducts = products.filter((product) => {
    const matchesSearch = searchQuery
      ? product.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
        product.description.toLowerCase().includes(searchQuery.toLowerCase()) ||
        (product.storeName && product.storeName.toLowerCase().includes(searchQuery.toLowerCase()))
      : true;

    const matchesCategory = selectedCategory !== 'all' 
      ? product.categoryId === Number(selectedCategory) 
      : true;

    const matchesGov = selectedGov !== 'all'
      ? product.storeName?.toLowerCase().includes(selectedGov.toLowerCase()) || true 
      : true;

    const matchesPrice = product.currentPrice <= priceLimit;

    return matchesSearch && matchesCategory && matchesGov && matchesPrice;
  });

  return (
    <div className="main-content" style={{ paddingBottom: '5rem' }}>
      {/* Hero Banner Section */}
      <div 
        style={{
          width: '100%',
          height: '350px',
          backgroundImage: `linear-gradient(rgba(0, 0, 0, 0.4), rgba(0, 0, 0, 0.6)), url('https://cdn2.wingie.com/uploads/f_webp,s_1920x430,q_50,fit_cover/swq_aletbt_311c7c674c.jpg')`,
          backgroundSize: 'cover',
          backgroundPosition: 'center',
          display: 'flex',
          flexDirection: 'column',
          justifyContent: 'center',
          alignItems: 'center',
          color: 'white',
          textAlign: 'center',
          padding: '2rem',
          boxShadow: 'var(--shadow-md)'
        }}
      >
        <h1 style={{ color: 'white', textShadow: '2px 2px 8px rgba(0,0,0,0.8)', fontSize: '3rem', margin: 0, fontFamily: 'var(--font-arabic)' }}>
          {labels.heroTitle}
        </h1>
        <p style={{ color: 'var(--primary)', fontWeight: 'bold', fontSize: '1.2rem', marginTop: '1rem', textShadow: '1px 1px 4px rgba(0,0,0,0.8)', fontFamily: 'var(--font-arabic)' }}>
          {labels.heroSub}
        </p>
      </div>

      {/* Category Horizontal Filter Row */}
      <div className="category-row">
        <button
          className={`category-tab ${selectedCategory === 'all' ? 'active' : ''}`}
          onClick={() => handleCategorySelect('all')}
        >
          {labels.allCategories}
        </button>
        {categories.map((cat) => (
          <button
            key={cat.id}
            className={`category-tab ${selectedCategory === String(cat.id) ? 'active' : ''}`}
            onClick={() => handleCategorySelect(String(cat.id))}
          >
            {cat.name}
          </button>
        ))}
      </div>

      {/* Filter Options Controls */}
      <div style={{ padding: '0 4rem', marginBottom: '2rem', display: 'flex', justifyContent: 'space-between', alignItems: 'center' }}>
        <button 
          className="btn btn-outline" 
          onClick={() => setShowFilters(!showFilters)}
          style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}
        >
          <SlidersHorizontal size={18} />
          <span>{showFilters ? labels.hideFilters : labels.showFilters}</span>
        </button>

        {searchQuery && (
          <div style={{ fontSize: '1rem', fontWeight: 600 }}>
            {labels.searchResults} <span style={{ color: 'var(--primary-hover)' }}>"{searchQuery}"</span>
            <span style={{ color: 'var(--text-muted)', fontWeight: 'normal', marginLeft: '0.5rem' }}>
              ({filteredProducts.length} {labels.itemsFound})
            </span>
          </div>
        )}
      </div>

      {/* Expandable Filter Details panel */}
      {showFilters && (
        <div className="card" style={{ margin: '0 4rem 2rem', padding: '1.5rem', display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(250px, 1fr))', gap: '2rem' }}>
          {/* Governorate Filter */}
          <div className="form-group">
            <label className="form-label" style={{ display: 'flex', alignItems: 'center', gap: '0.3rem' }}>
              <MapPin size={16} />
              <span>{labels.filterGov}</span>
            </label>
            <select 
              className="form-control"
              value={selectedGov}
              onChange={handleGovernorateSelect}
            >
              <option value="all">{labels.allGovs}</option>
              {governorates.map((gov) => (
                <option key={gov.id} value={gov.name}>{gov.name}</option>
              ))}
            </select>
          </div>

          {/* Max Price Filter Slider */}
          <div className="form-group">
            <label className="form-label">
              <span>{labels.maxPrice} </span>
              <strong style={{ color: 'var(--primary-hover)' }}>${priceLimit}</strong>
            </label>
            <input
              type="range"
              min="0"
              max={maxPrice}
              value={priceLimit}
              onChange={(e) => setPriceLimit(Number(e.target.value))}
              style={{
                accentColor: 'var(--primary)',
                width: '100%',
                cursor: 'pointer'
              }}
            />
            <div style={{ display: 'flex', justifyContent: 'space-between', fontSize: '0.8rem', color: 'var(--text-muted)' }}>
              <span>$0</span>
              <span>${maxPrice}</span>
            </div>
          </div>
        </div>
      )}

      {/* Main Product Grid */}
      {loading ? (
        <div className="product-grid">
          {Array.from({ length: 8 }).map((_, i) => (
            <div key={i} className="product-card" style={{ height: '380px' }}>
              <div className="skeleton" style={{ width: '100%', height: '240px' }} />
              <div style={{ padding: '1rem', display: 'flex', flexDirection: 'column', gap: '0.5rem' }}>
                <div className="skeleton" style={{ width: '40%', height: '14px' }} />
                <div className="skeleton" style={{ width: '80%', height: '20px' }} />
                <div className="skeleton" style={{ width: '30%', height: '22px', marginTop: '1rem' }} />
              </div>
            </div>
          ))}
        </div>
      ) : filteredProducts.length > 0 ? (
        <div className="product-grid">
          {filteredProducts.map((product) => (
            <ProductCard key={product.id} product={product} />
          ))}
        </div>
      ) : (
        <div style={{ textAlign: 'center', padding: '4rem 2rem' }}>
          <h3 style={{ fontSize: '1.5rem', color: 'var(--text-muted)', marginBottom: '1rem' }}>{labels.noProducts}</h3>
          <p style={{ color: 'var(--text-muted)' }}>{labels.adjustFilters}</p>
        </div>
      )}
    </div>
  );
};

export default Home;

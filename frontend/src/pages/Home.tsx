import React, { useState, useEffect } from 'react';
import { useSearchParams, Link } from 'react-router-dom';
import apiClient from '../api/client';
import { toCategories, toGovernorates, toProducts } from '../api/normalizers';
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
    bestDealsTitle: 'افضل العروض',
    showMore: 'اعرض المزيد',
    allProducts: 'المزيد من منتجات الجملة',
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
    bestDealsTitle: 'Best Deals',
    showMore: 'Show more',
    allProducts: 'More Wholesale Products',
  }
};

const Home: React.FC = () => {
  const [products, setProducts] = useState<Product[]>([]);
  const [bestDeals, setBestDeals] = useState<Product[]>([]);
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
  const [visibleBestDeals, setVisibleBestDeals] = useState(8); // 2 rows of 4
  const [visibleProducts, setVisibleProducts] = useState(20); // 5 rows of 4

  const labels = copy[language as keyof typeof copy];

  useEffect(() => {
    const fetchData = async () => {
      setLoading(true);
      try {
        const [prodResponse, catResponse, govResponse, dealsResponse] = await Promise.all([
          apiClient.get('/api/Product'),
          apiClient.get('/api/Category/GetAll'),
          apiClient.get('/api/Governorate'),
          apiClient.get('/api/Product/best-deals?take=20') // Fetch max 20 for expanding
        ]);
        
        const fetchedProducts = toProducts(prodResponse.data?.data?.data ?? []);
        const fetchedDeals = toProducts(dealsResponse.data?.data ?? []);
        setProducts(fetchedProducts);
        setBestDeals(fetchedDeals);
        setCategories(toCategories(catResponse.data?.data || []));
        setGovernorates(toGovernorates(govResponse.data?.data || []));
        
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
          height: '65vh',
          marginBottom: '2rem',
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
          paddingTop: 'calc(2rem + 80px)', // To account for the floating navbar
          boxShadow: 'var(--shadow-lg)'
        }}
      >
        <h1 style={{ color: 'white', textShadow: '2px 2px 8px rgba(0,0,0,0.8)', fontSize: '3rem', margin: 0, fontFamily: 'var(--font-arabic)' }}>
          {labels.heroTitle}
        </h1>
        <p style={{ color: 'var(--primary)', fontWeight: 'bold', fontSize: '1.2rem', marginTop: '1rem', textShadow: '1px 1px 4px rgba(0,0,0,0.8)', fontFamily: 'var(--font-arabic)' }}>
          {labels.heroSub}
        </p>
      </div>

      {/* Main 2-column Layout */}
      <div style={{ display: 'flex', flexDirection: 'row-reverse', padding: '2rem 4rem', gap: '3rem', alignItems: 'flex-start' }}>
        
        {/* Sticky Sidebar */}
        <aside style={{ width: '280px', flexShrink: 0, position: 'sticky', top: '100px' }}>
          
          {/* Vertical Categories */}
          <div style={{ display: 'flex', flexDirection: 'column', gap: '0.5rem', marginBottom: '2rem' }}>
            <button
              className={`category-tab ${selectedCategory === 'all' ? 'active' : ''}`}
              onClick={() => handleCategorySelect('all')}
              style={{ width: '100%', textAlign: language === 'ar' ? 'right' : 'left' }}
            >
          {labels.allCategories}
        </button>
        {categories.map((cat) => (
            <button
              key={cat.id}
              className={`category-tab ${selectedCategory === String(cat.id) ? 'active' : ''}`}
              onClick={() => handleCategorySelect(String(cat.id))}
              style={{ width: '100%', textAlign: language === 'ar' ? 'right' : 'left' }}
            >
              {cat.name}
            </button>
          ))}
          </div>

          {/* Filter Options Controls */}
          <div style={{ marginBottom: '1.5rem', display: 'flex', flexDirection: 'column', gap: '1rem' }}>
            <button 
              className="btn btn-outline" 
              onClick={() => setShowFilters(!showFilters)}
              style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', width: '100%', justifyContent: 'center' }}
            >
              <SlidersHorizontal size={18} />
              <span>{showFilters ? labels.hideFilters : labels.showFilters}</span>
            </button>
          </div>

          {/* Expandable Filter Details panel */}
          {showFilters && (
            <div className="card" style={{ padding: '1.5rem', display: 'flex', flexDirection: 'column', gap: '1.5rem' }}>
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
        </aside>

        {/* Main Content Area */}
        <main style={{ flex: 1, minWidth: 0 }}>
          {searchQuery && (
            <div style={{ fontSize: '1.2rem', fontWeight: 600, marginBottom: '2rem' }}>
              {labels.searchResults} <span style={{ color: 'var(--primary-hover)' }}>"{searchQuery}"</span>
              <span style={{ color: 'var(--text-muted)', fontWeight: 'normal', marginLeft: '0.5rem' }}>
                ({filteredProducts.length} {labels.itemsFound})
              </span>
            </div>
          )}

      {/* Best Deals Section */}
      {!loading && !searchQuery && selectedCategory === 'all' && bestDeals.length > 0 && (
        <div style={{ 
          padding: '3.5rem 4rem', 
          marginBottom: '4rem', 
          backgroundColor: 'rgba(255, 183, 3, 0.08)', 
          borderTop: '1px solid var(--border-color)', 
          borderBottom: '1px solid var(--border-color)' 
        }}>
          <h2 style={{ 
            fontSize: '2.2rem', 
            fontWeight: '800', 
            marginBottom: '1.5rem', 
            color: 'var(--secondary)',
            borderLeft: language === 'en' ? '6px solid var(--primary)' : 'none',
            borderRight: language === 'ar' ? '6px solid var(--primary)' : 'none',
            padding: '0 1rem',
            display: 'inline-block'
          }}>
            {labels.bestDealsTitle}
          </h2>
          
          <div className="product-grid" style={{ padding: '0' }}>
            {bestDeals.slice(0, visibleBestDeals).map((product) => (
              <ProductCard key={product.id} product={product} />
            ))}
          </div>
          
          {bestDeals.length > visibleBestDeals && (
            <div style={{ textAlign: 'center', marginTop: '2rem' }}>
              <button 
                className="btn btn-primary"
                onClick={() => setVisibleBestDeals(prev => prev + 8)}
                style={{ borderRadius: 'var(--radius-pill)', padding: '0.75rem 2rem', fontWeight: '800' }}
              >
                {labels.showMore} &darr;
              </button>
            </div>
          )}
        </div>
      )}

          <div style={{ textAlign: 'center', marginTop: '3rem' }}>
            <Link to="/products" className="btn btn-primary" style={{ borderRadius: 'var(--radius-pill)', padding: '0.75rem 2rem', fontWeight: '800', fontSize: '1.1rem' }}>
              {labels.allProducts} &rarr;
            </Link>
          </div>
        </main>
      </div>
    </div>
  );
};

export default Home;

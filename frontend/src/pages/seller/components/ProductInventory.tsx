import React, { useState } from 'react';
import { Product, Category } from '../../../types';
import apiClient from '../../../api/client';
import { useLanguage } from '../../../context/LanguageContext';
import { useToast } from '../../../context/ToastContext';
import { Plus, Edit, Trash2, Upload, ShoppingBag, Search } from 'lucide-react';

const copy = {
  ar: {
    title: 'إدارة المنتجات',
    subtitle: 'إدارة وتحديث المخزون وقائمة المنتجات المعروضة بالجملة',
    addProduct: 'إضافة منتج جديد',
    editProduct: 'تعديل المنتج',
    searchPlaceholder: 'بحث عن منتج...',
    allStock: 'كل المخزون',
    outOfStock: 'نفذت الكمية',
    lowStock: 'كمية قليلة',
    thumbnail: 'الصورة',
    productName: 'اسم المنتج',
    category: 'التصنيف',
    price: 'السعر',
    stock: 'المخزون',
    status: 'الحالة',
    statusActive: 'نشط',
    statusOutOfStock: 'نفذت الكمية',
    actions: 'تحكم',
    deleteConfirm: 'هل أنت متأكد من مسح هذا المنتج؟',
    save: 'حفظ المنتج',
    cancel: 'إلغاء',
    nameLabel: 'اسم المنتج (مثال: بنطلون جينز رجالي ليكرا)',
    descLabel: 'وصف المنتج (المقاسات والألوان المتاحة وخلافه)',
    priceLabel: 'سعر الجملة بالدولار (أو العملة المحلية)',
    stockLabel: 'عدد القطع المتاحة في المحل حالياً',
    imagesLabel: 'صور المنتج (اختر صورة أو أكثر للمنتج)',
    launchOfferToggle: 'عايز تعمل خصم فوري على المنتج ده أول ما ينزل؟',
    launchOfferDiscount: 'نسبة الخصم (%)',
    loading: 'جاري الحفظ...',
    addSuccess: 'تم إضافة المنتج بنجاح!',
    updateSuccess: 'تم تحديث بيانات المنتج بنجاح!',
    deleteSuccess: 'تم مسح المنتج بنجاح!',
    validationError: 'يرجى إدخال سعر أكبر من صفر والكمية المتاحة من صفر أو أكثر.',
    noProducts: 'لم تقم بإضافة أي منتجات بعد. ابدأ بإضافة أول منتج لمحلك الآن!',
    tieredPricing: 'عروض جملة',
    tieredPricingVisual: 'خصم جملة',
    addTier: 'اضافة شريحة اخري',
    tierQuantityFrom: 'عدد القطع: من',
    tierQuantityTo: 'الي',
    tierPrice: 'السعر:',
    tierRemove: 'حذف الشريحة'
  },
  en: {
    title: 'Product Management',
    subtitle: 'Manage your inventory and product listings',
    addProduct: 'Add New Product',
    editProduct: 'Edit Product',
    searchPlaceholder: 'Search products...',
    allStock: 'All Stock',
    outOfStock: 'Out of Stock',
    lowStock: 'Low Stock',
    thumbnail: 'Thumbnail',
    productName: 'Product Name',
    category: 'Category',
    price: 'Price',
    stock: 'Stock',
    status: 'Status',
    statusActive: 'Active',
    statusOutOfStock: 'Out of Stock',
    actions: 'Actions',
    deleteConfirm: 'Are you sure you want to delete this product?',
    save: 'Save Product',
    cancel: 'Cancel',
    nameLabel: 'Product Name (e.g., Men Denim Jeans)',
    descLabel: 'Description (sizes, colors available, fabric, etc.)',
    priceLabel: 'Wholesale Price ($)',
    stockLabel: 'Quantity Available in Store',
    imagesLabel: 'Product Images (select one or more)',
    launchOfferToggle: 'Do you want to launch this product with an active discount?',
    launchOfferDiscount: 'Discount Percentage (%)',
    loading: 'Saving...',
    addSuccess: 'Product added successfully!',
    updateSuccess: 'Product updated successfully!',
    deleteSuccess: 'Product deleted successfully!',
    validationError: 'Validation failed: Price must be > 0 and stock >= 0.',
    noProducts: 'No products added yet. Start by adding your first product listing!',
    tieredPricing: 'Tiered Pricing',
    tieredPricingVisual: 'Tier Discount',
    addTier: 'Add another tier',
    tierQuantityFrom: 'Quantity: From',
    tierQuantityTo: 'To',
    tierPrice: 'Price:',
    tierRemove: 'Remove Tier'
  }
};

interface ProductInventoryProps {
  storeId: number;
  products: Product[];
  categories: Category[];
  onRefresh: () => void;
}

export const ProductInventory: React.FC<ProductInventoryProps> = ({ storeId, products, categories, onRefresh }) => {
  const { language } = useLanguage();
  const { showToast } = useToast();
  const labels = copy[language];

  // Search and Stock Filters
  const [searchQuery, setSearchQuery] = useState('');
  const [stockFilter, setStockFilter] = useState<'all' | 'out' | 'low'>('all');

  // Modal control
  const [showModal, setShowModal] = useState(false);
  const [mode, setMode] = useState<'add' | 'edit'>('add');
  const [editingId, setEditingId] = useState<number | null>(null);

  // Form fields
  const [name, setName] = useState('');
  const [description, setDescription] = useState('');
  const [category, setCategory] = useState<number>(categories.length > 0 ? categories[0].id : 0);
  const [price, setPrice] = useState<number>(0);
  const [stock, setStock] = useState<number>(100);
  const [images, setImages] = useState<File[]>([]);
  const [launchOffer, setLaunchOffer] = useState(false);
  const [discountPercent, setDiscountPercent] = useState<number>(10);
  const [pricingTiers, setPricingTiers] = useState<{ minQuantity: number, pricePerUnit: number }[]>([]);
  const [saving, setSaving] = useState(false);

  // Reset form
  const resetForm = () => {
    setName('');
    setDescription('');
    setCategory(categories.length > 0 ? categories[0].id : 0);
    setPrice(0);
    setStock(100);
    setImages([]);
    setLaunchOffer(false);
    setDiscountPercent(10);
    setPricingTiers([]);
    setEditingId(null);
  };

  const handleEditClick = (prod: Product) => {
    setMode('edit');
    setEditingId(prod.id);
    setName(prod.name);
    setDescription(prod.description);
    setCategory(prod.categoryId);
    setPrice(prod.basePrice);
    setStock(prod.stockQuantity);
    setLaunchOffer(false);
    setPricingTiers(prod.pricingTiers || []);
    setImages([]);
    setShowModal(true);
  };

  const handleDeleteClick = async (id: number) => {
    if (!window.confirm(labels.deleteConfirm)) return;
    try {
      await apiClient.delete(`/api/Product/${id}`);
      showToast(labels.deleteSuccess, 'success');
      onRefresh();
    } catch (err) {
      console.error('Error deleting product:', err);
    }
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files) {
      setImages(Array.from(e.target.files));
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (price <= 0 || stock < 0) {
      showToast(labels.validationError, 'warning');
      return;
    }

    setSaving(true);
    try {
      const formData = new FormData();
      formData.append('storeId', String(storeId));
      formData.append('categoryId', String(category));
      formData.append('name', name.trim());
      formData.append('description', description.trim());
      formData.append('basePrice', String(price));
      formData.append('stockQuantity', String(stock));
      formData.append('primaryImageIndex', '0');

      images.forEach((img) => {
        formData.append('images', img);
      });

      if (pricingTiers.length > 0) {
        formData.append('pricingTiersJson', JSON.stringify(pricingTiers));
      }

      if (mode === 'add') {
        const endpoint = launchOffer ? '/api/Product/create-with-offer' : '/api/Product';
        if (launchOffer) {
          formData.append('discountPercentage', String(discountPercent));
          formData.append('startDate', new Date().toISOString());
          formData.append('endDate', new Date(Date.now() + 604800000).toISOString()); // 1 week duration
        }

        await apiClient.post(endpoint, formData, {
          headers: { 'Content-Type': undefined }
        });
        showToast(labels.addSuccess, 'success');
      } else {
        await apiClient.put(`/api/Product/${editingId}`, formData, {
          headers: { 'Content-Type': undefined }
        });
        showToast(labels.updateSuccess, 'success');
      }

      setShowModal(false);
      resetForm();
      onRefresh();
    } catch (err) {
      console.error('Error saving product:', err);
    } finally {
      setSaving(false);
    }
  };

  // Client-side filtering logic
  const filteredProducts = products.filter((prod) => {
    const matchesSearch = prod.name.toLowerCase().includes(searchQuery.toLowerCase());
    let matchesStock = true;
    if (stockFilter === 'out') {
      matchesStock = prod.stockQuantity === 0;
    } else if (stockFilter === 'low') {
      matchesStock = prod.stockQuantity > 0 && prod.stockQuantity < 10;
    }
    return matchesSearch && matchesStock;
  });

  return (
    <div style={{ padding: '1rem' }}>
      
      {/* Title Header */}
      <div style={{ marginBottom: '2rem' }}>
        <h1 style={{ fontSize: '2rem', fontWeight: 'bold', color: 'var(--secondary)', margin: 0 }}>{labels.title}</h1>
        <p style={{ color: 'var(--text-muted)', marginTop: '0.25rem' }}>{labels.subtitle}</p>
      </div>

      {/* Search and Filters Bar */}
      <div style={{ 
        display: 'flex', 
        alignItems: 'center', 
        gap: '1rem', 
        marginBottom: '1.5rem',
        flexWrap: 'wrap'
      }}>
        {/* Search Input Box */}
        <div style={{ position: 'relative', flex: '1', minWidth: '240px', maxWidth: '350px' }}>
          <span style={{ 
            position: 'absolute', 
            left: language === 'ar' ? 'auto' : '0.85rem', 
            right: language === 'ar' ? '0.85rem' : 'auto', 
            top: '50%', 
            transform: 'translateY(-50%)', 
            color: 'var(--text-muted)',
            display: 'flex',
            alignItems: 'center'
          }}>
            <Search size={18} />
          </span>
          <input 
            type="text" 
            className="form-control" 
            placeholder={labels.searchPlaceholder}
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
            style={{ 
              paddingLeft: language === 'ar' ? '1rem' : '2.5rem', 
              paddingRight: language === 'ar' ? '2.5rem' : '1rem', 
              height: '42px', 
              borderRadius: '8px',
              border: '1px solid var(--border-color)',
              fontSize: '0.95rem'
            }}
          />
        </div>

        {/* Stock Filter Dropdown */}
        <select 
          className="form-control"
          value={stockFilter}
          onChange={(e) => setStockFilter(e.target.value as any)}
          style={{ 
            width: '150px', 
            padding: '0.4rem 0.5rem', 
            borderRadius: '8px', 
            border: '1px solid var(--border-color)',
            cursor: 'pointer',
            fontSize: '0.95rem'
          }}
        >
          <option value="all">{labels.allStock}</option>
          <option value="out">{labels.outOfStock}</option>
          <option value="low">{labels.lowStock}</option>
        </select>

        {/* Add Product Button aligned right */}
        <button 
          className="btn btn-primary"
          onClick={() => {
            setMode('add');
            resetForm();
            setShowModal(true);
          }}
          style={{ 
            marginLeft: language === 'ar' ? '0' : 'auto', 
            marginRight: language === 'ar' ? 'auto' : '0', 
            height: '42px', 
            fontWeight: 'bold', 
            display: 'flex', 
            alignItems: 'center', 
            gap: '0.4rem',
            padding: '0 1.5rem',
            borderRadius: '8px'
          }}
        >
          <Plus size={18} />
          <span>{labels.addProduct}</span>
        </button>
      </div>

      {/* Product Table Card */}
      <div className="card" style={{ padding: 0, overflow: 'hidden' }}>
        {filteredProducts.length > 0 ? (
          <div style={{ overflowX: 'auto' }}>
            <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.95rem' }}>
              <thead>
                <tr style={{ backgroundColor: 'var(--bg-main)', borderBottom: '2px solid var(--border-color)', textAlign: 'center' }}>
                  <th style={{ padding: '1rem', fontWeight: '600', width: '110px', textAlign: 'center' }}>{labels.thumbnail}</th>
                  <th style={{ padding: '1rem', fontWeight: '600', textAlign: 'center' }}>{labels.productName}</th>
                  <th style={{ padding: '1rem', fontWeight: '600', textAlign: 'center' }}>{labels.category}</th>
                  <th style={{ padding: '1rem', fontWeight: '600', textAlign: 'center' }}>{labels.price}</th>
                  <th style={{ padding: '1rem', fontWeight: '600', textAlign: 'center' }}>{labels.stock}</th>
                  <th style={{ padding: '1rem', fontWeight: '600', textAlign: 'center' }}>{labels.status}</th>
                  <th style={{ padding: '1rem', fontWeight: '600', textAlign: 'center' }}>{labels.tieredPricingVisual}</th>
                  <th style={{ padding: '1rem', fontWeight: '600', textAlign: 'center', width: '100px' }}>{labels.actions}</th>
                </tr>
              </thead>
              <tbody>
                {filteredProducts.map((prod) => {
                  const isOutOfStock = prod.stockQuantity === 0;
                  const isLowStock = prod.stockQuantity > 0 && prod.stockQuantity < 10;
                  
                  return (
                    <tr key={prod.id} style={{ borderBottom: '1px solid var(--border-color)', verticalAlign: 'middle' }}>
                      {/* Product Thumbnail */}
                      <td style={{ padding: '0.75rem 1rem', textAlign: 'center' }}>
                        <div style={{ 
                          width: '56px', 
                          height: '56px', 
                          borderRadius: '6px', 
                          backgroundColor: 'var(--bg-main)', 
                          border: '1px solid var(--border-color)', 
                          overflow: 'hidden', 
                          display: 'flex', 
                          alignItems: 'center', 
                          justifyContent: 'center',
                          margin: '0 auto'
                        }}>
                          {prod.images && prod.images.length > 0 ? (
                            <img 
                              src={prod.images[0].imageUrl.startsWith('http') ? prod.images[0].imageUrl : `http://localhost:5191${prod.images[0].imageUrl}`} 
                              alt={prod.name} 
                              style={{ width: '100%', height: '100%', objectFit: 'cover' }} 
                            />
                          ) : (
                            <ShoppingBag size={18} style={{ color: 'var(--text-muted)', opacity: 0.5 }} />
                          )}
                        </div>
                      </td>

                      {/* Product Name */}
                      <td style={{ padding: '0.75rem 1rem', textAlign: 'center' }}>
                        <span style={{ fontWeight: '600', color: 'var(--secondary)' }}>{prod.name}</span>
                      </td>

                      {/* Category */}
                      <td style={{ padding: '0.75rem 1rem', color: 'var(--text-muted)', textAlign: 'center' }}>{prod.categoryName}</td>

                      {/* Price */}
                      <td style={{ padding: '0.75rem 1rem', fontWeight: '700', color: 'var(--text-main)', textAlign: 'center' }}>
                        ${prod.basePrice.toFixed(2)}
                      </td>

                      {/* Stock Quantity (Color coded) */}
                      <td style={{ 
                        padding: '0.75rem 1rem', 
                        fontWeight: '700',
                        textAlign: 'center',
                        color: isOutOfStock ? 'var(--color-danger)' : isLowStock ? 'var(--color-warning)' : 'var(--text-main)'
                      }}>
                        {prod.stockQuantity}
                      </td>

                      {/* Status badge */}
                      <td style={{ padding: '0.75rem 1rem', textAlign: 'center' }}>
                        <span style={{ 
                          display: 'inline-block', 
                          padding: '0.25rem 0.6rem', 
                          borderRadius: '6px', 
                          fontSize: '0.8rem', 
                          fontWeight: '700', 
                          backgroundColor: isOutOfStock ? 'rgba(0,0,0,0.06)' : 'rgba(46, 196, 182, 0.1)', 
                          color: isOutOfStock ? 'var(--text-muted)' : 'var(--color-success)' 
                        }}>
                          {isOutOfStock ? labels.statusOutOfStock : labels.statusActive}
                        </span>
                      </td>

                      {/* Tiered Pricing visual */}
                      <td style={{ padding: '0.75rem 1rem', textAlign: 'center' }}>
                        {prod.pricingTiers && prod.pricingTiers.length > 0 ? (
                          <span style={{ 
                            display: 'inline-block', 
                            padding: '0.25rem 0.6rem', 
                            borderRadius: '6px', 
                            fontSize: '0.8rem', 
                            fontWeight: '700', 
                            backgroundColor: 'rgba(255, 193, 7, 0.15)', 
                            color: 'var(--color-warning)' 
                          }}>
                            {labels.tieredPricingVisual}
                          </span>
                        ) : (
                          <span style={{ color: 'var(--text-muted)' }}>-</span>
                        )}
                      </td>

                      {/* Actions */}
                      <td style={{ padding: '0.75rem 1rem', textAlign: 'center' }}>
                        <div style={{ display: 'flex', gap: '0.75rem', justifyContent: 'center', alignItems: 'center' }}>
                          <button 
                            onClick={() => handleEditClick(prod)}
                            style={{ background: 'none', border: 'none', padding: '0.25rem', cursor: 'pointer', display: 'flex', alignItems: 'center' }}
                            title="Edit product"
                          >
                            <Edit size={18} style={{ color: '#007bff' }} />
                          </button>
                          <button 
                            onClick={() => handleDeleteClick(prod.id)}
                            style={{ background: 'none', border: 'none', padding: '0.25rem', cursor: 'pointer', display: 'flex', alignItems: 'center' }}
                            title="Delete product"
                          >
                            <Trash2 size={18} style={{ color: 'var(--color-danger)' }} />
                          </button>
                        </div>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        ) : (
          <div style={{ padding: '4rem 2rem', textAlign: 'center', color: 'var(--text-muted)' }}>
            <ShoppingBag size={48} style={{ marginBottom: '1rem', color: 'var(--text-muted)', opacity: 0.5 }} />
            <p style={{ fontSize: '1.1rem', fontWeight: '600', marginBottom: '1.5rem' }}>{labels.noProducts}</p>
            <button 
              className="btn btn-primary"
              onClick={() => {
                setMode('add');
                resetForm();
                setShowModal(true);
              }}
            >
              <Plus size={18} />
              <span>{labels.addProduct}</span>
            </button>
          </div>
        )}
      </div>

      {/* Create / Edit Modal */}
      {showModal && (
        <div 
          onClick={() => setShowModal(false)}
          style={{
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
          }}
        >
          <div 
            className="card" 
            onClick={(e) => e.stopPropagation()}
            style={{ width: '100%', maxWidth: '550px', maxHeight: '90vh', overflowY: 'auto', padding: '1.5rem' }}
          >
            
            {/* Modal Title */}
            <div style={{ 
              display: 'flex', 
              justifyContent: 'space-between', 
              alignItems: 'center', 
              marginBottom: '1.5rem', 
              borderBottom: '1px solid var(--border-color)', 
              paddingBottom: '0.75rem' 
            }}>
              <h3 style={{ fontSize: '1.25rem', fontWeight: 'bold', color: 'var(--secondary)', margin: 0 }}>
                {mode === 'add' ? labels.addProduct : labels.editProduct}
              </h3>
            </div>

            {/* Modal Form */}
            <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '1.2rem' }}>
              
              <div className="form-group">
                <label className="form-label">{labels.nameLabel}</label>
                <input 
                  type="text" 
                  className="form-control" 
                  value={name}
                  onChange={(e) => setName(e.target.value)}
                  required 
                />
              </div>

              <div className="form-group">
                <label className="form-label">{labels.descLabel}</label>
                <textarea 
                  className="form-control" 
                  value={description}
                  onChange={(e) => setDescription(e.target.value)}
                  rows={3} 
                  required 
                />
              </div>

              <div className="form-group">
                <label className="form-label">{labels.category}</label>
                <select 
                  className="form-control"
                  value={category}
                  onChange={(e) => setCategory(Number(e.target.value))}
                >
                  {categories.map((c) => (
                    <option key={c.id} value={c.id}>{c.name}</option>
                  ))}
                </select>
              </div>

              <div style={{ display: 'grid', gridTemplateColumns: 'repeat(2, 1fr)', gap: '1rem' }}>
                <div className="form-group">
                  <label className="form-label">{labels.priceLabel}</label>
                  <input 
                    type="number" 
                    step="0.01"
                    className="form-control" 
                    value={price}
                    onChange={(e) => setPrice(Number(e.target.value))}
                    required 
                  />
                </div>

                <div className="form-group">
                  <label className="form-label">{labels.stockLabel}</label>
                  <input 
                    type="number" 
                    className="form-control" 
                    value={stock}
                    onChange={(e) => setStock(Number(e.target.value))}
                    required 
                  />
                </div>
              </div>

              {/* Image upload selector */}
              <div className="form-group">
                <label className="form-label">{labels.imagesLabel}</label>
                <div style={{
                  border: '2px dashed var(--border-color)',
                  borderRadius: 'var(--radius-md)',
                  padding: '1.5rem',
                  textAlign: 'center',
                  cursor: 'pointer',
                  position: 'relative'
                }}>
                  <Upload size={24} style={{ color: 'var(--text-muted)', marginBottom: '0.5rem' }} />
                  <p style={{ fontSize: '0.85rem', color: 'var(--text-muted)', margin: 0 }}>
                    {images.length > 0 ? `${images.length} files selected` : 'Click to choose files'}
                  </p>
                  <input 
                    type="file" 
                    multiple 
                    onChange={handleFileChange}
                    style={{
                      position: 'absolute',
                      top: 0,
                      left: 0,
                      width: '100%',
                      height: '100%',
                      opacity: 0,
                      cursor: 'pointer'
                    }}
                  />
                </div>
              </div>

              {/* Offer launch option (Add mode only) */}
              {mode === 'add' && (
                <div style={{
                  padding: '1rem',
                  backgroundColor: 'var(--bg-main)',
                  borderRadius: 'var(--radius-md)',
                  border: '1px solid var(--border-color)',
                  display: 'flex',
                  flexDirection: 'column',
                  gap: '0.75rem'
                }}>
                  <label style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', cursor: 'pointer', fontWeight: '600', fontSize: '0.9rem' }}>
                    <input 
                      type="checkbox" 
                      checked={launchOffer}
                      onChange={(e) => setLaunchOffer(e.target.checked)}
                      style={{ width: '18px', height: '18px' }}
                    />
                    <span>{labels.launchOfferToggle}</span>
                  </label>
                  
                  {launchOffer && (
                    <div style={{ display: 'flex', alignItems: 'center', gap: '1rem', marginTop: '0.25rem' }}>
                      <span style={{ fontSize: '0.85rem', color: 'var(--text-muted)' }}>{labels.launchOfferDiscount}</span>
                      <input 
                        type="number" 
                        min="1"
                        max="99"
                        className="form-control" 
                        value={discountPercent}
                        onChange={(e) => setDiscountPercent(Number(e.target.value))}
                        style={{ maxWidth: '100px', padding: '0.4rem' }}
                      />
                    </div>
                  )}
                </div>
              )}

              {/* Tiered Pricing Section */}
              <div style={{
                padding: '1rem',
                backgroundColor: 'var(--bg-main)',
                borderRadius: 'var(--radius-md)',
                border: '1px solid var(--border-color)',
                display: 'flex',
                flexDirection: 'column',
                gap: '1rem'
              }}>
                <h4 style={{ margin: 0, fontSize: '1rem', color: 'var(--secondary)' }}>{labels.tieredPricing}</h4>
                
                {(() => {
                  const sortedTiers = [...pricingTiers].sort((a, b) => a.minQuantity - b.minQuantity);
                  return sortedTiers.map((tier, index) => {
                    const nextTier = sortedTiers[index + 1];

                    return (
                      <div key={index} style={{ 
                        display: 'flex', 
                        alignItems: 'center', 
                        gap: '1rem',
                        flexWrap: 'wrap',
                        padding: '0.5rem',
                        backgroundColor: 'rgba(0,0,0,0.02)',
                        borderRadius: 'var(--radius-sm)'
                      }}>
                        <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                          <span style={{ fontWeight: 'bold' }}>{labels.tierQuantityFrom}</span>
                          <input 
                            type="number"
                            min="2"
                            className="form-control"
                            value={tier.minQuantity}
                            onChange={(e) => {
                              const newMin = Number(e.target.value);
                              const newTiers = pricingTiers.map(t => t === tier ? { ...t, minQuantity: newMin } : t);
                              setPricingTiers(newTiers);
                            }}
                            style={{ width: '80px', padding: '0.3rem' }}
                          />
                          <span style={{ fontWeight: 'bold' }}>{labels.tierQuantityTo}</span>
                          <input 
                            type="number"
                            min={tier.minQuantity}
                            className="form-control"
                            value={nextTier ? nextTier.minQuantity - 1 : stock}
                            onChange={(e) => {
                              const val = Number(e.target.value);
                              if (nextTier) {
                                const newTiers = pricingTiers.map(t => t === nextTier ? { ...t, minQuantity: val + 1 } : t);
                                setPricingTiers(newTiers);
                              } else {
                                setStock(val);
                              }
                            }}
                            style={{ width: '80px', padding: '0.3rem' }}
                          />
                        </div>
                        
                        <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                          <span style={{ fontWeight: 'bold' }}>{labels.tierPrice}</span>
                          <input 
                            type="number"
                            step="0.01"
                            className="form-control"
                            value={tier.pricePerUnit}
                            onChange={(e) => {
                              const newPrice = Number(e.target.value);
                              const newTiers = pricingTiers.map(t => t === tier ? { ...t, pricePerUnit: newPrice } : t);
                              setPricingTiers(newTiers);
                            }}
                            style={{ width: '100px', padding: '0.3rem' }}
                          />
                        </div>
                        
                        <button 
                          type="button" 
                          onClick={() => {
                            const newTiers = pricingTiers.filter(t => t !== tier);
                            setPricingTiers(newTiers);
                          }}
                          style={{ background: 'none', border: 'none', color: 'var(--color-danger)', cursor: 'pointer', marginLeft: 'auto' }}
                          title={labels.tierRemove}
                        >
                          <Trash2 size={18} />
                        </button>
                      </div>
                    );
                  });
                })()}

                <button 
                  type="button" 
                  className="btn btn-outline btn-sm" 
                  onClick={() => setPricingTiers([...pricingTiers, { minQuantity: 5, pricePerUnit: price }])}
                  style={{ alignSelf: 'flex-start', display: 'flex', alignItems: 'center', gap: '0.4rem' }}
                >
                  <Plus size={14} />
                  {labels.addTier}
                </button>
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

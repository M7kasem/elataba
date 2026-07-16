import React, { useState } from 'react';
import { Offer, Product } from '../../../types';
import apiClient from '../../../api/client';
import { useLanguage } from '../../../context/LanguageContext';
import { useToast } from '../../../context/ToastContext';
import { Plus, Tag, Calendar, Trash2, ShieldAlert } from 'lucide-react';

const copy = {
  ar: {
    title: 'العروض والخصومات',
    subtitle: 'إدارة وتفعيل خصومات خاصة على منتجات محلك لجذب المشترين',
    addOffer: 'عمل عرض خصم جديد',
    discount: 'نسبة الخصم',
    appliesTo: 'ينطبق على',
    allProducts: 'كل بضاعة المحل',
    selectedProducts: 'منتجات معينة فقط (%{count} منتجات)',
    duration: 'فترة العرض',
    from: 'من:',
    to: 'إلى:',
    active: 'ساري حالياً',
    expired: 'منتهي',
    upcoming: 'لم يبدأ بعد',
    deleteConfirm: 'هل أنت متأكد من إلغاء/حذف هذا العرض؟',
    deleteSuccess: 'تم حذف العرض بنجاح!',
    addSuccess: 'تم إنشاء العرض وتفعيله بنجاح!',
    validationError1: 'يرجى إدخال نسبة خصم بين ١٪ و ١٠٠٪.',
    validationError2: 'يجب أن يكون تاريخ البداية قبل تاريخ النهاية.',
    validationError3: 'الرجاء اختيار منتج واحد على الأقل.',
    save: 'تشغيل العرض',
    cancel: 'إلغاء',
    discountLabel: 'نسبة الخصم (%)',
    startDateLabel: 'تاريخ بداية العرض',
    endDateLabel: 'تاريخ نهاية العرض',
    appliesAllToggle: 'تطبيق الخصم على جميع المنتجات في المحل؟',
    selectProductsLabel: 'اختر المنتجات المشمولة في الخصم:',
    noOffers: 'لا توجد عروض خصم حالية في محلك. ابدأ بإنشاء عرض الآن لجذب الزباين!',
    loading: 'جاري الحفظ...',
  },
  en: {
    title: 'Offers & Discounts',
    subtitle: 'Manage and activate special discount campaigns on your products',
    addOffer: 'Create Discount Offer',
    discount: 'Discount',
    appliesTo: 'Applies to',
    allProducts: 'All products',
    selectedProducts: 'Specific products (%{count} items)',
    duration: 'Duration',
    from: 'From:',
    to: 'To:',
    active: 'Active',
    expired: 'Expired',
    upcoming: 'Upcoming',
    deleteConfirm: 'Are you sure you want to delete/cancel this offer?',
    deleteSuccess: 'Offer deleted successfully!',
    addSuccess: 'Offer created successfully!',
    validationError1: 'Discount must be between 1 and 100.',
    validationError2: 'Start date must be before end date.',
    validationError3: 'Please select at least one product.',
    save: 'Launch Offer',
    cancel: 'Cancel',
    discountLabel: 'Discount Percentage (%)',
    startDateLabel: 'Start Date',
    endDateLabel: 'End Date',
    appliesAllToggle: 'Apply this discount to all items in my store?',
    selectProductsLabel: 'Choose products for this discount:',
    noOffers: 'No offers created yet. Create one now to attract buyers!',
    loading: 'Saving...',
  }
};

interface OfferManagementProps {
  storeId: number;
  offers: Offer[];
  products: Product[];
  onRefresh: () => void;
}

export const OfferManagement: React.FC<OfferManagementProps> = ({ storeId, offers, products, onRefresh }) => {
  const { language } = useLanguage();
  const { showToast } = useToast();
  const labels = copy[language];

  const [showModal, setShowModal] = useState(false);
  const [discount, setDiscount] = useState<number>(10);
  const [startDate, setStartDate] = useState('');
  const [endDate, setEndDate] = useState('');
  const [appliesAll, setAppliesAll] = useState(false);
  const [selectedProductIds, setSelectedProductIds] = useState<number[]>([]);
  const [saving, setSaving] = useState(false);

  const resetForm = () => {
    setDiscount(10);
    setStartDate('');
    setEndDate('');
    setAppliesAll(false);
    setSelectedProductIds([]);
  };

  const handleProductToggle = (id: number) => {
    setSelectedProductIds((prev) =>
      prev.includes(id) ? prev.filter((pid) => pid !== id) : [...prev, id]
    );
  };

  const handleDeleteClick = async (id: number) => {
    if (!window.confirm(labels.deleteConfirm)) return;
    try {
      await apiClient.delete(`/api/Offer/${id}`);
      showToast(labels.deleteSuccess, 'success');
      onRefresh();
    } catch (err) {
      console.error('Error deleting offer:', err);
    }
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (discount <= 0 || discount > 100) {
      showToast(labels.validationError1, 'warning');
      return;
    }
    if (!startDate || !endDate || new Date(startDate) >= new Date(endDate)) {
      showToast(labels.validationError2, 'warning');
      return;
    }
    if (!appliesAll && selectedProductIds.length === 0) {
      showToast(labels.validationError3, 'warning');
      return;
    }

    setSaving(true);
    try {
      const payload = {
        storeId,
        discountPercentage: Number(discount),
        startDate: new Date(startDate).toISOString(),
        endDate: new Date(endDate).toISOString(),
        appliesToAllProducts: appliesAll,
        productIds: appliesAll ? [] : selectedProductIds
      };

      await apiClient.post('/api/Offer', payload);
      showToast(labels.addSuccess, 'success');
      setShowModal(false);
      resetForm();
      onRefresh();
    } catch (err) {
      console.error('Error creating offer:', err);
    } finally {
      setSaving(false);
    }
  };

  const getOfferStatus = (start: string, end: string) => {
    const now = new Date();
    const startDateObj = new Date(start);
    const endDateObj = new Date(end);

    if (now > endDateObj) return { text: labels.expired, color: 'var(--text-muted)', bg: 'rgba(0,0,0,0.05)' };
    if (now < startDateObj) return { text: labels.upcoming, color: 'var(--color-info)', bg: 'rgba(0, 180, 216, 0.1)' };
    return { text: labels.active, color: 'var(--color-success)', bg: 'rgba(46, 196, 182, 0.1)' };
  };

  const formatDate = (dateStr: string) => {
    return new Date(dateStr).toLocaleDateString(language === 'ar' ? 'ar-EG' : 'en-US', {
      year: 'numeric',
      month: 'short',
      day: 'numeric'
    });
  };

  return (
    <div style={{ padding: '1rem' }}>
      
      {/* Header */}
      <div style={{ 
        display: 'flex', 
        justifyContent: 'space-between', 
        alignItems: 'center', 
        flexWrap: 'wrap', 
        gap: '1rem',
        marginBottom: '2rem' 
      }}>
        <div>
          <h1 style={{ fontSize: '2rem', fontWeight: 'bold', color: 'var(--secondary)', margin: 0 }}>{labels.title}</h1>
          <p style={{ color: 'var(--text-muted)' }}>{labels.subtitle}</p>
        </div>
        <button 
          className="btn btn-primary"
          onClick={() => {
            resetForm();
            setShowModal(true);
          }}
          style={{ padding: '0.8rem 1.5rem', fontWeight: 'bold', fontSize: '1rem' }}
        >
          <Plus size={20} />
          <span>{labels.addOffer}</span>
        </button>
      </div>

      {/* Offers Grid */}
      {offers.length > 0 ? (
        <div style={{
          display: 'grid',
          gridTemplateColumns: 'repeat(auto-fill, minmax(320px, 1fr))',
          gap: '1.5rem'
        }}>
          {offers.map((offer) => {
            const status = getOfferStatus(offer.startDate, offer.endDate);
            return (
              <div key={offer.id} className="card" style={{ padding: '1.5rem', borderTop: '4px solid var(--primary)', position: 'relative' }}>
                
                {/* Delete Button */}
                <button 
                  onClick={() => handleDeleteClick(offer.id)}
                  style={{
                    position: 'absolute',
                    top: '1rem',
                    right: language === 'ar' ? 'auto' : '1rem',
                    left: language === 'ar' ? '1rem' : 'auto',
                    background: 'none',
                    border: 'none',
                    color: 'var(--color-danger)',
                    cursor: 'pointer',
                    opacity: 0.8,
                    padding: '0.3rem'
                  }}
                  title="Remove offer"
                >
                  <Trash2 size={18} />
                </button>

                {/* Offer Discount Badge & Status */}
                <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.2rem', paddingRight: language === 'ar' ? '0' : '2rem', paddingLeft: language === 'ar' ? '2rem' : '0' }}>
                  <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
                    <Tag size={22} style={{ color: 'var(--primary)' }} />
                    <span style={{ fontSize: '1.5rem', fontWeight: '800', color: 'var(--secondary)' }}>
                      -{offer.discountPercentage}%
                    </span>
                  </div>
                  <span style={{
                    padding: '0.25rem 0.75rem',
                    borderRadius: 'var(--radius-pill)',
                    fontSize: '0.8rem',
                    fontWeight: '700',
                    color: status.color,
                    backgroundColor: status.bg
                  }}>
                    {status.text}
                  </span>
                </div>

                {/* Offer details */}
                <div style={{ display: 'flex', flexDirection: 'column', gap: '0.75rem', fontSize: '0.92rem' }}>
                  <div>
                    <span style={{ color: 'var(--text-muted)', fontWeight: '500' }}>{labels.appliesTo}: </span>
                    <span style={{ fontWeight: '700', color: 'var(--text-main)' }}>
                      {offer.appliesToAllProducts 
                        ? labels.allProducts 
                        : labels.selectedProducts.replace('{count}', String(offer.productIds?.length || 0))
                      }
                    </span>
                  </div>

                  <div style={{ borderTop: '1px solid var(--border-color)', paddingTop: '0.75rem', marginTop: '0.25rem' }}>
                    <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', color: 'var(--text-muted)', marginBottom: '0.3rem' }}>
                      <Calendar size={14} />
                      <span style={{ fontWeight: '600', fontSize: '0.85rem' }}>{labels.duration}</span>
                    </div>
                    <div style={{ paddingLeft: '1.2rem', paddingRight: '1.2rem' }}>
                      <div>{labels.from} <span style={{ fontWeight: '700' }}>{formatDate(offer.startDate)}</span></div>
                      <div>{labels.to} <span style={{ fontWeight: '700' }}>{formatDate(offer.endDate)}</span></div>
                    </div>
                  </div>
                </div>

              </div>
            );
          })}
        </div>
      ) : (
        <div className="card" style={{ padding: '4rem 2rem', textAlign: 'center', color: 'var(--text-muted)' }}>
          <Tag size={48} style={{ marginBottom: '1rem', color: 'var(--text-muted)', opacity: 0.5 }} />
          <p style={{ fontSize: '1.1rem', fontWeight: '600', marginBottom: '1.5rem' }}>{labels.noOffers}</p>
          <button 
            className="btn btn-primary"
            onClick={() => {
              resetForm();
              setShowModal(true);
            }}
          >
            <Plus size={18} />
            <span>{labels.addOffer}</span>
          </button>
        </div>
      )}

      {/* Create Offer Modal */}
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
                {labels.addOffer}
              </h3>
            </div>

            {/* Modal Form */}
            <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '1.2rem' }}>
              
              <div className="form-group">
                <label className="form-label">{labels.discountLabel}</label>
                <input 
                  type="number" 
                  min="1"
                  max="100"
                  className="form-control" 
                  value={discount}
                  onChange={(e) => setDiscount(Number(e.target.value))}
                  required 
                />
              </div>

              <div style={{ display: 'grid', gridTemplateColumns: 'repeat(2, 1fr)', gap: '1rem' }}>
                <div className="form-group">
                  <label className="form-label">{labels.startDateLabel}</label>
                  <input 
                    type="date" 
                    className="form-control" 
                    value={startDate}
                    onChange={(e) => setStartDate(e.target.value)}
                    required 
                  />
                </div>

                <div className="form-group">
                  <label className="form-label">{labels.endDateLabel}</label>
                  <input 
                    type="date" 
                    className="form-control" 
                    value={endDate}
                    onChange={(e) => setEndDate(e.target.value)}
                    required 
                  />
                </div>
              </div>

              {/* Applies to all products checkbox toggle */}
              <div style={{
                padding: '1.2rem',
                backgroundColor: appliesAll ? 'rgba(255, 183, 3, 0.1)' : 'var(--bg-main)',
                borderRadius: 'var(--radius-md)',
                border: appliesAll ? '2px solid var(--primary)' : '1px solid var(--border-color)',
                transition: 'all 0.2s ease',
              }}>
                <label style={{ display: 'flex', alignItems: 'center', gap: '0.75rem', cursor: 'pointer', fontWeight: '700', fontSize: '0.95rem', color: appliesAll ? 'var(--primary-hover)' : 'var(--text-main)' }}>
                  <input 
                    type="checkbox" 
                    checked={appliesAll}
                    onChange={(e) => setAppliesAll(e.target.checked)}
                    style={{ width: '20px', height: '20px', accentColor: 'var(--primary)' }}
                  />
                  <span>{labels.appliesAllToggle}</span>
                </label>

                {/* Product checklist if appliesAll is false */}
                {!appliesAll && (
                  <div style={{ marginTop: '1rem', borderTop: '1px solid var(--border-color)', paddingTop: '0.75rem' }}>
                    <p style={{ fontSize: '0.85rem', fontWeight: '600', color: 'var(--text-muted)', marginBottom: '0.5rem' }}>
                      {labels.selectProductsLabel}
                    </p>
                    <div style={{ maxHeight: '180px', overflowY: 'auto', display: 'flex', flexDirection: 'column', gap: '0.5rem', padding: '0.25rem' }}>
                      {products.map((prod) => (
                        <label key={prod.id} style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', cursor: 'pointer', fontSize: '0.9rem' }}>
                          <input 
                            type="checkbox" 
                            checked={selectedProductIds.includes(prod.id)}
                            onChange={() => handleProductToggle(prod.id)}
                            style={{ width: '16px', height: '16px' }}
                          />
                          <span>{prod.name} (sprice: ${prod.basePrice})</span>
                        </label>
                      ))}
                    </div>
                  </div>
                )}
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

import React, { useState, useEffect } from 'react';
import apiClient from '../../../api/client';
import { toCategories } from '../../../api/normalizers';
import { Category } from '../../../types';
import { useLanguage } from '../../../context/LanguageContext';
import { useToast } from '../../../context/ToastContext';

const copy = {
  ar: {
    title: 'إعدادات المحل',
    subtitle: 'تعديل بيانات المحل والنشاط التجاري الخاص بك',
    storeName: 'اسم المحل / العلامة التجارية',
    category: 'تصنيف المنتجات الرئيسي',
    productLines: 'خطوط الإنتاج والتصنيفات الإضافية (اختياري)',
    location: 'عنوان المحل (مثال: مول القدس، الدور الثاني، مكتب ١٥)',
    description: 'نبذة عن المحل (اكتب اللي بيميز محلك عشان الزباين تعرفه)',
    save: 'حفظ التعديلات',
    success: 'تم تحديث بيانات المحل بنجاح!',
    error: 'فشل في حفظ التعديلات، يرجى المحاولة مرة أخرى.',
    loading: 'جاري تحميل البيانات...',
  },
  en: {
    title: 'Store Settings',
    subtitle: 'Manage your store profile and business details',
    storeName: 'Store Name / Brand',
    category: 'Main Product Category',
    productLines: 'Additional Product Lines / Categories (Optional)',
    location: 'Store Location (e.g., Mall Al-Quds, 2nd Floor, Office 15)',
    description: 'Store Description (Write what makes your shop special)',
    save: 'Save Changes',
    success: 'Store settings updated successfully!',
    error: 'Failed to save changes, please try again.',
    loading: 'Loading store data...',
  }
};

interface StoreSettingsProps {
  storeId: number;
}

export const StoreSettings: React.FC<StoreSettingsProps> = ({ storeId }) => {
  const { language } = useLanguage();
  const { showToast } = useToast();
  const labels = copy[language];

  const [loading, setLoading] = useState(true);
  const [categories, setCategories] = useState<Category[]>([]);
  const [storeName, setStoreName] = useState('');
  const [storeLoc, setStoreLoc] = useState('');
  const [storeDesc, setStoreDesc] = useState('');
  const [storeCatId, setStoreCatId] = useState<number>(0);
  const [productLineIds, setProductLineIds] = useState<number[]>([]);
  const [saving, setSaving] = useState(false);

  useEffect(() => {
    const fetchStoreData = async () => {
      setLoading(true);
      try {
        const [catsRes, storeRes] = await Promise.all([
          apiClient.get('/api/Category/GetAll'),
          apiClient.get(`/api/Store/${storeId}`)
        ]);

        const catList = toCategories(catsRes.data?.data || []);
        setCategories(catList);

        const storeData = storeRes.data?.data;
        if (storeData) {
          setStoreName(storeData.storeName || '');
          setStoreLoc(storeData.location || '');
          setStoreDesc(storeData.description || '');
          setStoreCatId(storeData.categoryId || (catList.length > 0 ? catList[0].id : 0));
          setProductLineIds(storeData.productLineIds || []);
        }
      } catch (err) {
        console.error('Error fetching store settings:', err);
      } finally {
        setLoading(false);
      }
    };

    fetchStoreData();
  }, [storeId]);

  const handleProductLineToggle = (id: number) => {
    setProductLineIds((prev) =>
      prev.includes(id) ? prev.filter((pid) => pid !== id) : [...prev, id]
    );
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);
    try {
      const payload = {
        managerId: null,
        storeName: storeName.trim(),
        categoryId: Number(storeCatId),
        location: storeLoc.trim(),
        description: storeDesc.trim(),
        productLineIds: productLineIds
      };

      await apiClient.put(`/api/Store/${storeId}`, payload);
      showToast(labels.success, 'success');
    } catch (err) {
      console.error('Error updating store settings:', err);
      showToast(labels.error, 'error');
    } finally {
      setSaving(false);
    }
  };

  if (loading) {
    return <div style={{ padding: '2rem', textAlign: 'center' }}>{labels.loading}</div>;
  }

  // Categories that can be selected as additional lines (excluding the primary one)
  const additionalCategories = categories.filter((c) => c.id !== storeCatId);

  return (
    <div style={{ maxWidth: '650px', margin: '0 auto', padding: '1rem' }}>
      <div style={{ marginBottom: '2rem' }}>
        <h1 style={{ fontSize: '2rem', fontWeight: 'bold', color: 'var(--secondary)' }}>{labels.title}</h1>
        <p style={{ color: 'var(--text-muted)' }}>{labels.subtitle}</p>
      </div>

      <div className="card" style={{ padding: '2rem' }}>
        <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '1.5rem' }}>
          
          <div className="form-group">
            <label className="form-label" style={{ fontWeight: '600' }}>{labels.storeName}</label>
            <input 
              type="text" 
              className="form-control" 
              value={storeName}
              onChange={(e) => setStoreName(e.target.value)}
              required 
              style={{ padding: '0.8rem', fontSize: '1rem' }}
            />
          </div>

          <div className="form-group">
            <label className="form-label" style={{ fontWeight: '600' }}>{labels.category}</label>
            <select 
              className="form-control"
              value={storeCatId}
              onChange={(e) => {
                const newCatId = Number(e.target.value);
                setStoreCatId(newCatId);
                // Automatically remove from additional product lines if selected as primary
                setProductLineIds((prev) => prev.filter((id) => id !== newCatId));
              }}
              style={{ padding: '0.8rem', fontSize: '1rem' }}
            >
              {categories.map((c) => (
                <option key={c.id} value={c.id}>{c.name}</option>
              ))}
            </select>
          </div>

          {/* Additional Product Lines Checkboxes */}
          {additionalCategories.length > 0 && (
            <div className="form-group">
              <label className="form-label" style={{ fontWeight: '600' }}>{labels.productLines}</label>
              <div style={{ 
                display: 'grid', 
                gridTemplateColumns: 'repeat(auto-fill, minmax(180px, 1fr))', 
                gap: '0.75rem',
                padding: '1rem',
                backgroundColor: 'var(--bg-main)',
                borderRadius: 'var(--radius-md)',
                border: '1px solid var(--border-color)',
                maxHeight: '180px',
                overflowY: 'auto'
              }}>
                {additionalCategories.map((c) => (
                  <label key={c.id} style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', cursor: 'pointer', fontSize: '0.9rem', userSelect: 'none' }}>
                    <input 
                      type="checkbox"
                      checked={productLineIds.includes(c.id)}
                      onChange={() => handleProductLineToggle(c.id)}
                      style={{ width: '16px', height: '16px', accentColor: 'var(--primary)' }}
                    />
                    <span>{c.name}</span>
                  </label>
                ))}
              </div>
            </div>
          )}

          <div className="form-group">
            <label className="form-label" style={{ fontWeight: '600' }}>{labels.location}</label>
            <input 
              type="text" 
              className="form-control" 
              value={storeLoc}
              onChange={(e) => setStoreLoc(e.target.value)}
              required 
              style={{ padding: '0.8rem', fontSize: '1rem' }}
            />
          </div>

          <div className="form-group">
            <label className="form-label" style={{ fontWeight: '600' }}>{labels.description}</label>
            <textarea 
              className="form-control" 
              value={storeDesc}
              onChange={(e) => setStoreDesc(e.target.value)}
              rows={4} 
              required 
              style={{ padding: '0.8rem', fontSize: '1rem', resize: 'vertical' }}
            />
          </div>

          <button 
            type="submit" 
            className="btn btn-primary" 
            disabled={saving}
            style={{ 
              padding: '1rem', 
              fontSize: '1.1rem', 
              fontWeight: '700', 
              marginTop: '1rem' 
            }}
          >
            {saving ? '...' : labels.save}
          </button>
        </form>
      </div>
    </div>
  );
};

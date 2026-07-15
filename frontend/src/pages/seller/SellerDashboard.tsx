import React, { useState, useEffect, useRef } from 'react';
import apiClient from '../../api/client';
import { useAuth } from '../../context/AuthContext';
import { useToast } from '../../context/ToastContext';
import { Product, Order, Offer, Category, OrderStatus } from '../../types';
import Sidebar from '../../components/Sidebar';
import { 
  Plus, Edit, Trash2, Tag, Calendar, 
  ShoppingBag, Check, X, ShieldAlert, 
  Sparkles, Package, DollarSign, Upload 
} from 'lucide-react';

const SellerDashboard: React.FC = () => {
  const { storeId, userId } = useAuth();
  const { showToast } = useToast();

  const [activeTab, setActiveTab] = useState<'home' | 'products' | 'offers' | 'orders' | 'settings'>('home');
  const [products, setProducts] = useState<Product[]>([]);
  const [orders, setOrders] = useState<Order[]>([]);
  const [offers, setOffers] = useState<Offer[]>([]);
  const [categories, setCategories] = useState<Category[]>([]);
  const [loading, setLoading] = useState(true);

  // Modals / Forms States
  const [showProductModal, setShowProductModal] = useState(false);
  const [prodFormMode, setProdFormMode] = useState<'add' | 'edit'>('add');
  const [editingProdId, setEditingProdId] = useState<number | null>(null);
  
  // Product Form Input State
  const [prodName, setProdName] = useState('');
  const [prodDesc, setProdDesc] = useState('');
  const [prodCategory, setProdCategory] = useState<number>(0);
  const [prodBasePrice, setProdBasePrice] = useState<number>(0);
  const [prodStock, setProdStock] = useState<number>(100);
  const [prodImages, setProdImages] = useState<File[]>([]);
  const [launchWithOptions, setLaunchWithOptions] = useState(false);
  const [launchDiscount, setLaunchDiscount] = useState<number>(10);

  // Offer Form Input State
  const [showOfferModal, setShowOfferModal] = useState(false);
  const [offerDiscount, setOfferDiscount] = useState<number>(10);
  const [offerStart, setOfferStart] = useState('');
  const [offerEnd, setOfferEnd] = useState('');
  const [appliesAll, setAppliesAll] = useState(true);
  const [selectedProductIds, setSelectedProductIds] = useState<number[]>([]);

  // Order Status Modal State
  const [showOrderStatusModal, setShowOrderStatusModal] = useState(false);
  const [updatingOrderId, setUpdatingOrderId] = useState<number | null>(null);
  const [newOrderStatus, setNewOrderStatus] = useState<OrderStatus>(OrderStatus.Pending);
  const [trackingNumber, setTrackingNumber] = useState('');
  const [shippingCost, setShippingCost] = useState<number>(15);

  // Store Settings State
  const [storeName, setStoreName] = useState('');
  const [storeLoc, setStoreLoc] = useState('');
  const [storeDesc, setStoreDesc] = useState('');
  const [storeCatId, setStoreCatId] = useState<number>(0);

  useEffect(() => {
    const loadDashboardData = async () => {
      if (!storeId) return;
      setLoading(true);
      try {
        const [prodRes, ordersRes, offersRes, catsRes, storeRes] = await Promise.all([
          apiClient.get(`/api/Product?storeId=${storeId}`),
          apiClient.get(`/api/Order?storeId=${storeId}`).catch(() => ({ data: { data: [] } })),
          apiClient.get(`/api/Offer?storeId=${storeId}`).catch(() => ({ data: { data: [] } })),
          apiClient.get('/api/Category'),
          apiClient.get(`/api/Store/${storeId}`).catch(() => ({ data: { data: null } }))
        ]);

        // Filter products client-side just in case GET all was returned
        const storeProducts = (prodRes.data?.data || []).filter((p: Product) => p.storeId === storeId);
        setProducts(storeProducts);
        
        // Filter orders for this store
        const storeOrders = (ordersRes.data?.data || []).filter((o: Order) => o.storeId === storeId);
        setOrders(storeOrders);
        
        setOffers(offersRes.data?.data || []);
        
        const catList = catsRes.data?.data || [];
        setCategories(catList);
        if (catList.length > 0) {
          setProdCategory(catList[0].id);
          setStoreCatId(catList[0].id);
        }

        const storeData = storeRes.data?.data;
        if (storeData) {
          setStoreName(storeData.storeName);
          setStoreLoc(storeData.location);
          setStoreDesc(storeData.description);
          setStoreCatId(storeData.categoryId);
        }
      } catch (err) {
        console.error('Error fetching dashboard data:', err);
        showToast('Error loading dashboard analytics.', 'error');
      } finally {
        setLoading(false);
      }
    };

    loadDashboardData();
  }, [storeId, showToast]);

  const handleProductSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (prodBasePrice <= 0 || prodStock < 0) {
      showToast('Validation failed: Price must be > 0 and stock >= 0.', 'warning');
      return;
    }

    try {
      const formData = new FormData();
      formData.append('storeId', String(storeId));
      formData.append('categoryId', String(prodCategory));
      formData.append('name', prodName.trim());
      formData.append('description', prodDesc.trim());
      formData.append('basePrice', String(prodBasePrice));
      formData.append('stockQuantity', String(prodStock));

      // Append image files
      prodImages.forEach((img) => {
        formData.append('images', img);
      });

      // Handle Launch with Offer trigger
      const endpoint = launchWithOptions ? '/api/Product/create-with-offer' : '/api/Product';
      if (launchWithOptions) {
        formData.append('discountPercentage', String(launchDiscount));
        // Add default start/end dates
        formData.append('startDate', new Date().toISOString());
        formData.append('endDate', new Date(Date.now() + 604800000).toISOString()); // 1 week duration
      }

      let response;
      if (prodFormMode === 'add') {
        response = await apiClient.post(endpoint, formData, {
          headers: { 'Content-Type': undefined } // let browser set boundary
        });
        showToast('Product added successfully!', 'success');
      } else {
        response = await apiClient.put(`/api/Product/${editingProdId}`, {
          storeId,
          categoryId: prodCategory,
          name: prodName,
          description: prodDesc,
          basePrice: prodBasePrice,
          stockQuantity: prodStock
        });
        showToast('Product updated successfully!', 'success');
      }

      // Reload products list
      const prodRes = await apiClient.get(`/api/Product?storeId=${storeId}`);
      setProducts((prodRes.data?.data || []).filter((p: Product) => p.storeId === storeId));
      
      setShowProductModal(false);
      resetProductForm();
    } catch (err) {
      console.error('Error saving product:', err);
    }
  };

  const handleEditProductClick = (prod: Product) => {
    setProdFormMode('edit');
    setEditingProdId(prod.id);
    setProdName(prod.name);
    setProdDesc(prod.description);
    setProdCategory(prod.categoryId);
    setProdBasePrice(prod.basePrice);
    setProdStock(prod.stockQuantity);
    setLaunchWithOptions(false);
    setShowProductModal(true);
  };

  const handleDeleteProduct = async (id: number) => {
    if (!window.confirm('Are you sure you want to delete this product?')) return;
    try {
      await apiClient.delete(`/api/Product/${id}`);
      showToast('Product deleted.', 'success');
      setProducts(products.filter(p => p.id !== id));
    } catch (err) {
      console.error('Delete product error:', err);
    }
  };

  const handleOfferSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (offerDiscount <= 0 || offerDiscount > 100) {
      showToast('Discount must be between 1 and 100.', 'warning');
      return;
    }
    if (new Date(offerStart) >= new Date(offerEnd)) {
      showToast('Start date must be before end date.', 'warning');
      return;
    }

    try {
      const payload = {
        storeId,
        discountPercentage: Number(offerDiscount),
        startDate: new Date(offerStart).toISOString(),
        endDate: new Date(offerEnd).toISOString(),
        appliesToAllProducts: appliesAll,
        productIds: appliesAll ? [] : selectedProductIds
      };

      await apiClient.post('/api/Offer', payload);
      showToast('Offer created successfully!', 'success');
      
      // Reload offers
      const offersRes = await apiClient.get(`/api/Offer?storeId=${storeId}`);
      setOffers(offersRes.data?.data || []);
      setShowOfferModal(false);
    } catch (err) {
      console.error('Create offer error:', err);
    }
  };

  const handleOrderStatusUpdate = async (e: React.FormEvent) => {
    e.preventDefault();
    if (updatingOrderId === null) return;

    try {
      const payload = {
        orderStatus: Number(newOrderStatus),
        trackingNumber: trackingNumber.trim() || null,
        carrierId: null, // carrier assigned automatically
        shippingCost: Number(shippingCost)
      };

      // Put request to update order status
      await apiClient.put(`/api/Order/${updatingOrderId}/status`, payload);
      showToast('Order status updated successfully!', 'success');

      // Reload orders
      const ordersRes = await apiClient.get(`/api/Order?storeId=${storeId}`);
      setOrders((ordersRes.data?.data || []).filter((o: Order) => o.storeId === storeId));
      setShowOrderStatusModal(false);
    } catch (err) {
      console.error('Order status update error:', err);
    }
  };

  const handleStoreSettingsSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const payload = {
        storeName: storeName.trim(),
        categoryId: Number(storeCatId),
        location: storeLoc.trim(),
        description: storeDesc.trim(),
        ownerId: userId || 0
      };

      await apiClient.put(`/api/Store/${storeId}`, payload);
      showToast('Store settings updated!', 'success');
    } catch (err) {
      console.error('Store settings update error:', err);
    }
  };

  const resetProductForm = () => {
    setProdName('');
    setProdDesc('');
    setProdBasePrice(0);
    setProdStock(100);
    setProdImages([]);
    setLaunchWithOptions(false);
  };

  const getOrderStatusLabel = (status: OrderStatus) => {
    switch (status) {
      case OrderStatus.Pending: return 'Pending';
      case OrderStatus.Confirmed: return 'Confirmed';
      case OrderStatus.Shipped: return 'Shipped';
      case OrderStatus.Delivered: return 'Delivered';
      case OrderStatus.Cancelled: return 'Cancelled';
      default: return 'Unknown';
    }
  };

  if (loading) {
    return (
      <div style={{ display: 'flex', minHeight: '100vh' }}>
        <div className="skeleton" style={{ width: '260px', height: '100vh' }} />
        <div style={{ flex: 1, padding: '2rem' }}>
          <div className="skeleton" style={{ width: '30%', height: '40px', marginBottom: '2rem' }} />
          <div className="skeleton" style={{ width: '100%', height: '400px' }} />
        </div>
      </div>
    );
  }

  return (
    <div className="dashboard-layout">
      {/* Navigation Sidebar */}
      <div style={{ width: '260px', flexShrink: 0 }}>
        <div style={{ position: 'sticky', top: 0, height: '100vh' }}>
          {/* Custom sidebar trigger wrapper */}
          <div className="sidebar-container" style={{ height: '100%' }}>
            <Sidebar type="seller" />
          </div>
        </div>
      </div>

      {/* Main Panel Content */}
      <div className="dashboard-content">
        
        {/* TAB 1: Dashboard Home / Stats */}
        {activeTab === 'home' && (
          <div>
            <div style={{ marginBottom: '2rem' }}>
              <h1 style={{ fontSize: '2rem', marginBottom: '0.5rem' }}>Store Analytics Dashboard</h1>
              <p style={{ color: 'var(--text-muted)' }}>Overview of your wholesale shop activity</p>
            </div>

            {/* Quick Metrics Grid */}
            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(220px, 1fr))', gap: '1.5rem', marginBottom: '3rem' }}>
              <div className="card" style={{ padding: '1.5rem', display: 'flex', alignItems: 'center', gap: '1rem', borderLeft: '4px solid var(--primary)' }}>
                <div style={{ fontSize: '2rem' }}>📦</div>
                <div>
                  <span style={{ fontSize: '0.85rem', color: 'var(--text-muted)' }}>Listed Products</span>
                  <h3 style={{ fontSize: '1.8rem' }}>{products.length}</h3>
                </div>
              </div>

              <div className="card" style={{ padding: '1.5rem', display: 'flex', alignItems: 'center', gap: '1rem', borderLeft: '4px solid var(--color-success)' }}>
                <div style={{ fontSize: '2rem' }}>💰</div>
                <div>
                  <span style={{ fontSize: '0.85rem', color: 'var(--text-muted)' }}>Store Orders</span>
                  <h3 style={{ fontSize: '1.8rem' }}>{orders.length}</h3>
                </div>
              </div>

              <div className="card" style={{ padding: '1.5rem', display: 'flex', alignItems: 'center', gap: '1rem', borderLeft: '4px solid var(--color-warning)' }}>
                <div style={{ fontSize: '2rem' }}>🏷️</div>
                <div>
                  <span style={{ fontSize: '0.85rem', color: 'var(--text-muted)' }}>Active Offers</span>
                  <h3 style={{ fontSize: '1.8rem' }}>{offers.length}</h3>
                </div>
              </div>
            </div>

            {/* Recent Orders Overview */}
            <div className="card" style={{ padding: '1.5rem' }}>
              <h3 style={{ fontSize: '1.2rem', marginBottom: '1rem' }}>Recent Store Orders</h3>
              {orders.length > 0 ? (
                <div style={{ overflowX: 'auto' }}>
                  <table style={{ width: '100%', borderCollapse: 'collapse', fontSize: '0.9rem' }}>
                    <thead>
                      <tr style={{ borderBottom: '2px solid var(--border-color)', textAlign: 'left', backgroundColor: 'var(--bg-main)' }}>
                        <th style={{ padding: '0.75rem' }}>Order ID</th>
                        <th style={{ padding: '0.75rem' }}>Buyer Email</th>
                        <th style={{ padding: '0.75rem' }}>Amount</th>
                        <th style={{ padding: '0.75rem' }}>Status</th>
                      </tr>
                    </thead>
                    <tbody>
                      {orders.slice(0, 5).map((o) => (
                        <tr key={o.id} style={{ borderBottom: '1px solid var(--border-color)' }}>
                          <td style={{ padding: '0.75rem', fontWeight: 'bold' }}>#{o.id}</td>
                          <td style={{ padding: '0.75rem' }}>{o.buyerEmail || 'Buyer'}</td>
                          <td style={{ padding: '0.75rem', fontWeight: 'bold' }}>${o.totalAmount.toFixed(2)}</td>
                          <td style={{ padding: '0.75rem' }}>{getOrderStatusLabel(o.orderStatus)}</td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              ) : (
                <p style={{ color: 'var(--text-muted)' }}>No orders placed for your store items yet.</p>
              )}
            </div>
          </div>
        )}

        {/* TAB 2: Product Management panel */}
        {activeTab === 'products' && (
          <div>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '2rem' }}>
              <div>
                <h1 style={{ fontSize: '1.8rem', margin: 0 }}>Product Inventory</h1>
                <p style={{ color: 'var(--text-muted)' }}>Add and manage your wholesale listings</p>
              </div>
              <button 
                className="btn btn-primary"
                onClick={() => {
                  setProdFormMode('add');
                  resetProductForm();
                  setShowProductModal(true);
                }}
                style={{ gap: '0.4rem' }}
              >
                <Plus size={18} />
                <span>Add Product</span>
              </button>
            </div>

            {/* Products Table */}
            <div className="card" style={{ padding: 0, overflow: 'hidden' }}>
              <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left' }}>
                <thead>
                  <tr style={{ backgroundColor: 'var(--bg-main)', borderBottom: '2px solid var(--border-color)', fontSize: '0.85rem' }}>
                    <th style={{ padding: '1rem' }}>Product details</th>
                    <th style={{ padding: '1rem' }}>Category</th>
                    <th style={{ padding: '1rem' }}>Wholesale Base Price</th>
                    <th style={{ padding: '1rem' }}>Stock Quantity</th>
                    <th style={{ padding: '1rem' }}>Active Offer</th>
                    <th style={{ padding: '1rem', textAlign: 'center' }}>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {products.map((prod) => (
                    <tr key={prod.id} style={{ borderBottom: '1px solid var(--border-color)', fontSize: '0.92rem' }}>
                      <td style={{ padding: '1rem', fontWeight: 600 }}>{prod.name}</td>
                      <td style={{ padding: '1rem' }}>{prod.categoryName}</td>
                      <td style={{ padding: '1rem', fontWeight: 'bold' }}>${prod.basePrice}</td>
                      <td style={{ padding: '1rem' }}>{prod.stockQuantity} units</td>
                      <td style={{ padding: '1rem' }}>
                        {prod.hasActiveOffer ? (
                          <span style={{ color: 'var(--color-danger)', fontWeight: 'bold' }}>Active (-{prod.discountPercentage}%)</span>
                        ) : (
                          <span style={{ color: 'var(--text-muted)' }}>None</span>
                        )}
                      </td>
                      <td style={{ padding: '1rem', textAlign: 'center', display: 'flex', gap: '0.5rem', justifyContent: 'center' }}>
                        <button 
                          className="btn btn-outline btn-sm"
                          onClick={() => handleEditProductClick(prod)}
                          title="Edit product"
                        >
                          <Edit size={14} />
                        </button>
                        <button 
                          className="btn btn-sm btn-danger"
                          onClick={() => handleDeleteProduct(prod.id)}
                          title="Delete product"
                        >
                          <Trash2 size={14} />
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            {/* ADD/EDIT PRODUCT MODAL FORM */}
            {showProductModal && (
              <div style={{
                position: 'fixed',
                top: 0,
                left: 0,
                width: '100%',
                height: '100%',
                backgroundColor: 'rgba(0,0,0,0.6)',
                zIndex: 1000,
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                padding: '2rem'
              }}>
                <div className="card" style={{ width: '100%', maxWidth: '550px', maxHeight: '90vh', overflowY: 'auto' }}>
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem', borderBottom: '1px solid var(--border-color)', paddingBottom: '0.5rem' }}>
                    <h3 style={{ fontSize: '1.25rem' }}>{prodFormMode === 'add' ? 'Create Product Listing' : 'Edit Product'}</h3>
                    <button onClick={() => setShowProductModal(false)} style={{ background: 'none', border: 'none', fontSize: '1.5rem', cursor: 'pointer' }}>&times;</button>
                  </div>

                  <form onSubmit={handleProductSubmit}>
                    <div className="form-group">
                      <label className="form-label">Product Name</label>
                      <input 
                        type="text" 
                        className="form-control" 
                        value={prodName}
                        onChange={(e) => setProdName(e.target.value)}
                        required 
                      />
                    </div>

                    <div className="form-group">
                      <label className="form-label">Category</label>
                      <select 
                        className="form-control"
                        value={prodCategory}
                        onChange={(e) => setProdCategory(Number(e.target.value))}
                      >
                        {categories.map((c) => (
                          <option key={c.id} value={c.id}>{c.name}</option>
                        ))}
                      </select>
                    </div>

                    <div style={{ display: 'grid', gridTemplateColumns: 'repeat(2, 1fr)', gap: '1rem' }}>
                      <div className="form-group">
                        <label className="form-label">Base Price ($)</label>
                        <input 
                          type="number" 
                          className="form-control" 
                          value={prodBasePrice}
                          onChange={(e) => setProdBasePrice(Number(e.target.value))}
                          min={0.01}
                          step={0.01}
                          required 
                        />
                      </div>
                      <div className="form-group">
                        <label className="form-label">Stock Inventory</label>
                        <input 
                          type="number" 
                          className="form-control" 
                          value={prodStock}
                          onChange={(e) => setProdStock(Number(e.target.value))}
                          min={0}
                          required 
                        />
                      </div>
                    </div>

                    <div className="form-group">
                      <label className="form-label">Description</label>
                      <textarea 
                        className="form-control" 
                        value={prodDesc}
                        onChange={(e) => setProdDesc(e.target.value)}
                        rows={3} 
                        required 
                      />
                    </div>

                    {/* Image selector */}
                    {prodFormMode === 'add' && (
                      <div className="form-group" style={{ marginBottom: '1.5rem' }}>
                        <label className="form-label">Product Images</label>
                        <input
                          type="file"
                          multiple
                          onChange={(e) => {
                            if (e.target.files) {
                              setProdImages(Array.from(e.target.files));
                            }
                          }}
                          accept="image/*"
                          style={{ fontSize: '0.85rem' }}
                        />
                      </div>
                    )}

                    {/* Launch with active offer toggle */}
                    {prodFormMode === 'add' && (
                      <div style={{ marginBottom: '2rem', border: '1px solid var(--border-color)', padding: '1rem', borderRadius: 'var(--radius-md)', backgroundColor: 'var(--bg-main)' }}>
                        <label style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', cursor: 'pointer', fontWeight: 600 }}>
                          <input 
                            type="checkbox"
                            checked={launchWithOptions}
                            onChange={(e) => setLaunchWithOptions(e.target.checked)}
                            style={{ accentColor: 'var(--primary)' }}
                          />
                          <span>Launch with promotional offer discount</span>
                        </label>
                        {launchWithOptions && (
                          <div className="form-group" style={{ marginTop: '1rem' }}>
                            <label className="form-label">Discount Percentage (%)</label>
                            <input 
                              type="number" 
                              className="form-control" 
                              value={launchDiscount} 
                              onChange={(e) => setLaunchDiscount(Number(e.target.value))}
                              min={1}
                              max={100}
                            />
                          </div>
                        )}
                      </div>
                    )}

                    <button type="submit" className="btn btn-primary" style={{ width: '100%' }}>
                      {prodFormMode === 'add' ? 'Publish Listing' : 'Save Changes'}
                    </button>
                  </form>
                </div>
              </div>
            )}
          </div>
        )}

        {/* TAB 3: Offer Management panel */}
        {activeTab === 'offers' && (
          <div>
            <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '2rem' }}>
              <div>
                <h1 style={{ fontSize: '1.8rem', margin: 0 }}>Offers & Promotions</h1>
                <p style={{ color: 'var(--text-muted)' }}>Configure store-wide or product-specific discounts</p>
              </div>
              <button 
                className="btn btn-primary"
                onClick={() => {
                  setShowOfferModal(true);
                }}
                style={{ gap: '0.4rem' }}
              >
                <Plus size={18} />
                <span>Create Offer</span>
              </button>
            </div>

            {/* List of Offers */}
            {offers.length > 0 ? (
              <div style={{ display: 'flex', flexDirection: 'column', gap: '1.5rem' }}>
                {offers.map((off) => (
                  <div key={off.id} className="card" style={{ padding: '1.5rem', display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: '1rem' }}>
                    <div>
                      <div style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', marginBottom: '0.5rem' }}>
                        <span className="badge badge-delivered" style={{ fontSize: '0.8rem', padding: '0.3rem 0.6rem' }}>
                          -{off.discountPercentage}% Discount
                        </span>
                        <span style={{ fontWeight: 'bold' }}>
                          {off.appliesToAllProducts ? 'Store-Wide Offer' : 'Selected Products Offer'}
                        </span>
                      </div>
                      <div style={{ fontSize: '0.85rem', color: 'var(--text-muted)', display: 'flex', gap: '1rem' }}>
                        <span>Start: {new Date(off.startDate).toLocaleDateString()}</span>
                        <span>End: {new Date(off.endDate).toLocaleDateString()}</span>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            ) : (
              <div className="card" style={{ padding: '4rem 2rem', textAlign: 'center', color: 'var(--text-muted)' }}>
                <Tag size={48} style={{ marginBottom: '1rem' }} />
                <h3>No promotions configured</h3>
                <p>Add discounts to attract buyers looking for wholesale deals.</p>
              </div>
            )}

            {/* CREATE OFFER MODAL */}
            {showOfferModal && (
              <div style={{
                position: 'fixed',
                top: 0,
                left: 0,
                width: '100%',
                height: '100%',
                backgroundColor: 'rgba(0,0,0,0.6)',
                zIndex: 1000,
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                padding: '2rem'
              }}>
                <div className="card" style={{ width: '100%', maxWidth: '500px' }}>
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem', borderBottom: '1px solid var(--border-color)', paddingBottom: '0.5rem' }}>
                    <h3 style={{ fontSize: '1.25rem' }}>Create Store Discount Offer</h3>
                    <button onClick={() => setShowOfferModal(false)} style={{ background: 'none', border: 'none', fontSize: '1.5rem', cursor: 'pointer' }}>&times;</button>
                  </div>

                  <form onSubmit={handleOfferSubmit}>
                    <div className="form-group">
                      <label className="form-label">Discount Percentage (%)</label>
                      <input 
                        type="number" 
                        className="form-control" 
                        value={offerDiscount}
                        onChange={(e) => setOfferDiscount(Number(e.target.value))}
                        min={1}
                        max={100}
                        required 
                      />
                    </div>

                    <div style={{ display: 'grid', gridTemplateColumns: 'repeat(2, 1fr)', gap: '1rem' }}>
                      <div className="form-group">
                        <label className="form-label">Start Date</label>
                        <input 
                          type="datetime-local" 
                          className="form-control" 
                          value={offerStart}
                          onChange={(e) => setOfferStart(e.target.value)}
                          required 
                        />
                      </div>
                      <div className="form-group">
                        <label className="form-label">End Date</label>
                        <input 
                          type="datetime-local" 
                          className="form-control" 
                          value={offerEnd}
                          onChange={(e) => setOfferEnd(e.target.value)}
                          required 
                        />
                      </div>
                    </div>

                    <div className="form-group" style={{ marginBottom: '2rem' }}>
                      <label style={{ display: 'flex', alignItems: 'center', gap: '0.5rem', cursor: 'pointer', fontWeight: 600 }}>
                        <input 
                          type="checkbox"
                          checked={appliesAll}
                          onChange={(e) => setAppliesAll(e.target.checked)}
                          style={{ accentColor: 'var(--primary)' }}
                        />
                        <span>Apply discount to all store products</span>
                      </label>
                      
                      {!appliesAll && (
                        <div style={{ marginTop: '1rem', border: '1px solid var(--border-color)', borderRadius: 'var(--radius-sm)', maxHeight: '150px', overflowY: 'auto', padding: '0.5rem' }}>
                          <span style={{ fontSize: '0.8rem', fontWeight: 'bold', display: 'block', marginBottom: '0.5rem' }}>Select Products:</span>
                          {products.map((p) => (
                            <label key={p.id} style={{ display: 'flex', alignItems: 'center', gap: '0.4rem', fontSize: '0.85rem', marginBottom: '0.3rem', cursor: 'pointer' }}>
                              <input
                                type="checkbox"
                                checked={selectedProductIds.includes(p.id)}
                                onChange={(e) => {
                                  if (e.target.checked) {
                                    setSelectedProductIds([...selectedProductIds, p.id]);
                                  } else {
                                    setSelectedProductIds(selectedProductIds.filter(id => id !== p.id));
                                  }
                                }}
                              />
                              <span>{p.name}</span>
                            </label>
                          ))}
                        </div>
                      )}
                    </div>

                    <button type="submit" className="btn btn-primary" style={{ width: '100%' }}>
                      Create Offer
                    </button>
                  </form>
                </div>
              </div>
            )}
          </div>
        )}

        {/* TAB 4: Seller Orders panel */}
        {activeTab === 'orders' && (
          <div>
            <div style={{ marginBottom: '2rem' }}>
              <h1 style={{ fontSize: '1.8rem', margin: 0 }}>Store Orders</h1>
              <p style={{ color: 'var(--text-muted)' }}>Manage incoming buyer requests and ship items</p>
            </div>

            {orders.length > 0 ? (
              <div className="card" style={{ padding: 0, overflow: 'hidden' }}>
                <table style={{ width: '100%', borderCollapse: 'collapse', textAlign: 'left' }}>
                  <thead>
                    <tr style={{ backgroundColor: 'var(--bg-main)', borderBottom: '2px solid var(--border-color)', fontSize: '0.85rem' }}>
                      <th style={{ padding: '1rem' }}>Order ID</th>
                      <th style={{ padding: '1rem' }}>Buyer Address Snapshot</th>
                      <th style={{ padding: '1rem' }}>Grand Total</th>
                      <th style={{ padding: '1rem' }}>Carrier Info / Shipping</th>
                      <th style={{ padding: '1rem' }}>Status</th>
                      <th style={{ padding: '1rem', textAlign: 'center' }}>Actions</th>
                    </tr>
                  </thead>
                  <tbody>
                    {orders.map((o) => (
                      <tr key={o.id} style={{ borderBottom: '1px solid var(--border-color)', fontSize: '0.9rem' }}>
                        <td style={{ padding: '1rem', fontWeight: 'bold' }}>#{o.id}</td>
                        <td style={{ padding: '1rem', maxWidth: '220px', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
                          {o.shippingAddressSnapshot}
                        </td>
                        <td style={{ padding: '1rem', fontWeight: 'bold', color: 'var(--secondary-hover)' }}>
                          ${o.totalAmount.toFixed(2)}
                        </td>
                        <td style={{ padding: '1rem' }}>
                          <span style={{ fontSize: '0.8rem', display: 'block' }}>Tracking: {o.trackingNumber || 'Not Shipped'}</span>
                          <span style={{ fontSize: '0.8rem', color: 'var(--text-muted)' }}>Shipping cost: ${o.shippingCost}</span>
                        </td>
                        <td style={{ padding: '1rem' }}>
                          <span className={`badge ${
                            o.orderStatus === 3 ? 'badge-delivered' : o.orderStatus === 4 ? 'badge-cancelled' : 'badge-pending'
                          }`}>
                            {getOrderStatusLabel(o.orderStatus)}
                          </span>
                        </td>
                        <td style={{ padding: '1rem', textAlign: 'center' }}>
                          <button 
                            className="btn btn-outline btn-sm"
                            onClick={() => {
                              setUpdatingOrderId(o.id);
                              setNewOrderStatus(o.orderStatus);
                              setTrackingNumber(o.trackingNumber || '');
                              setShippingCost(o.shippingCost);
                              setShowOrderStatusModal(true);
                            }}
                          >
                            Update Status
                          </button>
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            ) : (
              <div className="card" style={{ padding: '4rem 2rem', textAlign: 'center', color: 'var(--text-muted)' }}>
                <ShoppingBag size={48} style={{ marginBottom: '1rem' }} />
                <h3>No orders placed yet</h3>
                <p>Incoming orders from buyers will appear here.</p>
              </div>
            )}

            {/* ORDER STATUS UPDATE MODAL */}
            {showOrderStatusModal && (
              <div style={{
                position: 'fixed',
                top: 0,
                left: 0,
                width: '100%',
                height: '100%',
                backgroundColor: 'rgba(0,0,0,0.6)',
                zIndex: 1000,
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                padding: '2rem'
              }}>
                <div className="card" style={{ width: '100%', maxWidth: '450px' }}>
                  <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', marginBottom: '1.5rem', borderBottom: '1px solid var(--border-color)', paddingBottom: '0.5rem' }}>
                    <h3 style={{ fontSize: '1.25rem' }}>Update Order Status</h3>
                    <button onClick={() => setShowOrderStatusModal(false)} style={{ background: 'none', border: 'none', fontSize: '1.5rem', cursor: 'pointer' }}>&times;</button>
                  </div>

                  <form onSubmit={handleOrderStatusUpdate}>
                    <div className="form-group">
                      <label className="form-label">Order Status</label>
                      <select 
                        className="form-control"
                        value={newOrderStatus}
                        onChange={(e) => setNewOrderStatus(Number(e.target.value) as OrderStatus)}
                      >
                        <option value={OrderStatus.Pending}>Pending</option>
                        <option value={OrderStatus.Confirmed}>Confirmed</option>
                        <option value={OrderStatus.Shipped}>Shipped</option>
                        <option value={OrderStatus.Delivered}>Delivered</option>
                        <option value={OrderStatus.Cancelled}>Cancelled</option>
                      </select>
                    </div>

                    <div className="form-group">
                      <label className="form-label">Tracking Number</label>
                      <input 
                        type="text" 
                        className="form-control" 
                        value={trackingNumber}
                        onChange={(e) => setTrackingNumber(e.target.value)}
                        placeholder="e.g. TRK123456789"
                      />
                    </div>

                    <div className="form-group" style={{ marginBottom: '2rem' }}>
                      <label className="form-label">Shipping Cost ($)</label>
                      <input 
                        type="number" 
                        className="form-control" 
                        value={shippingCost}
                        onChange={(e) => setShippingCost(Number(e.target.value))}
                        min={0}
                      />
                    </div>

                    <button type="submit" className="btn btn-primary" style={{ width: '100%' }}>
                      Save Status
                    </button>
                  </form>
                </div>
              </div>
            )}
          </div>
        )}

        {/* TAB 5: Store Settings panel */}
        {activeTab === 'settings' && (
          <div className="card" style={{ maxWidth: '650px', padding: '2rem' }}>
            <h2 style={{ fontSize: '1.4rem', marginBottom: '1.5rem', borderBottom: '1px solid var(--border-color)', paddingBottom: '0.5rem' }}>
              Store Management Settings
            </h2>

            <form onSubmit={handleStoreSettingsSubmit}>
              <div className="form-group">
                <label className="form-label">Wholesale Store Name</label>
                <input 
                  type="text" 
                  className="form-control" 
                  value={storeName}
                  onChange={(e) => setStoreName(e.target.value)}
                  required 
                />
              </div>

              <div className="form-group">
                <label className="form-label">Location / Mall Building address</label>
                <input 
                  type="text" 
                  className="form-control" 
                  value={storeLoc}
                  onChange={(e) => setStoreLoc(e.target.value)}
                  required 
                />
              </div>

              <div className="form-group">
                <label className="form-label">Store Description</label>
                <textarea 
                  className="form-control" 
                  value={storeDesc}
                  onChange={(e) => setStoreDesc(e.target.value)}
                  rows={3} 
                  required 
                />
              </div>

              <button type="submit" className="btn btn-primary" style={{ width: '100%', marginTop: '1rem' }}>
                Save Settings
              </button>
            </form>
          </div>
        )}

        {/* Floating Sidebar Toggle Helper (For Desktop layout switching) */}
        <div style={{
          position: 'fixed',
          top: '1rem',
          right: '5rem',
          zIndex: 99,
          display: 'flex',
          gap: '0.5rem',
          backgroundColor: 'var(--bg-card)',
          padding: '0.4rem',
          borderRadius: 'var(--radius-pill)',
          boxShadow: 'var(--shadow-sm)',
          border: '1px solid var(--border-color)'
        }}>
          <button className={`btn btn-sm ${activeTab === 'home' ? 'btn-primary' : 'btn-outline'}`} onClick={() => setActiveTab('home')}>Stats</button>
          <button className={`btn btn-sm ${activeTab === 'products' ? 'btn-primary' : 'btn-outline'}`} onClick={() => setActiveTab('products')}>Products</button>
          <button className={`btn btn-sm ${activeTab === 'offers' ? 'btn-primary' : 'btn-outline'}`} onClick={() => setActiveTab('offers')}>Offers</button>
          <button className={`btn btn-sm ${activeTab === 'orders' ? 'btn-primary' : 'btn-outline'}`} onClick={() => setActiveTab('orders')}>Orders</button>
          <button className={`btn btn-sm ${activeTab === 'settings' ? 'btn-primary' : 'btn-outline'}`} onClick={() => setActiveTab('settings')}>Store Settings</button>
        </div>

      </div>
    </div>
  );
};

export default SellerDashboard;

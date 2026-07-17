import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { useToast } from '../context/ToastContext';
import apiClient from '../api/client';
import { toGovernorates } from '../api/normalizers';
import { User, Governorate } from '../types';
import { User as UserIcon, Mail, Phone, MapPin, CheckCircle, ShieldAlert, Camera } from 'lucide-react';

const getAbsoluteImageUrl = (url: string) => {
  if (!url) return '';
  if (url.startsWith('http://') || url.startsWith('https://') || url.startsWith('data:')) {
    return url;
  }
  return `http://localhost:5191${url.startsWith('/') ? '' : '/'}${url}`;
};

const Profile: React.FC = () => {
  const { user, userId, email, fetchProfile, logout, role } = useAuth();
  const { showToast } = useToast();
  const navigate = useNavigate();

  const [governorates, setGovernorates] = useState<Governorate[]>([]);
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [phone, setPhone] = useState('');
  const [governorateId, setGovernorateId] = useState<number>(0);
  const [city, setCity] = useState('');
  const [shippingAddress, setShippingAddress] = useState('');
  const [profilePictureUrl, setProfilePictureUrl] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [uploadingPic, setUploadingPic] = useState(false);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    if (userId === null) return;

    const loadProfileData = async () => {
      setLoading(true);
      try {
        const [profile, govsResponse] = await Promise.all([
          fetchProfile(),
          apiClient.get('/api/Governorate')
        ]);
        
        setGovernorates(toGovernorates(govsResponse.data?.data || []));

        if (profile) {
          setFirstName(profile.firstName);
          setLastName(profile.lastName);
          setPhone(profile.phone);
          setGovernorateId(profile.governorateId);
          setCity(profile.city);
          setShippingAddress(profile.shippingAddress);
          setProfilePictureUrl(profile.profilePictureUrl || null);
        }
      } catch (err) {
        console.error('Error loading profile info:', err);
        showToast('Failed to load profile details.', 'error');
      } finally {
        setLoading(false);
      }
    };

    loadProfileData();
  }, [userId, fetchProfile, showToast]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsSubmitting(true);
    try {
      if (userId === null || email === null || role === null) {
        throw new Error('Your session is incomplete. Please log in again.');
      }

      const payload = {
        email,
        firstName: firstName.trim(),
        lastName: lastName.trim(),
        phone: phone.trim(),
        role,
        governorateId: Number(governorateId),
        city: city.trim(),
        shippingAddress: shippingAddress.trim()
      };

      await apiClient.put(`/api/User/${userId}`, payload);
      showToast('Profile updated successfully!', 'success');
      await fetchProfile(); // Reload
    } catch (err) {
      console.error('Error updating profile:', err);
    } finally {
      setIsSubmitting(false);
    }
  };

  const getRoleName = () => {
    if (role === null) return 'Visitor';
    switch (role) {
      case 0: return 'Buyer (مشتري جملة)';
      case 1: return 'Seller (تاجر)';
      case 2: return 'Admin (مدير النظام)';
      case 3: return 'Store Manager (مدير متجر)';
      default: return 'User';
    }
  };

  const handleProfilePictureUpload = async (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file || !userId) return;

    setUploadingPic(true);
    const formData = new FormData();
    formData.append('file', file);

    try {
      const res = await apiClient.put(`/api/User/${userId}/profile-picture`, formData, {
        headers: { 'Content-Type': 'multipart/form-data' }
      });
      if (res.data?.data) {
        setProfilePictureUrl(res.data.data);
        showToast('Profile picture updated successfully', 'success');
        await fetchProfile();
      }
    } catch (err) {
      console.error('Error uploading profile picture:', err);
      showToast('Failed to upload profile picture.', 'error');
    } finally {
      setUploadingPic(false);
    }
  };

  if (loading) {
    return (
      <div className="main-content" style={{ padding: '4rem' }}>
        <div className="skeleton" style={{ width: '200px', height: '30px', marginBottom: '2rem' }} />
        <div className="skeleton" style={{ width: '100%', height: '300px' }} />
      </div>
    );
  }

  return (
    <div className="main-content" style={{ padding: '2rem 4rem', maxWidth: '800px', margin: '0 auto' }}>
      <h1 style={{ fontSize: '2.2rem', marginBottom: '2rem', display: 'flex', alignItems: 'center', gap: '0.75rem' }}>
        <UserIcon size={28} />
        <span>My Account Settings</span>
      </h1>

      <div style={{ display: 'flex', gap: '2rem', flexWrap: 'wrap' }}>
        {/* Left Card: Summary details */}
        <div style={{ flex: '1 1 250px' }}>
          <div className="card" style={{ padding: '1.5rem', textAlign: 'center', display: 'flex', flexDirection: 'column', alignItems: 'center', gap: '1rem' }}>
            <div className="image-upload-container" style={{
              position: 'relative',
              width: '100px',
              height: '100px',
              borderRadius: '50%',
              backgroundColor: 'rgba(255, 183, 3, 0.1)',
              color: 'var(--primary-hover)',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              fontSize: '2rem',
              fontWeight: 'bold',
              overflow: 'hidden',
              border: '2px solid var(--border-color)'
            }}>
              {profilePictureUrl ? (
                <img src={getAbsoluteImageUrl(profilePictureUrl)} alt="Profile" style={{ width: '100%', height: '100%', objectFit: 'cover' }} />
              ) : (
                <>{firstName?.[0]}{lastName?.[0]}</>
              )}

              <label className="image-upload-overlay" style={{
                cursor: uploadingPic ? 'default' : 'pointer',
                opacity: uploadingPic ? 0.5 : undefined,
              }}>
                <Camera size={24} />
                <input 
                  type="file" 
                  accept="image/*" 
                  style={{ display: 'none' }} 
                  onChange={handleProfilePictureUpload}
                  disabled={uploadingPic}
                />
              </label>
            </div>
            
            <div>
              <h3 style={{ fontSize: '1.2rem', margin: 0 }}>{firstName} {lastName}</h3>
              <span className="badge badge-confirmed" style={{ marginTop: '0.5rem', fontSize: '0.8rem' }}>
                {getRoleName()}
              </span>
            </div>

            <div style={{ width: '100%', borderTop: '1px solid var(--border-color)', paddingTop: '1rem', textAlign: 'left', fontSize: '0.85rem', color: 'var(--text-muted)' }}>
              <div style={{ display: 'flex', alignItems: 'center', gap: '0.4rem', marginBottom: '0.5rem' }}>
                <Mail size={14} />
                <span>{user?.email}</span>
              </div>
              <div style={{ display: 'flex', alignItems: 'center', gap: '0.4rem' }}>
                <Phone size={14} />
                <span>{phone}</span>
              </div>
            </div>

            <button 
              className="btn btn-outline btn-sm" 
              onClick={logout} 
              style={{ width: '100%', borderColor: 'var(--color-danger)', color: 'var(--color-danger)', marginTop: '1rem' }}
            >
              Logout Account
            </button>
          </div>
        </div>

        {/* Right Card: Editable Profile Form */}
        <div style={{ flex: '1 1 450px' }}>
          <div className="card" style={{ padding: '2rem' }}>
            <h3 style={{ fontSize: '1.2rem', marginBottom: '1.5rem', borderBottom: '1px solid var(--border-color)', paddingBottom: '0.5rem' }}>
              Edit Account Information
            </h3>

            <form onSubmit={handleSubmit}>
              <div style={{ display: 'grid', gridTemplateColumns: 'repeat(2, 1fr)', gap: '1rem' }}>
                <div className="form-group">
                  <label className="form-label">First Name</label>
                  <input
                    type="text"
                    className="form-control"
                    value={firstName}
                    onChange={(e) => setFirstName(e.target.value)}
                    required
                  />
                </div>
                <div className="form-group">
                  <label className="form-label">Last Name</label>
                  <input
                    type="text"
                    className="form-control"
                    value={lastName}
                    onChange={(e) => setLastName(e.target.value)}
                    required
                  />
                </div>
              </div>

              <div className="form-group">
                <label className="form-label" style={{ display: 'flex', alignItems: 'center', gap: '0.3rem' }}>
                  <Phone size={16} />
                  <span>Phone Number</span>
                </label>
                <input
                  type="text"
                  className="form-control"
                  value={phone}
                  onChange={(e) => setPhone(e.target.value)}
                  required
                />
              </div>

              <div style={{ display: 'grid', gridTemplateColumns: 'repeat(2, 1fr)', gap: '1rem' }}>
                <div className="form-group">
                  <label className="form-label" style={{ display: 'flex', alignItems: 'center', gap: '0.3rem' }}>
                    <MapPin size={16} />
                    <span>Governorate</span>
                  </label>
                  <select
                    className="form-control"
                    value={governorateId}
                    onChange={(e) => setGovernorateId(Number(e.target.value))}
                  >
                    {governorates.map((gov) => (
                      <option key={gov.id} value={gov.id}>{gov.name}</option>
                    ))}
                  </select>
                </div>
                <div className="form-group">
                  <label className="form-label">City</label>
                  <input
                    type="text"
                    className="form-control"
                    value={city}
                    onChange={(e) => setCity(e.target.value)}
                    required
                  />
                </div>
              </div>

              <div className="form-group" style={{ marginBottom: '2rem' }}>
                <label className="form-label">Shipping Address Details</label>
                <textarea
                  className="form-control"
                  value={shippingAddress}
                  onChange={(e) => setShippingAddress(e.target.value)}
                  rows={2}
                  required
                />
              </div>

              <button
                type="submit"
                className="btn btn-primary"
                disabled={isSubmitting}
                style={{ width: '100%', gap: '0.5rem' }}
              >
                <CheckCircle size={18} />
                <span>{isSubmitting ? 'Saving Changes...' : 'Save Settings'}</span>
              </button>
            </form>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Profile;

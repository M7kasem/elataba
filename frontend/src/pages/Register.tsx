import React, { useState, useEffect } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { useToast } from '../context/ToastContext';
import apiClient from '../api/client';
import { Role, Governorate } from '../types';
import { UserPlus, Mail, Lock, Phone, MapPin, Briefcase } from 'lucide-react';

const Register: React.FC = () => {
  const { register } = useAuth();
  const { showToast } = useToast();
  const navigate = useNavigate();

  const [governorates, setGovernorates] = useState<Governorate[]>([]);
  const [loadingGovs, setLoadingGovs] = useState(true);

  // Form Fields
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [firstName, setFirstName] = useState('');
  const [lastName, setLastName] = useState('');
  const [phone, setPhone] = useState('');
  const [role, setRole] = useState<Role>(Role.Buyer);
  const [governorateId, setGovernorateId] = useState<number>(0);
  const [city, setCity] = useState('');
  const [shippingAddress, setShippingAddress] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    const fetchGovernorates = async () => {
      try {
        const response = await apiClient.get('/api/Governorate');
        const govList = response.data?.data || [];
        setGovernorates(govList);
        if (govList.length > 0) {
          setGovernorateId(govList[0].id);
        }
      } catch (err) {
        console.error('Error fetching governorates during registration:', err);
        showToast('Failed to load governorate options.', 'error');
      } finally {
        setLoadingGovs(false);
      }
    };
    fetchGovernorates();
  }, [showToast]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    // Client-side validations
    if (phone.length < 10) {
      showToast('Please enter a valid phone number (minimum 10 digits).', 'warning');
      return;
    }
    if (password.length < 6) {
      showToast('Password must be at least 6 characters.', 'warning');
      return;
    }

    setIsSubmitting(true);
    try {
      const payload = {
        email: email.trim(),
        password: password,
        firstName: firstName.trim(),
        lastName: lastName.trim(),
        phone: phone.trim(),
        role: Number(role),
        governorateId: Number(governorateId),
        city: city.trim(),
        shippingAddress: shippingAddress.trim()
      };

      await register(payload);
      showToast('Registration successful!', 'success');

      if (Number(role) === Role.Seller) {
        navigate('/seller/create-store');
      } else {
        navigate('/');
      }
    } catch (err) {
      console.error('Registration failed:', err);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="main-content" style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '80vh', padding: '3rem 2rem' }}>
      <div className="card" style={{ width: '100%', maxWidth: '650px', padding: '2.5rem', border: '1px solid var(--border-color)' }}>
        
        <div style={{ textAlign: 'center', marginBottom: '2rem' }}>
          <span style={{ fontSize: '3rem' }}>📝</span>
          <h2 style={{ fontSize: '1.8rem', marginTop: '1rem', marginBottom: '0.5rem' }}>Create Account</h2>
          <p style={{ color: 'var(--text-muted)', fontSize: '0.9rem' }}>Join ElAtaba wholesale marketplace to buy or sell products</p>
        </div>

        <form onSubmit={handleSubmit}>
          {/* Section: Basic Details */}
          <h3 style={{ fontSize: '1.1rem', borderBottom: '1px solid var(--border-color)', paddingBottom: '0.5rem', marginBottom: '1.5rem', color: 'var(--secondary)' }}>
            Personal Details
          </h3>

          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(240px, 1fr))', gap: '1.5rem' }}>
            <div className="form-group">
              <label className="form-label">First Name</label>
              <input
                type="text"
                className="form-control"
                value={firstName}
                onChange={(e) => setFirstName(e.target.value)}
                placeholder="John"
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
                placeholder="Doe"
                required
              />
            </div>
          </div>

          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(240px, 1fr))', gap: '1.5rem', marginTop: '1rem' }}>
            <div className="form-group">
              <label className="form-label" style={{ display: 'flex', alignItems: 'center', gap: '0.3rem' }}>
                <Mail size={16} />
                <span>Email Address</span>
              </label>
              <input
                type="email"
                className="form-control"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="john.doe@example.com"
                required
              />
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
                placeholder="01012345678"
                required
              />
            </div>
          </div>

          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(240px, 1fr))', gap: '1.5rem', marginTop: '1rem' }}>
            <div className="form-group">
              <label className="form-label" style={{ display: 'flex', alignItems: 'center', gap: '0.3rem' }}>
                <Lock size={16} />
                <span>Password</span>
              </label>
              <input
                type="password"
                className="form-control"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                placeholder="At least 6 characters"
                minLength={6}
                required
              />
            </div>

            {/* Account Role Selector */}
            <div className="form-group">
              <label className="form-label" style={{ display: 'flex', alignItems: 'center', gap: '0.3rem' }}>
                <Briefcase size={16} />
                <span>Account Role</span>
              </label>
              <select
                className="form-control"
                value={role}
                onChange={(e) => setRole(Number(e.target.value) as Role)}
              >
                <option value={Role.Buyer}>Buyer (مشتري جملة)</option>
                <option value={Role.Seller}>Seller / Wholesale Store (تاجر جملة)</option>
              </select>
            </div>
          </div>

          {/* Section: Shipping Details */}
          <h3 style={{ fontSize: '1.1rem', borderBottom: '1px solid var(--border-color)', paddingBottom: '0.5rem', marginBottom: '1.5rem', marginTop: '2.5rem', color: 'var(--secondary)' }}>
            Shipping & Location
          </h3>

          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(240px, 1fr))', gap: '1.5rem' }}>
            <div className="form-group">
              <label className="form-label" style={{ display: 'flex', alignItems: 'center', gap: '0.3rem' }}>
                <MapPin size={16} />
                <span>Governorate (المحافظة)</span>
              </label>
              <select
                className="form-control"
                value={governorateId}
                onChange={(e) => setGovernorateId(Number(e.target.value))}
                disabled={loadingGovs}
              >
                {governorates.map((gov) => (
                  <option key={gov.id} value={gov.id}>{gov.name}</option>
                ))}
              </select>
            </div>
            
            <div className="form-group">
              <label className="form-label">City (المدينة)</label>
              <input
                type="text"
                className="form-control"
                value={city}
                onChange={(e) => setCity(e.target.value)}
                placeholder="ElAtaba"
                required
              />
            </div>
          </div>

          <div className="form-group" style={{ marginTop: '1rem', marginBottom: '2.5rem' }}>
            <label className="form-label">Full Shipping Address Details</label>
            <textarea
              className="form-control"
              value={shippingAddress}
              onChange={(e) => setShippingAddress(e.target.value)}
              placeholder="Detailed street name, shop name, landmark description..."
              rows={2}
              required
            />
          </div>

          <button
            type="submit"
            className="btn btn-primary"
            disabled={isSubmitting || loadingGovs}
            style={{ width: '100%', padding: '1rem' }}
          >
            {isSubmitting ? 'Creating account...' : 'Register and Login'}
          </button>
        </form>

        <div style={{ textAlign: 'center', fontSize: '0.9rem', color: 'var(--text-muted)', marginTop: '1.5rem' }}>
          Already have an account?{' '}
          <Link to="/login" style={{ fontWeight: 'bold', color: 'var(--secondary-hover)' }}>
            Login here
          </Link>
        </div>
      </div>
    </div>
  );
};

export default Register;

import React, { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { useToast } from '../context/ToastContext';
import { Role } from '../types';
import { Lock, Mail, ArrowRight } from 'lucide-react';

const Login: React.FC = () => {
  const { login } = useAuth();
  const { showToast } = useToast();
  const navigate = useNavigate();

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!email.trim() || !password.trim()) {
      showToast('Please fill out all fields.', 'warning');
      return;
    }

    setIsSubmitting(true);
    try {
      const response = await login(email, password);
      // login method returns auth data on success
      showToast('Login successful! Welcome back.', 'success');
      
      // Determine redirection by user role
      const userRole = response?.data?.role;
      const userStoreId = response?.data?.storeId;

      if (userRole === Role.Admin) {
        navigate('/admin/dashboard');
      } else if (userRole === Role.Seller || userRole === Role.StoreManager) {
        if (userStoreId) {
          navigate('/seller/dashboard');
        } else {
          navigate('/seller/create-store');
        }
      } else {
        navigate('/'); // Buyers / Default redirect to main Catalog
      }
    } catch (err) {
      console.error('Login error:', err);
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="main-content" style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '70vh', padding: '2rem' }}>
      <div className="card" style={{ width: '100%', maxWidth: '420px', padding: '2.5rem', border: '1px solid var(--border-color)' }}>
        <div style={{ textAlign: 'center', marginBottom: '2rem' }}>
          <span style={{ fontSize: '3rem' }}>🔑</span>
          <h2 style={{ fontSize: '1.8rem', marginTop: '1rem', marginBottom: '0.5rem' }}>Login to ElAtaba</h2>
          <p style={{ color: 'var(--text-muted)', fontSize: '0.9rem' }}>Enter your email and password to access your dashboard</p>
        </div>

        <form onSubmit={handleSubmit}>
          {/* Email */}
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
              placeholder="name@example.com"
              required
            />
          </div>

          {/* Password */}
          <div className="form-group" style={{ marginBottom: '2rem' }}>
            <label className="form-label" style={{ display: 'flex', alignItems: 'center', gap: '0.3rem' }}>
              <Lock size={16} />
              <span>Password</span>
            </label>
            <input
              type="password"
              className="form-control"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="Enter password"
              required
            />
          </div>

          {/* Submit */}
          <button
            type="submit"
            className="btn btn-primary"
            disabled={isSubmitting}
            style={{ width: '100%', gap: '0.5rem', marginBottom: '1.5rem' }}
          >
            <span>{isSubmitting ? 'Logging in...' : 'Login'}</span>
            <ArrowRight size={18} />
          </button>
        </form>

        <div style={{ textAlign: 'center', fontSize: '0.9rem', color: 'var(--text-muted)' }}>
          Don't have an account?{' '}
          <Link to="/register" style={{ fontWeight: 'bold', color: 'var(--secondary-hover)' }}>
            Register here
          </Link>
        </div>
      </div>
    </div>
  );
};

export default Login;

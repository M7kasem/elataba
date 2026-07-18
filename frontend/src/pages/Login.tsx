import React, { useState, useEffect } from 'react';
import { useNavigate, Link, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { useToast } from '../context/ToastContext';
import { useLanguage } from '../context/LanguageContext';
import { Role } from '../types';
import { Lock, Mail, ArrowLeft, ArrowRight } from 'lucide-react';

const copy = {
  ar: {
    loginTitle: "تسجيل الدخول إلى العتبة",
    loginSub: "أدخل بريدك الإلكتروني وكلمة المرور للوصول إلى لوحة التحكم الخاصة بك",
    emailAddress: "البريد الإلكتروني",
    emailPlaceholder: "name@example.com",
    password: "كلمة المرور",
    passwordPlaceholder: "أدخل كلمة المرور",
    loginAsSeller: "تسجيل الدخول كبائع",
    loginBtn: "تسجيل الدخول",
    loggingIn: "جاري تسجيل الدخول...",
    dontHaveAccount: "ليس لديك حساب؟",
    registerHere: "أنشئ حساباً هنا",
    fillAll: "يرجى ملء جميع الحقول.",
    buyerWarn: "هذا الحساب مسجل كبائع. يرجى تفعيل خيار \"تسجيل الدخول كبائع\" للمتابعة.",
    sellerWarn: "هذا الحساب مسجل كمشترٍ. يرجى إلغاء تفعيل خيار \"تسجيل الدخول كبائع\" أو تسجيل حساب بائع جديد.",
    successMsg: "تم تسجيل الدخول بنجاح! مرحباً بعودتك."
  },
  en: {
    loginTitle: "Login to ElAtaba",
    loginSub: "Enter your email and password to access your dashboard",
    emailAddress: "Email Address",
    emailPlaceholder: "name@example.com",
    password: "Password",
    passwordPlaceholder: "Enter password",
    loginAsSeller: "Login as Seller",
    loginBtn: "Login",
    loggingIn: "Logging in...",
    dontHaveAccount: "Don't have an account?",
    registerHere: "Register here",
    fillAll: "Please fill out all fields.",
    buyerWarn: "This account is registered as a Seller. Please check \"Login as Seller\" to proceed.",
    sellerWarn: "This account is registered as a Buyer. Please uncheck \"Login as Seller\" or register a Seller account.",
    successMsg: "Login successful! Welcome back."
  }
};

const Login: React.FC = () => {
  const { login, logout } = useAuth();
  const { showToast } = useToast();
  const { language } = useLanguage();
  const navigate = useNavigate();
  const location = useLocation();

  const getIsSellerFromUrlOrState = () => {
    const searchParams = new URLSearchParams(location.search);
    return searchParams.get('role') === 'seller' || location.state?.loginAsSeller === true;
  };

  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [isSeller, setIsSeller] = useState(getIsSellerFromUrlOrState());
  const [isSubmitting, setIsSubmitting] = useState(false);

  useEffect(() => {
    if (getIsSellerFromUrlOrState()) {
      setIsSeller(true);
    }
  }, [location]);

  const labels = copy[language as keyof typeof copy];

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!email.trim() || !password.trim()) {
      showToast(labels.fillAll, 'warning');
      return;
    }

    setIsSubmitting(true);
    try {
      const response = await login(email, password);
      // login method returns auth data on success
      
      // Determine redirection by user role
      const apiRole = response?.data?.role;
      const userRole = typeof apiRole === 'string'
        ? Role[apiRole as keyof typeof Role]
        : apiRole;
      const userStoreId = response?.data?.storeId;

      // Validate selected role matches the actual account role
      if (userRole !== Role.Admin) {
        const isActuallySeller = userRole === Role.Seller || userRole === Role.StoreManager;
        if (isSeller && !isActuallySeller) {
          showToast(labels.sellerWarn, 'warning');
          await logout();
          return;
        }
        if (!isSeller && isActuallySeller) {
          showToast(labels.buyerWarn, 'warning');
          await logout();
          return;
        }
      }

      showToast(labels.successMsg, 'success');

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
    <div className="main-content" style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '70vh', padding: '2rem', direction: language === 'ar' ? 'rtl' : 'ltr' }}>
      <div className="card" style={{ width: '100%', maxWidth: '420px', padding: '2.5rem', border: '1px solid var(--border-color)' }}>
        <div style={{ textAlign: 'center', marginBottom: '2rem' }}>
          <span style={{ fontSize: '3rem' }}>🔑</span>
          <h2 style={{ fontSize: '1.8rem', marginTop: '1rem', marginBottom: '0.5rem' }}>{labels.loginTitle}</h2>
          <p style={{ color: 'var(--text-muted)', fontSize: '0.9rem' }}>{labels.loginSub}</p>
        </div>

        <form onSubmit={handleSubmit}>
          {/* Email */}
          <div className="form-group">
            <label className="form-label" style={{ display: 'flex', alignItems: 'center', gap: '0.3rem' }}>
              <Mail size={16} />
              <span>{labels.emailAddress}</span>
            </label>
            <input
              type="email"
              className="form-control"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder={labels.emailPlaceholder}
              required
            />
          </div>

          {/* Password */}
          <div className="form-group" style={{ marginBottom: '1.5rem' }}>
            <label className="form-label" style={{ display: 'flex', alignItems: 'center', gap: '0.3rem' }}>
              <Lock size={16} />
              <span>{labels.password}</span>
            </label>
            <input
              type="password"
              className="form-control"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder={labels.passwordPlaceholder}
              required
            />
          </div>

          {/* Role Distinction Checkbox */}
          <div style={{ display: 'flex', alignItems: 'center', gap: '0.6rem', marginBottom: '2rem', padding: '0.6rem 0.8rem', borderRadius: '8px', backgroundColor: 'var(--bg-main)', border: '1px solid var(--border-color)' }}>
            <input
              type="checkbox"
              id="isSeller"
              checked={isSeller}
              onChange={(e) => setIsSeller(e.target.checked)}
              style={{
                width: '18px',
                height: '18px',
                cursor: 'pointer',
                accentColor: 'var(--primary-hover)',
                margin: 0
              }}
            />
            <label htmlFor="isSeller" style={{ fontSize: '0.9rem', cursor: 'pointer', userSelect: 'none', fontWeight: '600', color: 'var(--text-main)' }}>
              {labels.loginAsSeller}
            </label>
          </div>

          {/* Submit */}
          <button
            type="submit"
            className="btn btn-primary"
            disabled={isSubmitting}
            style={{ width: '100%', gap: '0.5rem', marginBottom: '1.5rem', display: 'flex', alignItems: 'center', justifyContent: 'center' }}
          >
            <span>{isSubmitting ? labels.loggingIn : labels.loginBtn}</span>
            {language === 'ar' ? <ArrowLeft size={18} /> : <ArrowRight size={18} />}
          </button>
        </form>

        <div style={{ textAlign: 'center', fontSize: '0.9rem', color: 'var(--text-muted)' }}>
          {labels.dontHaveAccount}{' '}
          <Link to="/register" style={{ fontWeight: 'bold', color: 'var(--secondary-hover)' }}>
            {labels.registerHere}
          </Link>
        </div>
      </div>
    </div>
  );
};

export default Login;

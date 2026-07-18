import React, { useState, useEffect } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { useToast } from '../context/ToastContext';
import { useLanguage } from '../context/LanguageContext';
import apiClient from '../api/client';
import { toGovernorates } from '../api/normalizers';
import { Role, Governorate } from '../types';
import { UserPlus, Mail, Lock, Phone, MapPin, Briefcase } from 'lucide-react';

const copy = {
  ar: {
    createAccount: "إنشاء حساب جديد",
    subText: "انضم إلى سوق العتبة للجملة لشراء أو بيع المنتجات",
    personalDetails: "البيانات الشخصية",
    firstName: "الاسم الأول",
    lastName: "اسم العائلة",
    emailAddress: "البريد الإلكتروني",
    phoneNumber: "رقم الهاتف",
    password: "كلمة المرور",
    passwordPlaceholder: "على الأقل 6 أحرف",
    accountRole: "نوع الحساب",
    buyerOption: "مشتري (مشتري جملة)",
    sellerOption: "بائع / متجر جملة (تاجر جملة)",
    shippingLocation: "الشحن والموقع",
    governorate: "المحافظة",
    city: "المدينة",
    shippingAddress: "تفاصيل عنوان الشحن بالكامل",
    shippingPlaceholder: "اسم الشارع بالتفصيل، اسم المحل، علامة مميزة...",
    registerBtn: "تسجيل وإنشاء الحساب",
    creatingAccount: "جاري إنشاء الحساب...",
    alreadyHaveAccount: "هل لديك حساب بالفعل؟",
    loginHere: "سجل الدخول هنا",
    phoneWarning: "يرجى إدخال رقم هاتف صالح (10 أرقام على الأقل).",
    passwordWarning: "يجب أن تتكون كلمة المرور من 6 أحرف على الأقل.",
    successMsg: "تم إنشاء الحساب بنجاح!",
    loadGovError: "فشل تحميل المحافظات المتاحة."
  },
  en: {
    createAccount: "Create Account",
    subText: "Join ElAtaba wholesale marketplace to buy or sell products",
    personalDetails: "Personal Details",
    firstName: "First Name",
    lastName: "Last Name",
    emailAddress: "Email Address",
    phoneNumber: "Phone Number",
    password: "Password",
    passwordPlaceholder: "At least 6 characters",
    accountRole: "Account Role",
    buyerOption: "Buyer (مشتري جملة)",
    sellerOption: "Seller / Wholesale Store (تاجر جملة)",
    shippingLocation: "Shipping & Location",
    governorate: "Governorate (المحافظة)",
    city: "City (المدينة)",
    shippingAddress: "Full Shipping Address Details",
    shippingPlaceholder: "Detailed street name, shop name, landmark description...",
    registerBtn: "Register and Login",
    creatingAccount: "Creating account...",
    alreadyHaveAccount: "Already have an account?",
    loginHere: "Login here",
    phoneWarning: "Please enter a valid phone number (minimum 10 digits).",
    passwordWarning: "Password must be at least 6 characters.",
    successMsg: "Registration successful!",
    loadGovError: "Failed to load governorate options."
  }
};

const Register: React.FC = () => {
  const { register } = useAuth();
  const { showToast } = useToast();
  const { language } = useLanguage();
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

  const labels = copy[language as keyof typeof copy];

  useEffect(() => {
    const fetchGovernorates = async () => {
      try {
        const response = await apiClient.get('/api/Governorate');
        const govList = toGovernorates(response.data?.data || []);
        setGovernorates(govList);
        if (govList.length > 0) {
          setGovernorateId(govList[0].id);
        }
      } catch (err) {
        console.error('Error fetching governorates during registration:', err);
        showToast(labels.loadGovError, 'error');
      } finally {
        setLoadingGovs(false);
      }
    };
    fetchGovernorates();
  }, [showToast, labels.loadGovError]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    
    // Client-side validations
    if (phone.length < 10) {
      showToast(labels.phoneWarning, 'warning');
      return;
    }
    if (password.length < 6) {
      showToast(labels.passwordWarning, 'warning');
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
      showToast(labels.successMsg, 'success');

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
    <div className="main-content" style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '80vh', padding: '3rem 2rem', direction: language === 'ar' ? 'rtl' : 'ltr' }}>
      <div className="card" style={{ width: '100%', maxWidth: '650px', padding: '2.5rem', border: '1px solid var(--border-color)' }}>
        
        <div style={{ textAlign: 'center', marginBottom: '2rem' }}>
          <span style={{ fontSize: '3rem' }}>📝</span>
          <h2 style={{ fontSize: '1.8rem', marginTop: '1rem', marginBottom: '0.5rem' }}>{labels.createAccount}</h2>
          <p style={{ color: 'var(--text-muted)', fontSize: '0.9rem' }}>{labels.subText}</p>
        </div>

        <form onSubmit={handleSubmit}>
          {/* Section: Basic Details */}
          <h3 style={{ fontSize: '1.1rem', borderBottom: '1px solid var(--border-color)', paddingBottom: '0.5rem', marginBottom: '1.5rem', color: 'var(--secondary)', textAlign: language === 'ar' ? 'right' : 'left' }}>
            {labels.personalDetails}
          </h3>

          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(240px, 1fr))', gap: '1.5rem' }}>
            <div className="form-group">
              <label className="form-label">{labels.firstName}</label>
              <input
                type="text"
                className="form-control"
                value={firstName}
                onChange={(e) => setFirstName(e.target.value)}
                placeholder=""
                required
              />
            </div>
            <div className="form-group">
              <label className="form-label">{labels.lastName}</label>
              <input
                type="text"
                className="form-control"
                value={lastName}
                onChange={(e) => setLastName(e.target.value)}
                placeholder=""
                required
              />
            </div>
          </div>

          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(240px, 1fr))', gap: '1.5rem', marginTop: '1rem' }}>
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
                placeholder="john.doe@example.com"
                required
              />
            </div>
            <div className="form-group">
              <label className="form-label" style={{ display: 'flex', alignItems: 'center', gap: '0.3rem' }}>
                <Phone size={16} />
                <span>{labels.phoneNumber}</span>
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
                <span>{labels.password}</span>
              </label>
              <input
                type="password"
                className="form-control"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                placeholder={labels.passwordPlaceholder}
                minLength={6}
                required
              />
            </div>

            {/* Account Role Selector */}
            <div className="form-group">
              <label className="form-label" style={{ display: 'flex', alignItems: 'center', gap: '0.3rem' }}>
                <Briefcase size={16} />
                <span>{labels.accountRole}</span>
              </label>
              <select
                className="form-control"
                value={role}
                onChange={(e) => setRole(Number(e.target.value) as Role)}
              >
                <option value={Role.Buyer}>{labels.buyerOption}</option>
                <option value={Role.Seller}>{labels.sellerOption}</option>
              </select>
            </div>
          </div>

          {/* Section: Shipping Details */}
          <h3 style={{ fontSize: '1.1rem', borderBottom: '1px solid var(--border-color)', paddingBottom: '0.5rem', marginBottom: '1.5rem', marginTop: '2.5rem', color: 'var(--secondary)', textAlign: language === 'ar' ? 'right' : 'left' }}>
            {labels.shippingLocation}
          </h3>

          <div style={{ display: 'grid', gridTemplateColumns: 'repeat(auto-fit, minmax(240px, 1fr))', gap: '1.5rem' }}>
            <div className="form-group">
              <label className="form-label" style={{ display: 'flex', alignItems: 'center', gap: '0.3rem' }}>
                <MapPin size={16} />
                <span>{labels.governorate}</span>
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
              <label className="form-label">{labels.city}</label>
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
            <label className="form-label">{labels.shippingAddress}</label>
            <textarea
              className="form-control"
              value={shippingAddress}
              onChange={(e) => setShippingAddress(e.target.value)}
              placeholder={labels.shippingPlaceholder}
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
            {isSubmitting ? labels.creatingAccount : labels.registerBtn}
          </button>
        </form>

        <div style={{ textAlign: 'center', fontSize: '0.9rem', color: 'var(--text-muted)', marginTop: '1.5rem' }}>
          {labels.alreadyHaveAccount}{' '}
          <Link to="/login" style={{ fontWeight: 'bold', color: 'var(--secondary-hover)' }}>
            {labels.loginHere}
          </Link>
        </div>
      </div>
    </div>
  );
};

export default Register;

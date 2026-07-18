import React from 'react';
import { Link, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { useLanguage } from '../context/LanguageContext';
import { Role } from '../types';

const copy = {
  ar: {
    copyright: "حقوق النشر © 2026 بوابة سوق العتبة للجملة. جميع الحقوق محفوظة.",
    builtUsing: "تم التطوير باستخدام React و Vite وخدمات خلفية C# ASP.NET Core.",
    haveStore: "🏪 هل لديك متجر؟ اعرض منتجات الجملة الخاصة بك.",
    becomeSeller: "كن بائعاً الآن ←"
  },
  en: {
    copyright: "© 2026 ElAtaba Wholesale Marketplace. All rights reserved.",
    builtUsing: "Built using React, Vite, and C# ASP.NET Core Backend Services.",
    haveStore: "🏪 Have a store? List your wholesale products.",
    becomeSeller: "Become a Seller →"
  }
};

const Footer: React.FC = () => {
  const { role, isAuthenticated } = useAuth();
  const { language } = useLanguage();
  const location = useLocation();

  // Hide global footer on dashboard routes
  if (location.pathname.startsWith('/seller') || location.pathname.startsWith('/admin')) {
    return null;
  }

  // Show only when logged in as a Buyer
  const showBecomeSeller = isAuthenticated && role === Role.Buyer;
  const labels = copy[language as keyof typeof copy];

  return (
    <footer style={{ 
      padding: '1.5rem 4rem', 
      backgroundColor: 'var(--bg-footer)', 
      color: 'var(--text-light)', 
      fontSize: '0.9rem',
      borderTop: 'none',
      marginTop: 'auto',
      display: 'flex',
      justifyContent: 'space-between',
      alignItems: 'center',
      flexWrap: 'wrap',
      gap: '1.5rem',
      direction: language === 'ar' ? 'rtl' : 'ltr'
    }}>
      <div style={{ textAlign: language === 'ar' ? 'right' : 'left' }}>
        <div>{labels.copyright}</div>
        <div style={{ color: 'var(--text-muted)', fontSize: '0.75rem', marginTop: '0.25rem' }}>
          {labels.builtUsing}
        </div>
      </div>

      {showBecomeSeller && (
        <div style={{
          display: 'flex',
          alignItems: 'center',
          gap: '1rem',
          flexWrap: 'wrap',
          textAlign: language === 'ar' ? 'right' : 'left'
        }}>
          <span style={{ color: 'var(--text-muted)', fontSize: '0.85rem' }}>
            {labels.haveStore}
          </span>
          <Link 
            to="/login?role=seller" 
            state={{ loginAsSeller: true }}
            className="btn btn-sm btn-outline"
            style={{ 
              textDecoration: 'none', 
              padding: '0.35rem 0.75rem', 
              fontSize: '0.8rem',
              fontWeight: '600',
              borderColor: 'var(--secondary-color)',
              color: 'var(--secondary-color)'
            }}
          >
            {labels.becomeSeller}
          </Link>
        </div>
      )}
    </footer>
  );
};

export default Footer;

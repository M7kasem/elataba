import React from 'react';
import { Link, useLocation } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { Role } from '../types';

const Footer: React.FC = () => {
  const { role, isAuthenticated } = useAuth();
  const location = useLocation();

  // Hide global footer on dashboard routes
  if (location.pathname.startsWith('/seller') || location.pathname.startsWith('/admin')) {
    return null;
  }

  // Show only when logged in as a Buyer
  const showBecomeSeller = isAuthenticated && role === Role.Buyer;

  return (
    <footer style={{ 
      padding: '2rem 4rem', 
      backgroundColor: 'var(--bg-footer)', 
      color: 'var(--text-light)', 
      textAlign: 'center',
      fontSize: '0.9rem',
      borderTop: 'none',
      marginTop: 'auto',
      display: 'flex',
      flexDirection: 'column',
      gap: '1.5rem'
    }}>
      {showBecomeSeller && (
        <div style={{
          background: 'linear-gradient(135deg, rgba(230, 81, 0, 0.08) 0%, rgba(245, 124, 0, 0.08) 100%)',
          border: '1px dashed var(--secondary-color)',
          borderRadius: '12px',
          padding: '1.25rem 2rem',
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          flexWrap: 'wrap',
          gap: '1rem',
          textAlign: 'left',
          maxWidth: '1200px',
          margin: '0 auto 0.5rem auto',
          width: '100%'
        }}>
          <div>
            <h4 style={{ margin: 0, fontSize: '1.1rem', color: 'var(--secondary-color)', fontWeight: 'bold', display: 'flex', alignItems: 'center', gap: '0.5rem' }}>
              🏪 Have a store!! Become a seller
            </h4>
            <p style={{ margin: '0.25rem 0 0 0', color: 'var(--text-muted)', fontSize: '0.85rem' }}>
              List your wholesale products and reach buyers across the nation.
            </p>
          </div>
          <Link 
            to="/login?role=seller" 
            state={{ loginAsSeller: true }}
            className="btn btn-outline"
            style={{ 
              textDecoration: 'none', 
              padding: '0.5rem 1rem', 
              fontSize: '0.85rem',
              fontWeight: '600',
              borderColor: 'var(--secondary-color)',
              color: 'var(--secondary-color)'
            }}
          >
            Become a Seller &rarr;
          </Link>
        </div>
      )}
      
      <div>
        <div>&copy; {new Date().getFullYear()} ElAtaba Wholesale Marketplace. All rights reserved.</div>
        <div style={{ color: 'var(--text-muted)', fontSize: '0.75rem', marginTop: '0.5rem' }}>
          Built using React, Vite, and C# ASP.NET Core Backend Services.
        </div>
      </div>
    </footer>
  );
};

export default Footer;


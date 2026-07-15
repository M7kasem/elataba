import React, { useState } from 'react';
import { useAuth } from '../context/AuthContext';
import { Role } from '../types';

const FloatingRoleSwitcher: React.FC = () => {
  const { role, storeId, setDevRole, logout, email } = useAuth();
  const [isOpen, setIsOpen] = useState(true);

  const getRoleName = (r: Role | null) => {
    if (r === null) return 'Visitor';
    switch (r) {
      case Role.Buyer: return 'Buyer';
      case Role.Seller: return 'Seller';
      case Role.Admin: return 'Admin';
      case Role.StoreManager: return 'Store Manager';
      default: return 'Unknown';
    }
  };

  const handleSwitch = (targetRole: Role | null, sId?: number | null) => {
    setDevRole(targetRole, sId);
  };

  if (!isOpen) {
    return (
      <button 
        style={{
          position: 'fixed',
          bottom: '1rem',
          left: '1rem',
          zIndex: 9999,
          background: '#ffb703',
          border: '2px solid #023047',
          color: '#023047',
          borderRadius: '50%',
          width: '40px',
          height: '40px',
          fontWeight: 'bold',
          cursor: 'pointer',
          boxShadow: '0 4px 10px rgba(0,0,0,0.3)',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'center'
        }}
        onClick={() => setIsOpen(true)}
      >
        ⚙️
      </button>
    );
  }

  return (
    <div className="dev-role-switcher">
      <div className="dev-role-switcher-header">
        <span>Dev Role Switcher</span>
        <button 
          style={{ background: 'none', border: 'none', cursor: 'pointer', color: 'var(--text-muted)' }} 
          onClick={() => setIsOpen(false)}
        >
          &times;
        </button>
      </div>
      <div style={{ fontSize: '0.75rem', marginBottom: '0.5rem', color: 'var(--text-main)' }}>
        <strong>Current:</strong> {getRoleName(role)} {storeId ? `(Store #${storeId})` : ''}
        <br />
        <span style={{ fontSize: '0.65rem', color: 'var(--text-muted)' }}>{email || 'Not logged in'}</span>
      </div>
      <div className="dev-role-grid">
        <button 
          className={`dev-role-btn ${role === null ? 'active' : ''}`}
          onClick={() => handleSwitch(null)}
        >
          Visitor
        </button>
        <button 
          className={`dev-role-btn ${role === Role.Buyer ? 'active' : ''}`}
          onClick={() => handleSwitch(Role.Buyer)}
        >
          Buyer
        </button>
        <button 
          className={`dev-role-btn ${role === Role.Seller && storeId === 1 ? 'active' : ''}`}
          onClick={() => handleSwitch(Role.Seller, 1)}
        >
          Seller (St #1)
        </button>
        <button 
          className={`dev-role-btn ${role === Role.Seller && storeId === null ? 'active' : ''}`}
          onClick={() => handleSwitch(Role.Seller, null)}
        >
          Seller (No St)
        </button>
        <button 
          className={`dev-role-btn ${role === Role.StoreManager ? 'active' : ''}`}
          onClick={() => handleSwitch(Role.StoreManager, 1)}
        >
          Store Mgr
        </button>
        <button 
          className={`dev-role-btn ${role === Role.Admin ? 'active' : ''}`}
          onClick={() => handleSwitch(Role.Admin)}
        >
          Admin
        </button>
      </div>
      <button 
        style={{
          marginTop: '0.5rem',
          padding: '0.3rem',
          fontSize: '0.7rem',
          fontWeight: 'bold',
          width: '100%',
          background: 'var(--color-danger)',
          color: 'white',
          border: 'none',
          borderRadius: '4px',
          cursor: 'pointer'
        }}
        onClick={logout}
      >
        Clear Auth Cookie
      </button>
    </div>
  );
};

export default FloatingRoleSwitcher;

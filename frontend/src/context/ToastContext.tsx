import React, { createContext, useContext, useState, useEffect } from 'react';
import { registerAuthCallbacks } from '../api/client';

export type ToastType = 'success' | 'error' | 'warning' | 'info';

interface Toast {
  id: string;
  message: string;
  type: ToastType;
}

interface ToastContextType {
  toasts: Toast[];
  showToast: (message: string, type?: ToastType) => void;
  removeToast: (id: string) => void;
}

const ToastContext = createContext<ToastContextType | undefined>(undefined);

export const ToastProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [toasts, setToasts] = useState<Toast[]>([]);

  const showToast = (message: string, type: ToastType = 'info') => {
    const id = Math.random().toString(36).substring(2, 9);
    setToasts((prev) => [...prev, { id, message, type }]);

    // Auto-remove after 4 seconds
    setTimeout(() => {
      removeToast(id);
    }, 4000);
  };

  const removeToast = (id: string) => {
    setToasts((prev) => prev.filter((t) => t.id !== id));
  };

  // Wire up the global Axios client error callback to show toasts
  useEffect(() => {
    // If apiClient is already configured, we can register the callback.
    // However, since client.ts handles onGlobalErrorCallback, we bind it here.
    // Note: We also preserve unauthorized/forbidden handlers.
    const token = localStorage.getItem('elAtaba_token');
    registerAuthCallbacks({
      onUnauthorized: () => {
        // Clear local storage and dispatch event or handle globally.
        // AuthContext also handles this but we need to re-verify.
        localStorage.removeItem('elAtaba_token');
        localStorage.removeItem('elAtaba_userId');
        localStorage.removeItem('elAtaba_role');
        localStorage.removeItem('elAtaba_storeId');
        showToast('Session expired. Please log in again.', 'warning');
        // Trigger a reload or redirect
        setTimeout(() => {
          window.location.href = '/login';
        }, 1000);
      },
      onForbidden: () => {
        showToast('Access denied: You do not have permission to view this resource.', 'error');
      },
      onGlobalError: (msg) => {
        showToast(msg, 'error');
      },
    });
  }, []);

  return (
    <ToastContext.Provider value={{ toasts, showToast, removeToast }}>
      {children}
      {/* Global Toast Container */}
      <div className="toast-container">
        {toasts.map((toast) => (
          <div key={toast.id} className={`toast-card toast-${toast.type}`}>
            <span className="toast-message">{toast.message}</span>
            <button className="toast-close" onClick={() => removeToast(toast.id)}>
              &times;
            </button>
          </div>
        ))}
      </div>
    </ToastContext.Provider>
  );
};

export const useToast = () => {
  const context = useContext(ToastContext);
  if (context === undefined) {
    throw new Error('useToast must be used within a ToastProvider');
  }
  return context;
};

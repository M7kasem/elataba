import React, { createContext, useContext, useState, useEffect } from 'react';
import apiClient, { registerAuthCallbacks } from '../api/client';
import { Role, User } from '../types';

interface AuthContextType {
  token: string | null;
  userId: number | null;
  email: string | null;
  role: Role | null;
  storeId: number | null;
  user: User | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (email: string, password: string) => Promise<any>;
  register: (formData: any) => Promise<any>;
  logout: () => Promise<void>;
  updateUserStoreId: (storeId: number | null) => void;
  // Dev Helper: Change role directly for testing and demonstration
  setDevRole: (role: Role | null, storeId?: number | null) => void;
  fetchProfile: () => Promise<User>;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [token, setToken] = useState<string | null>(localStorage.getItem('elAtaba_token'));
  const [userId, setUserId] = useState<number | null>(
    localStorage.getItem('elAtaba_userId') ? Number(localStorage.getItem('elAtaba_userId')) : null
  );
  const [email, setEmail] = useState<string | null>(localStorage.getItem('elAtaba_email'));
  const [role, setRole] = useState<Role | null>(
    localStorage.getItem('elAtaba_role') !== null ? Number(localStorage.getItem('elAtaba_role')) as Role : null
  );
  const [storeId, setStoreId] = useState<number | null>(
    localStorage.getItem('elAtaba_storeId') ? Number(localStorage.getItem('elAtaba_storeId')) : null
  );
  const [user, setUser] = useState<User | null>(null);
  const [isLoading, setIsLoading] = useState(true);

  // Initialize and bind error interceptor callbacks
  useEffect(() => {
    registerAuthCallbacks({
      onUnauthorized: () => {
        handleLogoutLocal();
      },
      onForbidden: () => {
        // Redirection or flag can be handled at router layer,
        // we can set a custom error or alert
        console.warn('Forbidden access attempt.');
      },
      onGlobalError: (msg, type) => {
        // Handled by global toast, we register it inside App or ToastContext
        console.error(`[API Error] ${msg}`);
      },
    });

    if (token) {
      fetchProfile()
        .catch(() => {
          // Token might be expired, clear it
          handleLogoutLocal();
        })
        .finally(() => {
          setIsLoading(false);
        });
    } else {
      setIsLoading(false);
    }
  }, [token]);

  const fetchProfile = async (): Promise<User> => {
    try {
      const response = await apiClient.get('/api/User/profile');
      // Response shape: { statusCode, message, data: UserDto }
      const userProfile = response.data?.data;
      if (userProfile) {
        setUser(userProfile);
        setUserId(userProfile.userId);
        setRole(userProfile.role);
        setStoreId(userProfile.storeId || null);
        
        localStorage.setItem('elAtaba_userId', String(userProfile.userId));
        localStorage.setItem('elAtaba_role', String(userProfile.role));
        if (userProfile.storeId) {
          localStorage.setItem('elAtaba_storeId', String(userProfile.storeId));
        } else {
          localStorage.removeItem('elAtaba_storeId');
        }
      }
      return userProfile;
    } catch (err) {
      throw err;
    }
  };

  const login = async (emailInput: string, passwordInput: string) => {
    try {
      const response = await apiClient.post('/api/Account/login', {
        email: emailInput,
        password: passwordInput,
      });

      const authData = response.data?.data; // AuthResponseDto: Token, UserId, Email, Role, StoreId
      if (authData) {
        const { token: jwt, userId: uId, email: uEmail, role: uRole, storeId: sId } = authData;

        setToken(jwt);
        setUserId(uId);
        setEmail(uEmail);
        setRole(uRole);
        setStoreId(sId || null);

        localStorage.setItem('elAtaba_token', jwt);
        localStorage.setItem('elAtaba_userId', String(uId));
        localStorage.setItem('elAtaba_email', uEmail);
        localStorage.setItem('elAtaba_role', String(uRole));
        if (sId) {
          localStorage.setItem('elAtaba_storeId', String(sId));
        } else {
          localStorage.removeItem('elAtaba_storeId');
        }

        // Fetch detailed profile immediately
        await fetchProfile();
      }
      return response.data;
    } catch (err) {
      throw err;
    }
  };

  const register = async (formData: any) => {
    try {
      const response = await apiClient.post('/api/Account/register', formData);
      const authData = response.data?.data;
      if (authData) {
        const { token: jwt, userId: uId, email: uEmail, role: uRole, storeId: sId } = authData;

        setToken(jwt);
        setUserId(uId);
        setEmail(uEmail);
        setRole(uRole);
        setStoreId(sId || null);

        localStorage.setItem('elAtaba_token', jwt);
        localStorage.setItem('elAtaba_userId', String(uId));
        localStorage.setItem('elAtaba_email', uEmail);
        localStorage.setItem('elAtaba_role', String(uRole));
        if (sId) {
          localStorage.setItem('elAtaba_storeId', String(sId));
        }

        // Fetch profile to complete state loading
        await fetchProfile();
      }
      return response.data;
    } catch (err) {
      throw err;
    }
  };

  const handleLogoutLocal = () => {
    setToken(null);
    setUserId(null);
    setEmail(null);
    setRole(null);
    setStoreId(null);
    setUser(null);

    localStorage.removeItem('elAtaba_token');
    localStorage.removeItem('elAtaba_userId');
    localStorage.removeItem('elAtaba_email');
    localStorage.removeItem('elAtaba_role');
    localStorage.removeItem('elAtaba_storeId');
  };

  const logout = async () => {
    try {
      await apiClient.post('/api/Account/logout');
    } catch (err) {
      console.error('Error during backend logout', err);
    } finally {
      handleLogoutLocal();
    }
  };

  const updateUserStoreId = (newStoreId: number | null) => {
    setStoreId(newStoreId);
    if (newStoreId !== null) {
      localStorage.setItem('elAtaba_storeId', String(newStoreId));
      if (user) {
        setUser({ ...user, storeId: newStoreId });
      }
    } else {
      localStorage.removeItem('elAtaba_storeId');
      if (user) {
        setUser({ ...user, storeId: null });
      }
    }
  };

  const setDevRole = (targetRole: Role | null, devStoreId?: number | null) => {
    if (targetRole === null) {
      handleLogoutLocal();
      return;
    }

    setRole(targetRole);
    localStorage.setItem('elAtaba_role', String(targetRole));

    const mockStoreId = devStoreId !== undefined ? devStoreId : (targetRole === Role.Seller || targetRole === Role.StoreManager ? 1 : null);
    setStoreId(mockStoreId);
    if (mockStoreId !== null) {
      localStorage.setItem('elAtaba_storeId', String(mockStoreId));
    } else {
      localStorage.removeItem('elAtaba_storeId');
    }

    if (user) {
      setUser({
        ...user,
        role: targetRole,
        storeId: mockStoreId,
      });
    } else {
      setUser({
        userId: userId || 999,
        email: email || 'demo@elataba.com',
        firstName: 'Demo',
        lastName: 'User',
        phone: '01000000000',
        role: targetRole,
        governorateId: 1,
        city: 'ElAtaba',
        shippingAddress: 'Main Market Street',
        storeId: mockStoreId,
      });
    }
  };

  const isAuthenticated = !!token;

  return (
    <AuthContext.Provider
      value={{
        token,
        userId,
        email,
        role,
        storeId,
        user,
        isAuthenticated,
        isLoading,
        login,
        register,
        logout,
        updateUserStoreId,
        setDevRole,
        fetchProfile,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (context === undefined) {
    throw new Error('useAuth must be used within an AuthProvider');
  }
  return context;
};

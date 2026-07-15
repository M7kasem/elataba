import axios from 'axios';

// The local API base URL as per the backend handoff documentation
export const API_BASE_URL = 'http://localhost:5191';

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  withCredentials: true, // Crucial for HttpOnly Cookie support
  headers: {
    'Content-Type': 'application/json',
  },
});

// Setup JWT token from localStorage if available (Hybrid Auth fallback)
apiClient.interceptors.request.use(
  (config) => {
    const token = localStorage.getItem('elAtaba_token');
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

// Setup dynamic callbacks for global error Handling (to be bound in AuthContext/ToastContext)
let onUnauthorizedCallback: (() => void) | null = null;
let onForbiddenCallback: (() => void) | null = null;
let onGlobalErrorCallback: ((message: string, type: 'error' | 'warning') => void) | null = null;

export const registerAuthCallbacks = (callbacks: {
  onUnauthorized: () => void;
  onForbidden: () => void;
  onGlobalError: (message: string, type: 'error' | 'warning') => void;
}) => {
  onUnauthorizedCallback = callbacks.onUnauthorized;
  onForbiddenCallback = callbacks.onForbidden;
  onGlobalErrorCallback = callbacks.onGlobalError;
};

apiClient.interceptors.response.use(
  (response) => {
    // Standard response format: { statusCode, message, data }
    return response;
  },
  (error) => {
    const response = error.response;

    if (!response) {
      // Network error or server is down
      if (onGlobalErrorCallback) {
        onGlobalErrorCallback('Network error: Cannot connect to the server.', 'error');
      }
      return Promise.reject(error);
    }

    const status = response.status;
    const message = response.data?.message || 'Something went wrong';

    switch (status) {
      case 400:
        // Validation/Business rules error
        if (onGlobalErrorCallback) {
          const validationErrors = response.data?.errors;
          if (validationErrors) {
            // FluentValidation errors
            const errorMsg = Object.values(validationErrors).flat().join(', ');
            onGlobalErrorCallback(errorMsg || message, 'error');
          } else {
            onGlobalErrorCallback(message, 'error');
          }
        }
        break;
      case 401:
        // Unauthorized
        if (onUnauthorizedCallback) {
          onUnauthorizedCallback();
        }
        break;
      case 403:
        // Forbidden
        if (onForbiddenCallback) {
          onForbiddenCallback();
        }
        break;
      case 500:
        // Internal Server Error
        if (onGlobalErrorCallback) {
          onGlobalErrorCallback(`Server Error (500): ${message}`, 'error');
        }
        break;
      default:
        if (onGlobalErrorCallback) {
          onGlobalErrorCallback(message, 'error');
        }
    }

    return Promise.reject(error);
  }
);

export default apiClient;

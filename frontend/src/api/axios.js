import axios from 'axios';

export const TOKEN_KEY = 'fitbook_token';
export const UNAUTHORIZED_EVENT = 'fitbook:unauthorized';

const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? 'http://localhost:5080/api',
  headers: { 'Content-Type': 'application/json' },
});

// Attach the JWT token to every request.
api.interceptors.request.use((config) => {
  const token = localStorage.getItem(TOKEN_KEY);
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

// When the token expires the API answers 401 - log the user out.
api.interceptors.response.use(
  (response) => response,
  (error) => {
    const isAuthRequest = error.config?.url?.startsWith('/auth/');
    if (error.response?.status === 401 && !isAuthRequest) {
      window.dispatchEvent(new Event(UNAUTHORIZED_EVENT));
    }
    return Promise.reject(error);
  },
);

export default api;

import { createContext, useCallback, useContext, useEffect, useMemo, useState } from 'react';
import { authApi } from '../api/fitbookApi';
import { TOKEN_KEY, UNAUTHORIZED_EVENT } from '../api/axios';

const USER_KEY = 'fitbook_user';
const EXPIRES_KEY = 'fitbook_expires';

const AuthContext = createContext(null);

function loadStoredUser() {
  try {
    const token = localStorage.getItem(TOKEN_KEY);
    const expiresAt = localStorage.getItem(EXPIRES_KEY);
    const user = JSON.parse(localStorage.getItem(USER_KEY));
    if (!token || !user || !expiresAt || new Date(expiresAt) <= new Date()) {
      return null;
    }
    return user;
  } catch {
    return null;
  }
}

export function AuthProvider({ children }) {
  const [user, setUser] = useState(loadStoredUser);

  const saveSession = useCallback(({ token, expiresAt, user: loggedInUser }) => {
    localStorage.setItem(TOKEN_KEY, token);
    localStorage.setItem(EXPIRES_KEY, expiresAt);
    localStorage.setItem(USER_KEY, JSON.stringify(loggedInUser));
    setUser(loggedInUser);
    return loggedInUser;
  }, []);

  const logout = useCallback(() => {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(EXPIRES_KEY);
    localStorage.removeItem(USER_KEY);
    setUser(null);
  }, []);

  const login = useCallback(async (credentials) => saveSession(await authApi.login(credentials)), [saveSession]);
  const register = useCallback(async (data) => saveSession(await authApi.register(data)), [saveSession]);

  // Log out when the API rejects the token or when it expires.
  useEffect(() => {
    window.addEventListener(UNAUTHORIZED_EVENT, logout);
    if (!loadStoredUser()) {
      logout();
    }
    return () => window.removeEventListener(UNAUTHORIZED_EVENT, logout);
  }, [logout]);

  const value = useMemo(
    () => ({
      user,
      isAuthenticated: !!user,
      isAdmin: user?.role === 'Admin',
      isMember: user?.role === 'Member',
      login,
      register,
      logout,
    }),
    [user, login, register, logout],
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used inside AuthProvider');
  }
  return context;
}

import api from './axios';

export const authApi = {
  login: (data) => api.post('/auth/login', data).then((res) => res.data),
  register: (data) => api.post('/auth/register', data).then((res) => res.data),
  me: () => api.get('/auth/me').then((res) => res.data),
};

export const trainersApi = {
  getAll: (specialty) => api.get('/trainers', { params: { specialty } }).then((res) => res.data),
  create: (data) => api.post('/trainers', data).then((res) => res.data),
  update: (id, data) => api.put(`/trainers/${id}`, data).then((res) => res.data),
  remove: (id) => api.delete(`/trainers/${id}`),
};

export const sessionsApi = {
  getAll: (params) => api.get('/sessions', { params }).then((res) => res.data),
  create: (data) => api.post('/sessions', data).then((res) => res.data),
  update: (id, data) => api.put(`/sessions/${id}`, data).then((res) => res.data),
  remove: (id) => api.delete(`/sessions/${id}`),
  getParticipants: (id) => api.get(`/sessions/${id}/participants`).then((res) => res.data),
};

export const bookingsApi = {
  getMine: () => api.get('/bookings/my').then((res) => res.data),
  book: (sessionId) => api.post('/bookings', { sessionId }).then((res) => res.data),
  cancel: (bookingId) => api.delete(`/bookings/${bookingId}`),
};

export const dashboardApi = {
  get: () => api.get('/admin/dashboard').then((res) => res.data),
};

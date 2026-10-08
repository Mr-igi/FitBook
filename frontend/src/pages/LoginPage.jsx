import { useState } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { ErrorAlert } from '../components/common';
import { getErrorMessage } from '../utils/helpers';

export default function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const location = useLocation();
  const [form, setForm] = useState({ email: '', password: '' });
  const [error, setError] = useState('');
  const [submitting, setSubmitting] = useState(false);

  const handleChange = (e) => setForm({ ...form, [e.target.name]: e.target.value });

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setSubmitting(true);
    try {
      const user = await login(form);
      const fallback = user.role === 'Admin' ? '/admin' : '/schedule';
      navigate(location.state?.from ?? fallback, { replace: true });
    } catch (err) {
      setError(getErrorMessage(err));
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="container auth-wrapper py-5">
      <div className="card fb-card auth-card mx-auto p-4 p-md-5">
        <h2 className="fw-bold mb-1">Welcome back</h2>
        <p className="text-muted mb-4">Log in to book and manage your trainings.</p>

        <ErrorAlert message={error} onClose={() => setError('')} />

        <form onSubmit={handleSubmit} noValidate>
          <div className="mb-3">
            <label htmlFor="email" className="form-label">Email</label>
            <input id="email" name="email" type="email" className="form-control form-control-lg" value={form.email} onChange={handleChange} required autoFocus />
          </div>
          <div className="mb-4">
            <label htmlFor="password" className="form-label">Password</label>
            <input id="password" name="password" type="password" className="form-control form-control-lg" value={form.password} onChange={handleChange} required />
          </div>
          <button type="submit" className="btn btn-primary btn-lg w-100" disabled={submitting || !form.email || !form.password}>
            {submitting && <span className="spinner-border spinner-border-sm me-2" />}
            Log in
          </button>
        </form>

        <p className="text-center text-muted mt-4 mb-0">
          New to FitBook? <Link to="/register">Create an account</Link>
        </p>
      </div>
    </div>
  );
}

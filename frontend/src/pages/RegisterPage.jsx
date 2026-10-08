import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import { useToast } from '../context/ToastContext';
import { ErrorAlert } from '../components/common';
import { getErrorMessage } from '../utils/helpers';

const emptyForm = { fullName: '', email: '', password: '', confirmPassword: '' };

function validate(form) {
  const errors = {};
  if (form.fullName.trim().length < 2) errors.fullName = 'Please enter your full name.';
  if (!/^\S+@\S+\.\S+$/.test(form.email)) errors.email = 'Please enter a valid email address.';
  if (form.password.length < 6) errors.password = 'Password must be at least 6 characters long.';
  if (form.password !== form.confirmPassword) errors.confirmPassword = 'Passwords do not match.';
  return errors;
}

export default function RegisterPage() {
  const { register } = useAuth();
  const showToast = useToast();
  const navigate = useNavigate();
  const [form, setForm] = useState(emptyForm);
  const [errors, setErrors] = useState({});
  const [serverError, setServerError] = useState('');
  const [submitting, setSubmitting] = useState(false);

  const handleChange = (e) => setForm({ ...form, [e.target.name]: e.target.value });

  const handleSubmit = async (e) => {
    e.preventDefault();
    const validationErrors = validate(form);
    setErrors(validationErrors);
    if (Object.keys(validationErrors).length > 0) return;

    setServerError('');
    setSubmitting(true);
    try {
      const { fullName, email, password } = form;
      await register({ fullName, email, password });
      showToast('Welcome to FitBook! Your account is ready.');
      navigate('/schedule', { replace: true });
    } catch (err) {
      setServerError(getErrorMessage(err));
    } finally {
      setSubmitting(false);
    }
  };

  const field = (name, label, type = 'text', autoComplete) => (
    <div className="mb-3">
      <label htmlFor={name} className="form-label">{label}</label>
      <input
        id={name}
        name={name}
        type={type}
        autoComplete={autoComplete}
        className={`form-control form-control-lg ${errors[name] ? 'is-invalid' : ''}`}
        value={form[name]}
        onChange={handleChange}
      />
      {errors[name] && <div className="invalid-feedback">{errors[name]}</div>}
    </div>
  );

  return (
    <div className="container auth-wrapper py-5">
      <div className="card fb-card auth-card mx-auto p-4 p-md-5">
        <h2 className="fw-bold mb-1">Create your account</h2>
        <p className="text-muted mb-4">Join FitBook and start booking classes today.</p>

        <ErrorAlert message={serverError} onClose={() => setServerError('')} />

        <form onSubmit={handleSubmit} noValidate>
          {field('fullName', 'Full name', 'text', 'name')}
          {field('email', 'Email', 'email', 'email')}
          {field('password', 'Password', 'password', 'new-password')}
          {field('confirmPassword', 'Confirm password', 'password', 'new-password')}
          <button type="submit" className="btn btn-primary btn-lg w-100 mt-2" disabled={submitting}>
            {submitting && <span className="spinner-border spinner-border-sm me-2" />}
            Sign up
          </button>
        </form>

        <p className="text-center text-muted mt-4 mb-0">
          Already have an account? <Link to="/login">Log in</Link>
        </p>
      </div>
    </div>
  );
}

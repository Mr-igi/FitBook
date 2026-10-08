export const TRAINING_TYPES = {
  Yoga: { label: 'Yoga', icon: 'bi-flower1', className: 'type-yoga', description: 'Flexibility, balance and calm focus.' },
  CrossFit: { label: 'CrossFit', icon: 'bi-lightning-charge-fill', className: 'type-crossfit', description: 'High-intensity functional strength.' },
  Boxing: { label: 'Boxing', icon: 'bi-fire', className: 'type-boxing', description: 'Footwork, power and serious cardio.' },
};

export const TYPE_NAMES = Object.keys(TRAINING_TYPES);

const LOCALE = 'en-US';

export const formatDate = (value) =>
  new Date(value).toLocaleDateString(LOCALE, { weekday: 'long', month: 'long', day: 'numeric' });

export const formatShortDate = (value) =>
  new Date(value).toLocaleDateString(LOCALE, { weekday: 'short', month: 'short', day: 'numeric' });

export const formatTime = (value) =>
  new Date(value).toLocaleTimeString(LOCALE, { hour: '2-digit', minute: '2-digit' });

export const formatDateTime = (value) => `${formatShortDate(value)}, ${formatTime(value)}`;

/** Local calendar day key, e.g. "2026-10-07". */
export const dayKey = (value) => {
  const date = new Date(value);
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const day = String(date.getDate()).padStart(2, '0');
  return `${date.getFullYear()}-${month}-${day}`;
};

/** ISO string -> value for <input type="datetime-local">. */
export const toLocalInputValue = (value) => {
  const date = new Date(value);
  const offset = date.getTimezoneOffset() * 60000;
  return new Date(date.getTime() - offset).toISOString().slice(0, 16);
};

/** Value from <input type="datetime-local"> -> UTC ISO string. */
export const fromLocalInputValue = (value) => new Date(value).toISOString();

/** Reads a readable message from an API error (ProblemDetails or validation errors). */
export const getErrorMessage = (error, fallback = 'Something went wrong. Please try again.') => {
  if (!error?.response) {
    return 'Cannot reach the server. Make sure the API is running.';
  }
  const data = error.response.data;
  if (data?.errors) {
    return Object.values(data.errors).flat().join(' ');
  }
  return data?.detail || data?.title || fallback;
};

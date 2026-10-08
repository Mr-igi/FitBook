import { TRAINING_TYPES } from '../utils/helpers';

export function TypeBadge({ type }) {
  const info = TRAINING_TYPES[type];
  return (
    <span className={`type-badge ${info?.className ?? ''}`}>
      <i className={`bi ${info?.icon}`} />
      {info?.label ?? type}
    </span>
  );
}

export function TrainerAvatar({ trainer, small = false }) {
  const className = `avatar ${small ? 'avatar-sm' : ''} ${TRAINING_TYPES[trainer.specialty]?.className ?? ''}`;
  if (trainer.imageUrl) {
    return <img src={trainer.imageUrl} alt={trainer.fullName} className={className} />;
  }
  const initials = `${trainer.firstName?.[0] ?? ''}${trainer.lastName?.[0] ?? ''}`.toUpperCase();
  return <span className={className}>{initials}</span>;
}

export function CapacityBar({ booked, capacity }) {
  const percent = capacity > 0 ? Math.round((booked / capacity) * 100) : 0;
  const color = percent >= 100 ? 'bg-danger' : percent >= 75 ? 'bg-warning' : 'bg-success';
  return (
    <div className="capacity-bar progress" role="progressbar" aria-valuenow={percent} aria-valuemin="0" aria-valuemax="100">
      <div className={`progress-bar ${color}`} style={{ width: `${Math.min(percent, 100)}%` }} />
    </div>
  );
}

export function Loader({ text = 'Loading...' }) {
  return (
    <div className="d-flex justify-content-center align-items-center py-5 text-muted">
      <div className="spinner-border text-primary me-3" role="status" />
      {text}
    </div>
  );
}

export function ErrorAlert({ message, onClose }) {
  if (!message) return null;
  return (
    <div className="alert alert-danger d-flex align-items-start" role="alert">
      <i className="bi bi-exclamation-triangle-fill me-2" />
      <div className="flex-grow-1">{message}</div>
      {onClose && <button type="button" className="btn-close" aria-label="Close" onClick={onClose} />}
    </div>
  );
}

export function EmptyState({ icon = 'bi-calendar-x', title, children }) {
  return (
    <div className="empty-state">
      <i className={`bi ${icon}`} />
      <h5 className="mt-3 text-dark">{title}</h5>
      {children}
    </div>
  );
}

export function PageHeader({ title, subtitle, children }) {
  return (
    <section className="page-header">
      <div className="container d-flex flex-column flex-md-row justify-content-between align-items-md-end gap-3">
        <div>
          <h1 className="fw-bold mb-1">{title}</h1>
          {subtitle && <p>{subtitle}</p>}
        </div>
        {children}
      </div>
    </section>
  );
}

/** Simple React-controlled Bootstrap modal. */
export function Modal({ title, onClose, children, footer, size = '' }) {
  return (
    <>
      <div className="modal-backdrop-custom" onClick={onClose} />
      <div className="modal d-block" tabIndex="-1" role="dialog" aria-modal="true" style={{ zIndex: 1055 }}>
        <div className={`modal-dialog modal-dialog-centered modal-dialog-scrollable ${size}`}>
          <div className="modal-content border-0 rounded-4">
            <div className="modal-header">
              <h5 className="modal-title fw-bold">{title}</h5>
              <button type="button" className="btn-close" aria-label="Close" onClick={onClose} />
            </div>
            <div className="modal-body">{children}</div>
            {footer && <div className="modal-footer">{footer}</div>}
          </div>
        </div>
      </div>
    </>
  );
}

export function ConfirmModal({ title, message, confirmText = 'Confirm', variant = 'danger', busy, onConfirm, onClose }) {
  return (
    <Modal
      title={title}
      onClose={onClose}
      footer={
        <>
          <button className="btn btn-light" onClick={onClose} disabled={busy}>Back</button>
          <button className={`btn btn-${variant}`} onClick={onConfirm} disabled={busy}>
            {busy && <span className="spinner-border spinner-border-sm me-2" />}
            {confirmText}
          </button>
        </>
      }
    >
      <p className="mb-0">{message}</p>
    </Modal>
  );
}

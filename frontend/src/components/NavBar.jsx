import { Link, NavLink, useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';

export default function NavBar() {
  const { user, isAuthenticated, isAdmin, isMember, logout } = useAuth();
  const navigate = useNavigate();

  const handleLogout = () => {
    logout();
    navigate('/');
  };

  return (
    <nav className="navbar navbar-expand-lg navbar-dark fb-navbar sticky-top">
      <div className="container">
        <Link className="navbar-brand brand-logo fs-4" to="/">
          <i className="bi bi-lightning-charge-fill text-warning me-1" />
          Fit<span>Book</span>
        </Link>
        <button
          className="navbar-toggler"
          type="button"
          data-bs-toggle="collapse"
          data-bs-target="#mainNav"
          aria-controls="mainNav"
          aria-expanded="false"
          aria-label="Toggle navigation"
        >
          <span className="navbar-toggler-icon" />
        </button>

        <div className="collapse navbar-collapse" id="mainNav">
          <ul className="navbar-nav me-auto mb-2 mb-lg-0">
            <li className="nav-item">
              <NavLink className="nav-link" to="/schedule">Schedule</NavLink>
            </li>
            <li className="nav-item">
              <NavLink className="nav-link" to="/trainers">Trainers</NavLink>
            </li>
            {isMember && (
              <li className="nav-item">
                <NavLink className="nav-link" to="/my-trainings">My Trainings</NavLink>
              </li>
            )}
            {isAdmin && (
              <li className="nav-item">
                <NavLink className="nav-link" to="/admin">Admin Dashboard</NavLink>
              </li>
            )}
          </ul>

          <div className="d-flex align-items-center gap-2">
            {isAuthenticated ? (
              <>
                <span className="text-white-50 small me-2">
                  <i className={`bi ${isAdmin ? 'bi-shield-lock' : 'bi-person-circle'} me-1`} />
                  {user.fullName}
                  <span className="badge bg-secondary ms-2">{user.role}</span>
                </span>
                <button className="btn btn-outline-light btn-sm" onClick={handleLogout}>
                  <i className="bi bi-box-arrow-right me-1" />
                  Log out
                </button>
              </>
            ) : (
              <>
                <Link className="btn btn-outline-light btn-sm" to="/login">Log in</Link>
                <Link className="btn btn-primary btn-sm" to="/register">Join now</Link>
              </>
            )}
          </div>
        </div>
      </div>
    </nav>
  );
}

import { Link } from 'react-router-dom';

export default function NotFoundPage() {
  return (
    <div className="container text-center py-5 my-5">
      <h1 className="display-1 fw-bold text-primary">404</h1>
      <h3 className="fw-bold">Page not found</h3>
      <p className="text-muted">The page you are looking for does not exist.</p>
      <Link to="/" className="btn btn-primary mt-2">Back to home</Link>
    </div>
  );
}

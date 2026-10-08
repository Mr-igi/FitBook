export default function Footer() {
  return (
    <footer className="py-4 mt-5">
      <div className="container d-flex flex-column flex-md-row justify-content-between align-items-center gap-2 small">
        <span>
          <strong className="text-white">FitBook</strong> &copy; {new Date().getFullYear()} &middot; Yoga, CrossFit &amp; Boxing classes
        </span>
        <span>
          <i className="bi bi-geo-alt me-1" />
          Open every day 06:00 - 22:00
        </span>
      </div>
    </footer>
  );
}

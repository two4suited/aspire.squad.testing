import { useState, FormEvent } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useAuth } from '../contexts/AuthContext';
import './LoginPage.css';

export default function LoginPage() {
  const { login } = useAuth();
  const navigate = useNavigate();
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  async function handleSubmit(e: FormEvent) {
    e.preventDefault();
    setError(null);
    setLoading(true);

    console.log('[LoginPage] Form submission started');
    console.log('[LoginPage] Email:', email);

    try {
      console.log('[LoginPage] Calling login() with email:', email);
      await login(email, password);
      console.log('[LoginPage] Login successful, navigating to /');
      navigate('/');
    } catch (err) {
      const message = err instanceof Error ? err.message : 'Login failed';
      console.error('[LoginPage] Login error:', err);
      console.error('[LoginPage] Error message:', message);
      setError(`Failed: ${message}`);
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="login-page">
      {/* Hero Section */}
      <section className="login-hero">
        <div className="hero-content">
          <h1 className="hero-title">🐾 DogTeams</h1>
          <p className="hero-subtitle">Team collaboration for dog lovers</p>
          <p className="hero-description">Organize your pack, manage your projects, and stay connected</p>
        </div>
      </section>

      {/* Login Form Section */}
      <section className="login-container">
        <div className="login-card">
          <div className="login-header">
            <h2>Sign In</h2>
            <p className="login-subtext">Access your DogTeams account</p>
          </div>

          {error && <div className="error-banner">{error}</div>}

          <form onSubmit={handleSubmit} className="login-form">
            <div className="form-group">
              <label htmlFor="email">Email</label>
              <input
                id="email"
                type="email"
                className="form-input"
                value={email}
                onChange={e => setEmail(e.target.value)}
                required
                autoComplete="email"
                placeholder="you@example.com"
              />
            </div>

            <div className="form-group">
              <label htmlFor="password">Password</label>
              <div className="password-input-wrapper">
                <input
                  id="password"
                  type={showPassword ? 'text' : 'password'}
                  className="form-input"
                  value={password}
                  onChange={e => setPassword(e.target.value)}
                  required
                  autoComplete="current-password"
                  placeholder="Enter your password"
                />
                <button
                  type="button"
                  className="password-toggle"
                  onClick={() => setShowPassword(!showPassword)}
                  aria-label={showPassword ? 'Hide password' : 'Show password'}
                  title={showPassword ? 'Hide password' : 'Show password'}
                >
                  {showPassword ? '👁️' : '👁️‍🗨️'}
                </button>
              </div>
            </div>

            <button
              type="submit"
              disabled={loading}
              className="submit-button"
            >
              {loading ? 'Signing in…' : 'Sign In'}
            </button>
          </form>

          <div className="login-footer">
            <p>
              Don't have an account?{' '}
              <Link to="/register" className="register-link">
                Create one
              </Link>
            </p>
          </div>

          <div className="demo-info">
            <p className="demo-label">🧪 Demo Credentials:</p>
            <code className="demo-code">test@example.com / TestPassword123!</code>
          </div>
        </div>
      </section>
    </div>
  );
}

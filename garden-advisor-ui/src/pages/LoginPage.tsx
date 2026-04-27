import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { registerEmail } from '../services/apiService';

export default function LoginPage() {
  const navigate = useNavigate();
  const [email, setEmail] = useState('');
  const [name, setName] = useState('');
  const [error, setError] = useState('');
  const [loading, setLoading] = useState(false);

  const handleOAuth = (provider: string) => {
    alert(`${provider} sign-in is not configured in this environment. Please use email sign-in or skip.`);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!email.trim()) {
      setError('Please enter your email address.');
      return;
    }
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
      setError('Please enter a valid email address.');
      return;
    }
    setError('');
    setLoading(true);
    try {
      const result = await registerEmail(email, name || undefined);
      sessionStorage.setItem('userSession', JSON.stringify(result));
    } catch {
      const fallback = { sessionId: crypto.randomUUID(), email, name: name || '' };
      sessionStorage.setItem('userSession', JSON.stringify(fallback));
    } finally {
      setLoading(false);
      navigate('/processing');
    }
  };

  const handleSkip = () => {
    navigate('/processing');
  };

  return (
    <div>
      <header className="page-header">
        <span className="garden-logo">🌱</span>
        <div>
          <h1>GardenAdvisor</h1>
          <div className="subtitle">Your personalized garden planning assistant</div>
        </div>
      </header>

      <div className="step-indicator">
        <div className="step completed">
          <div className="step-num">✓</div>
          Location
        </div>
        <div className="step-divider" />
        <div className="step completed">
          <div className="step-num">✓</div>
          Plants
        </div>
        <div className="step-divider" />
        <div className="step active">
          <div className="step-num">3</div>
          Login
        </div>
        <div className="step-divider" />
        <div className="step">
          <div className="step-num">4</div>
          Results
        </div>
      </div>

      <div className="login-container">
        <div className="login-card">
          <h2>🌿 Get Your Garden Schedule</h2>
          <p className="login-subtitle">
            Sign in or provide your email to receive your personalized planting schedule and seasonal reminders.
          </p>

          <button className="oauth-btn" onClick={() => handleOAuth('Google')}>
            <svg width="20" height="20" viewBox="0 0 48 48">
              <path fill="#4285F4" d="M47.5 24.6c0-1.6-.1-3.1-.4-4.6H24v8.7h13.2c-.6 3-2.3 5.5-4.9 7.2v6h7.9c4.6-4.3 7.3-10.6 7.3-17.3z"/>
              <path fill="#34A853" d="M24 48c6.5 0 11.9-2.1 15.9-5.8l-7.9-6c-2.2 1.5-5 2.3-8 2.3-6.1 0-11.3-4.1-13.2-9.6H2.6v6.2C6.5 42.5 14.7 48 24 48z"/>
              <path fill="#FBBC05" d="M10.8 28.9c-.5-1.5-.7-3-.7-4.9s.3-3.4.7-4.9v-6.2H2.6C.9 16.4 0 20.1 0 24s.9 7.6 2.6 11.1l8.2-6.2z"/>
              <path fill="#EA4335" d="M24 9.5c3.4 0 6.5 1.2 8.9 3.5l6.7-6.7C35.9 2.4 30.4 0 24 0 14.7 0 6.5 5.5 2.6 13.6l8.2 6.2C12.7 13.6 17.9 9.5 24 9.5z"/>
            </svg>
            Continue with Google
          </button>

          <button className="oauth-btn" onClick={() => handleOAuth('Facebook')}>
            <svg width="20" height="20" viewBox="0 0 24 24">
              <path fill="#1877F2" d="M24 12.073C24 5.405 18.627 0 12 0S0 5.405 0 12.073C0 18.1 4.388 23.094 10.125 24v-8.437H7.078v-3.49h3.047V9.41c0-3.025 1.792-4.697 4.533-4.697 1.312 0 2.686.236 2.686.236v2.97h-1.513c-1.491 0-1.956.93-1.956 1.874v2.25h3.328l-.532 3.49h-2.796V24C19.612 23.094 24 18.1 24 12.073z"/>
            </svg>
            Continue with Facebook
          </button>

          <div className="oauth-divider">or</div>

          <form onSubmit={handleSubmit}>
            <div className="form-group">
              <label>Email Address</label>
              <input
                type="email"
                value={email}
                onChange={e => setEmail(e.target.value)}
                placeholder="you@example.com"
              />
            </div>
            <div className="form-group">
              <label>Name (optional)</label>
              <input
                type="text"
                value={name}
                onChange={e => setName(e.target.value)}
                placeholder="Your name"
              />
            </div>
            {error && <div className="error-message">{error}</div>}
            <button type="submit" className="btn btn-primary btn-large" style={{ width: '100%' }} disabled={loading}>
              {loading ? 'Setting up...' : '🌿 Get My Schedule'}
            </button>
          </form>

          <div className="skip-link">
            <button type="button" onClick={handleSkip} style={{ background: 'none', border: 'none', padding: 0, color: '#2d6a4f', textDecoration: 'underline', cursor: 'pointer', font: 'inherit' }}>
              Skip for now (results won't be saved)
            </button>
          </div>
        </div>
      </div>
    </div>
  );
}

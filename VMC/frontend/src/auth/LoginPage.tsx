import { useState } from "react";
import { useAuth } from "./AuthContext";

export const LoginPage = () => {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [isLoggingIn, setIsLoggingIn] = useState(false);
  const { login } = useAuth();

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoggingIn(true);
    setError(null);
    try {
      await login(username, password);
    } catch (err) {
      setError((err as Error).message || "Login failed");
    } finally {
      setIsLoggingIn(false);
    }
  };

  return (
    <div className="glass-login-wrapper">
      {/* Background ambient orbs */}
      <div className="glass-orb glass-orb--1"></div>
      <div className="glass-orb glass-orb--2"></div>
      <div className="glass-orb glass-orb--3"></div>

      <div className="glass-login-card">
        {/* Card Header */}
        <div className="glass-login-card__header">
          <div className="glass-login-logo">
            <div className="glass-login-logo__glyph">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
                <path strokeLinecap="round" strokeLinejoin="round" d="M19.428 15.428a2 2 0 00-1.022-.547l-2.387-.477a6 6 0 00-3.86.517l-.318.158a6 6 0 01-3.86.517L6.05 15.21a2 2 0 00-1.806.547M8 4h8l-1 1v5.172a2 2 0 00.586 1.414l5 5c1.26 1.26.367 3.414-1.415 3.414H4.828c-1.782 0-2.674-2.154-1.414-3.414l5-5A2 2 0 009 10.172V5L8 4z" />
              </svg>
            </div>
          </div>
          <h1 className="glass-login-card__title">VMC Operator HMI</h1>
          <p className="glass-login-card__subtitle">
            Vertical Machining Center • Autonomous CNC Control Panel
          </p>
          <div className="glass-login-card__badge">
            <span>JOB: AL MOUNTING BRACKET (25 PCS)</span>
          </div>
        </div>

        {/* Error Alert */}
        {error && (
          <div className="glass-login-error">
            <svg viewBox="0 0 20 20" fill="currentColor">
              <path fillRule="evenodd" d="M18 10a8 8 0 11-16 0 8 8 0 0116 0zm-7 4a1 1 0 11-2 0 1 1 0 012 0zm-1-9a1 1 0 00-1 1v4a1 1 0 102 0V6a1 1 0 00-1-1z" clipRule="evenodd" />
            </svg>
            <span>{error}</span>
          </div>
        )}

        {/* Form */}
        <form onSubmit={handleSubmit} className="glass-login-form">
          <div className="glass-form-group">
            <label htmlFor="username">OPERATOR USERNAME</label>
            <div className="glass-input-wrap">
              <svg viewBox="0 0 20 20" fill="currentColor" className="input-icon">
                <path fillRule="evenodd" d="M10 9a3 3 0 100-6 3 3 0 000 6zm-7 9a7 7 0 1114 0H3z" clipRule="evenodd" />
              </svg>
              <input
                id="username"
                type="text"
                value={username}
                onChange={(e) => setUsername(e.target.value)}
                placeholder="operator"
                disabled={isLoggingIn}
                autoComplete="username"
                required
              />
            </div>
          </div>

          <div className="glass-form-group">
            <label htmlFor="password">ACCESS PASSWORD</label>
            <div className="glass-input-wrap">
              <svg viewBox="0 0 20 20" fill="currentColor" className="input-icon">
                <path fillRule="evenodd" d="M5 9V7a5 5 0 0110 0v2a2 2 0 012 2v5a2 2 0 01-2 2H5a2 2 0 01-2-2v-5a2 2 0 012-2zm8-2v2H7V7a3 3 0 016 0z" clipRule="evenodd" />
              </svg>
              <input
                id="password"
                type="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                placeholder="••••••••••••"
                disabled={isLoggingIn}
                autoComplete="current-password"
                required
              />
            </div>
          </div>

          <button
            type="submit"
            className="glass-btn-submit"
            disabled={isLoggingIn}
          >
            {isLoggingIn ? (
              <span className="spinner-text">
                <span className="glass-spinner"></span> Authenticating Session...
              </span>
            ) : (
              <span>Unlock Operator Terminal</span>
            )}
          </button>
        </form>
      </div>
    </div>
  );
};




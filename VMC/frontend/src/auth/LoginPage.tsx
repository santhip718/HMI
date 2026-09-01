import { useState } from "react";
import { useAuth } from "./AuthContext";
import { getApiBaseUrl } from "../api/apiClient";

export const LoginPage = () => {
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState<string | null>(null);
  const [isLoggingIn, setIsLoggingIn] = useState(false);
  const [showServerConfig, setShowServerConfig] = useState(false);
  const [customUrl, setCustomUrl] = useState(getApiBaseUrl());
  const { login } = useAuth();

  const handleSaveServerUrl = () => {
    if (customUrl.trim()) {
      localStorage.setItem("hmi_custom_backend_url", customUrl.trim());
    } else {
      localStorage.removeItem("hmi_custom_backend_url");
    }
    setShowServerConfig(false);
    setError(null);
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoggingIn(true);
    setError(null);
    try {
      await login(username, password);
    } catch (err: any) {
      const msg = err?.message || "Login failed";
      if (msg.includes("404")) {
        setError(`Cannot reach API at ${getApiBaseUrl()}. Please verify your backend Render service is Live.`);
      } else {
        setError(msg);
      }
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

        {/* Server Endpoint Indicator & Config Toggle */}
        <div style={{ marginTop: "18px", borderTop: "1px solid rgba(255,255,255,0.06)", paddingTop: "12px", display: "flex", flexDirection: "column", gap: "8px" }}>
          <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", fontSize: "0.72rem", color: "var(--text-muted)" }}>
            <span>Backend: <code style={{ color: "var(--accent-cyan)" }}>{getApiBaseUrl()}</code></span>
            <button
              type="button"
              onClick={() => setShowServerConfig(!showServerConfig)}
              style={{ background: "transparent", border: "none", color: "var(--text-secondary)", cursor: "pointer", textDecoration: "underline", fontSize: "0.7rem" }}
            >
              {showServerConfig ? "Close" : "Change URL"}
            </button>
          </div>

          {showServerConfig && (
            <div style={{ display: "flex", gap: "6px", marginTop: "4px" }}>
              <input
                type="text"
                value={customUrl}
                onChange={(e) => setCustomUrl(e.target.value)}
                placeholder="https://vmc-hmi-backend.onrender.com"
                style={{ flex: 1, padding: "6px 10px", background: "rgba(0,0,0,0.3)", border: "1px solid var(--border-glass)", borderRadius: "6px", color: "#fff", fontSize: "0.75rem" }}
              />
              <button
                type="button"
                onClick={handleSaveServerUrl}
                style={{ padding: "6px 12px", background: "var(--accent-cyan)", color: "#000", fontWeight: "bold", border: "none", borderRadius: "6px", cursor: "pointer", fontSize: "0.75rem" }}
              >
                Set
              </button>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};





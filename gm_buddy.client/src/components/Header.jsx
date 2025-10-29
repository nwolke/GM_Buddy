import React from 'react';

export default function Header() {
  return (
    <header className="site-header">
      <div className="site-brand">
        <div className="brand-glyph" aria-hidden="true">
          {/* simple rune SVG */}
          <svg width="28" height="28" viewBox="0 0 24 24" fill="none" aria-hidden>
            <path d="M12 3v18M3 12h18" stroke="#CFA84A" strokeWidth="1.6" strokeLinecap="round" strokeLinejoin="round"/>
          </svg>
        </div>
        <div>
          <h1 className="site-title">GM Buddy</h1>
          <div className="site-sub">A tabletop toolkit</div>
        </div>
      </div>
      <div style={{marginLeft:'auto'}}>
        <button className="btn primary" onClick={() => window.location.reload()}>Reload</button>
      </div>
    </header>
  );
}

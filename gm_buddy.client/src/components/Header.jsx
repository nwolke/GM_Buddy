import React from 'react';
import logo from '../assets/temporary logo for b.png';

export default function Header() {
  return (
    <header className="site-header">
      <div className="site-brand">
        <img src={logo} alt="GM Buddy Logo" className="header-logo" />
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

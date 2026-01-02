import React, { useState, useContext } from 'react';
import { NavContext } from '../contexts/contexts.js';
import logo from '../assets/temporary logo for b.png';

export default function Header() {
  const [showAppDropdown, setShowAppDropdown] = useState(false);
  const changePage = useContext(NavContext);

  const handleAppSelect = (app) => {
    changePage(app);
    setShowAppDropdown(false);
  };

  return (
    <header className="site-header">
      <div className="site-brand">
        <img src={logo} alt="GM Buddy Logo" className="header-logo" onClick={() => changePage('home')} style={{cursor: 'pointer'}} />
        <div>
          <h1 className="site-title" onClick={() => changePage('home')} style={{cursor: 'pointer'}}>GM Buddy</h1>
          <div className="site-sub">A tabletop toolkit</div>
        </div>
      </div>

      <nav className="header-nav">
        <div className="app-dropdown">
          <button 
            className="btn" 
            onClick={() => setShowAppDropdown(!showAppDropdown)}
          >
            Applications ?
          </button>
          {showAppDropdown && (
            <div className="dropdown-menu">
              <button onClick={() => handleAppSelect('npc-manager')}>NPC Manager</button>
              <button onClick={() => handleAppSelect('generic-app')}>Generic App</button>
            </div>
          )}
        </div>
      </nav>

      <div className="header-actions">
        <button className="btn" onClick={() => changePage('login')}>Login</button>
        <button className="btn primary" onClick={() => changePage('signup')}>Sign Up</button>
      </div>
    </header>
  );
}

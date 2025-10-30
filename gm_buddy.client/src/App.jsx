import { useState } from 'react';
import './App.css';
import './theme.css';
import Header from './components/Header';
import NpcGrid from './NpcGrid';
import Auth from './Auth';
import { NavContext } from './contexts/contexts.js';

function App() {
    const [isActive, setIsActive] = useState('home');
    const changeActive = (p) => setIsActive(p);
    NavContext.changePage = changeActive;
    let currentPage;
    switch (isActive) {
        case 'grid': currentPage = <NpcGrid />; break;
        case 'login': currentPage = <Auth />; break;
        default:
            currentPage = (
                <div className="page-content">
                    <h2 className="page-title">Welcome, GM</h2>
                    <p>Choose a panel to begin.</p>
                    <div style={{display:'flex', gap:'.5rem', marginTop:'1rem', justifyContent:'center'}}>
                        <button className="btn primary" onClick={() => changeActive('grid')}>NPCs</button>
                        <button className="btn" onClick={() => changeActive('login')}>Login</button>
                    </div>
                </div>
            );
    }
    return (
        <div>
            <Header />
            <NavContext.Provider value={changeActive}>
                <div className="app-container">
                    {currentPage}
                </div>
            </NavContext.Provider>
        </div>
    );
}

export default App;

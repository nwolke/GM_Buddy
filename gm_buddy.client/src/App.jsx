import { useState } from 'react';
import './App.css';
import './theme.css';
import Header from './components/Header';
import NpcGrid from './NpcGrid';
import Auth from './Auth';
import Home from './Home';
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
            currentPage = <Home />;
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

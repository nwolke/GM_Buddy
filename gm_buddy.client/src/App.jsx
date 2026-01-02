import { useState } from 'react';
import './App.css';
import './theme.css';
import Header from './components/Header';
import NpcManagerApp from './apps/npc-manager/NpcManagerApp';
import GenericApp from './apps/generic-app/GenericApp';
import Auth from './apps/auth/Auth';
import Home from './apps/home/Home';
import { NavContext } from './contexts/contexts.js';

function App() {
    const [isActive, setIsActive] = useState('home');
    const changeActive = (p) => setIsActive(p);
    NavContext.changePage = changeActive;
    
    let currentPage;
    switch (isActive) {
        case 'npc-manager': 
            currentPage = <NpcManagerApp />; 
            break;
        case 'generic-app': 
            currentPage = <GenericApp />; 
            break;
        case 'login': 
        case 'signup':
            currentPage = <Auth />; 
            break;
        default:
            currentPage = <Home />;
    }
    
    return (
        <div>
            <NavContext.Provider value={changeActive}>
                <Header />
                <div className="app-container">
                    {currentPage}
                </div>
            </NavContext.Provider>
        </div>
    );
}

export default App;

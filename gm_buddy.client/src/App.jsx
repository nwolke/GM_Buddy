import { useState, lazy, Suspense } from 'react';
import './App.css';
import './theme.css';
import Header from './components/Header';
import { NavContext } from './contexts/contexts.js';
import { Box, CircularProgress } from '@mui/material';

// Lazy load route components - only loaded when needed
const NpcManagerApp = lazy(() => import('./apps/npc-manager/NpcManagerApp'));
const GenericApp = lazy(() => import('./apps/generic-app/GenericApp'));
const Auth = lazy(() => import('./apps/auth/Auth'));
const Home = lazy(() => import('./apps/home/Home'));

// Loading fallback component
const PageLoader = () => (
    <Box sx={{ display: 'flex', justifyContent: 'center', alignItems: 'center', py: 8, minHeight: '50vh' }}>
        <CircularProgress sx={{ color: 'var(--accent-gold)' }} />
    </Box>
);

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
                    <Suspense fallback={<PageLoader />}>
                        {currentPage}
                    </Suspense>
                </div>
            </NavContext.Provider>
        </div>
    );
}

export default App;

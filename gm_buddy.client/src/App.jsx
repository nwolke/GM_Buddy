import { useState } from 'react';
import './App.css';
import NpcGrid from './NpcGrid';
import Auth from './Auth';
import { NavContext } from './contexts/contexts.js';

function App() {
    const [isActive, setIsActive] = useState('home');
    const changeActive = (p) => {
        setIsActive(p);
    }
    NavContext.changePage = changeActive;
    let currentPage;
    switch (isActive) {
        case 'grid':
            currentPage = <NpcGrid></NpcGrid>;
            break;
        case 'login':
            currentPage = <Auth></Auth>;
            break;
        case 'home':
            currentPage = <div>
                <div>Hello</div>
                <button onClick={() => changeActive('grid')}>Go To Grid</button>
            </div>
            break;
    }
    return (
        <div>
            <h1 id="tableLabel">GM Buddy</h1>
            <NavContext.Provider value={changeActive}>
                {currentPage}
            </NavContext.Provider>
        </div>
    );
}

export default App;
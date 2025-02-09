import { useEffect, useState } from 'react';
import './App.css';
import NpcGrid from './NpcGrid';

function App() {

    const [calledBack, setCalledBack] = useState();
    const [isActive, setIsActive] = useState('main');
    const [isLoading, setIsLoading] = useState(true);

    useEffect(() => {
        if (isLoading) {
            get_test_npcs();
        }
    }, []);
    let oldActive = 'main';

    const changeActive = (newActive) => {
        if (newActive !== oldActive) {
            setIsActive(newActive);
            oldActive = newActive;
        }
    }

    return (
        <div>
            <h1 id="tableLabel">GM Buddy</h1>
           
                {/*<div>*/}
                {/*<span>Hello</span>*/}
                {/*<button onClick={changeActive('grid')}></button>*/}
            {/*</div>*/}

            <NpcGrid allNpcs={calledBack}></NpcGrid>
            
        </div>
    );

    
    async function get_test_npcs() {
        const response = await fetch('https://localhost:7256/Npcs?account_id=1');
        if (response.ok) {
            const data = await response.json();
            setCalledBack(data);
            setIsLoading(false);
        } else {
            console.error('Failed to fetch data', response.status);
        }
    }
}

export default App;
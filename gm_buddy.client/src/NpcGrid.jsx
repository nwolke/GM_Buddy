import PropTypes from 'prop-types';
import { useState, useContext, useEffect } from 'react';
import { NavContext } from './contexts/contexts.js';
import NpcCard from './components/NpcCard';
import { API_BASE } from './api';

function NpcGrid() {
    const [npcData, setNpcData] = useState(undefined);
    const changePage = useContext(NavContext);
    
    useEffect(() => {
        get_test_npcs();
    }, []);

    async function get_test_npcs() {
        try {
            const response = await fetch(`${API_BASE}/Npcs?account_id=1`);
            if (response.ok) {
                const data = await response.json();
                setNpcData(data);
            } else {
                console.error('Failed to fetch data', response.status);
            }
        } catch (e) {
            console.error(e);
        }
    }

    return (
        <div className="container">
            <div style={{display:'flex', gap:'0.5rem', marginBottom:'1rem'}}>
                <button className="btn" onClick={() => changePage('home')}>Go Home</button>
                <button className="btn" onClick={() => setNpcData(undefined)}>Refresh</button>
            </div>

            {/* Cards for mobile */}
            <div className="cards-only">
                {npcData ? npcData.map((n, i) => <NpcCard key={i} npc={n} />) : <div>Loading...</div>}
            </div>

            {/* Table for desktop */}
            <div className="table-only">
                <table className="npc-table">
                    <thead>
                        <tr>
                            <th>Name</th>
                            <th>Lineage</th>
                            <th>Occupation</th>
                            <th>Strength</th>
                            <th>Description</th>
                        </tr>
                    </thead>
                    <tbody>
                        {npcData && npcData.map((data, i) => (
                            <tr key={i}>
                                <td>{data.name}</td>
                                <td>{data.lineage}</td>
                                <td>{data.occupation}</td>
                                <td>{data.stats?.attributes?.strength ?? '-'}</td>
                                <td>{data.description}</td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>
        </div>
    );
}

NpcGrid.propTypes = {
    allNpcs: PropTypes.array,
}

export default NpcGrid;
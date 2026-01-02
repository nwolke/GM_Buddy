import PropTypes from 'prop-types';
import { useState, useContext, useEffect } from 'react';
import { NavContext } from '../../../../contexts/contexts.js';
import NpcCard from '../../../../components/NpcCard';
import { API_BASE } from '../../../../api';

function NpcGrid() {
    const [npcData, setNpcData] = useState(undefined);
    const changePage = useContext(NavContext);
    
    useEffect(() => {
        get_test_npcs();
    }, []);

    async function get_test_npcs() {
        try {
            const url = `${API_BASE}/Npcs?account_id=1`;
            console.log('[NpcGrid] Fetching from URL:', url);
            const response = await fetch(url);
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
        <div className="page-content">
            <h2 className="page-title">NPC Collection</h2>

            {/* Action Buttons */}
            <div className="page-actions">
                <button className="btn" onClick={() => changePage('home')}>Go Home</button>
                <button className="btn" onClick={() => get_test_npcs()}>Refresh</button>
            </div>

            {/* Cards for mobile */}
            <div className="cards-only">
                {npcData ? npcData.map((n, i) => <NpcCard key={n.Npc_Id ?? i} npc={n} />) : <div>Loading...</div>}
            </div>

            {/* Table for desktop */}
            <div className="table-only">
                <div className="table-wrapper">
                    <table className="npc-table">
                        <thead>
                            <tr>
                                <th>ID</th>
                                <th>Name</th>
                                <th>Lineage</th>
                                <th>Occupation</th>
                                <th>STR</th>
                                <th>DEX</th>
                                <th>CON</th>
                                <th>INT</th>
                                <th>WIS</th>
                                <th>CHA</th>
                                <th>System</th>
                            </tr>
                        </thead>
                        <tbody>
                            {npcData && npcData.map((npc) => (
                                <tr key={npc.npc_id}>
                                    <td>{npc.npc_id ?? '-'}</td>
                                    <td>{npc.stats?.name ?? '-'}</td>
                                    <td>{npc.stats?.lineage ?? '-'}</td>
                                    <td>{npc.stats?.occupation ?? '-'}</td>
                                    <td>{npc.stats?.attributes?.strength ?? '-'}</td>
                                    <td>{npc.stats?.attributes?.dexterity ?? '-'}</td>
                                    <td>{npc.stats?.attributes?.constitution ?? '-'}</td>
                                    <td>{npc.stats?.attributes?.intelligence ?? '-'}</td>
                                    <td>{npc.stats?.attributes?.wisdom ?? '-'}</td>
                                    <td>{npc.stats?.attributes?.charisma ?? '-'}</td>
                                    <td>{npc.system ?? '-'}</td>
                                </tr>
                            ))}
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    );
}

NpcGrid.propTypes = {
    allNpcs: PropTypes.array,
}

export default NpcGrid;

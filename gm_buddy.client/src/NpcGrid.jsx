import PropTypes from 'prop-types';
import { useState, useContext, useEffect } from 'react';
import { NavContext } from './contexts/contexts.js';

function NpcGrid() {
    const [npcData, setNpcData] = useState(undefined);
    const changePage = useContext(NavContext);
    useEffect(() => {

        get_test_npcs();

    },[])


    const grid = <div>
        <table id='npcGridTable' style={{ border: '1px solid black' }} >
            <tbody>
                <tr>
                    <th>Name</th>
                    <th>Lineage</th>
                    <th>Occupation</th>
                    <th>Strength</th>
                    <th>Description</th>
                </tr>
                {npcData && npcData.map((data, i) => (
                    <tr key={i}>
                        <td>{data.name}</td>
                        <td>{data.lineage}</td>
                        <td>{data.occupation}</td>
                        <td>{data.stats.attributes.strength}</td>
                        <td>{data.description}</td>
                    </tr>
                ))}
            </tbody>
        </table>
    </div>;
    const loading = <div>I&apo;m loading. Chill.</div>;
    return (
        <div>
            <button onClick={()=> changePage('home')}>Go Home</button>
            {npcData ? grid : loading}
        </div>
    );
     
    async function get_test_npcs() {
        const response = await fetch('https://localhost:5001/Npcs?account_id=1');
        if (response.ok) {
            const data = await response.json();
            setNpcData(data);
        } else {
            console.error('Failed to fetch data', response.status);
        }
    }
}



NpcGrid.propTypes = {
    allNpcs: PropTypes.array,
}

export default NpcGrid;
import PropTypes from 'prop-types';

function NpcGrid({ allNpcs }) {

    if (allNpcs) {
        return (
            <div>
                <table id='npcGridTable' style={{ border: '1px solid black' }} >
                    <tbody>
                        <tr>
                            <th>Name</th>
                            <th>Lineage</th>
                            <th>Occupation</th>
                            <th>Strength</th>
                            <th>Description</th>
                        </tr>
                        {allNpcs && allNpcs.map((data, i) => (
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
            </div>
        );
    }
    else {
        return (
            <div>NOT GOOD</div>
        )
    }
}

NpcGrid.propTypes = {
    allNpcs: PropTypes.array,
}

export default NpcGrid;
import React from 'react';
import PropTypes from 'prop-types';

export default function NpcCard({ npc }) {
  const stats = npc?.Stats;
  const attrs = stats?.Attributes;
  
  return (
    <article className="npc-card" role="article" aria-label={stats?.Name ?? 'NPC'}>
      <div className="npc-row">
        <div className="npc-meta">
          <h3 className="npc-name">{stats?.Name ?? 'Unnamed NPC'}</h3>
          <p className="npc-sub">
            {stats?.Lineage ?? 'Unknown'} • {stats?.Occupation ?? 'Unknown'}
          </p>
          <div style={{marginTop:'.6rem', display:'flex', alignItems:'center', gap:'0.5rem', flexWrap:'wrap'}}>
            <span className="stat-chip">STR {attrs?.Strength ?? '-'}</span>
            <span className="stat-chip">DEX {attrs?.Dexterity ?? '-'}</span>
            <span className="stat-chip">CON {attrs?.Constitution ?? '-'}</span>
            <span className="stat-chip">INT {attrs?.Intelligence ?? '-'}</span>
            <span className="stat-chip">WIS {attrs?.Wisdom ?? '-'}</span>
            <span className="stat-chip">CHA {attrs?.Charisma ?? '-'}</span>
            {stats?.Gender && (
              <span style={{marginLeft:'auto', color:'#6b3b81', fontWeight:600}}>
                {stats.Gender}
              </span>
            )}
          </div>
        </div>
      </div>
      {stats?.Description && (
        <p style={{marginTop:'.8rem', color:'#3b3531'}}>{stats.Description}</p>
      )}
      {stats?.Languages && stats.Languages.length > 0 && (
        <p style={{marginTop:'.5rem', fontSize:'0.9rem', color:'#6b3b81'}}>
          Languages: {stats.Languages.join(', ')}
        </p>
      )}
      <p style={{marginTop:'.5rem', fontSize:'0.85rem', color:'#999'}}>
        System: {npc?.System ?? 'Unknown'}
      </p>
    </article>
  );
}

NpcCard.propTypes = {
  npc: PropTypes.object.isRequired,
};

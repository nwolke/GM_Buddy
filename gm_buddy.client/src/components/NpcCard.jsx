import React from 'react';
import PropTypes from 'prop-types';

export default function NpcCard({ npc }) {
  const strength = npc?.stats?.attributes?.strength ?? '-';
  return (
    <article className="npc-card" role="article" aria-label={npc?.name ?? 'npc'}>
      <div className="npc-row">
        <div className="npc-meta">
          <h3 className="npc-name">{npc?.name}</h3>
          <p className="npc-sub">{npc?.lineage} — {npc?.occupation}</p>
          <div style={{marginTop:'.6rem'}}>
            <span className="stat-chip">STR {strength}</span>
            <span className="stat-chip">DEX {npc?.stats?.attributes?.dexterity ?? '-'}</span>
            <span style={{marginLeft:'auto', color:'#6b3b81', fontWeight:600}}>{npc?.gender ?? ''}</span>
          </div>
        </div>
      </div>
      <p style={{marginTop:'.8rem', color:'#3b3531'}}>{npc?.description}</p>
    </article>
  );
}

NpcCard.propTypes = {
  npc: PropTypes.object.isRequired,
};

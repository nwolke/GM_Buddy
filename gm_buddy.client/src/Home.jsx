import React from 'react';
import { useState, useContext, useEffect } from 'react';
import { NavContext } from './contexts/contexts.js';

function Home() {
    const changePage = useContext(NavContext);
    return (
        <div className="home-container">
            <h1>Welcome to GM Buddy</h1>
            <p>Your companion for game mastering and campaign management.</p>
            <button>Manage NPCs</button>
            <button>Manage PCs</button>
            <button>Manage Campaigns</button>
            <button>Manage Relationships</button>
        </div>
    );
}

export default Home;
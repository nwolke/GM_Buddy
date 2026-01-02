import React from 'react';
import { useState, useContext, useEffect } from 'react';
import { NavContext } from '../../contexts/contexts.js';

function Home() {
    const changePage = useContext(NavContext);
    return (
        <div className="home-container">
            <div className="hero-image-container">
                <img src="/hero-image.jpg" alt="GM Buddy Hero" className="hero-image" />
            </div>
            <h1>Welcome to GM Buddy</h1>
            <p>Your companion for game mastering and campaign management.</p>
        </div>
    );
}

export default Home;

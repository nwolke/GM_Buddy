import { useState } from 'react';
import { AUTH_API_BASE } from './api';

function Auth() {
    const [email, setEmail] = useState();
    const [pw, setPw] = useState();

    async function authorize() {
        const requestOptions = {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                email: email,
                password: pw
            })
        };

        try {
            const response = await fetch(`${AUTH_API_BASE}/login`, requestOptions);
            if (!response.ok) {
                throw new Error(`Status: ${response.status} Error: ${response.statusText}`);
            }
            const data = await response.json();
            document.getElementById('result').innerText = data.accessToken;
        } catch (error) {
            document.getElementById('result').innerText = error.message;
        }
    }

    return (
        <div className="page-content">
            <h2 className="page-title">Login</h2>
            <div style={{maxWidth: '400px', width: '100%'}}>
                <div style={{marginBottom: '1rem'}}>
                    <label htmlFor='emailAuth' style={{display: 'block', marginBottom: '.25rem', color: 'var(--muted-ink)'}}>Email</label>
                    <input 
                        type='email' 
                        id='emailAuth' 
                        onChange={t => setEmail(t.target.value)}
                        style={{width: '100%', padding: '.5rem', borderRadius: '6px', border: '1px solid rgba(0,0,0,0.1)'}}
                    />
                </div>
                <div style={{marginBottom: '1rem'}}>
                    <label htmlFor='pwAuth' style={{display: 'block', marginBottom: '.25rem', color: 'var(--muted-ink)'}}>Password</label>
                    <input 
                        type='password' 
                        id='pwAuth' 
                        onChange={p => setPw(p.target.value)}
                        style={{width: '100%', padding: '.5rem', borderRadius: '6px', border: '1px solid rgba(0,0,0,0.1)'}}
                    />
                </div>
                <button className="btn primary" type='button' onClick={authorize} style={{width: '100%'}}>Login</button>
                <div id='result' style={{marginTop: '1rem', padding: '.75rem', background: 'rgba(0,0,0,0.03)', borderRadius: '6px', wordBreak: 'break-all', fontSize: '.85rem'}}></div>
            </div>
        </div>
    );
}

export default Auth;
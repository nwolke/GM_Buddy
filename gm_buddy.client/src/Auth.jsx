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
        <div>
            <div>
                <label htmlFor='emailAuth'>Email</label>
                <input type='email' id='emailAuth' onChange={t => setEmail(t.target.value)} />
            </div>
            <div>
                <label htmlFor='pwAuth'>Password</label>
                <input type='password' id='pwAuth' onChange={p => setPw(p.target.value)} />
            </div>
            <button type='button' onClick={authorize}>Login</button>
            <div id='result'></div>
        </div>
    );
}

export default Auth;
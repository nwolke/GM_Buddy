import { useState } from 'react';
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
        fetch('https://localhost:7256/login', requestOptions)
            .then(response => {
                if (response.ok) {
                    return response.json();
                }
                throw new Error('Status:' + response.status + ' Error: ' + response.statusText);
            })
            .then(data =>
                document.getElementById('result').innerText = data.accessToken
            )
            .catch(error => {
                document.getElementById('result').innerText = error.message;
            });
    }

    return (
        <div>
            <div>
                <label htmlFor='emailAuth'>Email</label>
                <input type='email' id='emailAuth' onChange={t =>
                    setEmail(t.target.value)
                }></input>
            </div>
            <div>
                <label htmlFor='pwAuth'>Password</label>
                <input type='password' id='pwAuth' onChange={p =>
                    setPw(p.target.value)
                }></input>
            </div>
            <button type='button' onClick={authorize}>Login</button>
            <div id='result'></div>
        </div>

    )
}

export default Auth;

function Auth() {

    const authorize=()=>{

    }

    return (
        <div>
            <input type='email' id='emailAuth'></input>
            <input type='password' id='pwAuth'></input>
            <button type='button' onClick={authorize}></button>
        </div>

    )
}

export default Auth;
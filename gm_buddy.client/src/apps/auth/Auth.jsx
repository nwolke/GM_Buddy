import { useState, useContext, useEffect } from 'react';
import {
    Container,
    Box,
    Typography,
    TextField,
    Button,
    Paper,
    Alert
} from '@mui/material';
import { useAuth } from '../../contexts/AuthContext.jsx';
import { NavContext } from '../../contexts/contexts.js';

function Auth() {
    const [email, setEmail] = useState('');
    const [pw, setPw] = useState('');
    const [feedback, setFeedback] = useState(null);
    const { login, loading, error: authError, isAuthenticated, user } = useAuth();
    const changePage = useContext(NavContext);

    useEffect(() => {
        if (isAuthenticated) {
            setFeedback({ type: 'success', message: `Already signed in as ${user?.name ?? user?.email}.` });
        }
    }, [isAuthenticated, user]);

    async function handleAuthorize() {
        setFeedback(null);
        const result = await login(email, pw);
        if (result.success) {
            setFeedback({ type: 'success', message: `Welcome back, ${result.user.name}!` });
            changePage('home');
        } else {
            setFeedback({ type: 'error', message: result.error });
        }
    }

    return (
        <Container maxWidth="sm" sx={{ mt: 4, mb: 4 }}>
            <Box
                sx={{
                    display: 'flex',
                    flexDirection: 'column',
                    alignItems: 'center',
                    gap: 3
                }}
            >
                <Typography
                    variant="h3"
                    component="h2"
                    sx={{
                        fontFamily: "'Cinzel', serif",
                        fontWeight: 700,
                        color: 'var(--ink)',
                        letterSpacing: '0.08em',
                        mb: 2
                    }}
                >
                    Login
                </Typography>

                <Paper
                    elevation={2}
                    sx={{
                        width: '100%',
                        p: 4,
                        borderRadius: 'var(--radius)',
                        border: '1px solid rgba(0,0,0,0.04)',
                        background: 'linear-gradient(180deg, rgba(255,255,255,0.6), rgba(255,255,255,0.45))'
                    }}
                >
                    <Box
                        component="form"
                        sx={{
                            display: 'flex',
                            flexDirection: 'column',
                            gap: 2
                        }}
                        onSubmit={(e) => {
                            e.preventDefault();
                            handleAuthorize();
                        }}
                    >
                        <TextField
                            fullWidth
                            type="email"
                            id="emailAuth"
                            label="Email"
                            variant="outlined"
                            value={email}
                            onChange={(e) => setEmail(e.target.value)}
                            required
                            autoComplete="username"
                            sx={{
                                '& .MuiOutlinedInput-root': {
                                    backgroundColor: 'rgba(255,255,255,0.8)',
                                    '&:hover fieldset': {
                                        borderColor: 'rgba(207,168,74,0.5)'
                                    },
                                    '&.Mui-focused fieldset': {
                                        borderColor: 'var(--accent-gold)'
                                    }
                                }
                            }}
                        />

                        <TextField
                            fullWidth
                            type="password"
                            id="pwAuth"
                            label="Password"
                            variant="outlined"
                            value={pw}
                            onChange={(e) => setPw(e.target.value)}
                            required
                            autoComplete="current-password"
                            sx={{
                                '& .MuiOutlinedInput-root': {
                                    backgroundColor: 'rgba(255,255,255,0.8)',
                                    '&:hover fieldset': {
                                        borderColor: 'rgba(207,168,74,0.5)'
                                    },
                                    '&.Mui-focused fieldset': {
                                        borderColor: 'var(--accent-gold)'
                                    }
                                }
                            }}
                        />

                        <Button
                            type="submit"
                            variant="contained"
                            size="large"
                            fullWidth
                            disabled={loading}
                            sx={{
                                mt: 1,
                                background: 'linear-gradient(180deg, rgba(207,168,74,0.14), rgba(207,168,74,0.06))',
                                color: 'var(--ink)',
                                border: '1px solid rgba(207,168,74,0.32)',
                                boxShadow: 'none',
                                '&:hover': {
                                    background: 'linear-gradient(180deg, rgba(207,168,74,0.24), rgba(207,168,74,0.16))',
                                    boxShadow: '0 2px 8px rgba(207,168,74,0.2)'
                                }
                            }}
                        >
                            {loading ? 'Signing In…' : 'Login'}
                        </Button>

                        {(feedback || authError) && (
                            <Alert
                                severity={(feedback?.type === 'error' || authError) ? 'error' : 'success'}
                                sx={{ mt: 2, wordBreak: 'break-all', fontSize: '0.85rem' }}
                            >
                                {feedback?.message || authError}
                            </Alert>
                        )}
                    </Box>
                </Paper>
            </Box>
        </Container>
    );
}

export default Auth;

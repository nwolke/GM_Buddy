import React, { useContext } from 'react';
import { NavContext } from '../../contexts/contexts.js';
import {
    Container,
    Box,
    Typography,
    Paper
} from '@mui/material';

function Home() {
    const changePage = useContext(NavContext);
    
    return (
        <Container maxWidth="md" sx={{ mt: 4, mb: 4 }}>
            <Box
                sx={{
                    textAlign: 'center',
                    display: 'flex',
                    flexDirection: 'column',
                    alignItems: 'center',
                    gap: 3
                }}
            >
                <Paper
                    elevation={2}
                    sx={{
                        width: '100%',
                        overflow: 'hidden',
                        borderRadius: 'var(--radius)',
                        border: '1px solid rgba(0,0,0,0.04)'
                    }}
                >
                    <Box
                        component="img"
                        src="/hero-image.jpg"
                        alt="GM Buddy Hero"
                        sx={{
                            width: '100%',
                            height: 'auto',
                            display: 'block'
                        }}
                    />
                </Paper>
                
                <Typography
                    variant="h2"
                    component="h1"
                    sx={{
                        fontFamily: "'Cinzel', serif",
                        fontSize: { xs: '1.8rem', sm: '2.5rem' },
                        color: 'var(--ink)',
                        mb: 1
                    }}
                >
                    Welcome to GM Buddy
                </Typography>
                
                <Typography
                    variant="h6"
                    sx={{
                        fontSize: { xs: '1rem', sm: '1.2rem' },
                        color: 'var(--muted-ink)',
                        mb: 2
                    }}
                >
                    Your companion for game mastering and campaign management.
                </Typography>
            </Box>
        </Container>
    );
}

export default Home;

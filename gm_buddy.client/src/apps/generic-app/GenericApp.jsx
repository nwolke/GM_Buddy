import React from 'react';
import {
    Container,
    Box,
    Typography,
    Paper
} from '@mui/material';

function GenericApp() {
    return (
        <Container maxWidth="lg" sx={{ mt: 4, mb: 4 }}>
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
                        letterSpacing: '0.08em'
                    }}
                >
                    Generic Application
                </Typography>

                <Paper
                    elevation={2}
                    sx={{
                        width: '100%',
                        p: 4,
                        borderRadius: 'var(--radius)',
                        border: '1px solid rgba(0,0,0,0.04)',
                        background: 'linear-gradient(180deg, rgba(255,255,255,0.6), rgba(255,255,255,0.45))',
                        textAlign: 'center'
                    }}
                >
                    <Typography
                        variant="body1"
                        sx={{
                            color: 'var(--muted-ink)',
                            fontSize: '1.05rem'
                        }}
                    >
                        This is a placeholder for your second application.
                    </Typography>
                </Paper>
            </Box>
        </Container>
    );
}

export default GenericApp;

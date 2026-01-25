import React, { useContext } from 'react';
import { NavContext } from '../../contexts/contexts.js';
import { useAuth } from '../../contexts/AuthContext.jsx';
import {
    Box,
    Breadcrumbs,
    Link,
    Typography,
    Paper,
    Button,
    Container
} from '@mui/material';
import {
    Home as HomeIcon,
    Add as AddIcon
} from '@mui/icons-material';
import NpcGrid from './features/npcs/NpcGrid.jsx';

function NpcManagerApp() {
    const changePage = useContext(NavContext);
    const { isAuthenticated } = useAuth();

    const handleBreadcrumbClick = (event) => {
        event.preventDefault();
        changePage('home');
    };

    if (!isAuthenticated) {
        return (
            <Container maxWidth="md" sx={{ mt: 4, mb: 4 }}>
                <Paper sx={{ p: 4, textAlign: 'center', borderRadius: 'var(--radius)' }}>
                    <Typography variant="h4" sx={{ fontFamily: "'Cinzel', serif", mb: 2 }}>
                        Please log in to manage your NPCs
                    </Typography>
                    <Typography color="text.secondary" sx={{ mb: 3 }}>
                        Access to NPC management requires an authenticated account.
                    </Typography>
                    <Button variant="contained" onClick={() => changePage('login')}>
                        Sign In
                    </Button>
                </Paper>
            </Container>
        );
    }

    return (
        <Container maxWidth="xl" sx={{ mt: 2, mb: 4 }}>
            {/* Breadcrumb Navigation */}
            <Breadcrumbs 
                aria-label="breadcrumb" 
                sx={{ 
                    mb: 3,
                    '& .MuiBreadcrumbs-separator': {
                        color: 'var(--muted-ink)'
                    }
                }}
            >
                <Link
                    underline="hover"
                    sx={{ 
                        display: 'flex', 
                        alignItems: 'center',
                        color: 'var(--accent-gold)',
                        cursor: 'pointer',
                        '&:hover': {
                            color: 'var(--ink)'
                        }
                    }}
                    onClick={handleBreadcrumbClick}
                >
                    <HomeIcon sx={{ mr: 0.5 }} fontSize="inherit" />
                    Home
                </Link>
                <Typography
                    sx={{ 
                        display: 'flex', 
                        alignItems: 'center',
                        color: 'var(--ink)',
                        fontWeight: 600
                    }}
                >
                    NPC Manager
                </Typography>
            </Breadcrumbs>

            {/* Page Title */}
            <Typography 
                variant="h3" 
                component="h1" 
                sx={{ 
                    mb: 3,
                    fontFamily: "'Cinzel', serif",
                    fontWeight: 700,
                    color: 'var(--ink)',
                    letterSpacing: '0.08em'
                }}
            >
                NPC Manager
            </Typography>

            {/* NPC Content */}
            <Paper 
                elevation={2}
                sx={{ 
                    borderRadius: 'var(--radius)',
                    overflow: 'hidden',
                    border: '1px solid rgba(0,0,0,0.04)',
                    p: 3
                }}
            >
                <Box sx={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', mb: 3 }}>
                    <Typography variant="h5" sx={{ fontFamily: "'Cinzel', serif" }}>
                        Your NPCs
                    </Typography>
                    <Button 
                        variant="contained" 
                        startIcon={<AddIcon />}
                        sx={{ 
                            background: 'linear-gradient(180deg, rgba(207,168,74,0.14), rgba(207,168,74,0.06))',
                            color: 'var(--ink)',
                            border: '1px solid rgba(207,168,74,0.32)',
                            '&:hover': {
                                background: 'linear-gradient(180deg, rgba(207,168,74,0.24), rgba(207,168,74,0.16))',
                            }
                        }}
                    >
                        Create NPC
                    </Button>
                </Box>
                
                <NpcGrid />
            </Paper>
        </Container>
    );
}

export default NpcManagerApp;

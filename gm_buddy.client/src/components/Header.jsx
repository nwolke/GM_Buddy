import React, { useState, useContext } from 'react';
import { NavContext } from '../contexts/contexts.js';
import { useAuth } from '../contexts/AuthContext.jsx';
import {
    AppBar,
    Toolbar,
    Box,
    Button,
    Menu,
    MenuItem,
    Typography
} from '@mui/material';
import { ArrowDropDown } from '@mui/icons-material';
import logo from '../assets/temporary logo for b.png';

export default function Header() {
    const [appMenuAnchor, setAppMenuAnchor] = useState(null);
    const changePage = useContext(NavContext);
    const showAppDropdown = Boolean(appMenuAnchor);
    const { isAuthenticated, user, logout } = useAuth();

    const handleAppMenuOpen = (event) => {
        setAppMenuAnchor(event.currentTarget);
    };

    const handleAppMenuClose = () => {
        setAppMenuAnchor(null);
    };

    const handleAppSelect = (app) => {
        changePage(app);
        handleAppMenuClose();
    };

    const handleLogoClick = () => {
        changePage('home');
    };

    return (
        <AppBar 
            position="static" 
            elevation={0}
            sx={{ 
                background: 'linear-gradient(180deg, rgba(255,255,255,0.04), transparent)',
                backdropFilter: 'blur(2px)',
                borderBottom: '2px solid rgba(0,0,0,0.05)',
                color: 'var(--ink)'
            }}
        >
            <Toolbar sx={{ gap: 2, flexWrap: 'wrap', py: 1 }}>
                {/* Site Brand */}
                <Box 
                    sx={{ 
                        display: 'flex', 
                        alignItems: 'center', 
                        gap: 1.5,
                        cursor: 'pointer'
                    }}
                    onClick={handleLogoClick}
                >
                    <Box
                        component="img"
                        src={logo}
                        alt="GM Buddy Logo"
                        sx={{
                            width: { xs: 48, sm: 64 },
                            height: { xs: 48, sm: 64 },
                            objectFit: 'contain',
                            borderRadius: 2,
                            boxShadow: '0 2px 8px rgba(30,27,24,0.12)',
                            background: 'linear-gradient(135deg, rgba(207,168,74,0.08), rgba(30,70,47,0.04))',
                            p: 0.5
                        }}
                    />
                    <Box>
                        <Typography
                            variant="h6"
                            component="h1"
                            sx={{
                                fontFamily: "'Cinzel', serif",
                                fontWeight: 700,
                                fontSize: { xs: '1.3rem', sm: '1.6rem' },
                                letterSpacing: '0.06em',
                                margin: 0,
                                color: 'var(--ink)'
                            }}
                        >
                            GM Buddy
                        </Typography>
                        <Typography
                            variant="caption"
                            sx={{
                                fontSize: '0.85rem',
                                color: 'var(--muted-ink)',
                                display: 'block'
                            }}
                        >
                            A tabletop toolkit
                        </Typography>
                    </Box>
                </Box>

                {/* Navigation */}
                <Box sx={{ display: 'flex', alignItems: 'center', gap: 1, ml: { sm: 2 } }}>
                    <Button
                        variant="outlined"
                        endIcon={<ArrowDropDown />}
                        onClick={handleAppMenuOpen}
                        aria-controls={showAppDropdown ? 'app-menu' : undefined}
                        aria-haspopup="true"
                        aria-expanded={showAppDropdown ? 'true' : undefined}
                        sx={{
                            color: 'var(--ink)',
                            borderColor: 'rgba(0,0,0,0.06)',
                            '&:hover': {
                                borderColor: 'rgba(0,0,0,0.12)',
                                background: 'rgba(207,168,74,0.05)'
                            }
                        }}
                    >
                        Applications
                    </Button>
                    <Menu
                        id="app-menu"
                        anchorEl={appMenuAnchor}
                        open={showAppDropdown}
                        onClose={handleAppMenuClose}
                        MenuListProps={{
                            'aria-labelledby': 'app-menu-button',
                        }}
                        PaperProps={{
                            elevation: 3,
                            sx: {
                                mt: 1,
                                borderRadius: 2,
                                border: '1px solid rgba(0,0,0,0.08)',
                                minWidth: 180
                            }
                        }}
                    >
                        <MenuItem 
                            onClick={() => handleAppSelect('npc-manager')}
                            sx={{
                                '&:hover': {
                                    background: 'rgba(207,168,74,0.1)'
                                }
                            }}
                        >
                            NPC Manager
                        </MenuItem>
                    </Menu>
                </Box>

                {/* Spacer */}
                <Box sx={{ flexGrow: 1 }} />

                {/* Header Actions */}
                <Box 
                    sx={{ 
                        display: 'flex', 
                        alignItems: 'center', 
                        gap: 1,
                        flexDirection: { xs: 'column', sm: 'row' }
                    }}
                >
                    {isAuthenticated ? (
                        <>
                            <Typography
                                variant="body2"
                                sx={{
                                    color: 'var(--muted-ink)',
                                    fontWeight: 600
                                }}
                            >
                                {user?.name ?? user?.email}
                            </Typography>
                            <Button
                                variant="outlined"
                                onClick={logout}
                                sx={{
                                    color: 'var(--ink)',
                                    borderColor: 'rgba(0,0,0,0.06)',
                                    '&:hover': {
                                        borderColor: 'rgba(0,0,0,0.12)',
                                        background: 'rgba(207,168,74,0.05)'
                                    }
                                }}
                            >
                                Logout
                            </Button>
                        </>
                    ) : (
                        <>
                            <Button
                                variant="outlined"
                                onClick={() => changePage('login')}
                                sx={{
                                    color: 'var(--ink)',
                                    borderColor: 'rgba(0,0,0,0.06)',
                                    '&:hover': {
                                        borderColor: 'rgba(0,0,0,0.12)',
                                        background: 'rgba(207,168,74,0.05)'
                                    }
                                }}
                            >
                                Login
                            </Button>
                            <Button
                                variant="contained"
                                onClick={() => changePage('signup')}
                                sx={{
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
                                Sign Up
                            </Button>
                        </>
                    )}
                </Box>
            </Toolbar>
        </AppBar>
    );
}

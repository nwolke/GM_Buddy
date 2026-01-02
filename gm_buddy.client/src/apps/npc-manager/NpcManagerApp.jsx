import React, { useState, useContext } from 'react';
import { NavContext } from '../../contexts/contexts.js';
import {
    Box,
    Breadcrumbs,
    Link,
    Typography,
    Tabs,
    Tab,
    Paper,
    Button,
    Container
} from '@mui/material';
import {
    Home as HomeIcon,
    People as PeopleIcon,
    TheaterComedy as TheaterComedyIcon,
    MenuBook as MenuBookIcon,
    AccountBalance as AccountBalanceIcon,
    Link as LinkIcon
} from '@mui/icons-material';

function TabPanel({ children, value, index, ...other }) {
    return (
        <div
            role="tabpanel"
            hidden={value !== index}
            id={`manager-tabpanel-${index}`}
            aria-labelledby={`manager-tab-${index}`}
            {...other}
        >
            {value === index && (
                <Box sx={{ p: 3 }}>
                    {children}
                </Box>
            )}
        </div>
    );
}

function NpcManagerApp() {
    const [activeTab, setActiveTab] = useState(0);
    const changePage = useContext(NavContext);

    const handleTabChange = (event, newValue) => {
        setActiveTab(newValue);
    };

    const handleBreadcrumbClick = (event) => {
        event.preventDefault();
        changePage('home');
    };

    const tabs = [
        { label: 'NPCs', icon: <PeopleIcon />, content: 'NPC Manager' },
        { label: 'PCs', icon: <TheaterComedyIcon />, content: 'PC Manager' },
        { label: 'Campaigns', icon: <MenuBookIcon />, content: 'Campaign Manager' },
        { label: 'Factions', icon: <AccountBalanceIcon />, content: 'Faction Manager' },
        { label: 'Relationships', icon: <LinkIcon />, content: 'Relationship Manager' }
    ];

    return (
        <Container maxWidth="lg" sx={{ mt: 2, mb: 4 }}>
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
                    Relationship Manager
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
                Relationship Manager
            </Typography>

            {/* Tab Navigation */}
            <Paper 
                elevation={2}
                sx={{ 
                    borderRadius: 'var(--radius)',
                    overflow: 'hidden',
                    border: '1px solid rgba(0,0,0,0.04)'
                }}
            >
                <Tabs
                    value={activeTab}
                    onChange={handleTabChange}
                    variant="fullWidth"
                    aria-label="manager tabs"
                    sx={{
                        borderBottom: 1,
                        borderColor: 'divider',
                        background: 'linear-gradient(180deg, rgba(255,255,255,0.6), rgba(255,255,255,0.45))',
                        '& .MuiTab-root': {
                            fontFamily: "'Cinzel', serif",
                            fontWeight: 600,
                            fontSize: { xs: '0.75rem', sm: '0.85rem', md: '0.95rem' },
                            letterSpacing: '0.05em',
                            textTransform: 'none',
                            minHeight: { xs: '56px', sm: '64px' },
                            minWidth: { xs: '60px', sm: '100px', md: '140px' },
                            maxWidth: { xs: '120px', sm: '180px', md: '220px' },
                            color: 'var(--muted-ink)',
                            flex: 1,
                            '&.Mui-selected': {
                                color: 'var(--accent-gold)',
                            },
                            '& .MuiSvgIcon-root': {
                                fontSize: { xs: '1.2rem', sm: '1.4rem', md: '1.5rem' }
                            }
                        },
                        '& .MuiTabs-indicator': {
                            backgroundColor: 'var(--accent-gold)',
                            height: '3px'
                        },
                        '& .MuiTabs-flexContainer': {
                            justifyContent: 'space-between'
                        }
                    }}
                >
                    {tabs.map((tab, index) => (
                        <Tab
                            key={index}
                            icon={tab.icon}
                            label={tab.label}
                            iconPosition="start"
                            id={`manager-tab-${index}`}
                            aria-controls={`manager-tabpanel-${index}`}
                            sx={{
                                '@media (max-width: 600px)': {
                                    flexDirection: 'column',
                                    gap: 0.25,
                                    '& .MuiTab-iconWrapper': {
                                        marginRight: 0,
                                        marginBottom: '4px'
                                    }
                                }
                            }}
                        />
                    ))}
                </Tabs>

                {/* Tab Panels */}
                <TabPanel value={activeTab} index={0}>
                    <Typography variant="h5" sx={{ mb: 2, fontFamily: "'Cinzel', serif" }}>
                        NPC Manager
                    </Typography>
                    <Typography variant="body1" sx={{ mb: 3, color: 'var(--muted-ink)' }}>
                        Manage your Non-Player Characters here. Create, edit, and organize NPCs for your campaigns.
                    </Typography>
                    <Button variant="contained" sx={{ 
                        background: 'linear-gradient(180deg, rgba(207,168,74,0.14), rgba(207,168,74,0.06))',
                        color: 'var(--ink)',
                        border: '1px solid rgba(207,168,74,0.32)',
                        '&:hover': {
                            background: 'linear-gradient(180deg, rgba(207,168,74,0.24), rgba(207,168,74,0.16))',
                        }
                    }}>
                        Add New NPC
                    </Button>
                </TabPanel>

                <TabPanel value={activeTab} index={1}>
                    <Typography variant="h5" sx={{ mb: 2, fontFamily: "'Cinzel', serif" }}>
                        PC Manager
                    </Typography>
                    <Typography variant="body1" sx={{ mb: 3, color: 'var(--muted-ink)' }}>
                        Manage your Player Characters here. Track player characters across your campaigns.
                    </Typography>
                    <Button variant="contained" sx={{ 
                        background: 'linear-gradient(180deg, rgba(207,168,74,0.14), rgba(207,168,74,0.06))',
                        color: 'var(--ink)',
                        border: '1px solid rgba(207,168,74,0.32)',
                        '&:hover': {
                            background: 'linear-gradient(180deg, rgba(207,168,74,0.24), rgba(207,168,74,0.16))',
                        }
                    }}>
                        Add New PC
                    </Button>
                </TabPanel>

                <TabPanel value={activeTab} index={2}>
                    <Typography variant="h5" sx={{ mb: 2, fontFamily: "'Cinzel', serif" }}>
                        Campaign Manager
                    </Typography>
                    <Typography variant="body1" sx={{ mb: 3, color: 'var(--muted-ink)' }}>
                        Organize and manage your campaigns. Create new adventures and track ongoing stories.
                    </Typography>
                    <Button variant="contained" sx={{ 
                        background: 'linear-gradient(180deg, rgba(207,168,74,0.14), rgba(207,168,74,0.06))',
                        color: 'var(--ink)',
                        border: '1px solid rgba(207,168,74,0.32)',
                        '&:hover': {
                            background: 'linear-gradient(180deg, rgba(207,168,74,0.24), rgba(207,168,74,0.16))',
                        }
                    }}>
                        Create New Campaign
                    </Button>
                </TabPanel>

                <TabPanel value={activeTab} index={3}>
                    <Typography variant="h5" sx={{ mb: 2, fontFamily: "'Cinzel', serif" }}>
                        Faction Manager
                    </Typography>
                    <Typography variant="body1" sx={{ mb: 3, color: 'var(--muted-ink)' }}>
                        Manage factions and organizations. Define groups, guilds, and political entities in your world.
                    </Typography>
                    <Button variant="contained" sx={{ 
                        background: 'linear-gradient(180deg, rgba(207,168,74,0.14), rgba(207,168,74,0.06))',
                        color: 'var(--ink)',
                        border: '1px solid rgba(207,168,74,0.32)',
                        '&:hover': {
                            background: 'linear-gradient(180deg, rgba(207,168,74,0.24), rgba(207,168,74,0.16))',
                        }
                    }}>
                        Add New Faction
                    </Button>
                </TabPanel>

                <TabPanel value={activeTab} index={4}>
                    <Typography variant="h5" sx={{ mb: 2, fontFamily: "'Cinzel', serif" }}>
                        Relationship Manager
                    </Typography>
                    <Typography variant="body1" sx={{ mb: 3, color: 'var(--muted-ink)' }}>
                        Define and manage relationships between entities. Track friendships, rivalries, and alliances.
                    </Typography>
                    <Button variant="contained" sx={{ 
                        background: 'linear-gradient(180deg, rgba(207,168,74,0.14), rgba(207,168,74,0.06))',
                        color: 'var(--ink)',
                        border: '1px solid rgba(207,168,74,0.32)',
                        '&:hover': {
                            background: 'linear-gradient(180deg, rgba(207,168,74,0.24), rgba(207,168,74,0.16))',
                        }
                    }}>
                        Add New Relationship
                    </Button>
                </TabPanel>
            </Paper>
        </Container>
    );
}

export default NpcManagerApp;

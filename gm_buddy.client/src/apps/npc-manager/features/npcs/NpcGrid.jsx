import PropTypes from 'prop-types';
import { useState, useContext, useEffect } from 'react';
import { NavContext } from '../../../../contexts/contexts.js';
import NpcCard from '../../../../components/NpcCard';
import { API_BASE } from '../../../../api';
import {
    Container,
    Box,
    Typography,
    Button,
    Stack,
    Table,
    TableBody,
    TableCell,
    TableContainer,
    TableHead,
    TableRow,
    Paper,
    CircularProgress,
    useMediaQuery,
    useTheme
} from '@mui/material';
import { Refresh as RefreshIcon, Home as HomeIcon } from '@mui/icons-material';

function NpcGrid() {
    const [npcData, setNpcData] = useState(undefined);
    const [loading, setLoading] = useState(true);
    const changePage = useContext(NavContext);
    const theme = useTheme();
    const isMobile = useMediaQuery(theme.breakpoints.down('md'));
    
    useEffect(() => {
        get_test_npcs();
    }, []);

    async function get_test_npcs() {
        setLoading(true);
        try {
            const url = `${API_BASE}/Npcs?account_id=1`;
            console.log('[NpcGrid] Fetching from URL:', url);
            const response = await fetch(url);
            if (response.ok) {
                const data = await response.json();
                setNpcData(data);
            } else {
                console.error('Failed to fetch data', response.status);
            }
        } catch (e) {
            console.error(e);
        } finally {
            setLoading(false);
        }
    }

    return (
        <Container maxWidth="xl" sx={{ mt: 2, mb: 4 }}>
            <Box sx={{ display: 'flex', flexDirection: 'column', gap: 3 }}>
                {/* Page Title */}
                <Typography
                    variant="h3"
                    component="h2"
                    sx={{
                        fontFamily: "'Cinzel', serif",
                        fontWeight: 700,
                        color: 'var(--ink)',
                        letterSpacing: '0.08em',
                        textAlign: 'center'
                    }}
                >
                    NPC Collection
                </Typography>

                {/* Action Buttons */}
                <Stack
                    direction="row"
                    spacing={1}
                    justifyContent="center"
                    flexWrap="wrap"
                    sx={{ gap: 1 }}
                >
                    <Button
                        variant="outlined"
                        startIcon={<HomeIcon />}
                        onClick={() => changePage('home')}
                        sx={{
                            color: 'var(--ink)',
                            borderColor: 'rgba(0,0,0,0.06)',
                            '&:hover': {
                                borderColor: 'rgba(0,0,0,0.12)',
                                background: 'rgba(207,168,74,0.05)'
                            }
                        }}
                    >
                        Go Home
                    </Button>
                    <Button
                        variant="outlined"
                        startIcon={<RefreshIcon />}
                        onClick={() => get_test_npcs()}
                        sx={{
                            color: 'var(--ink)',
                            borderColor: 'rgba(0,0,0,0.06)',
                            '&:hover': {
                                borderColor: 'rgba(0,0,0,0.12)',
                                background: 'rgba(207,168,74,0.05)'
                            }
                        }}
                    >
                        Refresh
                    </Button>
                </Stack>

                {/* Loading State */}
                {loading && (
                    <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
                        <CircularProgress sx={{ color: 'var(--accent-gold)' }} />
                    </Box>
                )}

                {/* Cards for mobile/tablet */}
                {!loading && isMobile && (
                    <Stack spacing={2} alignItems="center">
                        {npcData && npcData.length > 0 ? (
                            npcData.map((n, i) => <NpcCard key={n.Npc_Id ?? i} npc={n} />)
                        ) : (
                            <Typography variant="body1" color="text.secondary">
                                No NPCs found
                            </Typography>
                        )}
                    </Stack>
                )}

                {/* Table for desktop */}
                {!loading && !isMobile && (
                    <TableContainer
                        component={Paper}
                        elevation={2}
                        sx={{
                            borderRadius: 'var(--radius)',
                            border: '1px solid rgba(0,0,0,0.04)',
                            background: 'linear-gradient(180deg, rgba(255,255,255,0.6), rgba(255,255,255,0.45))',
                            overflow: 'auto'
                        }}
                    >
                        <Table sx={{ minWidth: 650 }}>
                            <TableHead>
                                <TableRow>
                                    <TableCell sx={{ fontFamily: "'Cinzel', serif", fontWeight: 600, color: 'var(--muted-ink)' }}>ID</TableCell>
                                    <TableCell sx={{ fontFamily: "'Cinzel', serif", fontWeight: 600, color: 'var(--muted-ink)' }}>Name</TableCell>
                                    <TableCell sx={{ fontFamily: "'Cinzel', serif", fontWeight: 600, color: 'var(--muted-ink)' }}>Lineage</TableCell>
                                    <TableCell sx={{ fontFamily: "'Cinzel', serif", fontWeight: 600, color: 'var(--muted-ink)' }}>Occupation</TableCell>
                                    <TableCell align="center" sx={{ fontFamily: "'Cinzel', serif", fontWeight: 600, color: 'var(--muted-ink)' }}>STR</TableCell>
                                    <TableCell align="center" sx={{ fontFamily: "'Cinzel', serif", fontWeight: 600, color: 'var(--muted-ink)' }}>DEX</TableCell>
                                    <TableCell align="center" sx={{ fontFamily: "'Cinzel', serif", fontWeight: 600, color: 'var(--muted-ink)' }}>CON</TableCell>
                                    <TableCell align="center" sx={{ fontFamily: "'Cinzel', serif", fontWeight: 600, color: 'var(--muted-ink)' }}>INT</TableCell>
                                    <TableCell align="center" sx={{ fontFamily: "'Cinzel', serif", fontWeight: 600, color: 'var(--muted-ink)' }}>WIS</TableCell>
                                    <TableCell align="center" sx={{ fontFamily: "'Cinzel', serif", fontWeight: 600, color: 'var(--muted-ink)' }}>CHA</TableCell>
                                    <TableCell sx={{ fontFamily: "'Cinzel', serif", fontWeight: 600, color: 'var(--muted-ink)' }}>System</TableCell>
                                </TableRow>
                            </TableHead>
                            <TableBody>
                                {npcData && npcData.length > 0 ? (
                                    npcData.map((npc) => (
                                        <TableRow
                                            key={npc.npc_id}
                                            sx={{
                                                '&:hover': {
                                                    backgroundColor: 'rgba(207,168,74,0.05)'
                                                }
                                            }}
                                        >
                                            <TableCell>{npc.npc_id ?? '-'}</TableCell>
                                            <TableCell sx={{ fontWeight: 600 }}>{npc.stats?.name ?? '-'}</TableCell>
                                            <TableCell>{npc.stats?.lineage ?? '-'}</TableCell>
                                            <TableCell>{npc.stats?.occupation ?? '-'}</TableCell>
                                            <TableCell align="center">{npc.stats?.attributes?.strength ?? '-'}</TableCell>
                                            <TableCell align="center">{npc.stats?.attributes?.dexterity ?? '-'}</TableCell>
                                            <TableCell align="center">{npc.stats?.attributes?.constitution ?? '-'}</TableCell>
                                            <TableCell align="center">{npc.stats?.attributes?.intelligence ?? '-'}</TableCell>
                                            <TableCell align="center">{npc.stats?.attributes?.wisdom ?? '-'}</TableCell>
                                            <TableCell align="center">{npc.stats?.attributes?.charisma ?? '-'}</TableCell>
                                            <TableCell>
                                                <Typography variant="caption" sx={{ color: '#999' }}>
                                                    {npc.system ?? '-'}
                                                </Typography>
                                            </TableCell>
                                        </TableRow>
                                    ))
                                ) : (
                                    <TableRow>
                                        <TableCell colSpan={11} align="center">
                                            <Typography variant="body1" color="text.secondary" sx={{ py: 2 }}>
                                                No NPCs found
                                            </Typography>
                                        </TableCell>
                                    </TableRow>
                                )}
                            </TableBody>
                        </Table>
                    </TableContainer>
                )}
            </Box>
        </Container>
    );
}

NpcGrid.propTypes = {
    allNpcs: PropTypes.array,
}

export default NpcGrid;

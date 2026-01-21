import PropTypes from 'prop-types';
import { useState, useEffect, useCallback } from 'react';
import { useAuth } from '../../../../contexts/AuthContext.jsx';
import DndNpcCard from '../../../../components/DndNpcCard.jsx';
import { API_BASE, apiFetch } from '../../../../api';
import {
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
import { Refresh as RefreshIcon } from '@mui/icons-material';

function NpcGrid() {
    const [npcData, setNpcData] = useState(undefined);
    const [loading, setLoading] = useState(true);
    const theme = useTheme();
    const isMobile = useMediaQuery(theme.breakpoints.down('md'));
    const { token, accountId, isAuthenticated, loading: authLoading } = useAuth();

    // Memoize fetch function to prevent recreation on each render
    const fetchNpcs = useCallback(async () => {
        if (!isAuthenticated || !token || !accountId) {
            setNpcData([]);
            setLoading(false);
            return;
        }

        setLoading(true);
        try {
            const url = `${API_BASE}/Npcs?account_id=${accountId}`;
            console.log('[NpcGrid] Fetching from URL:', url);
            const data = await apiFetch(url, { authToken: token });
            setNpcData(data);
        } catch (e) {
            console.error(e);
        } finally {
            setLoading(false);
        }
    }, [accountId, isAuthenticated, token]);

    useEffect(() => {
        fetchNpcs();
    }, [fetchNpcs]);

    const showLoading = loading || authLoading;

    return (
        <Box sx={{ display: 'flex', flexDirection: 'column', gap: 2 }}>
            {/* Refresh Button */}
            <Box sx={{ display: 'flex', justifyContent: 'flex-end' }}>
                <Button
                    variant="outlined"
                    startIcon={<RefreshIcon />}
                    onClick={fetchNpcs}
                    disabled={!isAuthenticated || !accountId || showLoading}
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
            </Box>

            {/* Loading State */}
            {showLoading && (
                <Box sx={{ display: 'flex', justifyContent: 'center', py: 4 }}>
                    <CircularProgress sx={{ color: 'var(--accent-gold)' }} />
                </Box>
            )}

            {/* Cards for mobile/tablet */}
            {!showLoading && isMobile && (
                <Stack spacing={2} alignItems="center">
                    {npcData && npcData.length > 0 ? (
                        npcData.map((n, i) => <DndNpcCard key={n.Npc_Id ?? i} npc={n} />)
                    ) : (
                        <Typography variant="body1" color="text.secondary">
                            No NPCs found. Create your first NPC!
                        </Typography>
                    )}
                </Stack>
            )}

            {/* Table for desktop */}
            {!showLoading && !isMobile && (
                <TableContainer
                    component={Paper}
                    elevation={0}
                    sx={{
                        borderRadius: 'var(--radius)',
                        border: '1px solid rgba(0,0,0,0.08)',
                        background: 'rgba(255,255,255,0.5)',
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
                                            cursor: 'pointer',
                                            '&:hover': {
                                                backgroundColor: 'rgba(207,168,74,0.08)'
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
                                            No NPCs found. Create your first NPC!
                                        </Typography>
                                    </TableCell>
                                </TableRow>
                            )}
                        </TableBody>
                    </Table>
                </TableContainer>
            )}
        </Box>
    );
}

NpcGrid.propTypes = {
    allNpcs: PropTypes.array,
}

export default NpcGrid;

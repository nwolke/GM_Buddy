import React, { memo } from 'react';
import PropTypes from 'prop-types';
import {
    Card,
    CardContent,
    Typography,
    Chip,
    Stack
} from '@mui/material';

function DndNpcCard({ npc }) {
    const stats = npc?.Stats;
    const attrs = stats?.Attributes;
    
    const attributes = [
        { label: 'STR', value: attrs?.Strength },
        { label: 'DEX', value: attrs?.Dexterity },
        { label: 'CON', value: attrs?.Constitution },
        { label: 'INT', value: attrs?.Intelligence },
        { label: 'WIS', value: attrs?.Wisdom },
        { label: 'CHA', value: attrs?.Charisma }
    ];
    
    return (
        <Card
            elevation={2}
            sx={{
                background: 'linear-gradient(180deg, rgba(255,255,255,0.6), rgba(255,255,255,0.45))',
                borderRadius: 'var(--radius)',
                border: '1px solid rgba(0,0,0,0.04)',
                boxShadow: 'var(--shadow)',
                transition: 'transform 0.12s ease, box-shadow 0.12s ease',
                width: '100%',
                maxWidth: 500,
                '&:hover': {
                    transform: 'translateY(-6px)',
                    boxShadow: '0 14px 30px rgba(30,27,24,0.18)'
                }
            }}
        >
            <CardContent>
                <Typography
                    variant="h5"
                    component="h3"
                    sx={{
                        fontFamily: "'Cinzel', serif",
                        fontWeight: 700,
                        fontSize: '1.15rem',
                        color: 'var(--ink)',
                        mb: 0.5
                    }}
                >
                    {stats?.Name ?? 'Unnamed NPC'}
                </Typography>
                
                <Typography
                    variant="body2"
                    sx={{
                        color: 'var(--muted-ink)',
                        fontSize: '0.9rem',
                        mb: 1.5
                    }}
                >
                    {stats?.Lineage ?? 'Unknown'} • {stats?.Occupation ?? 'Unknown'}
                </Typography>

                {/* Attribute Chips */}
                <Stack
                    direction="row"
                    spacing={0.5}
                    flexWrap="wrap"
                    sx={{ gap: 0.5, mb: stats?.Description ? 1.5 : 0 }}
                >
                    {attributes.map((attr) => (
                        <Chip
                            key={attr.label}
                            label={`${attr.label} ${attr.value ?? '-'}`}
                            size="small"
                            sx={{
                                backgroundColor: 'rgba(30,70,47,0.06)',
                                color: 'var(--accent-green)',
                                fontWeight: 600,
                                fontSize: '0.85rem',
                                border: '1px solid rgba(30,70,47,0.06)'
                            }}
                        />
                    ))}
                    {stats?.Gender && (
                        <Chip
                            label={stats.Gender}
                            size="small"
                            sx={{
                                ml: 'auto',
                                backgroundColor: 'rgba(107,59,129,0.1)',
                                color: '#6b3b81',
                                fontWeight: 600
                            }}
                        />
                    )}
                </Stack>

                {/* Description */}
                {stats?.Description && (
                    <Typography
                        variant="body2"
                        sx={{
                            color: 'var(--muted-ink)',
                            mt: 1.5
                        }}
                    >
                        {stats.Description}
                    </Typography>
                )}

                {/* Languages */}
                {stats?.Languages && stats.Languages.length > 0 && (
                    <Typography
                        variant="body2"
                        sx={{
                            mt: 1,
                            fontSize: '0.9rem',
                            color: '#6b3b81'
                        }}
                    >
                        Languages: {stats.Languages.join(', ')}
                    </Typography>
                )}

                {/* System */}
                <Typography
                    variant="caption"
                    sx={{
                        display: 'block',
                        mt: 1,
                        fontSize: '0.85rem',
                        color: '#999'
                    }}
                >
                    System: {npc?.System ?? 'Unknown'}
                </Typography>
            </CardContent>
        </Card>
    );
}

DndNpcCard.propTypes = {
    npc: PropTypes.object.isRequired,
};

// Memoize to prevent re-renders when NPC data hasn't changed
export default memo(DndNpcCard);

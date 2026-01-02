import { createTheme } from '@mui/material/styles';

// Create a custom MUI theme that matches the parchment/fantasy aesthetic
const muiTheme = createTheme({
    palette: {
        mode: 'light',
        primary: {
            main: '#cfa84a', // accent-gold
            light: '#e0c580',
            dark: '#a68a3a',
            contrastText: '#1e1b18',
        },
        secondary: {
            main: '#1e462f', // accent-green
            light: '#3d6b51',
            dark: '#152f20',
            contrastText: '#f4ecd6',
        },
        background: {
            default: '#f4ecd6', // bg-parchment
            paper: 'rgba(255,255,255,0.6)',
        },
        text: {
            primary: '#1e1b18', // ink
            secondary: '#3b3531', // muted-ink
        },
        divider: 'rgba(0,0,0,0.06)',
    },
    typography: {
        fontFamily: "Inter, system-ui, -apple-system, 'Segoe UI', Roboto, 'Helvetica Neue', Arial",
        h1: {
            fontFamily: "'Cinzel', serif",
            fontWeight: 700,
            letterSpacing: '0.08em',
        },
        h2: {
            fontFamily: "'Cinzel', serif",
            fontWeight: 700,
            letterSpacing: '0.06em',
        },
        h3: {
            fontFamily: "'Cinzel', serif",
            fontWeight: 700,
            letterSpacing: '0.06em',
        },
        h4: {
            fontFamily: "'Cinzel', serif",
            fontWeight: 600,
            letterSpacing: '0.05em',
        },
        h5: {
            fontFamily: "'Cinzel', serif",
            fontWeight: 600,
            letterSpacing: '0.05em',
        },
        h6: {
            fontFamily: "'Cinzel', serif",
            fontWeight: 600,
            letterSpacing: '0.05em',
        },
        button: {
            textTransform: 'none',
            fontWeight: 500,
        },
    },
    shape: {
        borderRadius: 10, // var(--radius)
    },
    shadows: [
        'none',
        '0 2px 4px rgba(30,27,24,0.08)',
        '0 4px 8px rgba(30,27,24,0.12)',
        '0 6px 18px rgba(30,27,24,0.18)', // var(--shadow)
        '0 8px 24px rgba(30,27,24,0.22)',
        '0 12px 32px rgba(30,27,24,0.26)',
        '0 16px 40px rgba(30,27,24,0.30)',
        '0 20px 48px rgba(30,27,24,0.34)',
        '0 24px 56px rgba(30,27,24,0.38)',
        '0 28px 64px rgba(30,27,24,0.42)',
        '0 32px 72px rgba(30,27,24,0.46)',
        '0 36px 80px rgba(30,27,24,0.50)',
        '0 40px 88px rgba(30,27,24,0.54)',
        '0 44px 96px rgba(30,27,24,0.58)',
        '0 48px 104px rgba(30,27,24,0.62)',
        '0 52px 112px rgba(30,27,24,0.66)',
        '0 56px 120px rgba(30,27,24,0.70)',
        '0 60px 128px rgba(30,27,24,0.74)',
        '0 64px 136px rgba(30,27,24,0.78)',
        '0 68px 144px rgba(30,27,24,0.82)',
        '0 72px 152px rgba(30,27,24,0.86)',
        '0 76px 160px rgba(30,27,24,0.90)',
        '0 80px 168px rgba(30,27,24,0.94)',
        '0 84px 176px rgba(30,27,24,0.98)',
    ],
    components: {
        MuiPaper: {
            styleOverrides: {
                root: {
                    backgroundImage: 'linear-gradient(180deg, rgba(255,255,255,0.6), rgba(255,255,255,0.45))',
                },
            },
        },
        MuiButton: {
            styleOverrides: {
                root: {
                    borderRadius: 8,
                    padding: '0.5rem 0.9rem',
                    transition: 'transform 0.12s ease, box-shadow 0.12s ease',
                    '&:hover': {
                        transform: 'translateY(-2px)',
                    },
                },
                contained: {
                    boxShadow: '0 6px 18px rgba(30,27,24,0.18)',
                    '&:hover': {
                        boxShadow: '0 8px 24px rgba(30,27,24,0.24)',
                    },
                },
            },
        },
        MuiTab: {
            styleOverrides: {
                root: {
                    '&:hover': {
                        backgroundColor: 'rgba(207,168,74,0.05)',
                    },
                },
            },
        },
        MuiCard: {
            styleOverrides: {
                root: {
                    borderRadius: 10,
                    border: '1px solid rgba(0,0,0,0.04)',
                    boxShadow: '0 6px 18px rgba(30,27,24,0.18)',
                    transition: 'transform 0.12s ease, box-shadow 0.12s ease',
                    '&:hover': {
                        transform: 'translateY(-6px)',
                        boxShadow: '0 14px 30px rgba(30,27,24,0.24)',
                    },
                },
            },
        },
        MuiBreadcrumbs: {
            styleOverrides: {
                separator: {
                    color: '#3b3531',
                },
            },
        },
    },
});

export default muiTheme;

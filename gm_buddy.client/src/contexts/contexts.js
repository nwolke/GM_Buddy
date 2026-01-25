import { createContext } from 'react';

// Navigation context for page switching
export const NavContext = createContext('navContext');

// Re-export auth context and hook for convenience
export { AuthProvider, useAuth } from './AuthContext.jsx';
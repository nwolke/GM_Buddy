export interface NPC {
  id: number;
  name: string;
  race: string;
  class: string;
  description: string;
  campaignId?: string;
  system?: string; // Game system name (read-only from campaign)
  faction?: string;
  notes?: string;
  accountId?: number;
}

export interface Relationship {
  id: number;
  npcId1: number;
  npcId2: number;
  type: RelationshipType;
  description?: string;
}

export type RelationshipType = 'ally' | 'enemy' | 'family' | 'rival' | 'mentor' | 'student' | 'neutral';

// Auth context types
export interface User {
  cognitoSub: string;
  email: string;
  accountId?: number;
}

export interface AuthState {
  isAuthenticated: boolean;
  user: User | null;
  loading: boolean;
}


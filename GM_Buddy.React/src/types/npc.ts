export interface NPC {
  id: string;
  name: string;
  race: string;
  class: string;
  description: string;
  faction?: string;
  notes?: string;
  accountId?: number;
}

export interface Relationship {
  id: string;
  npcId1: string;
  npcId2: string;
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


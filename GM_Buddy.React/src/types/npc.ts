export interface NPC {
  id: number;
  name: string;
  lineage: string;
  class: string;
  description: string;
  campaignId?: number;
  faction?: string;
  notes?: string;
  accountId?: number;
}

export interface Relationship {
  id: number;
  npcId1: number;
  npcId2: number;
  entityType1: 'npc' | 'pc';
  entityType2: 'npc' | 'pc';
  type: RelationshipType;
  description?: string;
  attitudeScore: number;
  customType?: string;
}

export type RelationshipType =
  | 'acquaintance' | 'ally' | 'child' | 'contact'
  | 'employee' | 'employer' | 'enemy' | 'family'
  | 'follower' | 'friend' | 'informant' | 'leader'
  | 'lover' | 'member' | 'mentor' | 'parent'
  | 'patron' | 'protege' | 'rival' | 'sibling'
  | 'spouse' | 'stranger' | 'student' | 'vassal'
  | 'custom' | 'neutral';

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


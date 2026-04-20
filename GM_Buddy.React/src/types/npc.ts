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
  campaignId?: number;
}

export type RelationshipType =
  | 'acquaintance'
  | 'ally'
  | 'contact/informant'
  | 'employer'
  | 'enemy'
  | 'family'
  | 'lover'
  | 'mentor'
  | 'patron'
  | 'rival'
  | 'stranger'
  | 'vassal/follower'
  | 'neutral';

export const CUSTOM_RELATIONSHIP_SENTINEL = "__custom__";
export const DEFAULT_CUSTOM_RELATIONSHIP_TYPE: RelationshipType = 'stranger';

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
  isLoggingIn: boolean;
}

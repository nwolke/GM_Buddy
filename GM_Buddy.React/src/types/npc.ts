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
  disposition?: number | null; // -5 (Hostile) to +5 (Devoted)
  campaignId?: number;
}

// Disposition scale helpers for the frontend
export const DISPOSITION_MIN = -5;
export const DISPOSITION_MAX = 5;

export function getDispositionLabel(disposition: number | null | undefined): string {
  if (disposition === null || disposition === undefined) return 'Unset';
  if (disposition === -5) return 'Hostile';
  if (disposition <= -3) return 'Antagonistic';
  if (disposition <= -1) return 'Unfriendly';
  if (disposition === 0) return 'Neutral';
  if (disposition <= 2) return 'Friendly';
  if (disposition <= 4) return 'Loyal';
  if (disposition === 5) return 'Devoted';
  return 'Unknown';
}

export function getDispositionColor(disposition: number | null | undefined): string {
  if (disposition === null || disposition === undefined) return 'text-gray-400';
  if (disposition <= -3) return 'text-red-400';
  if (disposition < 0) return 'text-orange-400';
  if (disposition === 0) return 'text-gray-400';
  if (disposition <= 2) return 'text-green-400';
  return 'text-emerald-400';
}

export function getDispositionBgColor(disposition: number | null | undefined): string {
  if (disposition === null || disposition === undefined) return 'bg-gray-500/20 border-gray-500/30';
  if (disposition <= -3) return 'bg-red-500/20 border-red-500/30';
  if (disposition < 0) return 'bg-orange-500/20 border-orange-500/30';
  if (disposition === 0) return 'bg-gray-500/20 border-gray-500/30';
  if (disposition <= 2) return 'bg-green-500/20 border-green-500/30';
  return 'bg-emerald-500/20 border-emerald-500/30';
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


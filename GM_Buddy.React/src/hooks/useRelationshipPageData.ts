import { useState, useCallback } from 'react';
import { NPC, Relationship } from '@/types/npc';
import { PC } from '@/types/pc';
import { useNPCData } from './useNPCData';
import { usePCData } from './usePCData';

export type EntityType = 'npc' | 'pc';

export interface EntityItem {
  id: number;
  name: string;
  entityType: EntityType;
  // NPC-specific
  race?: string;
  class?: string;
  description?: string;
  faction?: string;
  notes?: string;
  campaignId?: number;
  // PC-specific
  // description also applies
}

export interface UseRelationshipPageDataReturn {
  npcs: NPC[];
  pcs: PC[];
  entities: EntityItem[];
  relationships: Relationship[];
  loading: boolean;
  error: string | null;
  selectedCampaignId: number | undefined;
  setSelectedCampaignId: (id: number | undefined) => void;
  refresh: () => Promise<void>;
  addRelationship: (relationship: Omit<Relationship, 'id'>) => Promise<void>;
  deleteRelationship: (id: number) => void;
}

export function useRelationshipPageData(): UseRelationshipPageDataReturn {
  const [selectedCampaignId, setSelectedCampaignId] = useState<number | undefined>(undefined);

  const {
    npcs,
    relationships,
    loading: npcLoading,
    error: npcError,
    refreshNpcs,
    addRelationship: addRelationshipFromNpcHook,
    deleteRelationship,
  } = useNPCData(selectedCampaignId);

  const {
    pcs,
    loading: pcLoading,
    error: pcError,
    refreshPcs,
  } = usePCData(selectedCampaignId);

  const loading = npcLoading || pcLoading;
  const error = npcError ?? pcError;

  const entities: EntityItem[] = [
    ...npcs.map((npc): EntityItem => ({
      id: npc.id,
      name: npc.name,
      entityType: 'npc',
      race: npc.race,
      class: npc.class,
      description: npc.description,
      faction: npc.faction,
      notes: npc.notes,
      campaignId: npc.campaignId,
    })),
    ...pcs.map((pc): EntityItem => ({
      id: pc.id,
      name: pc.name,
      entityType: 'pc',
      description: pc.description,
    })),
  ];

  const refresh = useCallback(async () => {
    await Promise.all([refreshNpcs(), refreshPcs()]);
  }, [refreshNpcs, refreshPcs]);

  const addRelationship = useCallback(async (relationship: Omit<Relationship, 'id'>) => {
    await addRelationshipFromNpcHook(relationship);
  }, [addRelationshipFromNpcHook]);

  return {
    npcs,
    pcs,
    entities,
    relationships,
    loading,
    error,
    selectedCampaignId,
    setSelectedCampaignId,
    refresh,
    addRelationship,
    deleteRelationship,
  };
}

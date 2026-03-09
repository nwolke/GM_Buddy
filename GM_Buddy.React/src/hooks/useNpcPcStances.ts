import { useState, useEffect, useCallback, useMemo } from 'react';
import { relationshipApi, ApiPcStance } from '@/services/api';
import { PC } from '@/types/pc';

export interface PcStance {
  entityRelationshipId: number | null; // null if no stance exists yet
  pcId: number;
  pcName: string;
  relationshipType: string | null;
  relationshipTypeId: number | null;
  disposition: number | null;
  description: string | null;
}

interface UseNpcPcStancesReturn {
  stances: PcStance[];
  loading: boolean;
  error: string | null;
  refreshStances: () => Promise<void>;
}

export function useNpcPcStances(
  npcId: number | null,
  campaignId: number | undefined,
  campaignPcs: PC[]
): UseNpcPcStancesReturn {
  const [apiStances, setApiStances] = useState<ApiPcStance[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const loadStances = useCallback(async () => {
    if (!npcId) {
      setApiStances([]);
      return;
    }

    setLoading(true);
    setError(null);

    try {
      const data = await relationshipApi.getPcStancesForNpc(npcId, campaignId);
      setApiStances(data);
    } catch (err) {
      console.error('[useNpcPcStances] Failed to load stances:', err);
      setError('Failed to load PC stances.');
    } finally {
      setLoading(false);
    }
  }, [npcId, campaignId]);

  useEffect(() => {
    loadStances();
  }, [loadStances]);

  // Merge: all campaign PCs with existing stances
  const stances: PcStance[] = useMemo(() => {
    return campaignPcs.map(pc => {
      const existing = apiStances.find(s => s.pc_id === pc.id);
      if (existing) {
        return {
          entityRelationshipId: existing.entity_relationship_id,
          pcId: existing.pc_id,
          pcName: existing.pc_name,
          relationshipType: existing.relationship_type,
          relationshipTypeId: existing.relationship_type_id,
          disposition: existing.disposition,
          description: existing.description,
        };
      }
      return {
        entityRelationshipId: null,
        pcId: pc.id,
        pcName: pc.name,
        relationshipType: null,
        relationshipTypeId: null,
        disposition: null,
        description: null,
      };
    });
  }, [campaignPcs, apiStances]);

  return {
    stances,
    loading,
    error,
    refreshStances: loadStances,
  };
}

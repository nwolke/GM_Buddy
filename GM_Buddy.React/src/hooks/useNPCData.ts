import { useState, useEffect, useCallback } from 'react';
import { NPC, Relationship } from '@/types/npc';
import { npcApi, CreateNpcRequest, relationshipApi, transformApiRelationshipToRelationship, getRelationshipTypeId } from '@/services/api';
import { useAuth } from '@/contexts/AuthContext';

interface UseNPCDataReturn {
  npcs: NPC[];
  relationships: Relationship[];
  loading: boolean;
  error: string | null;
  refreshNpcs: () => Promise<void>;
  saveNPC: (npcData: Omit<NPC, 'id'> | NPC) => Promise<void>;
  deleteNPC: (id: number) => Promise<void>;
  addRelationship: (relationship: Omit<Relationship, 'id'>) => void;
  deleteRelationship: (id: number) => void;
}

export function useNPCData(selectedCampaignId?: number): UseNPCDataReturn {
const { isAuthenticated } = useAuth();
const [npcs, setNPCs] = useState<NPC[]>([]);
const [relationships, setRelationships] = useState<Relationship[]>([]);
const [loading, setLoading] = useState(true);
const [error, setError] = useState<string | null>(null);

// Log on mount to verify code is running
useEffect(() => {
  console.log('[useNPCData] Hook initialized - v2');
}, []);

  // Load NPCs from API
  const loadNpcs = useCallback(async () => {
    setLoading(true);
    setError(null);

    console.log(`[useNPCData] Loading NPCs for authenticated user, isAuthenticated: ${isAuthenticated}, campaignId: ${selectedCampaignId}`);

    // Calculate storage key once for both success and error paths
    const storageKey = selectedCampaignId !== undefined && selectedCampaignId !== null
      ? `ttrpg-npcs-campaign-${selectedCampaignId}`
      : 'ttrpg-npcs';

    try {
      // Only pass filter if we have a valid campaign ID
      const filters = selectedCampaignId !== undefined && selectedCampaignId !== null
        ? { campaignId: selectedCampaignId }
        : undefined;
      
      console.log(`[useNPCData] Calling npcApi.getNpcs() with filters:`, filters);
      const apiNpcs = await npcApi.getNpcs(filters);
      console.log(`[useNPCData] API returned ${apiNpcs.length} NPCs:`, apiNpcs);
      
    setNPCs(apiNpcs);

    // Load relationship types first (this populates the type map)
    console.log('[useNPCData] Loading relationship types...');
    await relationshipApi.getRelationshipTypes();

    // Load relationships from the backend
    console.log('[useNPCData] Loading relationships from backend...');
    const apiRelationships = await relationshipApi.getAccountRelationships();
    console.log(`[useNPCData] API returned ${apiRelationships.length} relationships:`, apiRelationships);
    
    // Transform backend relationships to frontend format
    const transformedRelationships = apiRelationships.map(transformApiRelationshipToRelationship) as Relationship[];
    
    // When filtering by campaign, only show relationships where both NPCs are in the loaded set
    // This prevents showing relationships to NPCs outside the selected campaign
    let filteredRelationships = transformedRelationships;
    if (selectedCampaignId !== undefined && selectedCampaignId !== null) {
      const npcIds = new Set(apiNpcs.map(npc => npc.id));
      filteredRelationships = transformedRelationships.filter(
        rel => npcIds.has(rel.npcId1) && npcIds.has(rel.npcId2)
      );
      console.log(`[useNPCData] Filtered relationships to match campaign NPCs: ${filteredRelationships.length} of ${transformedRelationships.length}`);
    }
    
    setRelationships(filteredRelationships);
    console.log('[useNPCData] Transformed relationships:', filteredRelationships);

      // Only save to localStorage if we have data (prevent overwriting cache during loading)
      if (apiNpcs.length > 0) {
        localStorage.setItem(storageKey, JSON.stringify(apiNpcs));
      }
      
      if (filteredRelationships.length > 0 || transformedRelationships.length > 0) {
        localStorage.setItem('ttrpg-relationships', JSON.stringify(transformedRelationships));
      }
    } catch (err: unknown) {
      console.error('[useNPCData] Failed to load NPCs:', err);
      setError('Using cached data. Changes may not be saved.');
      
      // Fallback to localStorage with campaign-specific key
      console.log('[useNPCData] Falling back to localStorage...');
      
      const storedNPCs = localStorage.getItem(storageKey);
      const localNpcs = storedNPCs ? JSON.parse(storedNPCs) : [];
      
      if (localNpcs.length > 0) {
        console.log(`[useNPCData] Loaded ${localNpcs.length} NPCs from localStorage with key: ${storageKey}`);
        setNPCs(localNpcs);
      }
      
      const storedRelationships = localStorage.getItem('ttrpg-relationships');
      if (storedRelationships) {
        let localRelationships = JSON.parse(storedRelationships);
        
        // When filtering by campaign, only show relationships where both NPCs are in the loaded set
        if (selectedCampaignId !== undefined && selectedCampaignId !== null && localNpcs.length > 0) {
          const npcIds = new Set(localNpcs.map((npc: NPC) => npc.id));
          localRelationships = localRelationships.filter(
            (rel: Relationship) => npcIds.has(rel.npcId1) && npcIds.has(rel.npcId2)
          );
          console.log(`[useNPCData] Filtered localStorage relationships to match campaign NPCs`);
        }
        
        setRelationships(localRelationships);
      }
    } finally {
      setLoading(false);
    }
  }, [isAuthenticated, selectedCampaignId]);

  useEffect(() => {
    // Only load NPCs if user is authenticated
    if (isAuthenticated) {
      loadNpcs();
    } else {
      // Clear NPCs when not authenticated
      console.log('[useNPCData] User not authenticated, clearing NPCs');
      setNPCs([]);
      setLoading(false);
    }
  }, [loadNpcs, isAuthenticated]);

  const refreshNpcs = useCallback(async () => {
    await loadNpcs();
  }, [loadNpcs]);

  const saveNPC = useCallback(async (npcData: Omit<NPC, 'id'> | NPC) => {
    try {
      if (!npcData.campaignId) {
        setError('Campaign is required to save an NPC.');
        return;
      }

      const request: CreateNpcRequest = {
        name: npcData.name,
        description: npcData.description,
        campaignId: npcData.campaignId,
        race: npcData.race,
        class: npcData.class,
        faction: npcData.faction,
        notes: npcData.notes,
      };

      if ('id' in npcData && npcData.id) {
        // Check if campaign changed - if so, delete relationships first
        const existingNpc = npcs.find(n => n.id === npcData.id);
        if (existingNpc && existingNpc.campaignId !== npcData.campaignId) {
          console.log('[useNPCData] Campaign changed, deleting relationships for NPC:', npcData.id);
          
          // Delete all relationships for this NPC
          const npcRelationships = relationships.filter(
            rel => rel.npcId1 === npcData.id || rel.npcId2 === npcData.id
          );
          
          for (const rel of npcRelationships) {
            try {
              await relationshipApi.deleteRelationship(rel.id);
              console.log('[useNPCData] Deleted relationship:', rel.id);
            } catch (err) {
              console.error('[useNPCData] Failed to delete relationship:', rel.id, err);
            }
          }
        }
        
        // Update existing NPC
        await npcApi.updateNpc(npcData.id, request);
        console.log('Updated NPC:', npcData.id);
      } else {
        // Create new NPC
        console.log('Creating new NPC for authenticated user');
        const createdNpc = await npcApi.createNpc(request);
        console.log('Created NPC:', createdNpc);
      }
      
      // Refresh NPCs from server to respect current campaign filter
      // This ensures if an NPC was moved to a different campaign, it disappears from the current view
      await loadNpcs();
    } catch (err) {
      console.error('Failed to save NPC:', err);
      // Toast is shown automatically by the axios interceptor
    }
  }, [loadNpcs, npcs, relationships]);

  const deleteNPC = useCallback(async (id: number) => {
    try {
      await npcApi.deleteNpc(id);
      console.log('Deleted NPC:', id);
    } catch (err) {
      console.error('Failed to delete NPC from server:', err);
      // Continue with local deletion even if server fails
    }
    
    // Remove from local state
    setNPCs(prev => prev.filter(npc => npc.id !== id));
    // Remove all relationships involving this NPC
    setRelationships(prev => prev.filter(
      rel => rel.npcId1 !== id && rel.npcId2 !== id
    ));
  }, []);

  const addRelationship = useCallback(async (relationshipData: Omit<Relationship, 'id'>) => {
    try {
      // Create relationship on backend
      const relationshipTypeId = getRelationshipTypeId(relationshipData.type);
      const apiRelationship = {
        source_entity_type: 'npc',
        source_entity_id: relationshipData.npcId1,
        target_entity_type: 'npc',
        target_entity_id: relationshipData.npcId2,
        relationship_type_id: relationshipTypeId,
        description: relationshipData.description,
      };

      console.log('[useNPCData] Creating relationship:', apiRelationship);
      const newRelationshipId = await relationshipApi.createRelationship(apiRelationship);
      console.log('[useNPCData] Created relationship with ID:', newRelationshipId);

      // Add to local state with the backend ID
      const newRelationship: Relationship = {
        ...relationshipData,
        id: newRelationshipId,
      };
      setRelationships(prev => [...prev, newRelationship]);
    } catch (err) {
      console.error('[useNPCData] Failed to create relationship:', err);
      // Toast is shown automatically by the axios interceptor
      throw err;
    }
  }, []);

  const deleteRelationship = useCallback(async (id: number) => {
    try {
      await relationshipApi.deleteRelationship(id);
      console.log('[useNPCData] Deleted relationship:', id);
    } catch (err) {
      console.error('[useNPCData] Failed to delete relationship from server:', err);
      // Continue with local deletion even if server fails
    }
    
    // Remove from local state
    setRelationships(prev => prev.filter(rel => rel.id !== id));
  }, []);

  return {
    npcs,
    relationships,
    loading,
    error,
    refreshNpcs,
    saveNPC,
    deleteNPC,
    addRelationship,
    deleteRelationship,
  };
}

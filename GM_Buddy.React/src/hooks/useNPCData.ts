import { useState, useEffect, useCallback } from 'react';
import { NPC, Relationship } from '@/types/npc';
import { npcApi, CreateNpcRequest } from '@/services/api';
import { useAuth } from '@/contexts/AuthContext';

interface UseNPCDataReturn {
  npcs: NPC[];
  relationships: Relationship[];
  loading: boolean;
  error: string | null;
  refreshNpcs: () => Promise<void>;
  saveNPC: (npcData: Omit<NPC, 'id'> | NPC) => Promise<void>;
  deleteNPC: (id: string) => Promise<void>;
  addRelationship: (relationship: Omit<Relationship, 'id'>) => void;
  deleteRelationship: (id: string) => void;
}

// Default account ID for demo purposes - matches the seeded 'gm_admin' account (id=1)
const DEFAULT_ACCOUNT_ID = 1;

export function useNPCData(): UseNPCDataReturn {
const { user, isAuthenticated } = useAuth();
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

  const accountId = user?.accountId ?? DEFAULT_ACCOUNT_ID;
  console.log(`[useNPCData] Loading NPCs for accountId: ${accountId}, isAuthenticated: ${isAuthenticated}, user:`, user);

  try {
    console.log(`[useNPCData] Calling npcApi.getNpcsByAccount(${accountId})...`);
    const apiNpcs = await npcApi.getNpcsByAccount(accountId);
    console.log(`[useNPCData] API returned ${apiNpcs.length} NPCs:`, apiNpcs);
      
    setNPCs(apiNpcs);

      // Load relationships from localStorage
      const storedRelationships = localStorage.getItem('ttrpg-relationships');
      if (storedRelationships) {
        setRelationships(JSON.parse(storedRelationships));
      }
    } catch (err: unknown) {
      const errorMessage = err instanceof Error ? err.message : 'Unknown error';
      console.error('[useNPCData] Failed to load NPCs:', errorMessage, err);
      setError(`Failed to load NPCs. Using local storage as fallback.`);
      
      // Fallback to localStorage
      console.log('[useNPCData] Falling back to localStorage...');
      const storedNPCs = localStorage.getItem('ttrpg-npcs');
      if (storedNPCs) {
        const localNpcs = JSON.parse(storedNPCs);
        console.log(`[useNPCData] Loaded ${localNpcs.length} NPCs from localStorage`);
        setNPCs(localNpcs);
      }
    } finally {
      setLoading(false);
    }
  }, [isAuthenticated, user?.accountId]);

  useEffect(() => {
    loadNpcs();
  }, [loadNpcs]);

  // Save NPCs to localStorage whenever they change
  useEffect(() => {
    localStorage.setItem('ttrpg-npcs', JSON.stringify(npcs));
  }, [npcs]);

  // Save relationships to localStorage whenever they change
  useEffect(() => {
    localStorage.setItem('ttrpg-relationships', JSON.stringify(relationships));
  }, [relationships]);

  const refreshNpcs = useCallback(async () => {
    await loadNpcs();
  }, [loadNpcs]);

  const getAccountId = useCallback(() => {
    return user?.accountId ?? DEFAULT_ACCOUNT_ID;
  }, [user?.accountId]);

  const saveNPC = useCallback(async (npcData: Omit<NPC, 'id'> | NPC) => {
    const accountId = getAccountId();
    
    try {
      const request: CreateNpcRequest = {
        name: npcData.name,
        description: npcData.description,
        race: npcData.race,
        class: npcData.class,
        faction: npcData.faction,
        notes: npcData.notes,
      };

      if ('id' in npcData && npcData.id) {
        // Update existing NPC
        const npcId = parseInt(npcData.id);
        if (!isNaN(npcId)) {
          await npcApi.updateNpc(npcId, accountId, request);
          console.log('Updated NPC:', npcId);
        }
        // Update local state
        setNPCs(prev => prev.map(npc => npc.id === npcData.id ? npcData : npc));
      } else {
        // Create new NPC
        console.log('Creating new NPC for account:', accountId);
        const createdNpc = await npcApi.createNpc(accountId, request);
        console.log('Created NPC:', createdNpc);
        setNPCs(prev => [...prev, createdNpc]);
      }
    } catch (err) {
      console.error('Failed to save NPC:', err);
      setError('Failed to save NPC to server.');
      
      // Fallback to local storage for new NPCs
      if (!('id' in npcData)) {
        const newNPC: NPC = {
          ...npcData,
          id: crypto.randomUUID(),
          accountId: accountId,
        };
        setNPCs(prev => [...prev, newNPC]);
      }
    }
  }, [getAccountId]);

  const deleteNPC = useCallback(async (id: string) => {
    try {
      const npcId = parseInt(id);
      if (!isNaN(npcId)) {
        await npcApi.deleteNpc(npcId);
        console.log('Deleted NPC:', npcId);
      }
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

  const addRelationship = useCallback((relationshipData: Omit<Relationship, 'id'>) => {
    const newRelationship: Relationship = {
      ...relationshipData,
      id: crypto.randomUUID(),
    };
    setRelationships(prev => [...prev, newRelationship]);
  }, []);

  const deleteRelationship = useCallback((id: string) => {
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

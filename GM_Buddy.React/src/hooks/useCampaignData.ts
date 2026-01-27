import { useState, useEffect, useCallback } from 'react';
import { Campaign } from '@/types/campaign';
import { campaignApi, CreateCampaignRequest } from '@/services/api';
import { useAuth } from '@/contexts/AuthContext';

interface UseCampaignDataReturn {
  campaigns: Campaign[];
  loading: boolean;
  error: string | null;
  refreshCampaigns: () => Promise<void>;
  saveCampaign: (campaignData: Omit<Campaign, 'id'> | Campaign) => Promise<void>;
  deleteCampaign: (id: string) => Promise<void>;
}

export function useCampaignData(): UseCampaignDataReturn {
  const { isAuthenticated } = useAuth();
  const [campaigns, setCampaigns] = useState<Campaign[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    console.log('[useCampaignData] Hook initialized');
  }, []);

  const loadCampaigns = useCallback(async () => {
    setLoading(true);
    setError(null);

    console.log(`[useCampaignData] Loading campaigns, isAuthenticated: ${isAuthenticated}`);

    try {
      console.log(`[useCampaignData] Calling campaignApi.getCampaignsByAccount()...`);
      const apiCampaigns = await campaignApi.getCampaignsByAccount();
      console.log(`[useCampaignData] API returned ${apiCampaigns.length} campaigns:`, apiCampaigns);
      
      setCampaigns(apiCampaigns);
    } catch (err: unknown) {
      const errorMessage = err instanceof Error ? err.message : 'Unknown error';
      console.error('[useCampaignData] Failed to load campaigns:', errorMessage, err);
      setError(`Failed to load campaigns. Using local storage as fallback.`);
      
      // Fallback to localStorage
      console.log('[useCampaignData] Falling back to localStorage...');
      const storedCampaigns = localStorage.getItem('ttrpg-campaigns');
      if (storedCampaigns) {
        try {
          const localCampaigns = JSON.parse(storedCampaigns);
          if (Array.isArray(localCampaigns)) {
            console.log(
              `[useCampaignData] Loaded ${localCampaigns.length} campaigns from localStorage`
            );
            setCampaigns(localCampaigns);
          } else {
            console.warn('[useCampaignData] localStorage data is not an array, clearing');
            localStorage.removeItem('ttrpg-campaigns');
          }
        } catch (parseError) {
          console.error(
            '[useCampaignData] Failed to parse campaigns from localStorage, clearing corrupted data:',
            parseError
          );
          localStorage.removeItem('ttrpg-campaigns');
        }
      }
    } finally {
      setLoading(false);
    }
  }, [isAuthenticated]);

  useEffect(() => {
    if (isAuthenticated) {
      loadCampaigns();
    } else {
      console.log('[useCampaignData] User not authenticated, clearing campaigns');
      setCampaigns([]);
      setLoading(false);
    }
  }, [loadCampaigns, isAuthenticated]);

  useEffect(() => {
    localStorage.setItem('ttrpg-campaigns', JSON.stringify(campaigns));
  }, [campaigns]);

  const saveCampaign = async (campaignData: Omit<Campaign, 'id'> | Campaign) => {
    try {
      if ('id' in campaignData && campaignData.id) {
        // Update existing campaign
        console.log('[useCampaignData] Updating campaign:', campaignData.id);
        const updateRequest: CreateCampaignRequest = {
          name: campaignData.name,
          description: campaignData.description,
          game_system_id: campaignData.gameSystemId,
        };
        await campaignApi.updateCampaign(parseInt(campaignData.id), updateRequest);
        
        setCampaigns(prev => 
          prev.map(c => c.id === campaignData.id ? { ...c, ...campaignData } : c)
        );
      } else {
        // Create new campaign
        console.log('[useCampaignData] Creating new campaign:', campaignData);
        const createRequest: CreateCampaignRequest = {
          name: campaignData.name,
          description: campaignData.description,
          game_system_id: campaignData.gameSystemId,
        };
        const newCampaign = await campaignApi.createCampaign(createRequest);
        setCampaigns(prev => [...prev, newCampaign]);
      }
      
      await loadCampaigns();
    } catch (err) {
      console.error('[useCampaignData] Failed to save campaign:', err);
      throw err;
    }
  };

  const deleteCampaign = async (id: string) => {
    try {
      console.log('[useCampaignData] Deleting campaign:', id);
      await campaignApi.deleteCampaign(parseInt(id));
      setCampaigns(prev => prev.filter(c => c.id !== id));
    } catch (err) {
      console.error('[useCampaignData] Failed to delete campaign:', err);
      throw err;
    }
  };

  return {
    campaigns,
    loading,
    error,
    refreshCampaigns: loadCampaigns,
    saveCampaign,
    deleteCampaign,
  };
}

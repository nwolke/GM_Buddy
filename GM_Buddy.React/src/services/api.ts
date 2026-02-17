import axios, { AxiosError, InternalAxiosRequestConfig } from 'axios';
import { toast } from 'sonner';
import { NPC } from '@/types/npc';
import { PC } from '@/types/pc';
import { Campaign } from '@/types/campaign';
import { getIdToken, refreshTokens, clearTokens } from './cognito';
import { extractApiError } from './apiError';

// Storage key for auth state - must match AuthContext
const AUTH_STORAGE_KEY = 'gm_buddy_auth';

/**
 * Helper function to perform full logout when token refresh fails
 * Clears both Cognito tokens and auth state to keep UI and storage consistent
 * 
 * Emits a custom 'auth-logout' event that AuthContext listens for to update
 * its internal state and trigger any necessary UI updates (e.g., redirect to login).
 */
function performFullLogout(): void {
  clearTokens();
  localStorage.removeItem(AUTH_STORAGE_KEY);
  
  // Emit a custom event to notify AuthContext of the logout
  // Using CustomEvent instead of StorageEvent for better test compatibility
  window.dispatchEvent(new CustomEvent('auth-logout', { 
    detail: { reason: 'token-refresh-failed' } 
  }));
}

// API base URL - use environment variable or fall back to relative path
const API_BASE_URL = import.meta.env.VITE_API_URL
    ? `${import.meta.env.VITE_API_URL}/api`  // Production: full URL to backend + /api prefix
    : '/api';                                 // Development: proxy via Vite to /api

const apiClient = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    'Content-Type': 'application/json',
  },
  timeout: 10000, // 10 second timeout
});

// Add request interceptor for authentication and debugging
apiClient.interceptors.request.use(
  async (config) => {
    // Skip adding token for retry requests (they already have the updated token)
    if (!(config as any)._retry) {
      // Add JWT token to requests if available
      const token = await getIdToken();
      if (token) {
        config.headers.Authorization = `Bearer ${token}`;
      }
    }
    
    console.log(`API Request: ${config.method?.toUpperCase()} ${config.baseURL}${config.url}`, config.params || '');
    return config;
  },
  (error) => {
    console.error('API Request Error:', error);
    return Promise.reject(error);
  }
);

// Add response interceptor for debugging and 401 handling
apiClient.interceptors.response.use(
  (response) => {
    console.log(`API Response: ${response.status}`, response.data);
    return response;
  },
  async (error: AxiosError) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean };
    
    // Handle 401 Unauthorized - token expired
    if (error.response?.status === 401 && originalRequest && !originalRequest._retry) {
      console.log('[API Interceptor] 401 Unauthorized - attempting token refresh...');
      
      // Mark this request as retried to prevent infinite loops
      originalRequest._retry = true;
      
      try {
        // Attempt to refresh the token
        const newTokens = await refreshTokens();
        
        if (newTokens) {
          // Update the authorization header with the new token
          originalRequest.headers.Authorization = `Bearer ${newTokens.idToken}`;
          
          // Retry the original request
          console.log('[API Interceptor] Token refreshed, retrying request...');
          return apiClient(originalRequest);
        } else {
          // Refresh failed - perform full logout to keep UI and storage consistent
          console.error('[API Interceptor] Token refresh failed, performing full logout');
          performFullLogout();
        }
      } catch (refreshError) {
        console.error('[API Interceptor] Error during token refresh:', refreshError);
        performFullLogout();
      }
    }
    
    const apiError = extractApiError(error);
    console.error('API Error:', apiError.statusCode, apiError.message);

    // Show toast for non-401 errors (401 is already handled by retry/logout above)
    if (error.response?.status !== 401) {
      toast.error(apiError.userMessage);
    }

    return Promise.reject(apiError);
  }
);

// NPC API Types (matching backend response - ASP.NET uses camelCase by default)
// The property names might vary based on JSON serialization settings
export interface ApiNpc {
  npc_Id?: number;      // From BaseNpc.Npc_Id
  Npc_Id?: number;      // Alternative casing
  account_Id?: number;  // From BaseNpc.Account_Id  
  Account_Id?: number;  // Alternative casing
  campaign_Id?: number; // From BaseNpc.Campaign_Id
  Campaign_Id?: number; // Alternative casing
  name?: string;        // From BaseNpc.Name
  Name?: string;        // Alternative casing
  description?: string; // From BaseNpc.Description
  Description?: string; // Alternative casing
  stats?: {             // Stats object from DndNpc (supporting legacy and current shapes)
    // Legacy fields (older API shape)
    race?: string;
    class?: string;
    faction?: string;
    notes?: string;
    // Current DnDStats fields (backend shape)
    lineage?: string;
    occupation?: string;
  };
  Stats?: {             // Alternative casing (supporting legacy and current shapes)
    // Legacy fields (older API shape)
    race?: string;
    class?: string;
    faction?: string;
    notes?: string;
    // Current DnDStats fields (backend shape)
    lineage?: string;
    occupation?: string;
  };
}

// Helper to normalize API response
const normalizeApiNpc = (apiNpc: ApiNpc): { 
  npcId?: number; 
  accountId?: number; 
  campaignId?: number;
  name: string; 
  description?: string; 
  race?: string;
  class?: string;
  faction?: string;
  notes?: string;
} => {
  const stats = apiNpc.stats ?? apiNpc.Stats;
  return {
    npcId: apiNpc.npc_Id ?? apiNpc.Npc_Id,
    accountId: apiNpc.account_Id ?? apiNpc.Account_Id,
    campaignId: apiNpc.campaign_Id ?? apiNpc.Campaign_Id,
    name: apiNpc.name ?? apiNpc.Name ?? '',
    description: apiNpc.description ?? apiNpc.Description,
    // Support both legacy shape (race/class) and current DnDStats shape (lineage/occupation)
    race: stats?.race ?? stats?.lineage,
    class: stats?.class ?? stats?.occupation,
    faction: stats?.faction,
    notes: stats?.notes,
  };
};

export interface ApiRelationshipType {
  relationship_type_id: number;
  type_name: string;
  description?: string;
}

export interface ApiEntityRelationship {
  entity_relationship_id?: number;
  relationship_id?: number; // Alternative casing
  source_entity_type: string;
  source_entity_id: number;
  target_entity_type: string;
  target_entity_id: number;
  relationship_type_id: number;
  description?: string;
}

// Transform API NPC to frontend NPC
const transformApiNpcToNpc = (apiNpc: ApiNpc): NPC => {
  const normalized = normalizeApiNpc(apiNpc);
  console.log('[transformApiNpcToNpc] Raw:', apiNpc, 'Normalized:', normalized);
  return {
    id: normalized.npcId || 0,
    name: normalized.name,
    race: normalized.race || 'Unknown',
    class: normalized.class || 'Adventurer',
    description: normalized.description || '',
    campaignId: normalized.campaignId,
    faction: normalized.faction,
    notes: normalized.notes,
  };
};

// Map relationship type ID to type name
// This will be populated when relationship types are loaded
const relationshipTypeMap = new Map<number, string>();
const relationshipTypeNameToIdMap = new Map<string, number>();

// Transform API EntityRelationship to frontend Relationship
const transformApiRelationshipToRelationship = (apiRel: ApiEntityRelationship): { 
  id: number;
  npcId1: number;
  npcId2: number;
  type: string;
  description?: string;
} => {
  const id = (apiRel.entity_relationship_id ?? apiRel.relationship_id) || 0;
  const typeName = relationshipTypeMap.get(apiRel.relationship_type_id) || 'neutral';
  
  return {
    id,
    npcId1: apiRel.source_entity_id,
    npcId2: apiRel.target_entity_id,
    type: typeName,
    description: apiRel.description,
  };
};

// Get relationship type ID by name
export const getRelationshipTypeId = (typeName: string): number => {
  return relationshipTypeNameToIdMap.get(typeName.toLowerCase()) || 7; // Default to 'neutral' if not found
};

// Create NPC request type
export interface CreateNpcRequest {
  name: string;
  description?: string;
  campaignId: number;
  race?: string;
  class?: string;
  faction?: string;
  notes?: string;
}

// NPC filter params
export interface NpcFilterParams {
  campaignId?: number;
}

// NPC API calls
export const npcApi = {
  // Get all NPCs for the authenticated user with optional filters
  async getNpcs(filters?: NpcFilterParams): Promise<NPC[]> {
    // Remove undefined/null values from params
    const cleanParams = filters && Object.fromEntries(
      Object.entries(filters).filter(([_, v]) => v !== undefined && v !== null)
    );
    
    const response = await apiClient.get<ApiNpc[]>('/Npcs', {
      params: cleanParams,
    });
    return response.data.map(transformApiNpcToNpc);
  },

  // Get single NPC by ID
  async getNpc(id: number): Promise<NPC> {
    const response = await apiClient.get<ApiNpc>(`/Npcs/${id}`);
    return transformApiNpcToNpc(response.data);
  },

  // Create a new NPC for the authenticated user
  async createNpc(npc: CreateNpcRequest): Promise<NPC> {
    const response = await apiClient.post<ApiNpc>('/Npcs', npc);
    return transformApiNpcToNpc(response.data);
  },

  // Update an existing NPC owned by the authenticated user
  async updateNpc(id: number, npc: CreateNpcRequest): Promise<void> {
    await apiClient.put(`/Npcs/${id}`, npc);
  },

  // Delete an NPC
  async deleteNpc(id: number): Promise<void> {
    await apiClient.delete(`/Npcs/${id}`);
  },
};

// Relationship API calls
export const relationshipApi = {
  // Get all relationship types and populate the type map
  async getRelationshipTypes(): Promise<ApiRelationshipType[]> {
    const response = await apiClient.get<ApiRelationshipType[]>('/Relationships/types');
    // Populate the type maps for transformations
    response.data.forEach(type => {
      if (type.type_name) {
        const typeName = type.type_name.toLowerCase();
        relationshipTypeMap.set(type.relationship_type_id, typeName);
        relationshipTypeNameToIdMap.set(typeName, type.relationship_type_id);
      } else {
        console.warn('[relationshipApi] Skipping relationship type with missing name:', type);
      }
    });
    return response.data;
  },

  // Get all relationships for the authenticated user's account
  async getAccountRelationships(): Promise<ApiEntityRelationship[]> {
    const response = await apiClient.get<ApiEntityRelationship[]>('/Relationships/account');
    return response.data;
  },

  // Get relationships for an NPC
  async getRelationshipsForNpc(npcId: number): Promise<ApiEntityRelationship[]> {
    const response = await apiClient.get<ApiEntityRelationship[]>(`/Relationships/entity/npc/${npcId}`);
    return response.data;
  },

  // Create a new relationship
  async createRelationship(relationship: ApiEntityRelationship): Promise<number> {
    const response = await apiClient.post<number>('/Relationships', relationship);
    return response.data;
  },

  // Delete a relationship
  async deleteRelationship(id: number): Promise<void> {
    await apiClient.delete(`/Relationships/${id}`);
  },
};

// Account API response type
interface AccountResponse {
  accountId: number;
  email: string;
  displayName?: string;
  subscriptionTier: string;
  createdAt: string;
}

// Account API calls
export const accountApi = {
  // Sync account (create if not exists)
  async syncAccount(email: string): Promise<AccountResponse> {
    const response = await apiClient.post<AccountResponse>('/Account/sync', { email });
    return response.data;
  },

  // Get account by cognitoSub
  async getAccount(): Promise<{ account_id: number } | null> {
    try {
      const response = await apiClient.get<AccountResponse>('/Account/me');
      // Map accountId to account_id for consistency with existing code
      return { account_id: response.data.accountId };
    } catch {
      return null;
    }
  },

  // Export all account data
  async exportData() {
    return await apiClient.get('/Account/export');
  },

  // Delete account and all data
  async deleteAccount(): Promise<void> {
    await apiClient.delete('/Account');
  },
};

// Campaign API response type (matches CampaignDTO)
export interface ApiCampaign {
  campaign_id: number;
  name: string;
  description?: string;
}

// Create Campaign request type
export interface CreateCampaignRequest {
  name: string;
  description?: string;
}

// Update Campaign request type
export interface UpdateCampaignRequest {
  campaign_id: number;
  name: string;
  description?: string;
}

// Transform API Campaign to frontend Campaign
const transformApiCampaignToCampaign = (apiCampaign: ApiCampaign): Campaign => {
  return {
    id: apiCampaign.campaign_id,
    name: apiCampaign.name,
    description: apiCampaign.description,
  };
};

// Campaign API calls
export const campaignApi = {
  // Get all campaigns for the authenticated user's account
  async getCampaignsByAccount(): Promise<Campaign[]> {
    const response = await apiClient.get<ApiCampaign[]>('/Campaigns/account');
    return response.data.map(transformApiCampaignToCampaign);
  },

  // Get single campaign by ID
  async getCampaign(id: number): Promise<Campaign> {
    const response = await apiClient.get<ApiCampaign>(`/Campaigns/${id}`);
    return transformApiCampaignToCampaign(response.data);
  },

  // Create a new campaign
  async createCampaign(campaign: CreateCampaignRequest): Promise<Campaign> {
    const response = await apiClient.post<number>('/Campaigns', campaign);
    // The backend returns the new campaign ID, fetch the full campaign
    return await campaignApi.getCampaign(response.data);
  },

  // Update an existing campaign
  async updateCampaign(id: number, campaign: UpdateCampaignRequest): Promise<void> {
    await apiClient.put(`/Campaigns/${id}`, campaign);
  },

  // Delete a campaign
  async deleteCampaign(id: number): Promise<void> {
    await apiClient.delete(`/Campaigns/${id}`);
  },
};

// PC API Types (matching backend PcDto - ASP.NET uses camelCase in JSON by default)
interface ApiPcDto {
  pcId: number;
  name: string;
  description?: string;
  createdAt: string;
  updatedAt: string;
}

// Transform API PC DTO to frontend PC
const transformApiPcToPc = (apiPc: ApiPcDto): PC => {
  return {
    id: apiPc.pcId,
    name: apiPc.name,
    description: apiPc.description || '',
  };
};

// Create PC request type
export interface CreatePcRequest {
  name: string;
  description?: string;
}

// Update PC request type
export interface UpdatePcRequest {
  name: string;
  description?: string;
}

// PC API calls
export const pcApi = {
  // Get all PCs for the authenticated user
  async getPcs(): Promise<PC[]> {
    const response = await apiClient.get<ApiPcDto[]>('/Pcs');
    return response.data.map(transformApiPcToPc);
  },

  // Get single PC by ID
  async getPc(id: number): Promise<PC> {
    const response = await apiClient.get<ApiPcDto>(`/Pcs/${id}`);
    return transformApiPcToPc(response.data);
  },

  // Create a new PC for the authenticated user
  async createPc(pc: CreatePcRequest): Promise<PC> {
    const response = await apiClient.post<ApiPcDto>('/Pcs', pc);
    return transformApiPcToPc(response.data);
  },

  // Update an existing PC owned by the authenticated user
  async updatePc(id: number, pc: UpdatePcRequest): Promise<void> {
    await apiClient.put(`/Pcs/${id}`, pc);
  },

  // Delete a PC
  async deletePc(id: number): Promise<void> {
    await apiClient.delete(`/Pcs/${id}`);
  },
};

// Export transformation functions for use in hooks
export { transformApiRelationshipToRelationship };

// Re-export Campaign type for convenience
export type { Campaign } from '@/types/campaign';

export default apiClient;

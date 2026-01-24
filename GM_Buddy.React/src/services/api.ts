import axios from 'axios';
import { NPC } from '@/types/npc';
import { getIdToken } from './cognito';

// API base URL - proxied through nginx in production, Vite in development
const API_BASE_URL = '/api';

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
    // Add JWT token to requests if available
    const token = await getIdToken();
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    
    console.log(`API Request: ${config.method?.toUpperCase()} ${config.baseURL}${config.url}`, config.params || '');
    return config;
  },
  (error) => {
    console.error('API Request Error:', error);
    return Promise.reject(error);
  }
);

// Add response interceptor for debugging
apiClient.interceptors.response.use(
  (response) => {
    console.log(`API Response: ${response.status}`, response.data);
    return response;
  },
  (error) => {
    console.error('API Error:', error.response?.status, error.response?.data || error.message);
    return Promise.reject(error);
  }
);

// NPC API Types (matching backend response - ASP.NET uses camelCase by default)
// The property names might vary based on JSON serialization settings
export interface ApiNpc {
  npc_Id?: number;      // From BaseNpc.Npc_Id
  Npc_Id?: number;      // Alternative casing
  account_Id?: number;  // From BaseNpc.Account_Id  
  Account_Id?: number;  // Alternative casing
  name?: string;        // From BaseNpc.Name
  Name?: string;        // Alternative casing
  description?: string; // From BaseNpc.Description
  Description?: string; // Alternative casing
  system?: string;      // From BaseNpc.System
  System?: string;      // Alternative casing
}

// Helper to normalize API response
const normalizeApiNpc = (apiNpc: ApiNpc): { npcId?: number; accountId?: number; name: string; description?: string; system?: string } => ({
  npcId: apiNpc.npc_Id ?? apiNpc.Npc_Id,
  accountId: apiNpc.account_Id ?? apiNpc.Account_Id,
  name: apiNpc.name ?? apiNpc.Name ?? '',
  description: apiNpc.description ?? apiNpc.Description,
  system: apiNpc.system ?? apiNpc.System,
});

export interface ApiRelationshipType {
  relationship_type_id: number;
  type_name: string;
  description?: string;
}

export interface ApiEntityRelationship {
  relationship_id?: number;
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
    id: normalized.npcId?.toString() || '',
    name: normalized.name,
    race: normalized.system || 'Unknown', // Using system as race for now
    class: 'Adventurer', // Default class
    description: normalized.description || '',
    faction: undefined,
    notes: undefined,
  };
};

// Create NPC request type
export interface CreateNpcRequest {
  name: string;
  description?: string;
  system?: string;
  race?: string;
  class?: string;
  faction?: string;
  notes?: string;
}

// NPC API calls
export const npcApi = {
  // Get all NPCs for the authenticated user
  async getNpcs(): Promise<NPC[]> {
    const response = await apiClient.get<ApiNpc[]>('/Npcs');
    return response.data.map(transformApiNpcToNpc);
  },

  // Get all NPCs for the authenticated user's account
  async getNpcsByAccount(): Promise<NPC[]> {
    const response = await apiClient.get<ApiNpc[]>('/Npcs/account');
    return response.data.map(transformApiNpcToNpc);
  },

  // Get single NPC by ID
  async getNpc(id: number): Promise<NPC> {
    const response = await apiClient.get<ApiNpc>(`/Npcs/${id}`);
    return transformApiNpcToNpc(response.data);
  },

  // Search NPCs (searches within authenticated user's NPCs)
  async searchNpcs(name?: string): Promise<NPC[]> {
    const response = await apiClient.get<ApiNpc[]>('/Npcs/search', {
      params: { name },
    });
    return response.data.map(transformApiNpcToNpc);
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
  // Get all relationship types
  async getRelationshipTypes(): Promise<ApiRelationshipType[]> {
    const response = await apiClient.get<ApiRelationshipType[]>('/Relationships/types');
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
};

export default apiClient;

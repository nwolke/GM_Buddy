import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { renderHook, waitFor } from '@testing-library/react'
import { useNPCData } from '@/hooks/useNPCData'
import { npcApi, relationshipApi } from '@/services/api'
import { mockNPCs, mockRelationships, mockRelationshipTypes } from '@/test/mockData'

// Mock the API
vi.mock('@/services/api', () => ({
  npcApi: {
    getNpcs: vi.fn(),
    createNpc: vi.fn(),
    updateNpc: vi.fn(),
    deleteNpc: vi.fn(),
  },
  relationshipApi: {
    getAccountRelationships: vi.fn(),
    getRelationshipTypes: vi.fn(),
    createRelationship: vi.fn(),
    deleteRelationship: vi.fn(),
  },
  transformApiRelationshipToRelationship: vi.fn((rel) => rel),
  getRelationshipTypeId: vi.fn((type) => {
    const typeMap = {
      ally: 1,
      enemy: 2,
      family: 3,
      rival: 4,
      mentor: 5,
      student: 6,
      neutral: 7,
    }
    return typeMap[type] || 1
  }),
}))

// Mock the auth context
vi.mock('@/contexts/AuthContext', () => ({
  useAuth: vi.fn(),
}))

describe('useNPCData', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    localStorage.clear()
    
    // Mock useAuth to return authenticated state
    const { useAuth } = require('@/contexts/AuthContext')
    useAuth.mockReturnValue({
      isAuthenticated: true,
      user: { id: 1, email: 'test@test.com' },
    })
    
    // Default API responses
    vi.mocked(relationshipApi.getRelationshipTypes).mockResolvedValue(mockRelationshipTypes)
  })
  
  afterEach(() => {
    vi.clearAllMocks()
  })

  it('should load NPCs and relationships when authenticated', async () => {
    vi.mocked(npcApi.getNpcs).mockResolvedValue(mockNPCs)
    vi.mocked(relationshipApi.getAccountRelationships).mockResolvedValue(mockRelationships)

    const { result } = renderHook(() => useNPCData())

    expect(result.current.loading).toBe(true)

    await waitFor(() => {
      expect(result.current.loading).toBe(false)
    })

    expect(result.current.npcs).toEqual(mockNPCs)
    expect(result.current.relationships).toEqual(mockRelationships)
    expect(result.current.error).toBeNull()
    expect(npcApi.getNpcs).toHaveBeenCalledWith(undefined)
  })

  it('should filter NPCs by campaign when campaignId is provided', async () => {
    const campaign1NPCs = mockNPCs.filter(npc => npc.campaignId === 1)
    vi.mocked(npcApi.getNpcs).mockResolvedValue(campaign1NPCs)
    vi.mocked(relationshipApi.getAccountRelationships).mockResolvedValue(mockRelationships)

    const { result } = renderHook(() => useNPCData(1))

    await waitFor(() => {
      expect(result.current.loading).toBe(false)
    })

    expect(npcApi.getNpcs).toHaveBeenCalledWith({ campaign_id: 1 })
    expect(result.current.npcs).toEqual(campaign1NPCs)
  })

  it('should filter relationships to only include NPCs in the campaign', async () => {
    const campaign1NPCs = mockNPCs.filter(npc => npc.campaignId === 1)
    vi.mocked(npcApi.getNpcs).mockResolvedValue(campaign1NPCs)
    vi.mocked(relationshipApi.getAccountRelationships).mockResolvedValue(mockRelationships)

    const { result } = renderHook(() => useNPCData(1))

    await waitFor(() => {
      expect(result.current.loading).toBe(false)
    })

    // Only relationship 1 should be included (NPC 1 and 2 are both in campaign 1)
    // Relationship 2 should be excluded (NPC 3 is in campaign 2)
    const expectedRelationships = mockRelationships.filter(rel => {
      const npcIds = new Set(campaign1NPCs.map(npc => npc.id))
      return npcIds.has(rel.npcId1) && npcIds.has(rel.npcId2)
    })
    expect(result.current.relationships).toEqual(expectedRelationships)
  })

  it('should not load NPCs when not authenticated', async () => {
    const { useAuth } = require('@/contexts/AuthContext')
    useAuth.mockReturnValue({
      isAuthenticated: false,
      user: null,
    })

    const { result } = renderHook(() => useNPCData())

    await waitFor(() => {
      expect(result.current.loading).toBe(false)
    })

    expect(result.current.npcs).toEqual([])
    expect(npcApi.getNpcs).not.toHaveBeenCalled()
  })

  it('should handle API errors and fallback to localStorage', async () => {
    const localStorageNPCs = [mockNPCs[0]]
    localStorage.setItem('ttrpg-npcs', JSON.stringify(localStorageNPCs))
    localStorage.setItem('ttrpg-relationships', JSON.stringify(mockRelationships))
    
    vi.mocked(npcApi.getNpcs).mockRejectedValue(new Error('Network error'))

    const { result } = renderHook(() => useNPCData())

    await waitFor(() => {
      expect(result.current.loading).toBe(false)
    })

    expect(result.current.npcs).toEqual(localStorageNPCs)
    expect(result.current.error).toContain('Failed to load NPCs')
  })

  it('should create a new NPC', async () => {
    vi.mocked(npcApi.getNpcs).mockResolvedValue(mockNPCs)
    vi.mocked(relationshipApi.getAccountRelationships).mockResolvedValue(mockRelationships)
    
    const newNPC = {
      id: 4,
      name: 'New NPC',
      race: 'Halfling',
      class: 'Rogue',
      description: 'A sneaky halfling',
      campaignId: 1,
      faction: 'Thieves Guild',
      notes: 'Expert lockpicker',
    }
    vi.mocked(npcApi.createNpc).mockResolvedValue(newNPC)

    const { result } = renderHook(() => useNPCData())

    await waitFor(() => {
      expect(result.current.loading).toBe(false)
    })

    const npcData = {
      name: 'New NPC',
      race: 'Halfling',
      class: 'Rogue',
      description: 'A sneaky halfling',
      campaignId: 1,
      faction: 'Thieves Guild',
      notes: 'Expert lockpicker',
    }

    await result.current.saveNPC(npcData)

    expect(npcApi.createNpc).toHaveBeenCalledWith(npcData)
  })

  it('should update an existing NPC', async () => {
    vi.mocked(npcApi.getNpcs).mockResolvedValue(mockNPCs)
    vi.mocked(relationshipApi.getAccountRelationships).mockResolvedValue(mockRelationships)
    vi.mocked(npcApi.updateNpc).mockResolvedValue(undefined)

    const { result } = renderHook(() => useNPCData())

    await waitFor(() => {
      expect(result.current.loading).toBe(false)
    })

    const updatedNPC = {
      ...mockNPCs[0],
      name: 'Updated NPC Name',
    }

    await result.current.saveNPC(updatedNPC)

    expect(npcApi.updateNpc).toHaveBeenCalledWith(updatedNPC.id, {
      name: updatedNPC.name,
      description: updatedNPC.description,
      campaignId: updatedNPC.campaignId,
      race: updatedNPC.race,
      class: updatedNPC.class,
      faction: updatedNPC.faction,
      notes: updatedNPC.notes,
    })
  })

  it('should delete an NPC', async () => {
    vi.mocked(npcApi.getNpcs).mockResolvedValue(mockNPCs)
    vi.mocked(relationshipApi.getAccountRelationships).mockResolvedValue(mockRelationships)
    vi.mocked(npcApi.deleteNpc).mockResolvedValue(undefined)

    const { result } = renderHook(() => useNPCData())

    await waitFor(() => {
      expect(result.current.loading).toBe(false)
    })

    const initialCount = result.current.npcs.length

    await result.current.deleteNPC(mockNPCs[0].id)

    expect(npcApi.deleteNpc).toHaveBeenCalledWith(mockNPCs[0].id)
    expect(result.current.npcs.length).toBe(initialCount - 1)
    expect(result.current.npcs.find(n => n.id === mockNPCs[0].id)).toBeUndefined()
  })

  it('should delete relationships when NPC is deleted', async () => {
    vi.mocked(npcApi.getNpcs).mockResolvedValue(mockNPCs)
    vi.mocked(relationshipApi.getAccountRelationships).mockResolvedValue(mockRelationships)
    vi.mocked(npcApi.deleteNpc).mockResolvedValue(undefined)

    const { result } = renderHook(() => useNPCData())

    await waitFor(() => {
      expect(result.current.loading).toBe(false)
    })

    const npcId = mockNPCs[0].id

    await result.current.deleteNPC(npcId)

    // Relationships involving the deleted NPC should be removed
    const remainingRelationships = result.current.relationships.filter(
      rel => rel.npcId1 === npcId || rel.npcId2 === npcId
    )
    expect(remainingRelationships.length).toBe(0)
  })

  it('should add a relationship', async () => {
    vi.mocked(npcApi.getNpcs).mockResolvedValue(mockNPCs)
    vi.mocked(relationshipApi.getAccountRelationships).mockResolvedValue(mockRelationships)
    vi.mocked(relationshipApi.createRelationship).mockResolvedValue(3)

    const { result } = renderHook(() => useNPCData())

    await waitFor(() => {
      expect(result.current.loading).toBe(false)
    })

    const newRelationship = {
      npcId1: mockNPCs[0].id,
      npcId2: mockNPCs[2].id,
      type: 'family' as const,
      description: 'Brothers',
    }

    await result.current.addRelationship(newRelationship)

    expect(relationshipApi.createRelationship).toHaveBeenCalledWith({
      source_entity_type: 'npc',
      source_entity_id: newRelationship.npcId1,
      target_entity_type: 'npc',
      target_entity_id: newRelationship.npcId2,
      relationship_type_id: 3, // family
      description: newRelationship.description,
    })
  })

  it('should delete a relationship', async () => {
    vi.mocked(npcApi.getNpcs).mockResolvedValue(mockNPCs)
    vi.mocked(relationshipApi.getAccountRelationships).mockResolvedValue(mockRelationships)
    vi.mocked(relationshipApi.deleteRelationship).mockResolvedValue(undefined)

    const { result } = renderHook(() => useNPCData())

    await waitFor(() => {
      expect(result.current.loading).toBe(false)
    })

    const initialCount = result.current.relationships.length
    const relationshipId = mockRelationships[0].id

    await result.current.deleteRelationship(relationshipId)

    expect(relationshipApi.deleteRelationship).toHaveBeenCalledWith(relationshipId)
    expect(result.current.relationships.length).toBe(initialCount - 1)
    expect(result.current.relationships.find(r => r.id === relationshipId)).toBeUndefined()
  })

  it('should set error when saving NPC without campaignId', async () => {
    vi.mocked(npcApi.getNpcs).mockResolvedValue(mockNPCs)
    vi.mocked(relationshipApi.getAccountRelationships).mockResolvedValue(mockRelationships)

    const { result } = renderHook(() => useNPCData())

    await waitFor(() => {
      expect(result.current.loading).toBe(false)
    })

    const npcWithoutCampaign = {
      name: 'Invalid NPC',
      race: 'Human',
      class: 'Fighter',
      description: 'No campaign',
      campaignId: undefined as any,
      faction: '',
      notes: '',
    }

    await result.current.saveNPC(npcWithoutCampaign)

    expect(result.current.error).toContain('Campaign is required')
    expect(npcApi.createNpc).not.toHaveBeenCalled()
  })
})

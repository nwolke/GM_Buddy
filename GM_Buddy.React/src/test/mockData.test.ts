import { describe, it, expect } from 'vitest'
import { mockCampaigns, mockNPCs, mockRelationships, mockRelationshipTypes } from '@/test/mockData'

describe('Mock Data', () => {
  describe('mockCampaigns', () => {
    it('should have valid campaign data', () => {
      expect(mockCampaigns).toHaveLength(2)
      expect(mockCampaigns[0]).toHaveProperty('id')
      expect(mockCampaigns[0]).toHaveProperty('name')
      expect(mockCampaigns[0]).toHaveProperty('gameSystemId')
    })

    it('should have unique IDs', () => {
      const ids = mockCampaigns.map(c => c.id)
      const uniqueIds = new Set(ids)
      expect(uniqueIds.size).toBe(ids.length)
    })
  })

  describe('mockNPCs', () => {
    it('should have valid NPC data', () => {
      expect(mockNPCs).toHaveLength(3)
      mockNPCs.forEach(npc => {
        expect(npc).toHaveProperty('id')
        expect(npc).toHaveProperty('name')
        expect(npc).toHaveProperty('race')
        expect(npc).toHaveProperty('class')
        expect(npc).toHaveProperty('description')
      })
    })

    it('should have NPCs in different campaigns', () => {
      const campaign1NPCs = mockNPCs.filter(npc => npc.campaignId === 1)
      const campaign2NPCs = mockNPCs.filter(npc => npc.campaignId === 2)
      
      expect(campaign1NPCs.length).toBeGreaterThan(0)
      expect(campaign2NPCs.length).toBeGreaterThan(0)
    })

    it('should have unique IDs', () => {
      const ids = mockNPCs.map(n => n.id)
      const uniqueIds = new Set(ids)
      expect(uniqueIds.size).toBe(ids.length)
    })
  })

  describe('mockRelationships', () => {
    it('should have valid relationship data', () => {
      expect(mockRelationships).toHaveLength(2)
      mockRelationships.forEach(rel => {
        expect(rel).toHaveProperty('id')
        expect(rel).toHaveProperty('npcId1')
        expect(rel).toHaveProperty('npcId2')
        expect(rel).toHaveProperty('type')
      })
    })

    it('should reference valid NPCs', () => {
      const npcIds = new Set(mockNPCs.map(n => n.id))
      
      mockRelationships.forEach(rel => {
        expect(npcIds.has(rel.npcId1)).toBe(true)
        expect(npcIds.has(rel.npcId2)).toBe(true)
      })
    })

    it('should have valid relationship types', () => {
      const validTypes = ['ally', 'enemy', 'family', 'rival', 'mentor', 'student', 'neutral']
      
      mockRelationships.forEach(rel => {
        expect(validTypes).toContain(rel.type)
      })
    })
  })

  describe('mockRelationshipTypes', () => {
    it('should have all relationship types', () => {
      expect(mockRelationshipTypes).toHaveLength(7)
      
      const expectedTypes = ['ally', 'enemy', 'family', 'rival', 'mentor', 'student', 'neutral']
      const actualTypes = mockRelationshipTypes.map(t => t.name)
      
      expectedTypes.forEach(type => {
        expect(actualTypes).toContain(type)
      })
    })

    it('should have unique IDs', () => {
      const ids = mockRelationshipTypes.map(t => t.id)
      const uniqueIds = new Set(ids)
      expect(uniqueIds.size).toBe(ids.length)
    })
  })
})

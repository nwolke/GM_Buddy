import { describe, it, expect } from 'vitest'
import type { Campaign } from '@/types/campaign'
import type { NPC, Relationship, RelationshipType } from '@/types/npc'

describe('Type Definitions', () => {
  describe('Campaign', () => {
    it('should allow valid campaign objects', () => {
      const campaign: Campaign = {
        id: 1,
        name: 'Test Campaign',
        description: 'A test campaign',
        accountId: 1,
      }

      expect(campaign.id).toBe(1)
      expect(campaign.name).toBe('Test Campaign')
    })

    it('should allow campaigns without optional fields', () => {
      const campaign: Campaign = {
        id: 1,
        name: 'Minimal Campaign',
      }

      expect(campaign.description).toBeUndefined()
    })
  })

  describe('NPC', () => {
    it('should allow valid NPC objects', () => {
      const npc: NPC = {
        id: 1,
        name: 'Test NPC',
        race: 'Human',
        class: 'Fighter',
        description: 'A brave warrior',
        campaignId: 1,
        faction: 'Good Guys',
        notes: 'Likes swords',
      }

      expect(npc.id).toBe(1)
      expect(npc.name).toBe('Test NPC')
      expect(npc.race).toBe('Human')
      expect(npc.class).toBe('Fighter')
    })

    it('should allow NPCs without optional fields', () => {
      const npc: NPC = {
        id: 1,
        name: 'Minimal NPC',
        race: 'Elf',
        class: 'Wizard',
        description: 'A wise mage',
      }

      expect(npc.campaignId).toBeUndefined()
      expect(npc.faction).toBeUndefined()
      expect(npc.notes).toBeUndefined()
    })
  })

  describe('Relationship', () => {
    it('should allow valid relationship objects', () => {
      const relationship: Relationship = {
        id: 1,
        npcId1: 1,
        npcId2: 2,
        type: 'ally',
        description: 'Close friends',
      }

      expect(relationship.id).toBe(1)
      expect(relationship.npcId1).toBe(1)
      expect(relationship.npcId2).toBe(2)
      expect(relationship.type).toBe('ally')
    })

    it('should allow relationships without description', () => {
      const relationship: Relationship = {
        id: 1,
        npcId1: 1,
        npcId2: 2,
        type: 'enemy',
      }

      expect(relationship.description).toBeUndefined()
    })

    it('should enforce valid relationship types', () => {
      const validTypes: RelationshipType[] = [
        'ally',
        'enemy',
        'family',
        'rival',
        'mentor',
        'student',
        'neutral',
      ]

      validTypes.forEach(type => {
        const relationship: Relationship = {
          id: 1,
          npcId1: 1,
          npcId2: 2,
          type: type,
        }
        expect(relationship.type).toBe(type)
      })
    })
  })
})

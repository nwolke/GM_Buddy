import { Campaign, NPC, Relationship } from '@/types'
import { PC } from '@/types/pc'

export const mockCampaigns: Campaign[] = [
  {
    id: 1,
    name: 'Test Campaign 1',
    description: 'A test campaign',
    accountId: 1,
  },
  {
    id: 2,
    name: 'Test Campaign 2',
    description: 'Another test campaign',
    accountId: 1,
  },
]

export const mockNPCs: NPC[] = [
  {
    id: 1,
    name: 'Test NPC 1',
    lineage: 'Human',
    class: 'Fighter',
    description: 'A brave warrior',
    campaignId: 1,
    faction: 'Good Guys',
    notes: 'Likes swords',
  },
  {
    id: 2,
    name: 'Test NPC 2',
    lineage: 'Elf',
    class: 'Wizard',
    description: 'A wise mage',
    campaignId: 1,
    faction: 'Magic Circle',
    notes: 'Studies arcane magic',
  },
  {
    id: 3,
    name: 'Test NPC 3',
    lineage: 'Dwarf',
    class: 'Cleric',
    description: 'A holy dwarf',
    campaignId: 2,
    faction: 'Temple',
    notes: 'Worships the mountain god',
  },
]

export const mockRelationships: Relationship[] = [
  {
    id: 1,
    npcId1: 1,
    npcId2: 2,
    entityType1: 'npc',
    entityType2: 'npc',
    type: 'ally',
    description: 'Close friends',
    attitudeScore: 3,
  },
  {
    id: 2,
    npcId1: 1,
    npcId2: 3,
    entityType1: 'npc',
    entityType2: 'npc',
    type: 'rival',
    description: 'Competing for glory',
    attitudeScore: -1,
  },
]

export const mockPCs: PC[] = [
  {
    id: 1,
    campaignId: 1,
    name: 'Thorin Ironforge',
    description: 'A stout dwarven cleric',
  },
  {
    id: 2,
    campaignId: 1,
    name: 'Lyra Shadowstep',
    description: 'A nimble half-elf rogue',
  },
  {
    id: 3,
    campaignId: 2,
    name: 'Silent Scout',
    description: undefined,
  },
]

export const mockRelationshipTypes = [
  { id: 1, name: 'ally', description: 'Allied characters' },
  { id: 2, name: 'enemy', description: 'Enemy characters' },
  { id: 3, name: 'family', description: 'Family members' },
  { id: 4, name: 'rival', description: 'Rival characters' },
  { id: 5, name: 'mentor', description: 'Mentor relationship' },
  { id: 6, name: 'stranger', description: 'Stranger relationship' },
  { id: 7, name: 'neutral', description: 'Neutral relationship' },
]

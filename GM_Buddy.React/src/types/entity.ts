export type EntityType = 'npc' | 'pc';

export interface EntityItem {
  id: number;
  name: string;
  entityType: EntityType;
  // NPC-specific
  lineage?: string;
  class?: string;
  description?: string;
  faction?: string;
  notes?: string;
  campaignId?: number;
  // PC-specific
  // description and campaignId also apply
}

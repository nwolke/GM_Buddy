import axios, { AxiosInstance } from 'axios';

/**
 * D&D 5e SRD API Client
 * 
 * Integration with https://www.dnd5eapi.co/
 * 
 * API Selection Rationale:
 * - dnd5eapi.co is the official hosted endpoint for the 5e-bits/5e-database project
 * - No authentication required
 * - Free, open-source, OGL-compliant (only contains SRD content)
 * - Stable and well-maintained community project
 * - No rate limiting for reasonable use
 * 
 * Legal Compliance:
 * - Uses only content from D&D 5E System Reference Document (SRD)
 * - Licensed under MIT (code) and OGL v1.0a (content)
 * - Safe for commercial use with proper attribution
 */

const DND_API_BASE_URL = 'https://www.dnd5eapi.co';

// Common types used across multiple resources
export interface APIReference {
  index: string;
  name: string;
  url: string;
}

export interface Choice {
  desc?: string;
  choose: number;
  type: string;
  from: {
    option_set_type?: string;
    options?: Array<{
      option_type: string;
      item?: APIReference;
      items?: APIReference[];
    }>;
  };
}

export interface DC {
  dc_type: APIReference;
  dc_value?: number;
  success_type: string;
}

export interface Damage {
  damage_type: APIReference;
  damage_dice?: string;
}

export interface DamageAtLevel {
  [level: string]: string;
}

// Spell types
export interface Spell {
  index: string;
  name: string;
  desc: string[];
  higher_level?: string[];
  range: string;
  components: string[];
  material?: string;
  ritual: boolean;
  duration: string;
  concentration: boolean;
  casting_time: string;
  level: number;
  attack_type?: string;
  damage?: {
    damage_type: APIReference;
    damage_at_slot_level?: DamageAtLevel;
    damage_at_character_level?: DamageAtLevel;
  };
  school: APIReference;
  classes: APIReference[];
  subclasses: APIReference[];
  url: string;
}

export interface SpellListResponse {
  count: number;
  results: APIReference[];
}

// Monster types
export interface MonsterAction {
  name: string;
  desc: string;
  attack_bonus?: number;
  dc?: DC;
  damage?: Damage[];
  multiattack_type?: string;
  actions?: {
    action_name: string;
    count: number | string;
    type: string;
  }[];
}

export interface MonsterSpeed {
  walk?: string;
  swim?: string;
  fly?: string;
  burrow?: string;
  climb?: string;
}

export interface MonsterProficiency {
  value: number;
  proficiency: APIReference;
}

export interface Monster {
  index: string;
  name: string;
  desc?: string;
  size: string;
  type: string;
  subtype?: string;
  alignment: string;
  armor_class: Array<{
    type: string;
    value: number;
    armor?: APIReference[];
  }>;
  hit_points: number;
  hit_dice: string;
  hit_points_roll: string;
  speed: MonsterSpeed;
  strength: number;
  dexterity: number;
  constitution: number;
  intelligence: number;
  wisdom: number;
  charisma: number;
  proficiencies: MonsterProficiency[];
  damage_vulnerabilities: string[];
  damage_resistances: string[];
  damage_immunities: string[];
  condition_immunities: APIReference[];
  senses: {
    [key: string]: string;
  };
  languages: string;
  challenge_rating: number;
  proficiency_bonus: number;
  xp: number;
  special_abilities?: MonsterAction[];
  actions?: MonsterAction[];
  legendary_actions?: MonsterAction[];
  image?: string;
  url: string;
}

export interface MonsterListResponse {
  count: number;
  results: APIReference[];
}

// Class types
export interface CharacterClass {
  index: string;
  name: string;
  hit_die: number;
  proficiency_choices: Choice[];
  proficiencies: APIReference[];
  saving_throws: APIReference[];
  starting_equipment: Array<{
    equipment: APIReference;
    quantity: number;
  }>;
  starting_equipment_options: Choice[];
  class_levels: string;
  multi_classing: {
    prerequisites?: Array<{
      ability_score: APIReference;
      minimum_score: number;
    }>;
    proficiencies?: APIReference[];
    proficiency_choices?: Choice[];
  };
  subclasses: APIReference[];
  spellcasting?: {
    level: number;
    spellcasting_ability: APIReference;
    info: Array<{
      name: string;
      desc: string[];
    }>;
  };
  spells?: string;
  url: string;
}

export interface ClassListResponse {
  count: number;
  results: APIReference[];
}

// Race types
export interface Race {
  index: string;
  name: string;
  speed: number;
  ability_bonuses: Array<{
    ability_score: APIReference;
    bonus: number;
  }>;
  alignment: string;
  age: string;
  size: string;
  size_description: string;
  starting_proficiencies: APIReference[];
  starting_proficiency_options?: Choice;
  languages: APIReference[];
  language_desc: string;
  traits: APIReference[];
  subraces: APIReference[];
  url: string;
}

export interface RaceListResponse {
  count: number;
  results: APIReference[];
}

// Equipment types
export interface Equipment {
  index: string;
  name: string;
  equipment_category: APIReference;
  weapon_category?: string;
  weapon_range?: string;
  category_range?: string;
  cost: {
    quantity: number;
    unit: string;
  };
  damage?: {
    damage_dice: string;
    damage_type: APIReference;
  };
  range?: {
    normal: number;
    long?: number;
  };
  weight?: number;
  properties?: APIReference[];
  armor_category?: string;
  armor_class?: {
    base: number;
    dex_bonus: boolean;
    max_bonus?: number;
  };
  str_minimum?: number;
  stealth_disadvantage?: boolean;
  desc?: string[];
  url: string;
}

export interface EquipmentListResponse {
  count: number;
  results: APIReference[];
}

// Condition types
export interface Condition {
  index: string;
  name: string;
  desc: string[];
  url: string;
}

export interface ConditionListResponse {
  count: number;
  results: APIReference[];
}

// Magic Item types
export interface MagicItem {
  index: string;
  name: string;
  equipment_category: APIReference;
  rarity: {
    name: string;
  };
  variants: APIReference[];
  variant: boolean;
  desc: string[];
  url: string;
}

export interface MagicItemListResponse {
  count: number;
  results: APIReference[];
}

/**
 * D&D 5e SRD API Client
 */
class Dnd5eApiClient {
  private client: AxiosInstance;

  constructor() {
    this.client = axios.create({
      baseURL: `${DND_API_BASE_URL}/api`,
      timeout: 10000,
      headers: {
        'Content-Type': 'application/json',
      },
    });

    // Request interceptor for logging
    this.client.interceptors.request.use(
      (config) => {
        console.log(`[D&D 5e API] Request: ${config.method?.toUpperCase()} ${config.baseURL}${config.url}`);
        return config;
      },
      (error) => {
        console.error('[D&D 5e API] Request Error:', error);
        return Promise.reject(error);
      }
    );

    // Response interceptor for logging
    this.client.interceptors.response.use(
      (response) => {
        console.log(`[D&D 5e API] Response: ${response.status}`, response.data);
        return response;
      },
      (error) => {
        console.error(
          '[D&D 5e API] Error:',
          error.response?.status,
          error.response?.data || error.message
        );
        return Promise.reject(error);
      }
    );
  }

  // Spells
  async getSpells(): Promise<SpellListResponse> {
    const response = await this.client.get<SpellListResponse>('/spells');
    return response.data;
  }

  async getSpell(index: string): Promise<Spell> {
    const response = await this.client.get<Spell>(`/spells/${index}`);
    return response.data;
  }

  async getSpellsByLevel(level: number): Promise<Spell[]> {
    // Note: The API supports filtering by level directly via query param
    const response = await this.client.get<SpellListResponse>('/spells', {
      params: { level }
    });
    // The API returns only references, so we still need to fetch details
    // For better performance, consider fetching details only when needed in your UI
    const spells = await Promise.all(
      response.data.results.map((ref) => this.getSpell(ref.index))
    );
    return spells;
  }

  // Monsters
  async getMonsters(): Promise<MonsterListResponse> {
    const response = await this.client.get<MonsterListResponse>('/monsters');
    return response.data;
  }

  async getMonster(index: string): Promise<Monster> {
    const response = await this.client.get<Monster>(`/monsters/${index}`);
    return response.data;
  }

  async getMonstersByChallengeRating(cr: number): Promise<Monster[]> {
    // Note: The API supports filtering by challenge_rating directly via query param
    const response = await this.client.get<MonsterListResponse>('/monsters', {
      params: { challenge_rating: cr }
    });
    // The API returns only references, so we still need to fetch details
    // For better performance, consider fetching details only when needed in your UI
    const monsters = await Promise.all(
      response.data.results.map((ref) => this.getMonster(ref.index))
    );
    return monsters;
  }

  // Classes
  async getClasses(): Promise<ClassListResponse> {
    const response = await this.client.get<ClassListResponse>('/classes');
    return response.data;
  }

  async getClass(index: string): Promise<CharacterClass> {
    const response = await this.client.get<CharacterClass>(`/classes/${index}`);
    return response.data;
  }

  // Races
  async getRaces(): Promise<RaceListResponse> {
    const response = await this.client.get<RaceListResponse>('/races');
    return response.data;
  }

  async getRace(index: string): Promise<Race> {
    const response = await this.client.get<Race>(`/races/${index}`);
    return response.data;
  }

  // Equipment
  async getEquipment(): Promise<EquipmentListResponse> {
    const response = await this.client.get<EquipmentListResponse>('/equipment');
    return response.data;
  }

  async getEquipmentItem(index: string): Promise<Equipment> {
    const response = await this.client.get<Equipment>(`/equipment/${index}`);
    return response.data;
  }

  // Conditions
  async getConditions(): Promise<ConditionListResponse> {
    const response = await this.client.get<ConditionListResponse>('/conditions');
    return response.data;
  }

  async getCondition(index: string): Promise<Condition> {
    const response = await this.client.get<Condition>(`/conditions/${index}`);
    return response.data;
  }

  // Magic Items
  async getMagicItems(): Promise<MagicItemListResponse> {
    const response = await this.client.get<MagicItemListResponse>('/magic-items');
    return response.data;
  }

  async getMagicItem(index: string): Promise<MagicItem> {
    const response = await this.client.get<MagicItem>(`/magic-items/${index}`);
    return response.data;
  }
}

// Export singleton instance
export const dnd5eApi = new Dnd5eApiClient();

// Named exports for convenience
export default dnd5eApi;

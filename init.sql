-- Create schema
CREATE SCHEMA IF NOT EXISTS auth;

-- signing_key
CREATE TABLE IF NOT EXISTS auth.signing_key (
  id SERIAL PRIMARY KEY,
  key_id TEXT NOT NULL UNIQUE,
  private_key TEXT NOT NULL,
  public_key TEXT NOT NULL,
  is_active BOOLEAN NOT NULL DEFAULT FALSE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  expires_at TIMESTAMPTZ
);

-- account (renamed from "user" for clarity and to match business domain)
CREATE TABLE IF NOT EXISTS auth.account (
  id SERIAL PRIMARY KEY,
  username VARCHAR(100) UNIQUE,
  first_name VARCHAR(100) NOT NULL,
  last_name VARCHAR(100),
  email VARCHAR(255) NOT NULL UNIQUE,
  password TEXT NOT NULL,
  salt TEXT NOT NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

-- role
CREATE TABLE IF NOT EXISTS auth.role (
  id SERIAL PRIMARY KEY,
  name VARCHAR(100) NOT NULL UNIQUE,
  description TEXT
);

-- user_role (join) - still called user_role for clarity about its purpose
CREATE TABLE IF NOT EXISTS auth.user_role (
  account_id INTEGER NOT NULL REFERENCES auth.account(id) ON DELETE CASCADE,
  role_id INTEGER NOT NULL REFERENCES auth.role(id) ON DELETE CASCADE,
  PRIMARY KEY (account_id, role_id)
);

-- client
CREATE TABLE IF NOT EXISTS auth.client (
  id SERIAL PRIMARY KEY,
  client_id VARCHAR(100) NOT NULL UNIQUE,
  name VARCHAR(100) NOT NULL,
  client_url VARCHAR(200) NOT NULL
);


-- seed roles
INSERT INTO auth.role (name, description) VALUES
  ('Admin', 'Administrator role'),
  ('User', 'Default user role')
ON CONFLICT (name) DO NOTHING;

-- seed web client for frontend authentication
INSERT INTO auth.client (client_id, name, client_url) VALUES
  ('gm-buddy-web', 'GM Buddy Web Application', 'https://localhost:49505')
ON CONFLICT (client_id) DO NOTHING;

-- Seed + schema for GM_Buddy public schema

CREATE TABLE IF NOT EXISTS public.game_system (
    game_system_id   int GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    game_system_name text NOT NULL UNIQUE
);

-- Campaign table
CREATE TABLE IF NOT EXISTS public.campaign (
    campaign_id     int GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    account_id      int NOT NULL,
    game_system_id  int NOT NULL,
    name            text NOT NULL,
    description     text,
    created_at      timestamptz NOT NULL DEFAULT now(),
    updated_at      timestamptz NOT NULL DEFAULT now(),
    FOREIGN KEY (account_id) REFERENCES auth.account(id) ON DELETE CASCADE,
    FOREIGN KEY (game_system_id) REFERENCES public.game_system(game_system_id)
);

-- NPC table:
-- Keep a lightweight `stats` column for backward compatibility / quick reads,
-- and store strongly-typed or system-specific JSON in `npc_additional_data`.
CREATE TABLE IF NOT EXISTS public.npc (
    npc_id          int GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    account_id      int NOT NULL,
    game_system_id  int NOT NULL,
    name            text NOT NULL,
    description     text,
    -- Use jsonb for the stats blob to enable JSON queries/indexing.
    stats  jsonb NOT NULL DEFAULT '{}'::jsonb,
    created_at      timestamptz NOT NULL DEFAULT now(),
    updated_at      timestamptz NOT NULL DEFAULT now(),
    FOREIGN KEY (account_id) REFERENCES auth.account(id) ON DELETE CASCADE,
    FOREIGN KEY (game_system_id) REFERENCES public.game_system(game_system_id)
);

-- PC table:
CREATE TABLE IF NOT EXISTS public.pc(
    pc_id           int GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    account_id      int NOT NULL,
    game_system_id  int NOT NULL,
    name            text NOT NULL,
    description     text,
    created_at      timestamptz NOT NULL DEFAULT now(),
    updated_at      timestamptz NOT NULL DEFAULT now(),
    FOREIGN KEY (account_id) REFERENCES auth.account(id) ON DELETE CASCADE,
    FOREIGN KEY (game_system_id) REFERENCES public.game_system(game_system_id)
);

-- Organization table:
CREATE TABLE IF NOT EXISTS public.organization (
    organization_id int GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    account_id      int NOT NULL,
    name            text NOT NULL,
    description     text,
    created_at      timestamptz NOT NULL DEFAULT now(),
    updated_at      timestamptz NOT NULL DEFAULT now(),
    FOREIGN KEY (account_id) REFERENCES auth.account(id) ON DELETE CASCADE
);

-- Relationship type table
CREATE TABLE IF NOT EXISTS public.relationship_type (
    relationship_type_id int GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    relationship_type_name text NOT NULL UNIQUE,
    description text,
    is_directional boolean DEFAULT true,
    inverse_type_id int,
    created_at timestamptz NOT NULL DEFAULT now(),
    FOREIGN KEY (inverse_type_id) REFERENCES public.relationship_type(relationship_type_id) ON DELETE SET NULL
);

-- Entity Relationship Table (Polymorphic Pattern)
CREATE TABLE IF NOT EXISTS public.entity_relationship (
    entity_relationship_id int GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    
    -- Source entity
    source_entity_type text NOT NULL CHECK (source_entity_type IN ('npc', 'pc', 'organization')),
    source_entity_id int NOT NULL,
    
    -- Target entity
    target_entity_type text NOT NULL CHECK (target_entity_type IN ('npc', 'pc', 'organization')),
    target_entity_id int NOT NULL,
    
    -- Relationship metadata
    relationship_type_id int NOT NULL,
    description text,
    strength int CHECK (strength BETWEEN 1 AND 10),
    is_active boolean DEFAULT true,
    campaign_id int,
    
    -- Audit
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now(),
    
    FOREIGN KEY (relationship_type_id) REFERENCES public.relationship_type(relationship_type_id) ON DELETE RESTRICT,
    FOREIGN KEY (campaign_id) REFERENCES public.campaign(campaign_id) ON DELETE CASCADE,
    
    UNIQUE (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, campaign_id)
);

-- Indexes
CREATE INDEX IF NOT EXISTS idx_entity_relationship_source ON public.entity_relationship (source_entity_type, source_entity_id);
CREATE INDEX IF NOT EXISTS idx_entity_relationship_target ON public.entity_relationship (target_entity_type, target_entity_id);
CREATE INDEX IF NOT EXISTS idx_entity_relationship_type ON public.entity_relationship (relationship_type_id);

-- Seed relationship types
INSERT INTO public.relationship_type (relationship_type_name, description, is_directional) VALUES
    ('Friend', 'Friendly relationship', false),
    ('Ally', 'Allied, working together', false),
    ('Enemy', 'Hostile relationship', false),
    ('Rival', 'Competitive relationship', false),
    ('Mentor', 'Teacher/guide relationship', true),
    ('Student', 'Learner', true),
    ('Member', 'Member of an organization', true),
    ('Leader', 'Leader or authority figure', true),
    ('Parent', 'Parent-child relationship', true),
    ('Child', 'Child-parent relationship', true),
    ('Sibling', 'Brother or sister', false),
    ('Spouse', 'Married or life partner', false)
ON CONFLICT (relationship_type_name) DO NOTHING;

-- Seed data
INSERT INTO auth.account (username, first_name, last_name, email, password, salt)
VALUES
  ('gm_admin', 'GM', 'Admin', 'gm_admin@example.local', 'temp_password', 'temp_salt')
ON CONFLICT (username) DO NOTHING;

INSERT INTO public.game_system (game_system_name)
VALUES
  ('Dungeons & Dragons (5e)'),
  ('Pathfinder 2e'),
  ('Generic')
ON CONFLICT (game_system_name) DO NOTHING;

-- Insert sample campaign
INSERT INTO public.campaign (account_id, game_system_id, name, description)
VALUES
  (
    (SELECT id FROM auth.account WHERE username = 'gm_admin' LIMIT 1),
    (SELECT game_system_id FROM public.game_system WHERE game_system_name = 'Dungeons & Dragons (5e)' LIMIT 1),
    'The Lost Mines of Phandelver',
    'A classic adventure in the Sword Coast region, where adventurers seek to uncover the secrets of Wave Echo Cave.'
  ),
  (
    (SELECT id FROM auth.account WHERE username = 'gm_admin' LIMIT 1),
    (SELECT game_system_id FROM public.game_system WHERE game_system_name = 'Dungeons & Dragons (5e)' LIMIT 1),
    'Curse of Strahd',
    'A gothic horror campaign set in the dread realm of Barovia, ruled by the vampire lord Strahd von Zarovich.'
  )
ON CONFLICT DO NOTHING;

-- Insert sample NPCs
INSERT INTO public.npc (account_id, game_system_id, name, description, stats)
VALUES
  (
    (SELECT id FROM auth.account WHERE username = 'gm_admin' LIMIT 1),
    (SELECT game_system_id FROM public.game_system WHERE game_system_name = 'Dungeons & Dragons (5e)' LIMIT 1),
    'Bob The Coolguy',
    'A mysterious figure with a penchant for adventure.',
    jsonb_build_object(
      'lineage', 'Human',
      'description', 'A grizzled sellsword who travels between towns, taking contracts from the highest bidder.',
      'gender', 'Male',
      'attributes', jsonb_build_object(
        'strength', 14,
        'dexterity', 12,
        'constitution', 13,
        'intelligence', 10,
        'wisdom', 11,
        'charisma', 9
      ),
      'languages', jsonb_build_array('Common', 'Thieves'' Cant')
    )
  ),
  (
    (SELECT id FROM auth.account WHERE username = 'gm_admin' LIMIT 1),
    (SELECT game_system_id FROM public.game_system WHERE game_system_name = 'Dungeons & Dragons (5e)' LIMIT 1),
    'Elara Moonwhisper',
    'A scholarly mage who studies ancient arcane texts in her tower.',
    jsonb_build_object(
      'lineage', 'Elf',
      'occupation', 'Wizard',
      'gender', 'Female',
      'attributes', jsonb_build_object(
        'strength', 8,
        'dexterity', 14,
        'constitution', 10,
        'intelligence', 17,
        'wisdom', 13,
        'charisma', 12
      ),
      'languages', jsonb_build_array('Common', 'Elvish', 'Draconic')
    )
  ),
  (
    (SELECT id FROM auth.account WHERE username = 'gm_admin' LIMIT 1),
    (SELECT game_system_id FROM public.game_system WHERE game_system_name = 'Dungeons & Dragons (5e)' LIMIT 1),
    'Gundren Rockseeker',
    'A dwarf prospector who discovered the location of Wave Echo Cave.',
    jsonb_build_object(
      'lineage', 'Dwarf',
      'occupation', 'Prospector',
      'gender', 'Male',
      'attributes', jsonb_build_object(
        'strength', 13,
        'dexterity', 9,
        'constitution', 15,
        'intelligence', 12,
        'wisdom', 14,
        'charisma', 10
      ),
      'languages', jsonb_build_array('Common', 'Dwarvish')
    )
  ),
  (
    (SELECT id FROM auth.account WHERE username = 'gm_admin' LIMIT 1),
    (SELECT game_system_id FROM public.game_system WHERE game_system_name = 'Dungeons & Dragons (5e)' LIMIT 1),
    'Sildar Hallwinter',
    'A human warrior and member of the Lords'' Alliance.',
    jsonb_build_object(
      'lineage', 'Human',
      'occupation', 'Fighter',
      'gender', 'Male',
      'attributes', jsonb_build_object(
        'strength', 15,
        'dexterity', 13,
        'constitution', 14,
        'intelligence', 11,
        'wisdom', 12,
        'charisma', 13
      ),
      'languages', jsonb_build_array('Common')
    )
  ),
  (
    (SELECT id FROM auth.account WHERE username = 'gm_admin' LIMIT 1),
    (SELECT game_system_id FROM public.game_system WHERE game_system_name = 'Dungeons & Dragons (5e)' LIMIT 1),
    'The Black Spider',
    'A mysterious drow villain seeking to claim Wave Echo Cave.',
    jsonb_build_object(
      'lineage', 'Drow',
      'occupation', 'Villain',
      'gender', 'Male',
      'attributes', jsonb_build_object(
        'strength', 10,
        'dexterity', 16,
        'constitution', 12,
        'intelligence', 14,
        'wisdom', 13,
        'charisma', 15
      ),
      'languages', jsonb_build_array('Common', 'Elvish', 'Undercommon')
    )
  )
ON CONFLICT DO NOTHING;

-- Insert sample PCs
INSERT INTO public.pc (account_id, game_system_id, name, description)
VALUES
  (
    (SELECT id FROM auth.account WHERE username = 'gm_admin' LIMIT 1),
    (SELECT game_system_id FROM public.game_system WHERE game_system_name = 'Dungeons & Dragons (5e)' LIMIT 1),
    'Thorin Ironforge',
    'A dwarven cleric devoted to Moradin, god of creation and forging.'
  ),
  (
    (SELECT id FROM auth.account WHERE username = 'gm_admin' LIMIT 1),
    (SELECT game_system_id FROM public.game_system WHERE game_system_name = 'Dungeons & Dragons (5e)' LIMIT 1),
    'Lyra Shadowstep',
    'A half-elf rogue with a mysterious past and quick fingers.'
  ),
  (
    (SELECT id FROM auth.account WHERE username = 'gm_admin' LIMIT 1),
    (SELECT game_system_id FROM public.game_system WHERE game_system_name = 'Dungeons & Dragons (5e)' LIMIT 1),
    'Aldric the Brave',
    'A human paladin sworn to protect the innocent and vanquish evil.'
  ),
  (
    (SELECT id FROM auth.account WHERE username = 'gm_admin' LIMIT 1),
    (SELECT game_system_id FROM public.game_system WHERE game_system_name = 'Dungeons & Dragons (5e)' LIMIT 1),
    'Zephyr Windwhisper',
    'An elven ranger who tracks and hunts dangerous beasts in the wilderness.'
  )
ON CONFLICT DO NOTHING;

-- Insert sample organizations
INSERT INTO public.organization (account_id, name, description)
VALUES
  (
    (SELECT id FROM auth.account WHERE username = 'gm_admin' LIMIT 1),
    'The Lords'' Alliance',
    'A coalition of rulers from cities across Faerun, united against common threats.'
  ),
  (
    (SELECT id FROM auth.account WHERE username = 'gm_admin' LIMIT 1),
    'The Harpers',
    'A secretive organization working to promote good, preserve history, and maintain balance.'
  ),
  (
    (SELECT id FROM auth.account WHERE username = 'gm_admin' LIMIT 1),
    'The Zhentarim',
    'A shadowy network of mercenaries and traders seeking power and profit.'
  ),
  (
    (SELECT id FROM auth.account WHERE username = 'gm_admin' LIMIT 1),
    'Rockseeker Brothers Mining Company',
    'A dwarven mining operation searching for lost mines and treasure.'
  )
ON CONFLICT DO NOTHING;

-- Insert entity relationships
-- Note: Using subqueries to get the IDs dynamically, assuming sequential insertion
INSERT INTO public.entity_relationship (
    source_entity_type, source_entity_id, 
    target_entity_type, target_entity_id, 
    relationship_type_id, description, strength, is_active, campaign_id
)
VALUES
  -- Bob The Coolguy (NPC 1) is Friends with Elara Moonwhisper (NPC 2)
  (
    'npc', (SELECT npc_id FROM public.npc WHERE name = 'Bob The Coolguy' LIMIT 1),
    'npc', (SELECT npc_id FROM public.npc WHERE name = 'Elara Moonwhisper' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Friend' LIMIT 1),
    'Old friends from their adventuring days',
    7,
    true,
    (SELECT campaign_id FROM public.campaign WHERE name = 'The Lost Mines of Phandelver' LIMIT 1)
  ),
  -- Gundren Rockseeker (NPC 3) is a Member of Rockseeker Brothers Mining Company (Org 4)
  (
    'npc', (SELECT npc_id FROM public.npc WHERE name = 'Gundren Rockseeker' LIMIT 1),
    'organization', (SELECT organization_id FROM public.organization WHERE name = 'Rockseeker Brothers Mining Company' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Member' LIMIT 1),
    'Founder and lead prospector',
    10,
    true,
    (SELECT campaign_id FROM public.campaign WHERE name = 'The Lost Mines of Phandelver' LIMIT 1)
  ),
  -- Sildar Hallwinter (NPC 4) is a Member of The Lords' Alliance (Org 1)
  (
    'npc', (SELECT npc_id FROM public.npc WHERE name = 'Sildar Hallwinter' LIMIT 1),
    'organization', (SELECT organization_id FROM public.organization WHERE name = 'The Lords'' Alliance' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Member' LIMIT 1),
    'Veteran agent of the Alliance',
    8,
    true,
    (SELECT campaign_id FROM public.campaign WHERE name = 'The Lost Mines of Phandelver' LIMIT 1)
  ),
  -- The Black Spider (NPC 5) is Enemy of Gundren Rockseeker (NPC 3)
  (
    'npc', (SELECT npc_id FROM public.npc WHERE name = 'The Black Spider' LIMIT 1),
    'npc', (SELECT npc_id FROM public.npc WHERE name = 'Gundren Rockseeker' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Enemy' LIMIT 1),
    'Seeks to steal the location of Wave Echo Cave',
    9,
    true,
    (SELECT campaign_id FROM public.campaign WHERE name = 'The Lost Mines of Phandelver' LIMIT 1)
  ),
  -- Thorin Ironforge (PC 1) is Ally of Gundren Rockseeker (NPC 3)
  (
    'pc', (SELECT pc_id FROM public.pc WHERE name = 'Thorin Ironforge' LIMIT 1),
    'npc', (SELECT npc_id FROM public.npc WHERE name = 'Gundren Rockseeker' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Ally' LIMIT 1),
    'Fellow dwarf helping to reclaim the mines',
    8,
    true,
    (SELECT campaign_id FROM public.campaign WHERE name = 'The Lost Mines of Phandelver' LIMIT 1)
  ),
  -- Lyra Shadowstep (PC 2) has Elara Moonwhisper (NPC 2) as Mentor
  (
    'npc', (SELECT npc_id FROM public.npc WHERE name = 'Elara Moonwhisper' LIMIT 1),
    'pc', (SELECT pc_id FROM public.pc WHERE name = 'Lyra Shadowstep' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Mentor' LIMIT 1),
    'Taught Lyra the basics of arcane magic',
    7,
    true,
    (SELECT campaign_id FROM public.campaign WHERE name = 'The Lost Mines of Phandelver' LIMIT 1)
  ),
  -- Aldric the Brave (PC 3) is Member of The Lords' Alliance (Org 1)
  (
    'pc', (SELECT pc_id FROM public.pc WHERE name = 'Aldric the Brave' LIMIT 1),
    'organization', (SELECT organization_id FROM public.organization WHERE name = 'The Lords'' Alliance' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Member' LIMIT 1),
    'Recently inducted paladin serving the Alliance',
    6,
    true,
    (SELECT campaign_id FROM public.campaign WHERE name = 'The Lost Mines of Phandelver' LIMIT 1)
  ),
  -- Zephyr Windwhisper (PC 4) is Member of The Harpers (Org 2)
  (
    'pc', (SELECT pc_id FROM public.pc WHERE name = 'Zephyr Windwhisper' LIMIT 1),
    'organization', (SELECT organization_id FROM public.organization WHERE name = 'The Harpers' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Member' LIMIT 1),
    'Secret Harper agent gathering intelligence',
    7,
    true,
    (SELECT campaign_id FROM public.campaign WHERE name = 'The Lost Mines of Phandelver' LIMIT 1)
  ),
  -- Bob The Coolguy (NPC 1) is Rival of The Black Spider (NPC 5)
  (
    'npc', (SELECT npc_id FROM public.npc WHERE name = 'Bob The Coolguy' LIMIT 1),
    'npc', (SELECT npc_id FROM public.npc WHERE name = 'The Black Spider' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Rival' LIMIT 1),
    'Competing for the same treasure',
    6,
    true,
    (SELECT campaign_id FROM public.campaign WHERE name = 'The Lost Mines of Phandelver' LIMIT 1)
  ),
  -- The Zhentarim (Org 3) is Enemy of The Lords' Alliance (Org 1)
  (
    'organization', (SELECT organization_id FROM public.organization WHERE name = 'The Zhentarim' LIMIT 1),
    'organization', (SELECT organization_id FROM public.organization WHERE name = 'The Lords'' Alliance' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Enemy' LIMIT 1),
    'Long-standing conflict over regional control',
    8,
    true,
    (SELECT campaign_id FROM public.campaign WHERE name = 'The Lost Mines of Phandelver' LIMIT 1)
  ),
  -- Thorin Ironforge (PC 1) and Lyra Shadowstep (PC 2) are Friends
  (
    'pc', (SELECT pc_id FROM public.pc WHERE name = 'Thorin Ironforge' LIMIT 1),
    'pc', (SELECT pc_id FROM public.pc WHERE name = 'Lyra Shadowstep' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Friend' LIMIT 1),
    'Adventuring companions who trust each other',
    9,
    true,
    (SELECT campaign_id FROM public.campaign WHERE name = 'The Lost Mines of Phandelver' LIMIT 1)
  ),
  -- Aldric the Brave (PC 3) and Zephyr Windwhisper (PC 4) are Allies
  (
    'pc', (SELECT pc_id FROM public.pc WHERE name = 'Aldric the Brave' LIMIT 1),
    'pc', (SELECT pc_id FROM public.pc WHERE name = 'Zephyr Windwhisper' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Ally' LIMIT 1),
    'United in their quest to protect the innocent',
    8,
    true,
    (SELECT campaign_id FROM public.campaign WHERE name = 'The Lost Mines of Phandelver' LIMIT 1)
  )
ON CONFLICT DO NOTHING;


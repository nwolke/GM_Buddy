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

-- Insert sample NPC(s)
-- Note: the JSON below follows the shape of DnDStats (name, lineage, occupation, description, gender, attributes, languages)
INSERT INTO public.npc (account_id, game_system_id, stats)
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
  )
ON CONFLICT DO NOTHING;

-- Put a separate system-specific blob into npc_additional_data (example)
INSERT INTO public.npc_additional_data (npc_id, game_system_id, data, source)
VALUES
(
  (SELECT npc_id FROM public.npc WHERE stats->>'name' = 'Roth the Wanderer' LIMIT 1),
  (SELECT game_system_id FROM public.game_system WHERE game_system_name = 'Dungeons & Dragons (5e)' LIMIT 1),
  jsonb_build_object(
    'lineage', 'Human',
    'occupation', 'Sellsword',
'gender', 'Male',
    'attributes', jsonb_build_object(
'strength', 14,
      'dexterity', 12,
      'constitution', 13,
      'intelligence', 10,
   'wisdom', 11,
'charisma', 9
    ),
    'languages', jsonb_build_array('Common', 'Thieves'' Cant'),
    'equipment', jsonb_build_array('Shortsword', 'Leather armor', '5 gp')
  ),
  'seed'
)
ON CONFLICT DO NOTHING;
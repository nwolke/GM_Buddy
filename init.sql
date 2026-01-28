-- Create schema
CREATE SCHEMA IF NOT EXISTS auth;

-- account - Links Cognito users to application data
-- Authentication is handled by AWS Cognito, so password/salt are optional (for legacy support)
CREATE TABLE IF NOT EXISTS auth.account (
  id SERIAL PRIMARY KEY,
  username VARCHAR(100) UNIQUE,
  first_name VARCHAR(100),
  last_name VARCHAR(100),
  email VARCHAR(255) NOT NULL UNIQUE,
  cognito_sub VARCHAR(255) UNIQUE,  -- Cognito user ID (sub claim from JWT)
  subscription_tier VARCHAR(50) DEFAULT 'free',  -- free, supporter, premium, lifetime
  password TEXT,  -- Optional: Only for legacy accounts
  salt TEXT,      -- Optional: Only for legacy accounts
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  last_login_at TIMESTAMPTZ
);

-- Index for fast Cognito sub lookups
CREATE INDEX IF NOT EXISTS idx_account_cognito_sub ON auth.account(cognito_sub);

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
-- NPCs are now associated with campaigns, which in turn have game systems.
-- Keep a lightweight `stats` column for backward compatibility / quick reads,
-- and store strongly-typed or system-specific JSON in `npc_additional_data`.
CREATE TABLE IF NOT EXISTS public.npc (
    npc_id          int GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    account_id      int NOT NULL,
    campaign_id     int NOT NULL,
    name            text NOT NULL,
    description     text,
    -- Use jsonb for the stats blob to enable JSON queries/indexing.
    stats  jsonb NOT NULL DEFAULT '{}'::jsonb,
    created_at      timestamptz NOT NULL DEFAULT now(),
    updated_at      timestamptz NOT NULL DEFAULT now(),
    FOREIGN KEY (account_id) REFERENCES auth.account(id) ON DELETE CASCADE,
    FOREIGN KEY (campaign_id) REFERENCES public.campaign(campaign_id) ON DELETE RESTRICT
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
-- Dev user with cognito_sub matching the dev mode login
INSERT INTO auth.account (username, first_name, last_name, email, cognito_sub, subscription_tier)
VALUES
  ('gm_admin', 'GM', 'Admin', 'nathanwolke@outlook.com', 'dev-user-sub', 'premium')
ON CONFLICT (username) DO NOTHING;

-- Demo user for React app demo login (fallback if gm_admin doesn't work)
INSERT INTO auth.account (username, first_name, last_name, email, cognito_sub, subscription_tier)
VALUES
  ('demo_user', 'Demo', 'User', 'demo@example.com', 'demo-user-sub', 'free')
ON CONFLICT (username) DO NOTHING;

INSERT INTO public.game_system (game_system_name)
VALUES
  ('Dungeons & Dragons (5e)'),
  ('Pathfinder 2e'),
  ('Generic')
ON CONFLICT (game_system_name) DO NOTHING;

-- Insert sample campaigns (generic fantasy content)
INSERT INTO public.campaign (account_id, game_system_id, name, description)
VALUES
  (
    (SELECT id FROM auth.account WHERE username = 'gm_admin' LIMIT 1),
    (SELECT game_system_id FROM public.game_system WHERE game_system_name = 'Dungeons & Dragons (5e)' LIMIT 1),
    'Shadows Over Millhaven',
    'A mystery unfolds in the peaceful town of Millhaven as strange occurrences and disappearances plague the locals.'
  ),
  (
    (SELECT id FROM auth.account WHERE username = 'gm_admin' LIMIT 1),
    (SELECT game_system_id FROM public.game_system WHERE game_system_name = 'Dungeons & Dragons (5e)' LIMIT 1),
    'The Northern Frontier',
    'An exploration campaign in the untamed wilderness, where adventurers seek fortune and face the dangers of the wild.'
  )
ON CONFLICT DO NOTHING;

-- Insert sample NPCs (generic fantasy characters)
INSERT INTO public.npc (account_id, campaign_id, name, description, stats)
VALUES
  (
    (SELECT id FROM auth.account WHERE username = 'gm_admin' LIMIT 1),
    (SELECT campaign_id FROM public.campaign WHERE name = 'Shadows Over Millhaven' LIMIT 1),
    'Marcus Blackwood',
    'The town magistrate who oversees law and order in Millhaven. Known for his fair judgment and dedication to the community.',
    jsonb_build_object(
      'lineage', 'Human',
      'occupation', 'Magistrate'
    )
  ),
  (
    (SELECT id FROM auth.account WHERE username = 'gm_admin' LIMIT 1),
    (SELECT campaign_id FROM public.campaign WHERE name = 'Shadows Over Millhaven' LIMIT 1),
    'Lyanna Swift',
    'A traveling merchant who deals in rare herbs and alchemical components. She has connections throughout the region.',
    jsonb_build_object(
      'lineage', 'Half-Elf',
      'occupation', 'Merchant'
    )
  ),
  (
    (SELECT id FROM auth.account WHERE username = 'gm_admin' LIMIT 1),
    (SELECT campaign_id FROM public.campaign WHERE name = 'The Northern Frontier' LIMIT 1),
    'Thorgar Stonefist',
    'A seasoned explorer and guide who knows the northern mountains better than anyone.',
    jsonb_build_object(
      'lineage', 'Dwarf',
      'occupation', 'Guide'
    )
  ),
  (
    (SELECT id FROM auth.account WHERE username = 'gm_admin' LIMIT 1),
    (SELECT campaign_id FROM public.campaign WHERE name = 'The Northern Frontier' LIMIT 1),
    'Kael Windrunner',
    'A skilled tracker and hunter who makes a living by trapping and guiding hunting expeditions.',
    jsonb_build_object(
      'lineage', 'Elf',
      'occupation', 'Tracker'
    )
  ),
  (
    (SELECT id FROM auth.account WHERE username = 'gm_admin' LIMIT 1),
    (SELECT campaign_id FROM public.campaign WHERE name = 'The Northern Frontier' LIMIT 1),
    'Red Scar',
    'A notorious bandit leader who controls the mountain passes and demands tribute from travelers.',
    jsonb_build_object(
      'lineage', 'Half-Orc',
      'occupation', 'Bandit Chief'
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

-- Also create NPCs for the demo_user account (for React demo login)
INSERT INTO public.npc (account_id, game_system_id, name, description, stats)
VALUES
  (
    (SELECT id FROM auth.account WHERE username = 'demo_user' LIMIT 1),
    (SELECT game_system_id FROM public.game_system WHERE game_system_name = 'Dungeons & Dragons (5e)' LIMIT 1),
    'Demo Merchant',
    'A friendly merchant for testing the demo.',
    jsonb_build_object(
      'lineage', 'Human',
      'occupation', 'Merchant',
      'gender', 'Male'
    )
  ),
  (
    (SELECT id FROM auth.account WHERE username = 'demo_user' LIMIT 1),
    (SELECT game_system_id FROM public.game_system WHERE game_system_name = 'Dungeons & Dragons (5e)' LIMIT 1),
    'Demo Guard',
    'A town guard for testing the demo.',
    jsonb_build_object(
      'lineage', 'Human',
      'occupation', 'Guard',
      'gender', 'Female'
    )
  )
ON CONFLICT DO NOTHING;


-- Create schema
CREATE SCHEMA IF NOT EXISTS auth;

-- account - Links Cognito users to application data
-- Authentication is handled by AWS Cognito
CREATE TABLE IF NOT EXISTS auth.account (
  id SERIAL PRIMARY KEY,
  username VARCHAR(100) UNIQUE,
  first_name VARCHAR(100),
  last_name VARCHAR(100),
  email VARCHAR(255) NOT NULL UNIQUE,
  cognito_sub VARCHAR(255) UNIQUE,  -- Cognito user ID (sub claim from JWT)
  subscription_tier VARCHAR(50) DEFAULT 'free',  -- free, supporter, premium, lifetime
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  last_login_at TIMESTAMPTZ
);

-- Index for fast Cognito sub lookups
CREATE INDEX IF NOT EXISTS idx_account_cognito_sub ON auth.account(cognito_sub);

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
-- Dev user account for testing
INSERT INTO auth.account (username, first_name, last_name, email, cognito_sub, subscription_tier)
VALUES
  ('gm_admin', 'GM', 'Admin', 'nathanwolke@outlook.com', '38318390-3021-70cf-5f3d-fae7caa59be1', 'premium')
ON CONFLICT (username) DO NOTHING;

INSERT INTO public.game_system (game_system_name)
VALUES
  ('Dungeons & Dragons (5e)'),
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
    'The Regional Alliance',
    'A coalition of regional leaders united to maintain peace and prosperity.'
  ),
  (
    (SELECT id FROM auth.account WHERE username = 'gm_admin' LIMIT 1),
    'The Archivists',
    'A secretive organization dedicated to preserving knowledge and maintaining balance.'
  ),
  (
    (SELECT id FROM auth.account WHERE username = 'gm_admin' LIMIT 1),
    'The Shadow Syndicate',
    'A shadowy network of mercenaries and traders seeking power and profit.'
  ),
  (
    (SELECT id FROM auth.account WHERE username = 'gm_admin' LIMIT 1),
    'Stonepeak Mining Consortium',
    'A mining operation searching for valuable mineral deposits and lost treasures.'
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
  -- Marcus Blackwood (NPC) is Friends with Lyanna Swift (NPC) in Shadows Over Millhaven
  (
    'npc', (SELECT npc_id FROM public.npc WHERE name = 'Marcus Blackwood' LIMIT 1),
    'npc', (SELECT npc_id FROM public.npc WHERE name = 'Lyanna Swift' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Friend' LIMIT 1),
    'The magistrate trusts the merchant for advice on regional affairs',
    7,
    true,
    (SELECT campaign_id FROM public.campaign WHERE name = 'Shadows Over Millhaven' LIMIT 1)
  ),
  -- Marcus Blackwood (NPC) is Ally with Lyanna Swift (NPC) in Shadows Over Millhaven
  (
    'npc', (SELECT npc_id FROM public.npc WHERE name = 'Marcus Blackwood' LIMIT 1),
    'npc', (SELECT npc_id FROM public.npc WHERE name = 'Lyanna Swift' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Ally' LIMIT 1),
    'Working together to solve the mysteries plaguing Millhaven',
    8,
    true,
    (SELECT campaign_id FROM public.campaign WHERE name = 'Shadows Over Millhaven' LIMIT 1)
  ),
  -- Thorgar Stonefist (NPC) is Enemy of Red Scar (NPC) in The Northern Frontier
  (
    'npc', (SELECT npc_id FROM public.npc WHERE name = 'Thorgar Stonefist' LIMIT 1),
    'npc', (SELECT npc_id FROM public.npc WHERE name = 'Red Scar' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Enemy' LIMIT 1),
    'The guide refuses to pay tribute to the bandit chief',
    9,
    true,
    (SELECT campaign_id FROM public.campaign WHERE name = 'The Northern Frontier' LIMIT 1)
  ),
  -- Kael Windrunner (NPC) is Ally of Thorgar Stonefist (NPC) in The Northern Frontier
  (
    'npc', (SELECT npc_id FROM public.npc WHERE name = 'Kael Windrunner' LIMIT 1),
    'npc', (SELECT npc_id FROM public.npc WHERE name = 'Thorgar Stonefist' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Ally' LIMIT 1),
    'Tracker and guide work together to map the frontier',
    8,
    true,
    (SELECT campaign_id FROM public.campaign WHERE name = 'The Northern Frontier' LIMIT 1)
  ),
  -- Kael Windrunner (NPC) is Enemy of Red Scar (NPC) in The Northern Frontier
  (
    'npc', (SELECT npc_id FROM public.npc WHERE name = 'Kael Windrunner' LIMIT 1),
    'npc', (SELECT npc_id FROM public.npc WHERE name = 'Red Scar' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Enemy' LIMIT 1),
    'The tracker actively hunts the bandit and his crew',
    7,
    true,
    (SELECT campaign_id FROM public.campaign WHERE name = 'The Northern Frontier' LIMIT 1)
  ),
  -- Thorin Ironforge (PC) is Member of The Regional Alliance (Org)
  -- Note: PC-to-Organization memberships use NULL campaign_id as they are campaign-agnostic
  (
    'pc', (SELECT pc_id FROM public.pc WHERE name = 'Thorin Ironforge' LIMIT 1),
    'organization', (SELECT organization_id FROM public.organization WHERE name = 'The Regional Alliance' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Member' LIMIT 1),
    'Dwarven cleric serving as a liaison for the Alliance',
    7,
    true,
    NULL
  ),
  -- Lyra Shadowstep (PC) is Member of The Archivists (Org)
  (
    'pc', (SELECT pc_id FROM public.pc WHERE name = 'Lyra Shadowstep' LIMIT 1),
    'organization', (SELECT organization_id FROM public.organization WHERE name = 'The Archivists' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Member' LIMIT 1),
    'Half-elf rogue working as a secret agent and researcher',
    8,
    true,
    NULL
  ),
  -- Aldric the Brave (PC) is Member of The Regional Alliance (Org)
  (
    'pc', (SELECT pc_id FROM public.pc WHERE name = 'Aldric the Brave' LIMIT 1),
    'organization', (SELECT organization_id FROM public.organization WHERE name = 'The Regional Alliance' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Member' LIMIT 1),
    'Paladin recently inducted into the Alliance',
    6,
    true,
    NULL
  ),
  -- Zephyr Windwhisper (PC) is Member of The Archivists (Org)
  (
    'pc', (SELECT pc_id FROM public.pc WHERE name = 'Zephyr Windwhisper' LIMIT 1),
    'organization', (SELECT organization_id FROM public.organization WHERE name = 'The Archivists' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Member' LIMIT 1),
    'Elven ranger gathering intelligence and preserving lore',
    7,
    true,
    NULL
  ),
  -- Thorin Ironforge (PC) is Ally of Marcus Blackwood (NPC) in Shadows Over Millhaven
  (
    'pc', (SELECT pc_id FROM public.pc WHERE name = 'Thorin Ironforge' LIMIT 1),
    'npc', (SELECT npc_id FROM public.npc WHERE name = 'Marcus Blackwood' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Ally' LIMIT 1),
    'The cleric assists the magistrate in investigating the mysteries',
    7,
    true,
    (SELECT campaign_id FROM public.campaign WHERE name = 'Shadows Over Millhaven' LIMIT 1)
  ),
  -- Lyanna Swift (NPC) is Mentor of Lyra Shadowstep (PC) in Shadows Over Millhaven
  (
    'npc', (SELECT npc_id FROM public.npc WHERE name = 'Lyanna Swift' LIMIT 1),
    'pc', (SELECT pc_id FROM public.pc WHERE name = 'Lyra Shadowstep' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Mentor' LIMIT 1),
    'The merchant taught Lyra about rare herbs and alchemy',
    8,
    true,
    (SELECT campaign_id FROM public.campaign WHERE name = 'Shadows Over Millhaven' LIMIT 1)
  ),
  -- Thorgar Stonefist (NPC) is Mentor of Zephyr Windwhisper (PC) in The Northern Frontier
  (
    'npc', (SELECT npc_id FROM public.npc WHERE name = 'Thorgar Stonefist' LIMIT 1),
    'pc', (SELECT pc_id FROM public.pc WHERE name = 'Zephyr Windwhisper' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Mentor' LIMIT 1),
    'The guide taught the ranger the secrets of the northern mountains',
    7,
    true,
    (SELECT campaign_id FROM public.campaign WHERE name = 'The Northern Frontier' LIMIT 1)
  ),
  -- Aldric the Brave (PC) is Ally of Kael Windrunner (NPC) in The Northern Frontier
  (
    'pc', (SELECT pc_id FROM public.pc WHERE name = 'Aldric the Brave' LIMIT 1),
    'npc', (SELECT npc_id FROM public.npc WHERE name = 'Kael Windrunner' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Ally' LIMIT 1),
    'Paladin and tracker united in protecting travelers from bandits',
    8,
    true,
    (SELECT campaign_id FROM public.campaign WHERE name = 'The Northern Frontier' LIMIT 1)
  ),
  -- The Shadow Syndicate (Org) is Enemy of The Regional Alliance (Org)
  -- Note: Organization-to-Organization relationships use NULL campaign_id as they exist across all campaigns
  (
    'organization', (SELECT organization_id FROM public.organization WHERE name = 'The Shadow Syndicate' LIMIT 1),
    'organization', (SELECT organization_id FROM public.organization WHERE name = 'The Regional Alliance' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Enemy' LIMIT 1),
    'Long-standing conflict over regional influence and trade routes',
    8,
    true,
    NULL
  ),
  -- The Archivists (Org) is Ally of The Regional Alliance (Org)
  (
    'organization', (SELECT organization_id FROM public.organization WHERE name = 'The Archivists' LIMIT 1),
    'organization', (SELECT organization_id FROM public.organization WHERE name = 'The Regional Alliance' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Ally' LIMIT 1),
    'United in promoting stability and opposing tyranny',
    7,
    true,
    NULL
  ),
  -- Thorin Ironforge (PC) and Lyra Shadowstep (PC) are Friends
  -- Note: PC-to-PC friendships use NULL campaign_id as they persist across campaigns
  (
    'pc', (SELECT pc_id FROM public.pc WHERE name = 'Thorin Ironforge' LIMIT 1),
    'pc', (SELECT pc_id FROM public.pc WHERE name = 'Lyra Shadowstep' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Friend' LIMIT 1),
    'Adventuring companions who trust each other completely',
    9,
    true,
    NULL
  ),
  -- Aldric the Brave (PC) and Zephyr Windwhisper (PC) are Allies
  (
    'pc', (SELECT pc_id FROM public.pc WHERE name = 'Aldric the Brave' LIMIT 1),
    'pc', (SELECT pc_id FROM public.pc WHERE name = 'Zephyr Windwhisper' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Ally' LIMIT 1),
    'United in their quest to protect the innocent and fight evil',
    8,
    true,
    NULL
  ),
  -- Thorin Ironforge (PC) is Member of Stonepeak Mining Consortium (Org)
  (
    'pc', (SELECT pc_id FROM public.pc WHERE name = 'Thorin Ironforge' LIMIT 1),
    'organization', (SELECT organization_id FROM public.organization WHERE name = 'Stonepeak Mining Consortium' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Member' LIMIT 1),
    'Fellow dwarf invested in the mining consortium ventures',
    6,
    true,
    NULL
  )
ON CONFLICT DO NOTHING;

-- Reference Data Tables for game systems (system-agnostic terminology)

-- Reference Lineage table (system-agnostic term for race/ancestry)
CREATE TABLE IF NOT EXISTS public.reference_lineage (
    lineage_id int GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    game_system_id int NOT NULL,
    account_id int,
    campaign_id int,
    name text NOT NULL,
    description text,
    is_active boolean NOT NULL DEFAULT true,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now(),
    FOREIGN KEY (game_system_id) REFERENCES public.game_system(game_system_id) ON DELETE CASCADE,
    FOREIGN KEY (account_id) REFERENCES auth.account(id) ON DELETE CASCADE,
    FOREIGN KEY (campaign_id) REFERENCES public.campaign(campaign_id) ON DELETE CASCADE,
    UNIQUE (game_system_id, account_id, campaign_id, name),
    -- Enforce that entries are either SRD (both NULL) or campaign-specific (both NOT NULL)
    CHECK ((account_id IS NULL AND campaign_id IS NULL) OR (account_id IS NOT NULL AND campaign_id IS NOT NULL))
);

-- Indexes for reference_lineage
CREATE INDEX IF NOT EXISTS idx_reference_lineage_game_system ON public.reference_lineage(game_system_id);
CREATE INDEX IF NOT EXISTS idx_reference_lineage_account ON public.reference_lineage(account_id);
CREATE INDEX IF NOT EXISTS idx_reference_lineage_campaign ON public.reference_lineage(campaign_id);

-- Reference Occupation table (system-agnostic term for class/profession)
CREATE TABLE IF NOT EXISTS public.reference_occupation (
    occupation_id int GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    game_system_id int NOT NULL,
    account_id int,
    campaign_id int,
    name text NOT NULL,
    description text,
    is_active boolean NOT NULL DEFAULT true,
    created_at timestamptz NOT NULL DEFAULT now(),
    updated_at timestamptz NOT NULL DEFAULT now(),
    FOREIGN KEY (game_system_id) REFERENCES public.game_system(game_system_id) ON DELETE CASCADE,
    FOREIGN KEY (account_id) REFERENCES auth.account(id) ON DELETE CASCADE,
    FOREIGN KEY (campaign_id) REFERENCES public.campaign(campaign_id) ON DELETE CASCADE,
    UNIQUE (game_system_id, account_id, campaign_id, name),
    -- Enforce that entries are either SRD (both NULL) or campaign-specific (both NOT NULL)
    CHECK ((account_id IS NULL AND campaign_id IS NULL) OR (account_id IS NOT NULL AND campaign_id IS NOT NULL))
);

-- Indexes for reference_occupation
CREATE INDEX IF NOT EXISTS idx_reference_occupation_game_system ON public.reference_occupation(game_system_id);
CREATE INDEX IF NOT EXISTS idx_reference_occupation_account ON public.reference_occupation(account_id);
CREATE INDEX IF NOT EXISTS idx_reference_occupation_campaign ON public.reference_occupation(campaign_id);

-- Seed SRD Lineages for D&D 5e (account_id = NULL and campaign_id = NULL means SRD/global content)
INSERT INTO public.reference_lineage (game_system_id, account_id, campaign_id, name, description)
VALUES
    (
        (SELECT game_system_id FROM public.game_system WHERE game_system_name = 'Dungeons & Dragons (5e)' LIMIT 1),
        NULL,
        NULL,
        'Dragonborn',
        'Dragonborn look very much like dragons standing erect in humanoid form, though they lack wings or a tail.'
    ),
    (
        (SELECT game_system_id FROM public.game_system WHERE game_system_name = 'Dungeons & Dragons (5e)' LIMIT 1),
        NULL,
        NULL,
        'Dwarf',
        'Bold and hardy, dwarves are known as skilled warriors, miners, and workers of stone and metal.'
    ),
    (
        (SELECT game_system_id FROM public.game_system WHERE game_system_name = 'Dungeons & Dragons (5e)' LIMIT 1),
        NULL,
        NULL,
        'Elf',
        'Elves are a magical people of otherworldly grace, living in the world but not entirely part of it.'
    ),
    (
        (SELECT game_system_id FROM public.game_system WHERE game_system_name = 'Dungeons & Dragons (5e)' LIMIT 1),
        NULL,
        NULL,
        'Gnome',
        'A gnome''s energy and enthusiasm for living shines through every inch of his or her tiny body.'
    ),
    (
        (SELECT game_system_id FROM public.game_system WHERE game_system_name = 'Dungeons & Dragons (5e)' LIMIT 1),
        NULL,
        NULL,
        'Half-Elf',
        'Walking in two worlds but truly belonging to neither, half-elves combine what some say are the best qualities of both races.'
    ),
    (
        (SELECT game_system_id FROM public.game_system WHERE game_system_name = 'Dungeons & Dragons (5e)' LIMIT 1),
        NULL,
        NULL,
        'Half-Orc',
        'Whether united under the leadership of a mighty warlock or having fought to a standstill after years of conflict, orc and human tribes sometimes form alliances.'
    ),
    (
        (SELECT game_system_id FROM public.game_system WHERE game_system_name = 'Dungeons & Dragons (5e)' LIMIT 1),
        NULL,
        NULL,
        'Halfling',
        'The diminutive halflings survive in a world full of larger creatures by avoiding notice or, barring that, avoiding offense.'
    ),
    (
        (SELECT game_system_id FROM public.game_system WHERE game_system_name = 'Dungeons & Dragons (5e)' LIMIT 1),
        NULL,
        NULL,
        'Human',
        'Humans are the most adaptable and ambitious people among the common races, with widely varying tastes, morals, and customs.'
    ),
    (
        (SELECT game_system_id FROM public.game_system WHERE game_system_name = 'Dungeons & Dragons (5e)' LIMIT 1),
        NULL,
        NULL,
        'Tiefling',
        'To be greeted with stares and whispers, to suffer violence and insult, to see mistrust and fear in every eye: this is the lot of the tiefling.'
    )
ON CONFLICT (game_system_id, account_id, campaign_id, name) DO NOTHING;

-- Seed SRD Occupations for D&D 5e (account_id = NULL and campaign_id = NULL means SRD/global content)
INSERT INTO public.reference_occupation (game_system_id, account_id, campaign_id, name, description)
VALUES
    (
        (SELECT game_system_id FROM public.game_system WHERE game_system_name = 'Dungeons & Dragons (5e)' LIMIT 1),
        NULL,
        NULL,
        'Barbarian',
        'A fierce warrior of primitive background who can enter a battle rage.'
    ),
    (
        (SELECT game_system_id FROM public.game_system WHERE game_system_name = 'Dungeons & Dragons (5e)' LIMIT 1),
        NULL,
        NULL,
        'Bard',
        'An inspiring magician whose power echoes the music of creation.'
    ),
    (
        (SELECT game_system_id FROM public.game_system WHERE game_system_name = 'Dungeons & Dragons (5e)' LIMIT 1),
        NULL,
        NULL,
        'Cleric',
        'A priestly champion who wields divine magic in service of a higher power.'
    ),
    (
        (SELECT game_system_id FROM public.game_system WHERE game_system_name = 'Dungeons & Dragons (5e)' LIMIT 1),
        NULL,
        NULL,
        'Druid',
        'A priest of the Old Faith, wielding the powers of nature and adopting animal forms.'
    ),
    (
        (SELECT game_system_id FROM public.game_system WHERE game_system_name = 'Dungeons & Dragons (5e)' LIMIT 1),
        NULL,
        NULL,
        'Fighter',
        'A master of martial combat, skilled with a variety of weapons and armor.'
    ),
    (
        (SELECT game_system_id FROM public.game_system WHERE game_system_name = 'Dungeons & Dragons (5e)' LIMIT 1),
        NULL,
        NULL,
        'Monk',
        'A master of martial arts, harnessing the power of the body in pursuit of physical and spiritual perfection.'
    ),
    (
        (SELECT game_system_id FROM public.game_system WHERE game_system_name = 'Dungeons & Dragons (5e)' LIMIT 1),
        NULL,
        NULL,
        'Paladin',
        'A holy warrior bound to a sacred oath.'
    ),
    (
        (SELECT game_system_id FROM public.game_system WHERE game_system_name = 'Dungeons & Dragons (5e)' LIMIT 1),
        NULL,
        NULL,
        'Ranger',
        'A warrior who combats threats on the edges of civilization.'
    ),
    (
        (SELECT game_system_id FROM public.game_system WHERE game_system_name = 'Dungeons & Dragons (5e)' LIMIT 1),
        NULL,
        NULL,
        'Rogue',
        'A scoundrel who uses stealth and trickery to overcome obstacles and enemies.'
    ),
    (
        (SELECT game_system_id FROM public.game_system WHERE game_system_name = 'Dungeons & Dragons (5e)' LIMIT 1),
        NULL,
        NULL,
        'Sorcerer',
        'A spellcaster who draws on inherent magic from a gift or bloodline.'
    ),
    (
        (SELECT game_system_id FROM public.game_system WHERE game_system_name = 'Dungeons & Dragons (5e)' LIMIT 1),
        NULL,
        NULL,
        'Warlock',
        'A wielder of magic that is derived from a bargain with an extraplanar entity.'
    ),
    (
        (SELECT game_system_id FROM public.game_system WHERE game_system_name = 'Dungeons & Dragons (5e)' LIMIT 1),
        NULL,
        NULL,
        'Wizard',
        'A scholarly magic-user capable of manipulating the structures of reality.'
    )
ON CONFLICT (game_system_id, account_id, campaign_id, name) DO NOTHING;

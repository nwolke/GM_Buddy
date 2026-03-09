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
    name            text NOT NULL,
    description     text,
    created_at      timestamptz NOT NULL DEFAULT now(),
    updated_at      timestamptz NOT NULL DEFAULT now(),
    FOREIGN KEY (account_id) REFERENCES auth.account(id) ON DELETE CASCADE
);

-- NPC table:
-- NPCs are associated with campaigns.
-- Lineage, class, and faction are stored as direct columns for easier querying.
CREATE TABLE IF NOT EXISTS public.npc (
    npc_id          int GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    account_id      int NOT NULL,
    campaign_id     int NOT NULL,
    name            text NOT NULL,
    description     text,
    lineage         text,
    class           text,
    faction         text,
    notes           text,
    created_at      timestamptz NOT NULL DEFAULT now(),
    updated_at      timestamptz NOT NULL DEFAULT now(),
    FOREIGN KEY (account_id) REFERENCES auth.account(id) ON DELETE CASCADE,
    FOREIGN KEY (campaign_id) REFERENCES public.campaign(campaign_id) ON DELETE RESTRICT
);

-- PC table:
CREATE TABLE IF NOT EXISTS public.pc(
    pc_id           int GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    account_id      int NOT NULL,
    campaign_id     int NOT NULL,
    name            text NOT NULL,
    description     text,
    created_at      timestamptz NOT NULL DEFAULT now(),
    updated_at      timestamptz NOT NULL DEFAULT now(),
    FOREIGN KEY (account_id) REFERENCES auth.account(id) ON DELETE CASCADE,
    FOREIGN KEY (campaign_id) REFERENCES public.campaign(campaign_id) ON DELETE RESTRICT
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
    custom_type varchar(100),
    description text,
    strength int CHECK (strength BETWEEN 1 AND 10),
    attitude_score int DEFAULT 0 CHECK (attitude_score BETWEEN -5 AND 5),
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
    ('Acquaintance', 'Casual or passing familiarity', false),
    ('Ally', 'Allied, working together', false),
    ('Child', 'Child-parent relationship', true),
    ('Contact', 'Useful connection or informant', false),
    ('Employee', 'Works for another entity', true),
    ('Employer', 'Employs or commissions another entity', true),
    ('Enemy', 'Hostile relationship', false),
    ('Family', 'Blood relative or adopted kin', false),
    ('Follower', 'Vassal, devotee, or loyal follower', true),
    ('Friend', 'Friendly relationship', false),
    ('Informant', 'Provides intelligence or secret information', true),
    ('Leader', 'Leader or authority figure', true),
    ('Lover', 'Romantic relationship', false),
    ('Member', 'Member of an organization', true),
    ('Mentor', 'Teacher/guide relationship', true),
    ('Parent', 'Parent-child relationship', true),
    ('Patron', 'Sponsor or benefactor', true),
    ('Protege', 'Sponsored or supported by a patron', true),
    ('Rival', 'Competitive relationship', false),
    ('Sibling', 'Brother or sister', false),
    ('Spouse', 'Married or life partner', false),
    ('Stranger', 'No established relationship yet', false),
    ('Student', 'Learner', true),
    ('Vassal', 'Sworn servant or feudal subject', true)
ON CONFLICT (relationship_type_name) DO NOTHING;

-- Set inverse type pairs for directional relationships
UPDATE public.relationship_type SET inverse_type_id = (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Student') WHERE relationship_type_name = 'Mentor';
UPDATE public.relationship_type SET inverse_type_id = (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Mentor') WHERE relationship_type_name = 'Student';
UPDATE public.relationship_type SET inverse_type_id = (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Child') WHERE relationship_type_name = 'Parent';
UPDATE public.relationship_type SET inverse_type_id = (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Parent') WHERE relationship_type_name = 'Child';
UPDATE public.relationship_type SET inverse_type_id = (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Follower') WHERE relationship_type_name = 'Leader';
UPDATE public.relationship_type SET inverse_type_id = (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Leader') WHERE relationship_type_name = 'Follower';
UPDATE public.relationship_type SET inverse_type_id = (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Employee') WHERE relationship_type_name = 'Employer';
UPDATE public.relationship_type SET inverse_type_id = (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Employer') WHERE relationship_type_name = 'Employee';
UPDATE public.relationship_type SET inverse_type_id = (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Protege') WHERE relationship_type_name = 'Patron';
UPDATE public.relationship_type SET inverse_type_id = (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Patron') WHERE relationship_type_name = 'Protege';


-- Seed data
-- Dev user for local development and seed data
INSERT INTO auth.account (username, first_name, last_name, email, cognito_sub, subscription_tier)
VALUES
  ('gm_admin', 'GM', 'Admin', 'admin@gmbuddy.local', 'c801b3d0-3071-709f-ebb9-c69f61e297f5', 'premium')
ON CONFLICT (username) DO NOTHING;

INSERT INTO public.game_system (game_system_name)
VALUES
  ('Dungeons & Dragons (5e)'),
  ('Generic')
ON CONFLICT (game_system_name) DO NOTHING;

-- Insert sample campaigns (generic fantasy content)
INSERT INTO public.campaign (account_id, name, description)
VALUES
  (
    (SELECT id FROM auth.account WHERE username = 'gm_admin' LIMIT 1),
    'Shadows Over Millhaven',
    'A mystery unfolds in the peaceful town of Millhaven as strange occurrences and disappearances plague the locals.'
  ),
  (
    (SELECT id FROM auth.account WHERE username = 'gm_admin' LIMIT 1),
    'The Northern Frontier',
    'An exploration campaign in the untamed wilderness, where adventurers seek fortune and face the dangers of the wild.'
  )
ON CONFLICT DO NOTHING;

-- Insert sample NPCs (generic fantasy characters)
INSERT INTO public.npc (account_id, campaign_id, name, description, lineage, class, faction, notes)
VALUES
  (
    (SELECT id FROM auth.account WHERE username = 'gm_admin' LIMIT 1),
    (SELECT campaign_id FROM public.campaign WHERE name = 'Shadows Over Millhaven' LIMIT 1),
    'Marcus Blackwood',
    'The town magistrate who oversees law and order in Millhaven. Known for his fair judgment and dedication to the community.',
    'Human',
    'Magistrate',
    NULL,
    NULL
  ),
  (
    (SELECT id FROM auth.account WHERE username = 'gm_admin' LIMIT 1),
    (SELECT campaign_id FROM public.campaign WHERE name = 'Shadows Over Millhaven' LIMIT 1),
    'Lyanna Swift',
    'A traveling merchant who deals in rare herbs and alchemical components. She has connections throughout the region.',
    'Half-Elf',
    'Merchant',
    NULL,
    NULL
  ),
  (
    (SELECT id FROM auth.account WHERE username = 'gm_admin' LIMIT 1),
    (SELECT campaign_id FROM public.campaign WHERE name = 'The Northern Frontier' LIMIT 1),
    'Thorgar Stonefist',
    'A seasoned explorer and guide who knows the northern mountains better than anyone.',
    'Dwarf',
    'Guide',
    NULL,
    NULL
  ),
  (
    (SELECT id FROM auth.account WHERE username = 'gm_admin' LIMIT 1),
    (SELECT campaign_id FROM public.campaign WHERE name = 'The Northern Frontier' LIMIT 1),
    'Kael Windrunner',
    'A skilled tracker and hunter who makes a living by trapping and guiding hunting expeditions.',
    'Elf',
    'Tracker',
    NULL,
    NULL
  ),
  (
    (SELECT id FROM auth.account WHERE username = 'gm_admin' LIMIT 1),
    (SELECT campaign_id FROM public.campaign WHERE name = 'The Northern Frontier' LIMIT 1),
    'Red Scar',
    'A notorious bandit leader who controls the mountain passes and demands tribute from travelers.',
    'Half-Orc',
    'Bandit Chief',
    NULL,
    NULL
  )
ON CONFLICT DO NOTHING;

-- Insert sample PCs
INSERT INTO public.pc (account_id, campaign_id, name, description)
VALUES
  (
    (SELECT id FROM auth.account WHERE username = 'gm_admin' LIMIT 1),
    (SELECT campaign_id FROM public.campaign WHERE name = 'Shadows Over Millhaven' LIMIT 1),
    'Thorin Ironforge',
    'A dwarven cleric devoted to Moradin, god of creation and forging.'
  ),
  (
    (SELECT id FROM auth.account WHERE username = 'gm_admin' LIMIT 1),
    (SELECT campaign_id FROM public.campaign WHERE name = 'Shadows Over Millhaven' LIMIT 1),
    'Lyra Shadowstep',
    'A half-elf rogue with a mysterious past and quick fingers.'
  ),
  (
    (SELECT id FROM auth.account WHERE username = 'gm_admin' LIMIT 1),
    (SELECT campaign_id FROM public.campaign WHERE name = 'The Northern Frontier' LIMIT 1),
    'Aldric the Brave',
    'A human paladin sworn to protect the innocent and vanquish evil.'
  ),
  (
    (SELECT id FROM auth.account WHERE username = 'gm_admin' LIMIT 1),
    (SELECT campaign_id FROM public.campaign WHERE name = 'The Northern Frontier' LIMIT 1),
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
    relationship_type_id, description, strength, attitude_score, is_active, campaign_id
)
VALUES
  -- Marcus Blackwood (NPC) is Friends with Lyanna Swift (NPC) in Shadows Over Millhaven
  (
    'npc', (SELECT npc_id FROM public.npc WHERE name = 'Marcus Blackwood' LIMIT 1),
    'npc', (SELECT npc_id FROM public.npc WHERE name = 'Lyanna Swift' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Friend' LIMIT 1),
    'The magistrate trusts the merchant for advice on regional affairs',
    7, 4,
    true,
    (SELECT campaign_id FROM public.campaign WHERE name = 'Shadows Over Millhaven' LIMIT 1)
  ),
  -- Marcus Blackwood (NPC) is Ally with Lyanna Swift (NPC) in Shadows Over Millhaven
  (
    'npc', (SELECT npc_id FROM public.npc WHERE name = 'Marcus Blackwood' LIMIT 1),
    'npc', (SELECT npc_id FROM public.npc WHERE name = 'Lyanna Swift' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Ally' LIMIT 1),
    'Working together to solve the mysteries plaguing Millhaven',
    8, 3,
    true,
    (SELECT campaign_id FROM public.campaign WHERE name = 'Shadows Over Millhaven' LIMIT 1)
  ),
  -- Thorgar Stonefist (NPC) is Enemy of Red Scar (NPC) in The Northern Frontier
  (
    'npc', (SELECT npc_id FROM public.npc WHERE name = 'Thorgar Stonefist' LIMIT 1),
    'npc', (SELECT npc_id FROM public.npc WHERE name = 'Red Scar' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Enemy' LIMIT 1),
    'The guide refuses to pay tribute to the bandit chief',
    9, -4,
    true,
    (SELECT campaign_id FROM public.campaign WHERE name = 'The Northern Frontier' LIMIT 1)
  ),
  -- Kael Windrunner (NPC) is Ally of Thorgar Stonefist (NPC) in The Northern Frontier
  (
    'npc', (SELECT npc_id FROM public.npc WHERE name = 'Kael Windrunner' LIMIT 1),
    'npc', (SELECT npc_id FROM public.npc WHERE name = 'Thorgar Stonefist' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Ally' LIMIT 1),
    'Tracker and guide work together to map the frontier',
    8, 3,
    true,
    (SELECT campaign_id FROM public.campaign WHERE name = 'The Northern Frontier' LIMIT 1)
  ),
  -- Kael Windrunner (NPC) is Enemy of Red Scar (NPC) in The Northern Frontier
  (
    'npc', (SELECT npc_id FROM public.npc WHERE name = 'Kael Windrunner' LIMIT 1),
    'npc', (SELECT npc_id FROM public.npc WHERE name = 'Red Scar' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Enemy' LIMIT 1),
    'The tracker actively hunts the bandit and his crew',
    7, -3,
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
    7, 2,
    true,
    NULL
  ),
  -- Lyra Shadowstep (PC) is Member of The Archivists (Org)
  (
    'pc', (SELECT pc_id FROM public.pc WHERE name = 'Lyra Shadowstep' LIMIT 1),
    'organization', (SELECT organization_id FROM public.organization WHERE name = 'The Archivists' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Member' LIMIT 1),
    'Half-elf rogue working as a secret agent and researcher',
    8, 3,
    true,
    NULL
  ),
  -- Aldric the Brave (PC) is Member of The Regional Alliance (Org)
  (
    'pc', (SELECT pc_id FROM public.pc WHERE name = 'Aldric the Brave' LIMIT 1),
    'organization', (SELECT organization_id FROM public.organization WHERE name = 'The Regional Alliance' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Member' LIMIT 1),
    'Paladin recently inducted into the Alliance',
    6, 1,
    true,
    NULL
  ),
  -- Zephyr Windwhisper (PC) is Member of The Archivists (Org)
  (
    'pc', (SELECT pc_id FROM public.pc WHERE name = 'Zephyr Windwhisper' LIMIT 1),
    'organization', (SELECT organization_id FROM public.organization WHERE name = 'The Archivists' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Member' LIMIT 1),
    'Elven ranger gathering intelligence and preserving lore',
    7, 2,
    true,
    NULL
  ),
  -- Thorin Ironforge (PC) is Ally of Marcus Blackwood (NPC) in Shadows Over Millhaven
  (
    'pc', (SELECT pc_id FROM public.pc WHERE name = 'Thorin Ironforge' LIMIT 1),
    'npc', (SELECT npc_id FROM public.npc WHERE name = 'Marcus Blackwood' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Ally' LIMIT 1),
    'The cleric assists the magistrate in investigating the mysteries',
    7, 2,
    true,
    (SELECT campaign_id FROM public.campaign WHERE name = 'Shadows Over Millhaven' LIMIT 1)
  ),
  -- Lyanna Swift (NPC) is Mentor of Lyra Shadowstep (PC) in Shadows Over Millhaven
  (
    'npc', (SELECT npc_id FROM public.npc WHERE name = 'Lyanna Swift' LIMIT 1),
    'pc', (SELECT pc_id FROM public.pc WHERE name = 'Lyra Shadowstep' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Mentor' LIMIT 1),
    'The merchant taught Lyra about rare herbs and alchemy',
    8, 3,
    true,
    (SELECT campaign_id FROM public.campaign WHERE name = 'Shadows Over Millhaven' LIMIT 1)
  ),
  -- Thorgar Stonefist (NPC) is Mentor of Zephyr Windwhisper (PC) in The Northern Frontier
  (
    'npc', (SELECT npc_id FROM public.npc WHERE name = 'Thorgar Stonefist' LIMIT 1),
    'pc', (SELECT pc_id FROM public.pc WHERE name = 'Zephyr Windwhisper' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Mentor' LIMIT 1),
    'The guide taught the ranger the secrets of the northern mountains',
    7, 2,
    true,
    (SELECT campaign_id FROM public.campaign WHERE name = 'The Northern Frontier' LIMIT 1)
  ),
  -- Aldric the Brave (PC) is Ally of Kael Windrunner (NPC) in The Northern Frontier
  (
    'pc', (SELECT pc_id FROM public.pc WHERE name = 'Aldric the Brave' LIMIT 1),
    'npc', (SELECT npc_id FROM public.npc WHERE name = 'Kael Windrunner' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Ally' LIMIT 1),
    'Paladin and tracker united in protecting travelers from bandits',
    8, 3,
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
    8, -4,
    true,
    NULL
  ),
  -- The Archivists (Org) is Ally of The Regional Alliance (Org)
  (
    'organization', (SELECT organization_id FROM public.organization WHERE name = 'The Archivists' LIMIT 1),
    'organization', (SELECT organization_id FROM public.organization WHERE name = 'The Regional Alliance' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Ally' LIMIT 1),
    'United in promoting stability and opposing tyranny',
    7, 3,
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
    9, 5,
    true,
    NULL
  ),
  -- Aldric the Brave (PC) and Zephyr Windwhisper (PC) are Allies
  (
    'pc', (SELECT pc_id FROM public.pc WHERE name = 'Aldric the Brave' LIMIT 1),
    'pc', (SELECT pc_id FROM public.pc WHERE name = 'Zephyr Windwhisper' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Ally' LIMIT 1),
    'United in their quest to protect the innocent and fight evil',
    8, 3,
    true,
    NULL
  ),
  -- Thorin Ironforge (PC) is Member of Stonepeak Mining Consortium (Org)
  (
    'pc', (SELECT pc_id FROM public.pc WHERE name = 'Thorin Ironforge' LIMIT 1),
    'organization', (SELECT organization_id FROM public.organization WHERE name = 'Stonepeak Mining Consortium' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Member' LIMIT 1),
    'Fellow dwarf invested in the mining consortium ventures',
    6, 1,
    true,
    NULL
  )
ON CONFLICT DO NOTHING;

-- Game System Table (Technical Debt)
-- Note: Reference lineage and occupation tables removed as part of GM-108.
-- Game systems table is retained for potential future use but is not currently
-- referenced by campaigns, NPCs, PCs, or organizations.

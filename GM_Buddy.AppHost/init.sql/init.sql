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
    strength int DEFAULT 0 CHECK (strength BETWEEN 0 AND 10),
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
  ('gm_admin', 'GM', 'Admin', 'admin@gmbuddy.local', '5821f310-a001-70b7-2c0a-cb1483564fbe', 'premium')
ON CONFLICT (username) DO NOTHING;

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

-- =============================================================
-- Demo Account Seed Script
-- Cognito sub: 189113d0-e051-706c-6261-9717745d6070
-- Email: gmbuddy@outlook.com
-- =============================================================

BEGIN;

-- Create the demo account (skip if already exists)
INSERT INTO auth.account (cognito_sub, email, username, subscription_tier)
VALUES (
    '189113d0-e051-706c-6261-9717745d6070',
    'gmbuddy@outlook.com',
    'demo',
    'premium'
)
ON CONFLICT (cognito_sub) DO NOTHING;

DO $$
DECLARE
    v_account_id  INTEGER;
    v_campaign_id INTEGER;

    -- PC IDs
    v_mira_id    INTEGER;
    v_caelan_id  INTEGER;
    v_tessa_id   INTEGER;
    v_dusk_id    INTEGER;

    -- NPC IDs
    v_vane_id      INTEGER;
    v_seraphine_id INTEGER;
    v_gretch_id    INTEGER;
    v_rowena_id    INTEGER;
    v_harro_id     INTEGER;
    v_whisper_id   INTEGER;
    v_elara_id     INTEGER;
    v_sylvara_id   INTEGER;
    v_brennan_id   INTEGER;
    v_aldus_id     INTEGER;

    -- Relationship type IDs (from public.relationship_type)
    RT_ACQUAINTANCE CONSTANT INTEGER := 1;
    RT_ALLY         CONSTANT INTEGER := 2;
    RT_CONTACT      CONSTANT INTEGER := 4;
    RT_EMPLOYEE     CONSTANT INTEGER := 5;
    RT_EMPLOYER     CONSTANT INTEGER := 6;
    RT_ENEMY        CONSTANT INTEGER := 7;
    RT_FRIEND       CONSTANT INTEGER := 10;
    RT_INFORMANT    CONSTANT INTEGER := 11;
    RT_MENTOR       CONSTANT INTEGER := 15;
    RT_PATRON       CONSTANT INTEGER := 17;
    RT_RIVAL        CONSTANT INTEGER := 19;
BEGIN
    SELECT id INTO v_account_id
    FROM auth.account
    WHERE cognito_sub = '189113d0-e051-706c-6261-9717745d6070';

    -- ===========================
    -- CAMPAIGN
    -- ===========================
    INSERT INTO public.campaign (account_id, name, description)
    VALUES (
        v_account_id,
        'Thornveil Unraveled — Demo',
        'A city on the edge of corruption. The players pull at threads in the ancient port city of Thornveil, where the line between law and crime disappeared long ago — if it ever existed at all.'
    )
    RETURNING campaign_id INTO v_campaign_id;

    -- ===========================
    -- PLAYER CHARACTERS
    -- ===========================
    INSERT INTO public.pc (account_id, campaign_id, name, description)
    VALUES (v_account_id, v_campaign_id, 'Mira Ashford',
        'A cunning rogue who grew up in Thornveil''s lower district. Sharp instincts and a network of street contacts make her indispensable — and unpredictable.')
    RETURNING pc_id INTO v_mira_id;

    INSERT INTO public.pc (account_id, campaign_id, name, description)
    VALUES (v_account_id, v_campaign_id, 'Caelan Stormwright',
        'A scholarly mage driven by a quiet obsession: finding his missing mentor. Methodical and reserved, he notices everything and says little.')
    RETURNING pc_id INTO v_caelan_id;

    INSERT INTO public.pc (account_id, campaign_id, name, description)
    VALUES (v_account_id, v_campaign_id, 'Tessa Ironveil',
        'A stalwart paladin whose faith is tested daily by the corruption she witnesses in Thornveil''s institutions. She has not broken yet, but she is bending.')
    RETURNING pc_id INTO v_tessa_id;

    INSERT INTO public.pc (account_id, campaign_id, name, description)
    VALUES (v_account_id, v_campaign_id, 'Dusk',
        'A wandering ranger who knows the wilderness surrounding Thornveil better than its streets. Carries a past they don''t discuss. Prefers the company of trees.')
    RETURNING pc_id INTO v_dusk_id;

    -- ===========================
    -- NON-PLAYER CHARACTERS
    -- ===========================
    INSERT INTO public.npc (account_id, campaign_id, name, description, lineage, class, faction, notes)
    VALUES (v_account_id, v_campaign_id, 'Lord Aldric Vane',
        'The city''s magistrate and its shadow ruler. Charming at court and ruthless behind closed doors, he controls Thornveil''s criminal networks through carefully insulated proxies.',
        'Human', 'Magistrate / Schemer', 'City Council',
        'Primary antagonist. Has informants everywhere. The party should assume anything they say near a city official reaches him.')
    RETURNING npc_id INTO v_vane_id;

    INSERT INTO public.npc (account_id, campaign_id, name, description, lineage, class, faction, notes)
    VALUES (v_account_id, v_campaign_id, 'Seraphine Vale',
        'Innkeeper of The Tarnished Flagon. She has seen everything and chosen to remember very little — on purpose. Fiercely protective of those she considers her own.',
        'Human', 'Innkeeper', 'Neutral',
        'The party''s unofficial home base. Will shelter them without question. Do not repay that trust poorly.')
    RETURNING npc_id INTO v_seraphine_id;

    INSERT INTO public.npc (account_id, campaign_id, name, description, lineage, class, faction, notes)
    VALUES (v_account_id, v_campaign_id, 'Gretch',
        'Scarred crime lord who controls the docks district with patience and force in equal measure. Practical and unsentimental, but he respects those who deal straight.',
        'Half-Orc', 'Crime Lord', 'The Dockside Syndicate',
        'In an uneasy truce with Vane — for now. Could become a powerful, if uncomfortable, ally if Vane overreaches.')
    RETURNING npc_id INTO v_gretch_id;

    INSERT INTO public.npc (account_id, campaign_id, name, description, lineage, class, faction, notes)
    VALUES (v_account_id, v_campaign_id, 'Sister Rowena',
        'Compassionate healer at the Temple of the Lantern. Knows more about Thornveil''s dark history than she reveals. Carries knowledge like a burden so others don''t have to.',
        'Human', 'Cleric', 'Temple of the Lantern',
        'Can be trusted. Has access to temple records predating the current council. Share information with her slowly and she will share back.')
    RETURNING npc_id INTO v_rowena_id;

    INSERT INTO public.npc (account_id, campaign_id, name, description, lineage, class, faction, notes)
    VALUES (v_account_id, v_campaign_id, 'Captain Harro Thane',
        'City guard captain who appears loyal to Vane and is quietly building a case against him. Walks a razor''s edge every single day.',
        'Human', 'Guard Captain', 'City Guard',
        'Potential key ally. Do not approach openly — Vane has eyes on him. Use intermediaries. His patience is running out.')
    RETURNING npc_id INTO v_harro_id;

    INSERT INTO public.npc (account_id, campaign_id, name, description, lineage, class, faction, notes)
    VALUES (v_account_id, v_campaign_id, 'The Whisper',
        'An elusive information broker whose true identity is unknown. Operates through dead drops and intermediaries. Has never been seen twice in the same guise.',
        'Unknown', 'Information Broker', 'Independent',
        'Sells to anyone who can pay. Has never been wrong. Price is always steeper than it first appears.')
    RETURNING npc_id INTO v_whisper_id;

    INSERT INTO public.npc (account_id, campaign_id, name, description, lineage, class, faction, notes)
    VALUES (v_account_id, v_campaign_id, 'Elara Dawnforge',
        'Master blacksmith trapped by debts owed to Gretch. Brilliant, proud, and quietly desperate to buy her way free.',
        'Dwarf', 'Blacksmith / Artisan', 'The Dockside Syndicate',
        'Can craft or acquire most equipment. Her loyalty follows whoever frees her from Gretch''s ledger.')
    RETURNING npc_id INTO v_elara_id;

    INSERT INTO public.npc (account_id, campaign_id, name, description, lineage, class, faction, notes)
    VALUES (v_account_id, v_campaign_id, 'Sylvara',
        'A figure who always seems to know things before they happen. Speaks in half-truths. Her agenda is entirely her own and she has no intention of explaining it.',
        'Unknown', 'Seer', 'Unknown',
        'Do not trust blindly. Do not dismiss. Her warnings have saved lives. Her motives remain opaque.')
    RETURNING npc_id INTO v_sylvara_id;

    INSERT INTO public.npc (account_id, campaign_id, name, description, lineage, class, faction, notes)
    VALUES (v_account_id, v_campaign_id, 'Brennan Cole',
        'A retired adventurer running a pawnshop called Second Chances. Serves as a fence. Gruff, fair, and loyal to those who treat him with respect.',
        'Human', 'Fence / Former Adventurer', 'Neutral',
        'Good source of gear, coin, and rumors. Has a soft spot for adventurers — reminds him of who he used to be.')
    RETURNING npc_id INTO v_brennan_id;

    -- Backstory NPC — connects ONLY to Caelan
    INSERT INTO public.npc (account_id, campaign_id, name, description, lineage, class, faction, notes)
    VALUES (v_account_id, v_campaign_id, 'Aldus Greymantle',
        'Caelan''s former mentor and a master arcanist of considerable renown. Vanished months ago after uncovering something connected to Vane''s past. Currently in hiding and afraid.',
        'Human', 'Archmage', 'Independent',
        'Caelan''s primary personal quest hook. Finding him may be the key to everything. He is alive. He does not want to be found — but he needs to be.')
    RETURNING npc_id INTO v_aldus_id;

    -- ===========================
    -- RELATIONSHIPS
    -- ===========================

    -- Lord Vane → all PCs (enemy)
    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, strength, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_vane_id, 'pc', v_mira_id, RT_ENEMY, 'Vane''s agents have been watching Mira since she got too close to his dock operations. She is a loose thread he intends to cut.', 7, -4, true, v_campaign_id);

    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, strength, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_vane_id, 'pc', v_caelan_id, RT_ENEMY, 'Vane suspects Caelan is searching for Greymantle and knows that if he finds him, old secrets surface.', 8, -5, true, v_campaign_id);

    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, strength, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_vane_id, 'pc', v_tessa_id, RT_ENEMY, 'Tessa''s investigation into civic corruption is the most direct threat to Vane''s position. He considers her the most dangerous of the group.', 9, -5, true, v_campaign_id);

    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, strength, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_vane_id, 'pc', v_dusk_id, RT_RIVAL, 'Dusk witnessed something in the wilderness outside the city that connects to Vane. Vane wants that memory buried.', 5, -3, true, v_campaign_id);

    -- Seraphine → all PCs (friend / ally)
    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, strength, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_seraphine_id, 'pc', v_mira_id, RT_FRIEND, 'Has known Mira since she was a street kid stealing scraps near the Flagon. Treats her like a wayward daughter.', 9, 5, true, v_campaign_id);

    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, strength, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_seraphine_id, 'pc', v_caelan_id, RT_FRIEND, 'Rents him a room and feeds him when he forgets to eat. Motherly exasperation, genuine warmth.', 7, 4, true, v_campaign_id);

    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, strength, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_seraphine_id, 'pc', v_tessa_id, RT_ALLY, 'Respects Tessa''s convictions. Passes along useful gossip from patrons without being asked.', 6, 4, true, v_campaign_id);

    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, strength, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_seraphine_id, 'pc', v_dusk_id, RT_FRIEND, 'Dusk''s quiet nature suits her fine. She leaves a plate out and asks no questions.', 6, 3, true, v_campaign_id);

    -- Gretch → PCs (varied)
    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, strength, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_gretch_id, 'pc', v_mira_id, RT_EMPLOYER, 'Has hired Mira for jobs before. Respects her skill. Would not hurt her unless cornered into it.', 6, 1, true, v_campaign_id);

    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, strength, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_gretch_id, 'pc', v_caelan_id, RT_CONTACT, 'Gretch once needed a mage. Caelan helped. Gretch owes him a favor, which puts Gretch in an uncomfortable position.', 4, 1, true, v_campaign_id);

    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, strength, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_gretch_id, 'pc', v_tessa_id, RT_ENEMY, 'Tessa has disrupted two of his operations. He considers her a problem to manage rather than escalate — for now.', 5, -3, true, v_campaign_id);

    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, strength, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_gretch_id, 'pc', v_dusk_id, RT_ALLY, 'Dusk helped track a smuggler through the wilds outside the city. Gretch respects competence above almost everything.', 5, 2, true, v_campaign_id);

    -- Sister Rowena → all PCs (ally / friend)
    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, strength, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_rowena_id, 'pc', v_mira_id, RT_ALLY, 'Has patched Mira up more times than she can count. Offers care without judgment.', 6, 4, true, v_campaign_id);

    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, strength, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_rowena_id, 'pc', v_caelan_id, RT_ALLY, 'Shares temple records with Caelan carefully — one page at a time — as trust is built.', 7, 3, true, v_campaign_id);

    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, strength, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_rowena_id, 'pc', v_tessa_id, RT_FRIEND, 'Sees Tessa''s faith as a mirror of her own — and a warning of what unchecked zeal can become.', 8, 5, true, v_campaign_id);

    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, strength, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_rowena_id, 'pc', v_dusk_id, RT_ALLY, 'Has helped Dusk heal wounds that were more than physical. Speaks little, listens completely.', 5, 3, true, v_campaign_id);

    -- Captain Harro → all PCs (informant / contact / ally)
    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, strength, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_harro_id, 'pc', v_mira_id, RT_CONTACT, 'Turns a blind eye to Mira''s activities in exchange for intelligence on Syndicate movements.', 5, 2, true, v_campaign_id);

    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, strength, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_harro_id, 'pc', v_caelan_id, RT_INFORMANT, 'Passed Caelan one clue about Greymantle''s disappearance. Does not know how much Caelan already knows.', 4, 2, true, v_campaign_id);

    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, strength, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_harro_id, 'pc', v_tessa_id, RT_ALLY, 'Secretly hopes Tessa will expose Vane so he does not have to. Feeds her evidence with trembling caution.', 7, 3, true, v_campaign_id);

    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, strength, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_harro_id, 'pc', v_dusk_id, RT_CONTACT, 'Has hired Dusk twice as an unofficial scout beyond the city walls. Pays from his own coin.', 5, 2, true, v_campaign_id);

    -- The Whisper → all PCs (informant / contact)
    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, strength, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_whisper_id, 'pc', v_mira_id, RT_CONTACT, 'A professional arrangement built on mutual usefulness. Information for coin — or favors owed.', 6, 0, true, v_campaign_id);

    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, strength, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_whisper_id, 'pc', v_caelan_id, RT_INFORMANT, 'Has sold Caelan fragments of information about Greymantle. Always wants more than coin in return.', 5, 0, true, v_campaign_id);

    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, strength, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_whisper_id, 'pc', v_tessa_id, RT_CONTACT, 'Reached out to Tessa once with a warning she did not ask for. Has not explained the reason.', 4, 1, true, v_campaign_id);

    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, strength, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_whisper_id, 'pc', v_dusk_id, RT_ACQUAINTANCE, 'Met Dusk once in the wilderness. Already knew who they were. Left before questions could be asked.', 3, 0, true, v_campaign_id);

    -- Elara Dawnforge → all PCs (contact / ally)
    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, strength, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_elara_id, 'pc', v_mira_id, RT_CONTACT, 'Provides custom blades and asks no questions about how they are used. Strictly professional.', 6, 2, true, v_campaign_id);

    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, strength, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_elara_id, 'pc', v_caelan_id, RT_CONTACT, 'Crafts enchantment-ready components for Caelan. Fascinated by his arcane work despite herself.', 5, 2, true, v_campaign_id);

    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, strength, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_elara_id, 'pc', v_tessa_id, RT_FRIEND, 'Repaired Tessa''s armor after a brutal fight and refused payment. Quietly admires her principles.', 6, 3, true, v_campaign_id);

    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, strength, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_elara_id, 'pc', v_dusk_id, RT_CONTACT, 'Supplies arrows and trail gear. The two share a mutual appreciation for silence and clean work.', 5, 2, true, v_campaign_id);

    -- Sylvara → all PCs (mysterious contact)
    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, strength, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_sylvara_id, 'pc', v_mira_id, RT_CONTACT, 'Warned Mira about an ambush before it happened. Mira doesn''t know what to make of her — which is exactly how Sylvara prefers it.', 4, 1, true, v_campaign_id);

    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, strength, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_sylvara_id, 'pc', v_caelan_id, RT_ALLY, 'Has appeared to Caelan in moments of magical crisis. Seems drawn to his arcane presence for reasons she has not shared.', 6, 2, true, v_campaign_id);

    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, strength, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_sylvara_id, 'pc', v_tessa_id, RT_CONTACT, 'Passed cryptic warnings to Tessa about a darkness within the upper temple hierarchy. Has not elaborated.', 5, 1, true, v_campaign_id);

    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, strength, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_sylvara_id, 'pc', v_dusk_id, RT_ALLY, 'Dusk has encountered Sylvara in the wilderness three times. Each time, something dangerous was narrowly avoided. Coincidence is a word Dusk no longer uses.', 7, 2, true, v_campaign_id);

    -- Brennan Cole → all PCs (friend / contact)
    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, strength, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_brennan_id, 'pc', v_mira_id, RT_ALLY, 'Buys and sells through Mira regularly. Gives fair cuts and useful rumors. Reliable.', 7, 3, true, v_campaign_id);

    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, strength, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_brennan_id, 'pc', v_caelan_id, RT_CONTACT, 'Has sourced rare books and arcane components at below-market prices. Enjoys the intellectual company.', 5, 2, true, v_campaign_id);

    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, strength, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_brennan_id, 'pc', v_tessa_id, RT_FRIEND, 'Tessa once recovered something precious he had lost. He has been quietly useful to her cause ever since.', 6, 3, true, v_campaign_id);

    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, strength, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_brennan_id, 'pc', v_dusk_id, RT_ACQUAINTANCE, 'Fellow traveler, different roads. Brennan respects Dusk''s self-reliance. They share a drink occasionally and leave it at that.', 4, 2, true, v_campaign_id);

    -- Aldus Greymantle → ONLY Caelan (mentor — backstory connection)
    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, strength, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_aldus_id, 'pc', v_caelan_id, RT_MENTOR,
        'Aldus trained Caelan from adolescence and is the closest thing to a father he has known. His disappearance is what brought Caelan to Thornveil. Whatever Aldus uncovered, he believed it was worth vanishing for.',
        10, 5, true, v_campaign_id);

    RAISE NOTICE 'Demo seed complete. Account ID: %, Campaign ID: %', v_account_id, v_campaign_id;

END $$;

COMMIT;

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
    relationship_type_id, description, attitude_score, is_active, campaign_id
)
VALUES
  -- Marcus Blackwood (NPC) is Friends with Lyanna Swift (NPC) in Shadows Over Millhaven
  (
    'npc', (SELECT npc_id FROM public.npc WHERE name = 'Marcus Blackwood' LIMIT 1),
    'npc', (SELECT npc_id FROM public.npc WHERE name = 'Lyanna Swift' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Friend' LIMIT 1),
    'The magistrate trusts the merchant for advice on regional affairs',
    4,
    true,
    (SELECT campaign_id FROM public.campaign WHERE name = 'Shadows Over Millhaven' LIMIT 1)
  ),
  -- Marcus Blackwood (NPC) is Ally with Lyanna Swift (NPC) in Shadows Over Millhaven
  (
    'npc', (SELECT npc_id FROM public.npc WHERE name = 'Marcus Blackwood' LIMIT 1),
    'npc', (SELECT npc_id FROM public.npc WHERE name = 'Lyanna Swift' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Ally' LIMIT 1),
    'Working together to solve the mysteries plaguing Millhaven',
    3,
    true,
    (SELECT campaign_id FROM public.campaign WHERE name = 'Shadows Over Millhaven' LIMIT 1)
  ),
  -- Thorgar Stonefist (NPC) is Enemy of Red Scar
  (
    'npc', (SELECT npc_id FROM public.npc WHERE name = 'Thorgar Stonefist' LIMIT 1),
    'npc', (SELECT npc_id FROM public.npc WHERE name = 'Red Scar' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Enemy' LIMIT 1),
    'The guide refuses to pay tribute to the bandit chief',
    -4,
    true,
    (SELECT campaign_id FROM public.campaign WHERE name = 'The Northern Frontier' LIMIT 1)
  ),
  -- Kael Windrunner (NPC) is Ally of Thorgar Stonefist (NPC) in The Northern Frontier
  (
    'npc', (SELECT npc_id FROM public.npc WHERE name = 'Kael Windrunner' LIMIT 1),
    'npc', (SELECT npc_id FROM public.npc WHERE name = 'Thorgar Stonefist' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Ally' LIMIT 1),
    'Tracker and guide work together to map the frontier',
    3,
    true,
    (SELECT campaign_id FROM public.campaign WHERE name = 'The Northern Frontier' LIMIT 1)
  ),
  -- Kael Windrunner (NPC) is Enemy of Red Scar (NPC) in The Northern Frontier
  (
    'npc', (SELECT npc_id FROM public.npc WHERE name = 'Kael Windrunner' LIMIT 1),
    'npc', (SELECT npc_id FROM public.npc WHERE name = 'Red Scar' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Enemy' LIMIT 1),
    'The tracker actively hunts the bandit and his crew',
    -3,
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
    2,
    true,
    NULL
  ),
  -- Lyra Shadowstep (PC) is Member of The Archivists
  (
    'pc', (SELECT pc_id FROM public.pc WHERE name = 'Lyra Shadowstep' LIMIT 1),
    'organization', (SELECT organization_id FROM public.organization WHERE name = 'The Archivists' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Member' LIMIT 1),
    'Half-elf rogue working as a secret agent and researcher',
    3,
    true,
    NULL
  ),
  -- Aldric the Brave (PC) is Member of The Regional Alliance
  (
    'pc', (SELECT pc_id FROM public.pc WHERE name = 'Aldric the Brave' LIMIT 1),
    'organization', (SELECT organization_id FROM public.organization WHERE name = 'The Regional Alliance' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Member' LIMIT 1),
    'Paladin recently inducted into the Alliance',
    1,
    true,
    NULL
  ),
  -- Zephyr Windwhisper (PC) is Member of The Archivists (Org)
  (
    'pc', (SELECT pc_id FROM public.pc WHERE name = 'Zephyr Windwhisper' LIMIT 1),
    'organization', (SELECT organization_id FROM public.organization WHERE name = 'The Archivists' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Member' LIMIT 1),
    'Elven ranger gathering intelligence and preserving lore',
    2,
    true,
    NULL
  ),
  -- Thorin Ironforge (PC) is Ally of Marcus Blackwood
  (
    'pc', (SELECT pc_id FROM public.pc WHERE name = 'Thorin Ironforge' LIMIT 1),
    'npc', (SELECT npc_id FROM public.npc WHERE name = 'Marcus Blackwood' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Ally' LIMIT 1),
    'The cleric assists the magistrate in investigating the mysteries',
    2,
    true,
    (SELECT campaign_id FROM public.campaign WHERE name = 'Shadows Over Millhaven' LIMIT 1)
  ),
  -- Lyanna Swift (NPC) is Mentor of Lyra Shadowstep (PC) in Shadows Over Millhaven
  (
    'npc', (SELECT npc_id FROM public.npc WHERE name = 'Lyanna Swift' LIMIT 1),
    'pc', (SELECT pc_id FROM public.pc WHERE name = 'Lyra Shadowstep' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Mentor' LIMIT 1),
    'The merchant taught Lyra about rare herbs and alchemy',
    3,
    true,
    (SELECT campaign_id FROM public.campaign WHERE name = 'Shadows Over Millhaven' LIMIT 1)
  ),
  -- Thorgar Stonefist (NPC) is Mentor of Zephyr Windwhisper (PC) in The Northern Frontier
  (
    'npc', (SELECT npc_id FROM public.npc WHERE name = 'Thorgar Stonefist' LIMIT 1),
    'pc', (SELECT pc_id FROM public.pc WHERE name = 'Zephyr Windwhisper' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Mentor' LIMIT 1),
    'The guide taught the ranger the secrets of the northern mountains',
    2,
    true,
    (SELECT campaign_id FROM public.campaign WHERE name = 'The Northern Frontier' LIMIT 1)
  ),
  -- Aldric the Brave (PC) is Ally of Kael Windrunner (NPC) in The Northern Frontier
  (
    'pc', (SELECT pc_id FROM public.pc WHERE name = 'Aldric the Brave' LIMIT 1),
    'npc', (SELECT npc_id FROM public.npc WHERE name = 'Kael Windrunner' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Ally' LIMIT 1),
    'Paladin and tracker united in protecting travelers from bandits',
    3,
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
    -4,
    true,
    NULL
  ),
  -- The Archivists (Org) is Ally of The Regional Alliance (Org)
  (
    'organization', (SELECT organization_id FROM public.organization WHERE name = 'The Archivists' LIMIT 1),
    'organization', (SELECT organization_id FROM public.organization WHERE name = 'The Regional Alliance' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Ally' LIMIT 1),
    'United in promoting stability and opposing tyranny',
    3,
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
    5,
    true,
    NULL
  ),
  -- Aldric the Brave (PC) and Zephyr Windwhisper (PC) are Allies
  (
    'pc', (SELECT pc_id FROM public.pc WHERE name = 'Aldric the Brave' LIMIT 1),
    'pc', (SELECT pc_id FROM public.pc WHERE name = 'Zephyr Windwhisper' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Ally' LIMIT 1),
    'United in their quest to protect the innocent and fight evil',
    3,
    true,
    NULL
  ),
  -- Thorin Ironforge (PC) is Member of Stonepeak Mining Consortium (Org)
  (
    'pc', (SELECT pc_id FROM public.pc WHERE name = 'Thorin Ironforge' LIMIT 1),
    'organization', (SELECT organization_id FROM public.organization WHERE name = 'Stonepeak Mining Consortium' LIMIT 1),
    (SELECT relationship_type_id FROM public.relationship_type WHERE relationship_type_name = 'Member' LIMIT 1),
    'Fellow dwarf invested in the mining consortium ventures',
    1,
    true,
    NULL
  )
ON CONFLICT DO NOTHING;

-- =============================================================
-- Demo Account Seed Script
-- Cognito sub: 189113d0-e051-706c-6261-9717745d6070
-- Email: gmbuddy@outlook.com
-- =============================================================

BEGIN;

-- Create the demo account (skip if already exists on any unique column)
INSERT INTO auth.account (cognito_sub, email, username, subscription_tier)
SELECT
    '189113d0-e051-706c-6261-9717745d6070',
    'gmbuddy@outlook.com',
    'demo',
    'premium'
WHERE NOT EXISTS (
    SELECT 1
    FROM auth.account
    WHERE cognito_sub = '189113d0-e051-706c-6261-9717745d6070'
       OR email = 'gmbuddy@outlook.com'
       OR username = 'demo'
);

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

    -- Relationship type IDs (resolved by name to avoid hardcoded identity PK assumptions)
    RT_ACQUAINTANCE INTEGER;
    RT_ALLY         INTEGER;
    RT_CONTACT      INTEGER;
    RT_EMPLOYEE     INTEGER;
    RT_EMPLOYER     INTEGER;
    RT_ENEMY        INTEGER;
    RT_FRIEND       INTEGER;
    RT_INFORMANT    INTEGER;
    RT_MENTOR       INTEGER;
    RT_PATRON       INTEGER;
    RT_RIVAL        INTEGER;
BEGIN
    SELECT id INTO v_account_id
    FROM auth.account
    WHERE cognito_sub = '189113d0-e051-706c-6261-9717745d6070';

    SELECT relationship_type_id INTO RT_ACQUAINTANCE FROM public.relationship_type WHERE relationship_type_name = 'Acquaintance';
    SELECT relationship_type_id INTO RT_ALLY         FROM public.relationship_type WHERE relationship_type_name = 'Ally';
    SELECT relationship_type_id INTO RT_CONTACT      FROM public.relationship_type WHERE relationship_type_name = 'Contact';
    SELECT relationship_type_id INTO RT_EMPLOYEE     FROM public.relationship_type WHERE relationship_type_name = 'Employee';
    SELECT relationship_type_id INTO RT_EMPLOYER     FROM public.relationship_type WHERE relationship_type_name = 'Employer';
    SELECT relationship_type_id INTO RT_ENEMY        FROM public.relationship_type WHERE relationship_type_name = 'Enemy';
    SELECT relationship_type_id INTO RT_FRIEND       FROM public.relationship_type WHERE relationship_type_name = 'Friend';
    SELECT relationship_type_id INTO RT_INFORMANT    FROM public.relationship_type WHERE relationship_type_name = 'Informant';
    SELECT relationship_type_id INTO RT_MENTOR       FROM public.relationship_type WHERE relationship_type_name = 'Mentor';
    SELECT relationship_type_id INTO RT_PATRON       FROM public.relationship_type WHERE relationship_type_name = 'Patron';
    SELECT relationship_type_id INTO RT_RIVAL        FROM public.relationship_type WHERE relationship_type_name = 'Rival';

    -- Skip if demo data already exists
    IF NOT EXISTS (
        SELECT 1 FROM public.campaign
        WHERE account_id = v_account_id
          AND name = 'Thornveil Unraveled — Demo'
    ) THEN

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
    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_vane_id, 'pc', v_mira_id, RT_ENEMY, 'Vane''s agents have been watching Mira since she got too close to his dock operations. She is a loose thread he intends to cut.', -4, true, v_campaign_id)
    ON CONFLICT (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, campaign_id) DO NOTHING;

    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_vane_id, 'pc', v_caelan_id, RT_ENEMY, 'Vane suspects Caelan is searching for Greymantle and knows that if he finds him, old secrets surface.', -5, true, v_campaign_id)
    ON CONFLICT (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, campaign_id) DO NOTHING;

    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_vane_id, 'pc', v_tessa_id, RT_ENEMY, 'Tessa''s investigation into civic corruption is the most direct threat to Vane''s position. He considers her the most dangerous of the group.', -5, true, v_campaign_id)
    ON CONFLICT (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, campaign_id) DO NOTHING;

    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_vane_id, 'pc', v_dusk_id, RT_RIVAL, 'Dusk witnessed something in the wilderness outside the city that connects to Vane. Vane wants that memory buried.', -3, true, v_campaign_id)
    ON CONFLICT (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, campaign_id) DO NOTHING;

    -- Seraphine → all PCs (friend / ally)
    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_seraphine_id, 'pc', v_mira_id, RT_FRIEND, 'Has known Mira since she was a street kid stealing scraps near the Flagon. Treats her like a wayward daughter.', 5, true, v_campaign_id)
    ON CONFLICT (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, campaign_id) DO NOTHING;

    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_seraphine_id, 'pc', v_caelan_id, RT_FRIEND, 'Rents him a room and feeds him when he forgets to eat. Motherly exasperation, genuine warmth.', 4, true, v_campaign_id)
    ON CONFLICT (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, campaign_id) DO NOTHING;

    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_seraphine_id, 'pc', v_tessa_id, RT_ALLY, 'Respects Tessa''s convictions. Passes along useful gossip from patrons without being asked.', 4, true, v_campaign_id)
    ON CONFLICT (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, campaign_id) DO NOTHING;

    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_seraphine_id, 'pc', v_dusk_id, RT_FRIEND, 'Dusk''s quiet nature suits her fine. She leaves a plate out and asks no questions.', 3, true, v_campaign_id)
    ON CONFLICT (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, campaign_id) DO NOTHING;

    -- Gretch → PCs (varied)
    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_gretch_id, 'pc', v_mira_id, RT_EMPLOYER, 'Has hired Mira for jobs before. Respects her skill. Would not hurt her unless cornered into it.', 1, true, v_campaign_id)
    ON CONFLICT (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, campaign_id) DO NOTHING;

    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_gretch_id, 'pc', v_caelan_id, RT_CONTACT, 'Gretch once needed a mage. Caelan helped. Gretch owes him a favor, which puts Gretch in an uncomfortable position.', 1, true, v_campaign_id)
    ON CONFLICT (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, campaign_id) DO NOTHING;

    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_gretch_id, 'pc', v_tessa_id, RT_ENEMY, 'Tessa has disrupted two of his operations. He considers her a problem to manage rather than escalate — for now.', -3, true, v_campaign_id)
    ON CONFLICT (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, campaign_id) DO NOTHING;

    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_gretch_id, 'pc', v_dusk_id, RT_ALLY, 'Dusk helped track a smuggler through the wilds outside the city. Gretch respects competence above almost everything.', 2, true, v_campaign_id)
    ON CONFLICT (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, campaign_id) DO NOTHING;

    -- Sister Rowena → all PCs (ally / friend)
    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_rowena_id, 'pc', v_mira_id, RT_ALLY, 'Has patched Mira up more times than she can count. Offers care without judgment.', 4, true, v_campaign_id)
    ON CONFLICT (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, campaign_id) DO NOTHING;

    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_rowena_id, 'pc', v_caelan_id, RT_ALLY, 'Shares temple records with Caelan carefully — one page at a time — as trust is built.', 3, true, v_campaign_id)
    ON CONFLICT (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, campaign_id) DO NOTHING;

    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_rowena_id, 'pc', v_tessa_id, RT_FRIEND, 'Sees Tessa''s faith as a mirror of her own — and a warning of what unchecked zeal can become.', 5, true, v_campaign_id)
    ON CONFLICT (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, campaign_id) DO NOTHING;

    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_rowena_id, 'pc', v_dusk_id, RT_ALLY, 'Has helped Dusk heal wounds that were more than physical. Speaks little, listens completely.', 3, true, v_campaign_id)
    ON CONFLICT (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, campaign_id) DO NOTHING;

    -- Captain Harro → all PCs (informant / contact / ally)
    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_harro_id, 'pc', v_mira_id, RT_CONTACT, 'Turns a blind eye to Mira''s activities in exchange for intelligence on Syndicate movements.', 2, true, v_campaign_id)
    ON CONFLICT (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, campaign_id) DO NOTHING;

    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_harro_id, 'pc', v_caelan_id, RT_INFORMANT, 'Passed Caelan one clue about Greymantle''s disappearance. Does not know how much Caelan already knows.', 2, true, v_campaign_id)
    ON CONFLICT (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, campaign_id) DO NOTHING;

    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_harro_id, 'pc', v_tessa_id, RT_ALLY, 'Secretly hopes Tessa will expose Vane so he does not have to. Feeds her evidence with trembling caution.', 3, true, v_campaign_id)
    ON CONFLICT (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, campaign_id) DO NOTHING;

    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_harro_id, 'pc', v_dusk_id, RT_CONTACT, 'Has hired Dusk twice as an unofficial scout beyond the city walls. Pays from his own coin.', 2, true, v_campaign_id)
    ON CONFLICT (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, campaign_id) DO NOTHING;

    -- The Whisper → all PCs (informant / contact)
    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_whisper_id, 'pc', v_mira_id, RT_CONTACT, 'A professional arrangement built on mutual usefulness. Information for coin — or favors owed.', 0, true, v_campaign_id)
    ON CONFLICT (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, campaign_id) DO NOTHING;

    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_whisper_id, 'pc', v_caelan_id, RT_INFORMANT, 'Has sold Caelan fragments of information about Greymantle. Always wants more than coin in return.', 0, true, v_campaign_id)
    ON CONFLICT (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, campaign_id) DO NOTHING;

    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_whisper_id, 'pc', v_tessa_id, RT_CONTACT, 'Reached out to Tessa once with a warning she did not ask for. Has not explained the reason.', 1, true, v_campaign_id)
    ON CONFLICT (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, campaign_id) DO NOTHING;

    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_whisper_id, 'pc', v_dusk_id, RT_ACQUAINTANCE, 'Met Dusk once in the wilderness. Already knew who they were. Left before questions could be asked.', 0, true, v_campaign_id)
    ON CONFLICT (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, campaign_id) DO NOTHING;

    -- Elara Dawnforge → all PCs (contact / ally)
    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_elara_id, 'pc', v_mira_id, RT_CONTACT, 'Provides custom blades and asks no questions about how they are used. Strictly professional.', 2, true, v_campaign_id)
    ON CONFLICT (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, campaign_id) DO NOTHING;

    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_elara_id, 'pc', v_caelan_id, RT_CONTACT, 'Crafts enchantment-ready components for Caelan. Fascinated by his arcane work despite herself.', 2, true, v_campaign_id)
    ON CONFLICT (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, campaign_id) DO NOTHING;

    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_elara_id, 'pc', v_tessa_id, RT_FRIEND, 'Repaired Tessa''s armor after a brutal fight and refused payment. Quietly admires her principles.', 3, true, v_campaign_id)
    ON CONFLICT (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, campaign_id) DO NOTHING;

    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_elara_id, 'pc', v_dusk_id, RT_CONTACT, 'Supplies arrows and trail gear. The two share a mutual appreciation for silence and clean work.', 2, true, v_campaign_id)
    ON CONFLICT (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, campaign_id) DO NOTHING;

    -- Sylvara → all PCs (mysterious contact)
    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_sylvara_id, 'pc', v_mira_id, RT_CONTACT, 'Warned Mira about an ambush before it happened. Mira doesn''t know what to make of her — which is exactly how Sylvara prefers it.', 1, true, v_campaign_id)
    ON CONFLICT (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, campaign_id) DO NOTHING;

    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_sylvara_id, 'pc', v_caelan_id, RT_ALLY, 'Has appeared to Caelan in moments of magical crisis. Seems drawn to his arcane presence for reasons she has not shared.', 2, true, v_campaign_id)
    ON CONFLICT (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, campaign_id) DO NOTHING;

    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_sylvara_id, 'pc', v_tessa_id, RT_CONTACT, 'Passed cryptic warnings to Tessa about a darkness within the upper temple hierarchy. Has not elaborated.', 1, true, v_campaign_id)
    ON CONFLICT (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, campaign_id) DO NOTHING;

    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_sylvara_id, 'pc', v_dusk_id, RT_ALLY, 'Dusk has encountered Sylvara in the wilderness three times. Each time, something dangerous was narrowly avoided. Coincidence is a word Dusk no longer uses.', 2, true, v_campaign_id)
    ON CONFLICT (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, campaign_id) DO NOTHING;

    -- Brennan Cole → all PCs (friend / contact)
    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_brennan_id, 'pc', v_mira_id, RT_ALLY, 'Buys and sells through Mira regularly. Gives fair cuts and useful rumors. Reliable.', 3, true, v_campaign_id)
    ON CONFLICT (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, campaign_id) DO NOTHING;

    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_brennan_id, 'pc', v_caelan_id, RT_CONTACT, 'Has sourced rare books and arcane components at below-market prices. Enjoys the intellectual company.', 2, true, v_campaign_id)
    ON CONFLICT (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, campaign_id) DO NOTHING;

    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_brennan_id, 'pc', v_tessa_id, RT_FRIEND, 'Tessa once recovered something precious he had lost. He has been quietly useful to her cause ever since.', 3, true, v_campaign_id)
    ON CONFLICT (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, campaign_id) DO NOTHING;

    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_brennan_id, 'pc', v_dusk_id, RT_ACQUAINTANCE, 'Fellow traveler, different roads. Brennan respects Dusk''s self-reliance. They share a drink occasionally and leave it at that.', 2, true, v_campaign_id)
    ON CONFLICT (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, campaign_id) DO NOTHING;

    -- Aldus Greymantle → ONLY Caelan (mentor — backstory connection)
    INSERT INTO public.entity_relationship (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, description, attitude_score, is_active, campaign_id)
    VALUES ('npc', v_aldus_id, 'pc', v_caelan_id, RT_MENTOR,
        'Aldus trained Caelan from adolescence and is the closest thing to a father he has known. His disappearance is what brought Caelan to Thornveil. Whatever Aldus uncovered, he believed it was worth vanishing for.', 5, true, v_campaign_id)
    ON CONFLICT (source_entity_type, source_entity_id, target_entity_type, target_entity_id, relationship_type_id, campaign_id) DO NOTHING;

    RAISE NOTICE 'Demo seed complete. Account ID: %, Campaign ID: %', v_account_id, v_campaign_id;

    END IF;

END $$;

COMMIT;

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

-- user
CREATE TABLE IF NOT EXISTS auth."user" (
  id SERIAL PRIMARY KEY,
  first_name VARCHAR(100) NOT NULL,
  last_name VARCHAR(100),
  email VARCHAR(255) NOT NULL UNIQUE,
  password TEXT NOT NULL,
  salt TEXT NOT NULL
);

-- role
CREATE TABLE IF NOT EXISTS auth.role (
  id SERIAL PRIMARY KEY,
  name VARCHAR(100) NOT NULL UNIQUE,
  description TEXT
);

-- user_role (join)
CREATE TABLE IF NOT EXISTS auth.user_role (
  account_id INTEGER NOT NULL REFERENCES auth."user"(id) ON DELETE CASCADE,
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

-- Tables (id columns use IDENTITY for modern Postgres)
CREATE TABLE IF NOT EXISTS public.account (
    account_id    int GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    username      text NOT NULL UNIQUE,
    email         text NOT NULL UNIQUE,
    password_hash text NULL,
    created_at    timestamptz NOT NULL DEFAULT now()
);

CREATE TABLE IF NOT EXISTS public.game_system (
    game_system_id   int GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    game_system_name text NOT NULL UNIQUE
);

-- NPC table:
-- Keep a lightweight `stats` column for backward compatibility / quick reads,
-- and store strongly-typed or system-specific JSON in `npc_additional_data`.
CREATE TABLE IF NOT EXISTS public.npc (
    npc_id          int GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    account_id         int NOT NULL,
    game_system_id  int NOT NULL,
    -- Use jsonb for the stats blob to enable JSON queries/indexing.
    stats           jsonb NOT NULL DEFAULT '{}'::jsonb,
    created_at      timestamptz NOT NULL DEFAULT now(),
    updated_at      timestamptz NOT NULL DEFAULT now(),
    FOREIGN KEY (account_id) REFERENCES public.account(account_id) ON DELETE CASCADE,
    FOREIGN KEY (game_system_id) REFERENCES public.game_system(game_system_id)
);

-- Separate table for system-specific JSON blobs.
-- This is useful if:
--  - you want one NPC to have multiple blobs (e.g., versions or blobs per system)
--  - you want to index/query JSON specific to the system
--  - you prefer normalized design / lifecycle independent of the NPC row
CREATE TABLE IF NOT EXISTS public.npc_additional_data (
    npc_additional_data_id int GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    npc_id                 int NOT NULL,
    game_system_id         int NOT NULL,
    data                   jsonb NOT NULL,
    source                 text NULL, -- e.g. "import", "manual", "v1.2"
    created_at             timestamptz NOT NULL DEFAULT now(),
    updated_at             timestamptz NOT NULL DEFAULT now(),
    FOREIGN KEY (npc_id) REFERENCES public.npc(npc_id) ON DELETE CASCADE,
    FOREIGN KEY (game_system_id) REFERENCES public.game_system(game_system_id),
    UNIQUE (npc_id, game_system_id) -- prevents duplicates per system; remove if you want versions
);

-- Useful indexes for JSON queries
CREATE INDEX IF NOT EXISTS idx_npc_stats_gin ON public.npc USING GIN (stats);
CREATE INDEX IF NOT EXISTS idx_npc_additional_data_gin ON public.npc_additional_data USING GIN (data);

-- Seed data
INSERT INTO public.account (username, email, password_hash)
VALUES
  ('gm_admin', 'gm_admin@example.local', NULL)
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
    1,
    (SELECT game_system_id FROM public.game_system WHERE game_system_name = 'Dungeons & Dragons (5e)' LIMIT 1),
    jsonb_build_object(
      'name', 'Roth the Wanderer',
      'lineage', 'Human',
      'occupation', 'Sellsword',
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
    1,
    (SELECT game_system_id FROM public.game_system WHERE game_system_name = 'Dungeons & Dragons (5e)' LIMIT 1),
    jsonb_build_object(
      'name', 'Elara Moonwhisper',
      'lineage', 'Elf',
      'occupation', 'Wizard',
      'description', 'A scholarly mage who studies ancient arcane texts in her tower.',
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
  1,
  (SELECT game_system_id FROM public.game_system WHERE game_system_name = 'Dungeons & Dragons (5e)' LIMIT 1),
  jsonb_build_object(
    'name', 'Roth the Wanderer',
    'lineage', 'Human',
    'occupation', 'Sellsword',
    'description', 'A grizzled sellsword who travels between towns.',
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
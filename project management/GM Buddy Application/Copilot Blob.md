TTRPG npc relationship systems overview

NPC relationships shine when they model not just PC–NPC ties, but the webs between NPCs, factions, and social history. Popular approaches range from numeric attitude tracks to resource-like bonds and faction clocks, each giving GMs tools to evolve the world consistently through play. Examples include: attitude matrices for consistent NPC responses, relationship tracks from negative to positive, resource-based ties like Strings/Debts, faction status and clocks, and social currencies (Honor/Glory/Status) that structure encounters into phases. These systems help you track influence, obligations, rivalries, and emotional continuity so recurring drama feels earned and reactive.

- **Numeric tracks:** Simple scales (e.g., -5 to +5 or -10 to +10) with tags for context (rival, mentor, debtor), offering quick adjustments on significant actions.
- **Resource-based bonds:** Strings/Debts (PbtA): relationships as spendable resources that create leverage and obligations.
- **Faction mechanics:** Status bands and “clocks” to track progress toward goals, reprisals, or turf control, ideal for political or urban intrigue.
- **Social currencies and phases:** Honor/Glory/Status with structured social scene phases (Opening, Discourse, Resolution) for tactical social play.
- **Response matrices:** Attitude grids enabling consistent NPC behavior with drift over time based on events.
- **Web mapping:** 3x3 social webs (3 NPCs, 3 relationships each) to visualize hooks, hierarchy, affection, rivalry, and obligations.

Core concepts for your web app

Relationship primitives

- **Entity:** PC, NPC, Faction, Location, Organization.
- **Relationship edge:** Source entity, target entity, type (affection, rivalry, obligation, hierarchy, history), directionality, strength score, confidence, volatility.
- **Tags:** Narrative descriptors (e.g., “estranged siblings,” “owes debt,” “bitter rival”).
- **State machine:** Lifecycle (formed, strained, broken, mended), triggers (events, scenes).
- **Influence vectors:** Reputation, favor, debt balance, status differential.
- **Clocks/progress:** Named multi-step trackers tied to relationships or factions (e.g., “Gang’s retaliation 0/6”).
- **Notes and evidence:** Sessions/events that justify changes (log entries, timestamps, sources).

Relationship evolution rules

- **Adjustments:** Atomic events modifying strength, tags, and clocks with rationale.
- **Decay/refresh:** Optional automatic drift based on time since last interaction.
- **Cascades:** Change propagation across the web (e.g., harming an ally strains their allies).
- **Context-aware views:** Different perspectives (GM, player-facing, secret flags).

These align with numeric tracks, faction clocks, and structured social mechanics, while supporting mapped social webs for hooks and continuity.

Requirements

Functional requirements

- **Entity management:** CRUD for PCs, NPCs, factions, locations; merge and link duplicates.
- **Relationship graph:** Create, edit, and visualize directed edges with type, strength, tags.
- **Scoring and tags:** Adjustable scales (configurable ranges), descriptive labels, secret/public flags.
- **Faction clocks and status:** Create named clocks, increment/decrement, bind to factions/relationships.
- **Event logging:** Append-only event trail for changes (who/what/when/why).
- **Scenes/phases:** Optional social scene templates (Opening/Discourse/Resolution) with outcomes.
- **Views:** Town/faction dashboards, character-centric views, player-safe export.
- **Search/filter:** By tags, thresholds, types, recent changes.
- **Automation:** Rules to cascade changes (e.g., “hurt target → -1 with target’s allies”).
- **Import/export:** JSON/CSV for vaults; Obsidian-friendly Markdown exports.
- **Access control:** GM vs player roles, session locks, draft mode.
- **API:** REST endpoints for entities, relationships, events, clocks, tags.

These features reflect practical needs drawn from NPC response matrices, relationship tracks, faction systems, and social web mapping.

Non-functional requirements

- **Performance:** Snappy graph queries (<200ms typical), pagination for lists.
- **Scalability:** Support campaigns with thousands of entities/edges.
- **Consistency:** Event-sourced audit, eventual consistency for projections is acceptable.
- **Security:** Role-based access, audit trails, encrypted secrets.
- **Usability:** Diagram-first views, keyboard shortcuts, bulk edits.
- **Reliability:** Daily backups, migration-friendly schemas.
- **Extensibility:** Pluggable rule engines and custom relationship types.
- **Observability:** Structured logs, metrics on change volume and API latency.

Architecture and pattern choice

Recommended approach

- **3-tier with Clean Architecture layering:** Presentation (SPA), Application (C# services/use-cases), Domain (entities, value objects, rules), Infrastructure (PostgreSQL, persistence, external integrations). This isolates domain logic for rich relationship rules, supports testability, and cleanly separates data access from business rules.

2-tier vs 3-tier comparison

|||||
|---|---|---|---|
|||||
|||||

Sources:

Extras to consider

- **Event sourcing + CQRS (lightweight):** Store relationship changes as events; build read models (graphs, dashboards) for fast queries. Ideal for auditability and time travel.
- **Graph projections:** Maintain relational source of truth in PostgreSQL, with materialized views for common queries; optionally add a graph index via edge tables and GIN indexes for tags.

Data model and API design (PostgreSQL, .NET 9, C#)

Schema (relational core)

- **entities**

- **id:** UUID
- **type:** enum (pc, npc, faction, location, org)
- **name:** text
- **aliases:** text[]
- **visibility:** enum (gm_only, player)
- **metadata:** jsonb

- **relationships**

- **id:** UUID
- **source_id:** UUID (entities.id)
- **target_id:** UUID (entities.id)
- **type:** enum (affection, rivalry, obligation, hierarchy, history, custom)
- **direction:** enum (uni, bi)
- **strength:** int (configurable range)
- **confidence:** int
- **volatility:** int
- **state:** enum (formed, strained, broken, mended)
- **tags:** text[]
- **is_secret:** bool
- **created_at / updated_at**

- **faction_clocks**

- **id:** UUID
- **entity_id:** UUID (faction)
- **name:** text
- **current:** int
- **max:** int
- **tags:** text[]
- **is_secret:** bool

- **events**

- **id:** UUID
- **timestamp:** timestamptz
- **actor:** text (GM or system)
- **entity_id:** UUID nullable
- **relationship_id:** UUID nullable
- **clock_id:** UUID nullable
- **type:** enum (adjust_strength, add_tag, change_state, increment_clock, create_entity, etc.)
- **payload:** jsonb
- **note:** text

- **rules**

- **id:** UUID
- **name:** text
- **trigger:** jsonb (conditions)
- **effect:** jsonb (cascades, thresholds)
- **enabled:** bool

Indexes: GIN on tags and metadata, btree on foreign keys, partial indexes for is_secret=false, composite index on (source_id, target_id, type).

API endpoints (REST)

- **Entities:** GET /entities?type=..., POST /entities, PUT /entities/{id}, DELETE /entities/{id}
- **Relationships:** GET /relationships?sourceId=...&targetId=..., POST /relationships, PATCH /relationships/{id}
- **Clocks:** GET /factions/{id}/clocks, POST /clocks, PATCH /clocks/{id}
- **Events:** GET /events?entityId=...&relationshipId=..., POST /events
- **Rules:** GET/POST/PATCH /rules
- **Views:** GET /views/town/{id}, GET /views/character/{id}, GET /views/faction/{id}
- **Exports:** GET /exports/markdown?scope=...

Controller layer stays thin; application services implement use-cases (e.g., AdjustRelationshipStrength), domain objects validate rules, infrastructure maps to PostgreSQL.

Simple GM form and workflows

Relationship entry form (single screen)

- **Primary fields:**

- **Source:** Entity select
- **Target:** Entity select
- **Type:** Dropdown (affection, rivalry, obligation, hierarchy, history)
- **Direction:** Toggle (uni/bi)
- **Strength:** Slider (configurable range)
- **State:** Dropdown (formed, strained, broken, mended)

- **Context fields:**

- **Tags:** Multi-select with suggestions (rival, mentor, debtor)
- **Clock link:** Optional select (or create new clock)
- **Visibility:** Secret/public toggle
- **Note:** Free text for rationale

- **Event log:** Auto-create event with payload and timestamp.
- **Quick actions:**

- **+1/-1 strength** buttons
- **Increment clock** button
- **Apply rule** dropdown (e.g., “hurt ally → cascade -1 to allies”)

Scene phase helper (optional)

- **Opening:** Goals, leverage, initial stance; pre-fill from relationship tags and strengths.
- **Discourse:** Rolls/prompts/outcomes; proposed adjustments with preview.
- **Resolution:** Commit adjustments, log events, propose cascades.

This form supports numeric tracks, resource-like adjustments, and faction clock progress, reflecting the systems’ best practices for consistent, evolving social dynamics.

If you want, I can tailor the data ranges (e.g., -10 to +10 vs -5 to +5), define a minimal event schema for an MVP, or sketch a Clean Architecture folder layout for .NET 9. Also, tell me your preferred visualization: node-link graph, matrix, or character-centric dashboards.
- Create NPCs
	- Based on the game system, add required stats
		- this can be simple or complex based on the needs of the npc
	- other system specific data can be included
		- name
		- race/lineage/species
		- class/occupation
		- gender
		- description
		- spells
		- skills
		- ac
		- so on
	- this means saving this into a database
	- npcs will have a relationship tracker for up to 10 others. this is likely a free text field, but may be associated with known pcs or other npcs created?
- create factions
	- basic descriptions
	- members (npcs or pcs)
	- relationships with other factions
- Dashboard
	- displayed list, maybe top ten (sortable), of npcs
		- this will show a subset of npcs. maybe sortable by most recently created, or alphabetical, or other statistics
	- other functionality to be included
		- such as viewing publicly created npcs
- Edit NPC's
	- loads NPC
	- all fields editable
	- can be saved, updating existing npc
- View public npcs created by other users.
	- an npc is automatically set to private
	- a user can change this npc to be public
	- if public, this npc can be copied to the user viewing it
	- the dashboard, or maybe some other area, will show a top ten most recent or maybe most copied npcs from others that are public
- Datastore
	- PostgreSQL
		- its free to develop and cheaper to host
		- using relational stores for account data and relation info
		- using jsonb to handle schema-less data like NPC statistics


## MVP, phase 1
1. create NPC
	1. basic statistics based on DND
2. view NPC
	1. view stats and relationship values
3. modify NPC
	1. change stats
4. "use" NPC relationship controls
	1. when viewing, affect the relationships
# D&D 5e SRD API Integration

This document describes the integration with the D&D 5th Edition System Reference Document (SRD) API.

## API Selection

### Chosen API: dnd5eapi.co

**URL**: https://www.dnd5eapi.co/

### Why This API?

After evaluating both https://www.dnd5eapi.co/ and https://5e-bits.github.io/docs/, we selected **dnd5eapi.co** for the following reasons:

#### Technical Considerations:
- **Same Ecosystem**: Both URLs are part of the same project. dnd5eapi.co is the hosted API endpoint, while 5e-bits.github.io serves as documentation.
- **No Authentication Required**: Simple integration without API keys or OAuth flows.
- **RESTful & GraphQL**: Supports both REST and GraphQL endpoints (we use REST).
- **Stable Endpoints**: Well-established API with consistent endpoint structure.
- **No Rate Limiting**: No published rate limits for reasonable use.

#### Bandwidth Considerations:
- **Hosted Service**: dnd5eapi.co is a free, publicly hosted endpoint maintained by the community.
- **Caching Recommended**: For production use, client-side caching reduces API calls.
- **Self-Hosting Option**: If needed, the open-source codebase can be self-hosted from [5e-bits/5e-srd-api](https://github.com/5e-bits/5e-srd-api).

#### Legal Considerations:
- **Open Gaming License (OGL) v1.0a**: Content licensed under OGL for D&D 5E SRD.
- **MIT License**: API code is MIT licensed.
- **SRD Content Only**: Only includes content from the official System Reference Document, no proprietary material.
- **Commercial Use**: Safe for commercial projects with proper attribution.
- **Attribution Required**: Must attribute SRD content per OGL requirements.

### Legal Compliance

This integration uses only content from the D&D 5th Edition System Reference Document, which is:
- Freely available under the Open Gaming License
- Does NOT include proprietary content from D&D sourcebooks
- Safe for use in commercial projects
- Requires attribution: "This app uses data from the D&D 5th Edition SRD, licensed under the Open Gaming License."

**Important**: Do not extend this integration to include non-SRD content without proper licensing from Wizards of the Coast.

## Usage

### Basic Example

```typescript
import { dnd5eApi } from '@/services/dnd5eApi';

// Get all spells
const spells = await dnd5eApi.getSpells();
console.log(`Found ${spells.count} spells`);

// Get a specific spell
const fireball = await dnd5eApi.getSpell('fireball');
console.log(fireball.name, fireball.desc);

// Get all monsters
const monsters = await dnd5eApi.getMonsters();

// Get a specific monster
const dragon = await dnd5eApi.getMonster('adult-red-dragon');
console.log(dragon.name, dragon.challenge_rating);
```

### Available Resources

The API client provides access to:

1. **Spells**
   - `getSpells()` - List all spells
   - `getSpell(index)` - Get specific spell
   - `getSpellsByLevel(level)` - Filter by spell level

2. **Monsters**
   - `getMonsters()` - List all monsters
   - `getMonster(index)` - Get specific monster
   - `getMonstersByChallengeRating(cr)` - Filter by CR

3. **Character Classes**
   - `getClasses()` - List all classes
   - `getClass(index)` - Get specific class

4. **Races**
   - `getRaces()` - List all races
   - `getRace(index)` - Get specific race

5. **Equipment**
   - `getEquipment()` - List all equipment
   - `getEquipmentItem(index)` - Get specific item

6. **Conditions**
   - `getConditions()` - List all conditions
   - `getCondition(index)` - Get specific condition

7. **Magic Items**
   - `getMagicItems()` - List all magic items
   - `getMagicItem(index)` - Get specific magic item

### Example: Building a Spell Browser

```typescript
import { dnd5eApi, Spell } from '@/services/dnd5eApi';
import { useState, useEffect } from 'react';

function SpellBrowser() {
  const [spells, setSpells] = useState<Spell[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function loadSpells() {
      try {
        const spellList = await dnd5eApi.getSpells();
        // Load details for first 10 spells
        const spellDetails = await Promise.all(
          spellList.results.slice(0, 10).map(ref => 
            dnd5eApi.getSpell(ref.index)
          )
        );
        setSpells(spellDetails);
      } catch (error) {
        console.error('Failed to load spells:', error);
      } finally {
        setLoading(false);
      }
    }
    loadSpells();
  }, []);

  if (loading) return <div>Loading spells...</div>;

  return (
    <div>
      {spells.map(spell => (
        <div key={spell.index}>
          <h3>{spell.name}</h3>
          <p>Level: {spell.level}</p>
          <p>{spell.desc.join(' ')}</p>
        </div>
      ))}
    </div>
  );
}
```

### Example: Random Monster Generator

```typescript
import { dnd5eApi } from '@/services/dnd5eApi';

async function getRandomMonster() {
  const monsters = await dnd5eApi.getMonsters();
  const randomIndex = Math.floor(Math.random() * monsters.results.length);
  const monsterRef = monsters.results[randomIndex];
  return await dnd5eApi.getMonster(monsterRef.index);
}

// Usage
const monster = await getRandomMonster();
console.log(`Encounter: ${monster.name} (CR ${monster.challenge_rating})`);
```

## Type Safety

All API responses are fully typed with TypeScript interfaces. Import types as needed:

```typescript
import type { 
  Spell, 
  Monster, 
  CharacterClass, 
  Race, 
  Equipment,
  APIReference 
} from '@/services/dnd5eApi';
```

## Error Handling

The API client includes built-in error handling with console logging. Wrap calls in try-catch for user-facing error handling:

```typescript
try {
  const spell = await dnd5eApi.getSpell('invalid-spell');
} catch (error) {
  console.error('Spell not found:', error);
  // Show user-friendly error message
}
```

## Performance Considerations

1. **API Query Parameters**: The API supports filtering via query parameters (e.g., `level`, `challenge_rating`). The helper methods like `getSpellsByLevel()` and `getMonstersByChallengeRating()` use these parameters to reduce unnecessary requests.

2. **Caching**: Consider caching API responses in application state or local storage to minimize repeated calls.

3. **Lazy Loading**: The API list endpoints return only references (`index`, `name`, `url`). Load full details only when needed:
   ```typescript
   // Efficient: Only load list
   const spells = await dnd5eApi.getSpells();
   
   // Load details only when user selects a spell
   const selectedSpell = await dnd5eApi.getSpell(spells.results[0].index);
   ```

4. **Batch Requests**: Use `Promise.all()` carefully. While it parallelizes requests, too many simultaneous calls can overwhelm the API:
   ```typescript
   // Be cautious with large datasets
   const monsters = await dnd5eApi.getMonsters(); // Returns 300+ references
   
   // Instead of fetching all at once, paginate or limit:
   const first10 = await Promise.all(
     monsters.results.slice(0, 10).map(ref => dnd5eApi.getMonster(ref.index))
   );
   ```

5. **Reference Data**: When you only need names/indexes, use list endpoints directly without fetching full details.

## Future Enhancements

Potential additions to this integration:

- [ ] Client-side caching layer with TTL
- [ ] React hooks for common queries (`useSpells()`, `useMonsters()`, etc.)
- [ ] Search and filter utilities
- [ ] Offline support with service workers
- [ ] GraphQL endpoint integration (currently using REST)
- [ ] Self-hosted instance for production reliability

## Resources

- **API Documentation**: https://5e-bits.github.io/docs/
- **API Base URL**: https://www.dnd5eapi.co/api
- **GitHub Repository**: https://github.com/5e-bits/5e-srd-api
- **Data Repository**: https://github.com/5e-bits/5e-database
- **Open Gaming License**: [OGL v1.0a](https://www.opengamingfoundation.org/ogl.html)

## Attribution

This integration uses data from the Dungeons & Dragons 5th Edition System Reference Document, which is licensed under the Open Gaming License v1.0a. The API and data are provided by the 5e-bits community project.

**Required Attribution**: 
"This application uses data from the D&D 5th Edition SRD, which is licensed under the Open Gaming License v1.0a."

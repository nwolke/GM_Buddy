/**
 * D&D 5e API Integration Test Examples
 * 
 * This file contains example code for testing the D&D 5e API integration.
 * You can run these examples in your browser console or create a test page.
 */

import { dnd5eApi } from './dnd5eApi';
import type { Spell, Monster, CharacterClass } from './dnd5eApi';

/**
 * Example 1: Fetch and display all spell names
 */
export async function exampleGetAllSpells(): Promise<void> {
  console.log('=== Example 1: Get All Spells ===');
  try {
    const spells = await dnd5eApi.getSpells();
    console.log(`Total spells in SRD: ${spells.count}`);
    console.log('First 5 spells:', spells.results.slice(0, 5));
  } catch (error) {
    console.error('Error fetching spells:', error);
  }
}

/**
 * Example 2: Get detailed information about a specific spell
 */
export async function exampleGetFireball(): Promise<Spell | null> {
  console.log('=== Example 2: Get Fireball Spell Details ===');
  try {
    const fireball = await dnd5eApi.getSpell('fireball');
    console.log('Spell Name:', fireball.name);
    console.log('Level:', fireball.level);
    console.log('School:', fireball.school.name);
    console.log('Casting Time:', fireball.casting_time);
    console.log('Range:', fireball.range);
    console.log('Components:', fireball.components);
    console.log('Duration:', fireball.duration);
    console.log('Description:', fireball.desc.join('\n'));
    return fireball;
  } catch (error) {
    console.error('Error fetching Fireball:', error);
    return null;
  }
}

/**
 * Example 3: Get all cantrips (level 0 spells)
 */
export async function exampleGetCantrips(): Promise<void> {
  console.log('=== Example 3: Get All Cantrips ===');
  try {
    const cantrips = await dnd5eApi.getSpellsByLevel(0);
    console.log(`Total cantrips: ${cantrips.length}`);
    console.log('Cantrips:', cantrips.map(c => c.name));
  } catch (error) {
    console.error('Error fetching cantrips:', error);
  }
}

/**
 * Example 4: Get monster information
 */
export async function exampleGetDragon(): Promise<Monster | null> {
  console.log('=== Example 4: Get Adult Red Dragon ===');
  try {
    const dragon = await dnd5eApi.getMonster('adult-red-dragon');
    console.log('Monster:', dragon.name);
    console.log('Type:', dragon.type);
    console.log('Size:', dragon.size);
    console.log('Challenge Rating:', dragon.challenge_rating);
    console.log('HP:', dragon.hit_points);
    console.log('AC:', dragon.armor_class);
    console.log('STR:', dragon.strength);
    console.log('DEX:', dragon.dexterity);
    console.log('CON:', dragon.constitution);
    console.log('INT:', dragon.intelligence);
    console.log('WIS:', dragon.wisdom);
    console.log('CHA:', dragon.charisma);
    if (dragon.actions) {
      console.log('Actions:', dragon.actions.map(a => a.name));
    }
    return dragon;
  } catch (error) {
    console.error('Error fetching dragon:', error);
    return null;
  }
}

/**
 * Example 5: Get monsters by challenge rating
 */
export async function exampleGetLowCRMonsters(): Promise<void> {
  console.log('=== Example 5: Get CR 1 Monsters ===');
  try {
    const monsters = await dnd5eApi.getMonstersByChallengeRating(1);
    console.log(`Total CR 1 monsters: ${monsters.length}`);
    console.log('CR 1 monsters:', monsters.map(m => m.name));
  } catch (error) {
    console.error('Error fetching CR 1 monsters:', error);
  }
}

/**
 * Example 6: Get character class information
 */
export async function exampleGetWizardClass(): Promise<CharacterClass | null> {
  console.log('=== Example 6: Get Wizard Class ===');
  try {
    const wizard = await dnd5eApi.getClass('wizard');
    console.log('Class:', wizard.name);
    console.log('Hit Die:', `d${wizard.hit_die}`);
    console.log('Saving Throws:', wizard.saving_throws.map(st => st.name));
    console.log('Proficiencies:', wizard.proficiencies.map(p => p.name));
    if (wizard.spellcasting) {
      console.log('Spellcasting Ability:', wizard.spellcasting.spellcasting_ability.name);
    }
    console.log('Subclasses:', wizard.subclasses.map(sc => sc.name));
    return wizard;
  } catch (error) {
    console.error('Error fetching Wizard class:', error);
    return null;
  }
}

/**
 * Example 7: Get all available classes
 */
export async function exampleGetAllClasses(): Promise<void> {
  console.log('=== Example 7: Get All Classes ===');
  try {
    const classes = await dnd5eApi.getClasses();
    console.log(`Total classes: ${classes.count}`);
    console.log('Classes:', classes.results.map(c => c.name));
  } catch (error) {
    console.error('Error fetching classes:', error);
  }
}

/**
 * Example 8: Get race information
 */
export async function exampleGetElfRace(): Promise<void> {
  console.log('=== Example 8: Get Elf Race ===');
  try {
    const elf = await dnd5eApi.getRace('elf');
    console.log('Race:', elf.name);
    console.log('Speed:', elf.speed);
    console.log('Ability Bonuses:', elf.ability_bonuses.map(ab => 
      `${ab.ability_score.name} +${ab.bonus}`
    ));
    console.log('Languages:', elf.languages.map(l => l.name));
    console.log('Traits:', elf.traits.map(t => t.name));
    console.log('Subraces:', elf.subraces.map(sr => sr.name));
  } catch (error) {
    console.error('Error fetching Elf race:', error);
  }
}

/**
 * Example 9: Get equipment information
 */
export async function exampleGetLongsword(): Promise<void> {
  console.log('=== Example 9: Get Longsword ===');
  try {
    const longsword = await dnd5eApi.getEquipmentItem('longsword');
    console.log('Equipment:', longsword.name);
    console.log('Category:', longsword.equipment_category.name);
    console.log('Cost:', `${longsword.cost.quantity} ${longsword.cost.unit}`);
    console.log('Weight:', longsword.weight);
    if (longsword.damage) {
      console.log('Damage:', longsword.damage.damage_dice, longsword.damage.damage_type.name);
    }
    if (longsword.properties) {
      console.log('Properties:', longsword.properties.map(p => p.name));
    }
  } catch (error) {
    console.error('Error fetching Longsword:', error);
  }
}

/**
 * Example 10: Get conditions
 */
export async function exampleGetConditions(): Promise<void> {
  console.log('=== Example 10: Get All Conditions ===');
  try {
    const conditions = await dnd5eApi.getConditions();
    console.log(`Total conditions: ${conditions.count}`);
    console.log('Conditions:', conditions.results.map(c => c.name));
    
    // Get details for "Poisoned" condition
    const poisoned = await dnd5eApi.getCondition('poisoned');
    console.log('\nPoisoned Condition:');
    console.log(poisoned.desc.join('\n'));
  } catch (error) {
    console.error('Error fetching conditions:', error);
  }
}

/**
 * Example 11: Random encounter generator
 */
export async function exampleRandomEncounter(): Promise<void> {
  console.log('=== Example 11: Random Encounter Generator ===');
  try {
    const monsters = await dnd5eApi.getMonsters();
    const randomIndex = Math.floor(Math.random() * monsters.results.length);
    const monsterRef = monsters.results[randomIndex];
    const monster = await dnd5eApi.getMonster(monsterRef.index);
    
    console.log('🎲 Random Encounter!');
    console.log(`You encounter: ${monster.name}`);
    console.log(`Type: ${monster.type}`);
    console.log(`Challenge Rating: ${monster.challenge_rating}`);
    console.log(`XP: ${monster.xp}`);
  } catch (error) {
    console.error('Error generating random encounter:', error);
  }
}

/**
 * Run all examples
 */
export async function runAllExamples(): Promise<void> {
  console.log('🎲 Running all D&D 5e API examples...\n');
  
  await exampleGetAllSpells();
  console.log('\n');
  
  await exampleGetFireball();
  console.log('\n');
  
  await exampleGetCantrips();
  console.log('\n');
  
  await exampleGetDragon();
  console.log('\n');
  
  await exampleGetLowCRMonsters();
  console.log('\n');
  
  await exampleGetWizardClass();
  console.log('\n');
  
  await exampleGetAllClasses();
  console.log('\n');
  
  await exampleGetElfRace();
  console.log('\n');
  
  await exampleGetLongsword();
  console.log('\n');
  
  await exampleGetConditions();
  console.log('\n');
  
  await exampleRandomEncounter();
  console.log('\n');
  
  console.log('✅ All examples completed!');
}

// Export a convenience function for quick testing
export const testDndApi = {
  spells: exampleGetAllSpells,
  fireball: exampleGetFireball,
  cantrips: exampleGetCantrips,
  dragon: exampleGetDragon,
  crMonsters: exampleGetLowCRMonsters,
  wizard: exampleGetWizardClass,
  classes: exampleGetAllClasses,
  elf: exampleGetElfRace,
  longsword: exampleGetLongsword,
  conditions: exampleGetConditions,
  randomEncounter: exampleRandomEncounter,
  all: runAllExamples,
};

// For browser console usage:
// import { testDndApi } from '@/services/dnd5eApiExamples';
// testDndApi.fireball();
// testDndApi.dragon();
// testDndApi.all();

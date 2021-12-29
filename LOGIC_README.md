This file serves to document the randomizer logic.

## Skip Settings

### Precise Movement
- Precise Movement is a skip category used for ordinary movement with a very low margin of error. Precise Movement cases often, but not always, have a large penalty for failure.
- In logic, Precise Movement is indicated with the **PRECISEMOVEMENT** token.
- More advanced skips such as Fireball Skips or Acid Skips generally do not include the Precise Movement modifier.
- Examples include:
    - Jumping to *Hallownest_Seal-Fungal_Wastes_Sporgs* with no items.
    - Jumping to *Ancient_Basin_Map* with no items.
    
### Background Object Pogos
- Background Object Pogos are as the name suggests.
- Interactive objects such as spikes, enemies, or bounce shrooms are not included in this category.
- In logic, Background Object Pogos are indicated with the **BACKGROUNDPOGOS** token.
- Examples include:
    -
    
### Enemy Pogos
- Enemy Pogos are skips which require pogoing killable enemies or their projectiles.
- Pogoing spikes, Garpedes, or Wingsmoulds is not considered a skip. Pogoing the shade or its projectiles is classified under Shade Skips, and not Enemy Pogos.
- In logic, Enemy Pogos are indicated with the **ENEMYPOGOS** token.
- Examples include:
    -
    
### Obscure Skips
- Obscure Skips are progression paths which are subjectively determined to be too obscure for a new randomizer player to be expected to know.
- Advanced skips with other modifiers generally do not include the Obscure Skips modifier.
- In logic, Obscure Skips are indicated with the **OBSCURESKIPS** token.
- Examples include:
    - Destroying horizontal planks in Ancestral Mound or Kingdom's Edge using Crystal Heart.
    
### Shade Skips
- Shade Skips are skips which utilize the shade, often by pogoing it or using it for damage boosts.
- Generally, shade skips which require shade fireballs are not in logic, due to their RNG-dependence and relative obscurity.
- Some shade skips are only conditionally in logic, depending on the transition randomizer setting. Limitations of the randomizer make it infeasible to calculate bench accessibility for setting up shade skips in every location.
- In logic, Shade Skips are indicated with the **SHADESKIPS** token. More commonly, the **ITEMSHADESKIPS**, **AREASHADESKIPS**, or **ROOMSHADESKIPS** macros are used.
- Examples include:
    -
    
### Infection Skips
- Infection Skips are skips which are only possible after Forgotten Crossroads has been infected. These skips most commonly involve pogoing infection bubbles or Furious Vengeflies.
- In logic, Infection Skips are indicated with the **INFECTIONSKIPS** token. More commonly, the **INFECTED** macro is used.
- Examples include:
    -

### Fireball Skips
- Fireball Skips are skips which use Vengeful Spirit or Howling Wraiths (or their upgrades) to reset fall speed midair.
- Generally, Fireball Skips in logic require at most 3 consecutive casts of a spell, meaning that Spell Twister skips are not in logic.
- In logic, Fireball Skips are indicated with the **FIREBALLSKIPS** token. More commonly, the **AIRSTALL** macro is used.
- Examples include:
    -

### Acid Skips
- Acid Skips are skips which involve crossing over a pool of acid (or water, when the Swim ability is removed).
- In logic, Acid Skips are indicated with the **ACIDSKIPS** token. More commonly, the **LEFTSKIPACID** or **RIGHTSKIPACID** macros are used.
- Examples include:
    -
    
### Spike Tunnels
- Spike Tunnels are skips which involve crossing through a narrow passage lined with spikes.
- In logic, Spike Tunnels are indicated with the **SPIKETUNNELS** token. More commonly, the **LEFTTUNNEL** or **RIGHTTUNNEL** macros are used.
- Examples include:
    -

### Dark Rooms
- Dark Rooms are skips which involve passing through a dark room without Lumafly Lantern.
- In logic, Dark Rooms are indicated with the **DARKROOMS** token.
- Examples include:
    -
    
### Damage Boosts
- Damage Boosts are skips which involve intentionally taking damage from a hazard or enemy.
- In logic, Damage Boosts are indicated with the **DAMAGEBOOSTS** token.
- Examples include:
    -

### Dangerous Skips:
- Dangerous Skips are skips which carry a high risk of taking damage, due to aggressive enemies or other sources.
- Generally, this modifier is not applied to Acid Skips or Spike Tunnels, which are implicitly understood to be dangerous.
- In logic, Dangerous Skips are indicated with the **DANGEROUSSKIPS** token.
- Examples include:
	-

### Complex Skips
- Complex Skips are skips which have extended setup time or involve combining multiple types of skips in an unusual way.
- In logic, Complex Skips are indicated with the **COMPLEXSKIPS** token.
- Examples include:
    -

### Difficult Skips
- Difficult Skips are skips which are subjectively considered to be more difficult than usual.
- In logic, Difficult Skips are indicated with the **DIFFICULTSKIPS** token.
- Examples include:
    -

## Local Logic Edits

The randomizer supports editing logic in local files. To do so, navigate to the RandomizerMod folder in the Mods folder in the game files. Create the Logic folder within the RandomizerMod folder if it does not already exist, and navigate to this folder. Any JSON file within this folder will be parsed for logic edits, with two format options:
- The Dictionary<string, string> format used by the macros.json file in the randomizer logic source code. In this case, the name of the file should start with "macro" and the file will be parsed before other logic edits to allow creating new macros (e.g. for use in a location logic edit file) or overriding existing macros.
- The RawLogicDef[] format used by the locations.json file in the randomizer logic source code. In this case, the name of the file should not start with "macro", and the file will be parsed to edit location, waypoint, or transition logic.

With either type of edit, only the logic definitions which are being edited need to be included in the file. Additionally, in any edit, the **ORIG** token can be used to refer to the current value of the randomizer logic with the same name. For example, the **BOSS** macro could be edited to **ORIG + FULLCLAW** to change boss logic to additionally require full claw.
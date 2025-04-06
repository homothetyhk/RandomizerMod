This file serves to document the randomizer logic.

## Skip Settings

### Precise Movement
- Precise Movement is a skip category used for ordinary movement with a very low margin of error. Precise Movement cases often, but not always, have a large penalty for failure.
- In logic, Precise Movement is indicated with the **PRECISEMOVEMENT** token.
- More advanced skips such as Fireball Skips or Acid Skips generally do not include the Precise Movement modifier, but rather one of Obscure Skips, Complex Skips, or Difficult Skips as appropriate.
- Examples include:
    - Jumping to *Hallownest_Seal-Fungal_Wastes_Sporgs* with no items.
    - Jumping to *Ancient_Basin_Map* with no items.
	- Navigating through the Lifeblood Core room with no items (logic for *Lifeblood_Core*, *Arcane_Egg-Lifeblood_Core*, *Warp-Lifeblood_Core_to_Abyss*).
	- Coyote jump to reach the warp to Palace Grounds in White_Palace_03 (logic for *Warp-White_Palace_Atrium_to_Palace_Grounds*).
	- Coyote jump and right dash to reach the platform in lower King's Station, from the bottom left entrance without Swim (logic for transitions in *Ruins2_06* from *Ruins2_06[left2]*).
	- Using Mantis Claw and Crystal Heart to pass under conveyors (logic for *Mines_37*, including *Geo_Chest-Crystal_Peak*). Also considered an Obscure Skip.

### Proficient Combat
- Proficient Combat is a skip category used for ordinary combat with limited items or movement. Precise Combat is combat that should be doable after casual play, with at most a small amount of practice necessary.
- In logic, Proficient Combat is indicated with the **PROFICIENTCOMBAT** token. More commonly, the equivalent **MILDCOMBATSKIPS** macro is used, or the **SPICYCOMBATSKIPS** macro which also requires Difficult Skips.
- A nearly complete list of combat logic usage is located at the bottom of the macros.json file.
- Examples of **MILDCOMBATSKIPS** include:
    - Fighting bosses with the **AERIALMINIBOSS** modifier (dream warriors, Uumuu, various others) without a dash item.
    - Fighting bosses with the **BOSS** modifier (most bosses that would usually be encountered after wings) without a dash item.
	- Fighting Flukemarm without Shade Soul or Abyss Shriek, but instead normal **BOSS** requirements.
	- Using Cyclone Slash or Great Slash for nail combat (rather than right or left or up slashes, when nail is randomized).
- Examples of **SPICYCOMBATSKIPS** include:
	- Fighting bosses with the **MINIBOSS** modifier without a spell.
	- Fighting bosses with the **AERIALMINIBOSS** modifier without Vengeful Spirit or Howling Wraiths.
	- Fighting bosses with the **AERIALMINIBOSS** modifier with Cyclone Slash or Great Slash (rather than right or left or up slashes, when nail is randomized).
	- Using downslash only for nail combat.
    
### Background Object Pogos
- Background Object Pogos are as the name suggests.
- Interactive objects such as spikes, enemies, or bounce shrooms are not included in this category.
- In logic, Background Object Pogos are indicated with the **BACKGROUNDPOGOS** token.
- Examples include:
    - Pogoing to reach the Watcher's Spire without wings.
    
### Enemy Pogos
- Enemy Pogos are skips which require pogoing killable enemies or their projectiles.
- Pogoing spikes, saws, or enemies which do not take damage (Goams, Garpedes, Durandoos, Crystal Crawlers, Wingsmoulds, etc) is not considered a skip. Pogoing the shade or its projectiles is classified under Shade Skips, and not Enemy Pogos.
- In logic, Enemy Pogos are indicated with the **ENEMYPOGOS** token.
- Examples include:
    - Pogoing a Vengefly in Blue Lake to reach the rancid egg with wings.
    
### Obscure Skips
- Obscure Skips are progression paths which are subjectively determined to be too obscure for a new randomizer player to be expected to know.
- Advanced skips with other modifiers generally do not include the Obscure Skips modifier.
- In logic, Obscure Skips are indicated with the **OBSCURESKIPS** token.
- Examples include:
    - Destroying horizontal planks in Ancestral Mound or Kingdom's Edge using Crystal Heart.
	- Itemless explosion pogo to Mantis Village.
	- Corpse explosion pogo with wings below Fungal Wastes Cornifer.
	- Using Mantis Claw and Crystal Heart to pass under conveyors (logic for *Mines_37*, including *Geo_Chest-Crystal_Peak*). Also considered Precise Movement.
    
### Shade Skips
- Shade Skips are skips which utilize the shade, often by pogoing it or using it for damage boosts.
- Generally, shade skips which require shade fireballs are not in logic, due to their RNG-dependence and relative obscurity.
- With Void Heart, shade skips (and Sibling pogos) require unequipping Void Heart, which causes the enemies to become hostile again. Void Tendrils are not currently affected by unequipping Void Heart.
- In logic, Shade Skips are indicated with the **SHADESKIPS** token. More commonly, the **$SHADESKIP** state modifier is used, sometimes with arguments such as "2HITS", indicating that the player must be able to die with at least 4 masks so that the shade can take 2 nail hits.
- Examples include:
    - Pogoing the shade in a number of places to gain extra height. Most notably, shade skips can be used to reach Salubra with no items, or Blue Lake with vertical movement.
    
### Infection Skips
- Infection Skips are skips which are only possible after Forgotten Crossroads has been infected. These skips most commonly involve pogoing infection bubbles or Furious Vengeflies.
- In logic, Infection Skips are indicated with the **INFECTIONSKIPS** token. More commonly, the **INFECTED** macro is used.
- Examples include:
    - Pogoing infection bubbles to reach the Crossroads entrance to Fungal Wastes with no other items.
	- Pogoing the Furious Vengefly to reach Salubra or the Crossroads entrance to Blue Lake with no other items.
	- Pogoing the Furious Vengefly to reach *Hallownest_Seal-Crossroads_Well* with wings or dash and airstall.

### Fireball Skips
- Fireball Skips are skips which use Vengeful Spirit or Howling Wraiths (or their upgrades) to reset fall speed midair.
- Generally, Fireball Skips in logic require at most 3 consecutive casts of a spell, meaning that Spell Twister skips are not in logic.
- In logic, Fireball Skips are indicated with the **FIREBALLSKIPS** token. More commonly, the **SPELLAIRSTALL** macro is used.
- Examples include:
    - Using airstall to reach Salubra after Gruz Mother with no other movement.
	- Using airstall to cross Queen's Station from left to right with no other movement.

### Acid Skips
- Acid Skips are skips which involve crossing over a pool of acid (or water, when the Swim ability is removed).
- In logic, Acid Skips are indicated with the **ACIDSKIPS** token. More commonly, the **LEFTSKIPACID** or **RIGHTSKIPACID** macros are used.
- Examples include:
    - Using a low height wallcling Crystal Heart to cross several acid pools, such as at the Crossroads entrance to Fog Canyon or the Fog Canyon entrance to Queen's Gardens.
    
### Spike Tunnels
- Spike Tunnels are skips which involve crossing through a narrow passage lined with spikes.
- In logic, Spike Tunnels are indicated with the **SPIKETUNNELS** token.
- Examples include:
    - Crossing the spike tunnel on the way to Glowing Womb with dash and airstall or Dashmaster.
	- Entering the spike tunnels in the Waterways broken elevator shaft, using several possible item combinations.
	- Passing through the spike tunnel before the grub in Crystal Peak, which allows reaching the upper area with wings but no claw.

### Dark Rooms
- Dark Rooms are skips which involve passing through a dark room without Lumafly Lantern.
- In logic, Dark Rooms are indicated with the **DARKROOMS** token.
- Examples include:
    - Passing through the dark Deepnest rooms.
	- Passing through the dark Howling Cliffs room before Joni's Blessing.
	- Passing through the dark Stone Sanctuary room before the No Eyes mask shard.
	- Passing through the dark Crystal Peak room before Crystallized Mound.
    
### Damage Boosts
- Damage Boosts are skips which involve intentionally taking damage from a hazard or enemy.
- In logic, Damage Boosts are indicated with the **DAMAGEBOOSTS** token.
- Examples include:
    - Jumping into the Charged Lumaflies at the top of the room to boost to *Hallownest_Seal-Fog_Canyon_East* with no other movement.
	- Jumping into the acid or thorn hazards to reach *Vessel_Fragment-Greenpath* with wings but no claw.

### Dangerous Skips:
- Dangerous Skips are skips which carry a high risk of taking damage, due to aggressive enemies or other sources.
- Generally, this modifier is not applied to Acid Skips or Spike Tunnels, which are implicitly understood to be dangerous.
- In logic, Dangerous Skips are indicated with the **DANGEROUSSKIPS** token.
- Examples include:
	- Skips requiring pogoing Dirtcarvers in the trap Deepnest entrance (logic for *Deepnest_01b[top1]*).
	- Pogoing up the Quick Slash room (Deepnest_East_14b) without items (logic for *Quick_Slash*, *Deepnest_East_14b[top1]*).
	- Various Hwurmp and/or Flukefey pogos in *Waterways_04b* (Waterways Mask Shard room).
	- Pogoing the Flukefey to cross the water with right dash and no swim in *Waterways_02*.

### Slopeballs
- Slopeballs are skips which involving casting Vengeful Spirit into a curved surface. If the surface has the right curve, and the cast is from the right position, the fireball will be curved by the collision, and slowed enough for the player to pogo it.
- In logic, Slopeballs are indicated with the **SLOPEBALLSKIPS** token. More commonly, the **$SLOPEBALL** state modifier is used, sometimes with various arguments related to tracking soul usage.
- Examples include:
  - A slopeball to reach Fungal Wastes from Forgotten Crossroads with no other items
  - A slopeball to reach Palace Grounds from Ancient Basin with only claw or wings
  - A slopeball to cross the long spike tunnel in Ancient Basin with only claw and dash
  - A slopeball to cross from left City of Tears into right City of Tears with only claw
- Note: some slopeballs are only possible with nail range extensions, or with Shaman Stone to change the spell hitbox. The slopeballs currently in logic do not require any charms.

### Shriek Pogos
- Shriek Pogos are skips where the player casts Abyss Shriek, then immediately does a double jump as the cast finishes. With the right timing, the shriek can be pogoed, cutting off the double jump into a smaller pogo, but with the benefit of refreshing the player's ability to dash or double jump. To be precise, there is a 4 frame window to press double jump, and a 6 frame window to press downslash, starting when the player regains control at the end of the cast, in order to perform a shriek pogo. The height gained from the pogo is maximized when double jump is pressed as early as possible, and nail is pressed as late as possible.
- In logic, Shriek Pogos are indicated with the **SHRIEKPOGOSKIPS** token. More commonly, the **$SHRIEKPOGO** state modifier is used, sometimes with various arguments related to the number of shriek pogos required and other soul usage tracking.
- With **$SHRIEKPOGO**, a **DIFFICULTSKIP** requirement is automatically added to chains of 4 or more shriek pogos. In general, when more than 1 shriek pogo is chained, **$SHRIEKPOGO** may require using dash between casts to stall out the soul meter long enough for it to refill.
- Examples are typically along the lines of either: using shriek pogos to gain extra horizontal movement by refreshing dash midair, or using shriek pogos to gain minor extra vertical movement in situations where wings alone are not sufficient. For example, 3 shriek pogos can be used to enter Fungal Core without claw.

### Complex Skips
- Complex Skips are skips which have extended setup time or are obscure even by the standards of advanced skips.
- In logic, Complex Skips are indicated with the **COMPLEXSKIPS** token.
- Examples include:
	- Leftward Lake of Unn acid skip, which requires spell airstall or Sharp Shadow (logic for *Fungus1_26[left1]*).
	- Swim skip in *Ruins2_07* in either direction, if done without Crystal Heart, using a combination of wings, claw, dash, and airstall or Sharp Shadow.
	- *Love_Key* acid skip using Sharp Shadow.
	- Shade skip to Blue Lake using dash and airstall (logic for *Crossroads_04[right1]*).
	- Shade skip to *Geo_Chest-Crystal_Peak* using Grubberfly's Elegy to provoke the shade.
    - Vengefly pogo in King's Pass to Howling Cliffs (logic for *Tutorial_01[top2]*).
	- Vengefly pogo in Tower of Love to above Collector (logic for *Collector's_Map*, *Grub-Collector_1*, *Grub-Collector_2*, *Grub-Collector_3*).
	- Shade skips in Beast Den without Mantis Claw (logic for locations in *Deepnest_Spider_Town*).
	- Luring a Mantis Youth to pogo to *Geo_Rock-Mantis_Village_Above_Claw* without items.
	- Fireball skip with left dash, right claw, and wings to get to *Rancid_Egg-Waterways_West_Bluggsac* from the right without swim.

### Difficult Skips
- Difficult Skips are skips which are subjectively considered to be more difficult than usual.
- In logic, Difficult Skips are indicated with the **DIFFICULTSKIPS** token.
- Examples include:
    - Item rando shade skip from chest above Baldur Shell to Howling Cliffs (logic for *Fungus1_28[left1]*).
	- Pogoing a falling Gluttinous Husk to reach the bottom left transition in lower King's Station without Swim (logic for *Ruins2_06[left2]*).

## Geo Logic
Since geo is a consumable resource that can be used or lost in many ways (and is not available from randomized items, with some cursed settings), the randomizer does not track exact numbers of geo or relics in logic. However, any location which requires geo has logic to ensure that the player can farm geo off of respawning enemies. Additionally,
- The mid-expensive locations *Dash_Slash*, *Unbreakable_Heart*, *Unbreakable_Greed*, and *Unbreakable_Strength* also have logic to require the ability to visit Lemm, to reduce the odds of required farming.
- The most expensive location *Vessel_Fragment-Basin* has logic to require the ability to visit Lemm **and** the ability to complete the second colosseum trial. Thus, the logic guarantees the ability to farm large amounts of geo through the colosseum, as well as possibly geo through relics.

## Terminal Logic and Nonterminal Logic
- Briefly, logic is terminal if it represents a goal which has a permanently saved effect. In other words, after achieving that, the player could return to start and fully reset before continuing. Terminal logic includes:
	- Logic for locations
	- Logic for world events such as defeating bosses, triggering levers, destroying breakable walls, and so on.
- Nonterminal logic includes:
	- All other waypoints, such as room waypoints, warp waypoints, *Can_Stag*, and so on.
	- Logic for transitions.
- As of version 4.1.0, the randomizer now tracks conditional logic access through state logic. State encompasses the ability to shade skip, the number of charm slots available, the amount of soul left, and similar resources. As a result, it is now possible that nonterminal logic can require the player to expend such resources (e.g. cast spells, etc). There are a few limitations of this system:
  - The ability to refill soul by killing enemies in general is not taken into account. However, where logic requires cast spells, and there are nearby enemies to refill soul, that soul may be taken into account.
  - The ability to reset state by using dream gate is generally not taken into account. This is both for consistency and to avoid requiring the player to expend too much essence using this method.
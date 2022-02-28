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
- Pogoing spikes, Garpedes, or Wingsmoulds is not considered a skip. Pogoing the shade or its projectiles is classified under Shade Skips, and not Enemy Pogos.
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
- Some shade skips are only conditionally in logic, depending on the transition randomizer setting. Limitations of the randomizer make it infeasible to calculate bench accessibility for setting up shade skips in every location.
- With Void Heart, shade skips (and Sibling pogos) require unequipping Void Heart, which causes the enemies to become hostile again. Void Tendrils are not currently affected by unequipping Void Heart.
- In logic, Shade Skips are indicated with the **SHADESKIPS** token. More commonly, the **ITEMSHADESKIPS**, **AREASHADESKIPS**, or **ROOMSHADESKIPS** macros are used.
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
- In logic, Fireball Skips are indicated with the **FIREBALLSKIPS** token. More commonly, the **AIRSTALL** macro is used.
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
- In logic, Spike Tunnels are indicated with the **SPIKETUNNELS** token. More commonly, the **LEFTTUNNEL** or **RIGHTTUNNEL** macros are used.
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


## Terminal Logic and Nonterminal Logic
- Briefly, logic is terminal if it represents a goal which has a permanently saved effect. In other words, after achieving that, the player could return to start and fully reset before continuing. Terminal logic includes:
	- Logic for locations
	- Logic for world events such as defeating bosses, triggering levers, destroying breakable walls, and so on.
- Nonterminal logic includes:
	- All other waypoints, such as room waypoints, warp waypoints, *Can_Stag*, and so on.
	- Logic for transitions.
- The randomizer does not represent conditional logic access, such as access requiring spending soul or giving up the ability to shade skip or giving up charm slot space. As a result, conditional access cannot be freely used in nonterminal logic. Instead, logic uses the following convention:
- For terminal logic:
	- The player is assumed to be able to shade skip.
	- The player is assumed to have all charm slots available.
	- The player is assumed to have full soul.
- For nonterminal logic,
	- The player cannot be required to shade skip.
	- The player cannot be required to equip charms.
	- The player cannot be required to spend soul.
- There are some conditions upon which nonterminal logic can be treated as terminal:
	- In the current mode, there is a known way to restore the spent resource. For example,
		- Fireball Skips are allowed in Room Randomizer when it is possible to refill soul in the same room from enemies after the skip.
		- Shade Skips are allowed in any mode where there is bench access after the skip.
		- Most resources become available once Dream Gate and essence is obtained, provided it is possible to set a Dream Gate in the room in question.
			- Again, note that the randomizer does not track spent essence. Logic expects that essence exceeding the essence tolerance is reachable before requiring Dream Gate, and expects the player to choose a tolerance which will be sufficient for all required warps.
	- A list of exceptions follows:
		- Shade Skips:
			- In item randomizer, the shade skip in *Crossroads_04* to Blue Lake is allowed. Airstall can also be required for the shade skip. Shade skips are not required in Blue Lake unless the player can reach the bench in Resting Grounds.
			- In item or area randomizers, the shade skip in *Fungus1_11*, the room before Massive Moss Charger, is allowed when *Fungus1_11[left1]* is not reachable otherwise, since there are no itemless skips in the following rooms which require a shade.
			- In item or area randomizers, the wings shade skip in *Fungus1_28*, the room with Baldur Shell, to reach the upper transition *Fungus1_28[left1]* without claw, is allowed since the bench at Mato is subsequently reachable.
			- In item or map area randomizer, the shade skip in *Fungus2_11*, replacing explosion pogo to reach Mantis Village with no items, is allowed since there are no itemless skips in the following rooms which require a shade.
			- In item randomizer, the wings shade skip in *Fungus2_18*, below Fungal Wastes Cornifer and heading to Spore Shroom, is allowed since there are no other wings shade skips between there and the next bench in *Deepnest_30* (Deepnest Hot Springs).
			- In item randomizer, the shade skip in *Deepnest_01b* (below the trap entrance to Deepnest) to the top transition is allowed since no other shade skips can be required to reach the bench in Queen's Station.
			- In item or map area randomizer, the left claw shade skip in *Deepnest_03* (left of Deepnest Hot Springs) to the top transition is allowed [*missing justification*].
			- In item or area randomizers, the left claw shade skip in *Deepnest_03* (left of Deepnest Hot Springs) to the bottom-left transition (toward Nosk grub) is allowed, since no left claw shade skips can be required for the following two rooms.
			- In item or area randomizers, the right claw shade skip in *Deepnest_East_03* is allowed to reach the top transition (leading directly to *Wanderer's Journal-Kingdom's_Edge_Entrance*).
			- In item or area randomizers, the wings shade skip in *Abyss_10* to the upper left transition (leading directly to *Journal_Entry-Void_Tendrils*).
			- In item or area randomizers, the itemless shade skip in *Ruins2_06* is allowed to reach the upper right transition, leading to King's Station Stag, since that room contains a bench.
			- In item or area randomizers, the right claw shade skip in *Mines_05* is allowed to reach the transitions above the breakable wall leading to Deep Focus, since the Crystal Guardian bench is then accessible.
			- In item or area randomizers, the dash only shade skip in *Fungus3_13* (right of the Queen's Gardens Stag) is allowed to reach the upper transitions, since no other dash only shade skips can be required in upper Queen's Gardens.
			- In all modes, the shade skip to reach the rest of *Fungus2_13* from the bottom left transition is allowed, since the player can use the Bretta bench after killing the shade.
		- Fireball Skips:
			- In item and area randomizers, the fireball skip in *Crossroads_04* to reach the Salubra bench is allowed since no soul is required inside the transition.
			- In item randomizer, the airstall shade skip in *Crossroads_04* to Blue Lake is allowed since no soul is required in Blue Lake unless the player can reach the enemies on the opposite side.
			- In item or area randomizers, the fireball skip in *Fungus1_11*, the room before Massive Moss Charger, is allowed when *Fungus1_11[left1]* is not reachable otherwise, since soul can be recovered in the following rooms.
			- In item or area randomizers, the wings shade skip in *Fungus1_28* to upper Howling Cliffs can require airstall. Soul can be refilled in the following rooms.
			- In all modes, the fireball skip to reach the top transition in *Fungus3_26* (right Fog Canyon shaft) is allowed, since it only requires one cast, and soul can be refilled on the subsequent enemies.
			- In item and map area randomizer, the fireball skip to cross Queen's Station is allowed, since soul can be refilled in the first room of Fungal Wastes.
			- In item or area randomizers, the fireball skip to climb *Fungus2_06* (to cross the acid pool outside Leg Eater) is allowed, since soul can be refilled on the enemies in the upper part of the room.
			- In item randomizer, the fireball skip to cross *Fungus2_21* (to reach the City Crest gate) is allowed, since soul can be refilled in the first city room.
			- In all modes, fireball skips in *Deepnest_39* (the dark Deepnest room with the grub and whispering root) are allowed, since soul can be refilled from the many enemies.
			- In item and map area randomizer, fireball skips in *Ruins2_06* (lower King's Station) to reach the bottom left transition are allowed since soul can be refilled in the neighboring city room.
			- In item randomizer, airstall can be required as part of the swim skip to cross *Ruins2_07* left to right. Soul can be refilled in the first room of Kingdom's Edge.
			- In item or area randomizers, airstall can be required as part of the swim skip to cross *Ruins2_07* right to left. Soul can be refilled in the next room of King's Station.
			- In item or area randomizers, airstall can be required to go through the spike tunnel below the grub in *Mines_03* to reach upper Crystal Peak. Soul can be refilled in later rooms.
			- In item or area randomizers, airstall can be required to cross *Mines_28* (the room outside Crystallized Mound) right to left. Soul can be refilled in the dark room.
			- In item randomizer, airstall can be required to cross *Mines_28* left to right. Only two casts are required, so another cast can be used to open Crystallized Mound, and soul can be refilled inside the room.
			- In all modes, airstall can be required to reach the Bretta bench from the bottom left transition of *Fungus2_13*, since soul can be refilled on the subsequent enemies of the room.
			- In all modes, airstall can be required to reach the rest of *Deepnest_East_04* (Bardoon's room) from the bottom right transition, since soul can be refilled on the subsequent enemies of the room.
		- Charms:
			- In item or area randomizers, Sharp Shadow can be required for the right to left King's Station swim skip. The player is expected to bench afterward at the King's Station Stag.
			- In item randomizer, Sharp Shadow can be required for the left to right King's Station swim skip. The player is expected to bench afterward at Oro.
			- In item or area randomizers, Sharp Shadow can be required for the right to left Lake of Unn acid skip. Any skips which require other charms in Unn's room will adjust logic accordingly.
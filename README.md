## Randomizer 4

Welcome to Randomizer 4, the newest version of the Hollow Knight item and transition randomizer released for the Hollow Knight 1.5 update.

If you are new to the randomizer community, please consider joining the Hollow Knight discord server and the Hollow Knight Speedrunning discord server!

## Dependencies

Randomizer 4 requires the following to run:
- The Hollow Knight Modding API for the latest patch (1.5+). Randomizer 4 is not compatible with patches 1.4.3.2 or earlier.
- The MenuChanger and ItemChanger mods.
- The RandomizerCore library.

Additionally, the following mods are strongly recommended:
- Benchwarp, which can be required by randomizer logic for warping to the start location.
- QoL, which can be required to restore skips expected in certain modes.

## Tips

- Several items can be used to kill baldurs, including: Vengeful Spirit (or upgrades), Desolate Dive (or upgrades), Grubberfly's Elegy, Glowing Womb, Dash Slash with Dash, Weaversong, Spore Shroom, Cyclone Slash, Mark of Pride, and even just Dash on its own!
    - With default skip settings, only Vengeful Spirit, Desolate Dive, Grubberfly's Elegy, and Glowing Womb can be required in logic.
Baldur HP is reduced to 5 to make slower baldur kills less tedious, and to reduce RNG.
- Several items are randomized progressively, meaning that collecting any item in a given family always gives the first upgrade, collecting another gives the second upgrade, etc. For such items, any of the pickups in the family may be forced by randomizer logic. The families this pertains to are:
    - Dream Nail, Dream Gate, Awoken Dream Nail
    - Mothwing Cloak, Shade Cloak
    - Vengeful Spirit, Shade Soul
    - Desolate Dive, Descending Dark
    - Howling Wraiths, Abyss Shriek
- Grimmchild has special behavior: collecting it automatically activates the Nightmare Lantern. Additionally, if Charms are randomized and Grimmkin Flames are not randomized, collecting Grimmchild gives the first 6 Grimmkin Flames.
- The lifeblood door in Abyss opens if you enter the room with a single lifeblood mask. In logic, it requires a lifeblood charm.
- The randomizer adds extra platforms to various places that prevent softlocks. For example, there are several platforms added to Ancient Basin to prevent having to quit-out after checking certain locations without vertical movement. These platforms may disappear once the player has sufficient abilities to no longer need them.
- With skips allowed, the player is advised to take care to not get locked out of certain required pogos. Obtain:
    - No more than 1 nail upgrade before claw or wings
    - No more than 3 nail upgrades before claw
    - In Split Claw mode, instead obtain no more than 2 nail upgrades with only a single claw piece, and any number of nail upgrades with a single claw piece as well as wings
- There are several logs created per file in the Randomizer 4 folder in the save directory to help you with your playthrough.
    - Tracker Log: this log continuously records item locations and transition connections as you discover them.
    - Helper Log: this log computes which locations/transitions are accessible with your current equipment.
    - Spoiler Log: this log lists the exact locations of every randomized item and/or transition.

## Settings

### Pool Settings

These settings control which items are randomized.
- Dreamers: Lurien, Monomon, Herrah, and World Sense. World Sense is the Black Egg Temple pickup to view completion percentage
- Skills: all spells, nail arts, and movement abilities
- Charms
- Keys: all key objects, as well as King's Brand, Godtuner, and Collector's Map
- Mask Shards
- Vessel Fragments
- Pale Ore
- Charm Notches
- Geo Chests: all geo chests, except the one above Baldur Shell and those in the Junk Pit
- Relics: all Wanderer's Journals, Hallownest Seals, King's Idols, and Arcane Eggs.
- Rancid Eggs
- Stags
- Maps
- Whispering Roots
- Grubs
- Lifeblood Cocoons
- Soul Totems
- Grimmkin Flames
- Geo Rocks
- Boss Essence: essence drops from Dream Warriors and Dream Bosses
- Boss Geo: geo drops from Gruz Mother, Massive Moss Charger, Vengefly King, Gorgeous Husk, Crystal/Enraged Guardian and both Soul Warriors
- Lore Tablets 
- Journal Entries: The Hunter's Journal, along with the Goam, Garpede, Charged Lumafly, Void Tendrils and Seal of Binding locations.
- Junk Pit Chests


### Skip Settings

These settings control which difficult skips the randomizer may potentially require. Beginners are strongly recommended to leave these settings to their defaults, as many skip categories include skips that are quite difficult or obscure.

More detail about each setting is available in the LOGIC_README.md.

### Novelty Settings

These settings add new and unusual features to the randomizer:
- Randomize Swim: All pools of water become dangerous, and a new Swim item is randomized which allows swimming in water normally.
- Randomize Elevator Pass: The large City of Tears elevators to Forgotten Crossroads and Resting Grounds will be closed until the new Elevator Pass item is obtained. Additionally, a new randomized location is added in place of the toll machine next to the left elevator.
- Randomize Nail: Adds three extra items to the pool that allow swinging the nail left, right and up; by default in this mode, the nail can only be swung down.
- Randomize Focus: Removes the ability to heal and adds it as a randomized item. Additionally, a new randomized location is added in front of the Focus tutorial tablet in King's Pass.
- Split Claw: Replaces Mantis Claw with two items, a "Left Mantis Claw" that works only on left walls, and a "Right Mantis Claw" that works only on the right. Replaces the Mantis Claw location with two locations in Mantis Village.
- Split Cloak: Replaces Mothwing Cloak with two items, a "Left Mothwing Cloak" that lets you dash to the left, and a "Right Mothwing Cloak" that dashes to the right. Also adds a new location dropped after defeating Hornet in Greenpath. Shade Cloak is not split, meaning that there are effectively 3 dash upgrades in this mode: the ability to dash left, to dash right, and to shadowdash.
    - Dash items remain progressive. In this mode, Shade Cloak has a random directional bias, and will give the dash of that direction if it has not yet been obtained.
- Egg Shop: instead of summoning your shade, Jiji will now let you buy items for rancid eggs. If you've given her enough rancid eggs, any items you've reached the cost for will be summoned.

### Cost Settings

These settings control how certain randomized costs are generated for the following:
- Grubs (e.g. number required for a Grubfather location)
- Essence (e.g. number required for a Seer location)
- Rancid Eggs (e.g. number required for an Egg Shop location)
- Charms (e.g. number required for certain Salubra locations when Salubra Charm Notches are randomized)

For each category, there is a minimum, maximum, and tolerance. The costs will be taken uniformly at random between the minimum and maximum (inclusive), and the locations will be randomized under the constraint that for the cost to be affordable, the number of reachable items must exceed the cost by tolerance. Concretely, if the grub cost settings give a minimum of 1, a maximum of 23, and a tolerance of 2, then a possible cost is 19, and that cost will be in logic when 19 + 2 = 21 grubs are reachable.

### Long Location Settings

Settings which allow selectively not randomizing certain long locations, or modifying certain item previews.
- Randomization In White Palace: Allows removing all randomized items and locations in Path of Pain, or White Palace as a whole, and leaving them vanilla. For historical reasons, the King Fragment item and location are not affected by this setting, and are always randomized with the Charms pool.
    - In transition randomizer, this setting also unrandomizes transitions. The Exclude Path of Pain setting unrandomizes all transitions in Path of Pain and the transition leading to Path of Pain. The Exclude White Palace setting unrandomizes all transitions in White Palace and Path of Pain.
- Boss Essence Randomization: Allows selecting which bosses are included in the Boss Essence pool; for example, giving the option to remove White Defender and Grey Prince Zote, or to remove all Dream Bosses leaving only Dream Warriors.

For each preview setting, toggling the setting off will result in the corresponding location(s) no longer allowing preview of their items. For locations which carry randomized costs, the preview setting has four options:
- Cost And Name: default preview behavior
- Cost Only: item names will be redacted from the preview
- Name Only: cost details will be redacted from the preview
- None: all details except number of items will be redacted from the preview


### Start Location Settings

These settings control how the start location of the randomizer is determined. The key field is Start Location Type, with the following choices:
- Fixed: The start location will be that chosen by the user in the menu.
- Random: The start location will be chosen uniformly at random from the highlighted options.
- Random Excluding KP: The same as above, except that King's Pass will not be one of the options.

The possible random starts depend on the randomization settings as a whole, as some start locations do not lead to viable starts with certain settings. More starts can be unlocked by including more skips, more transition randomization, and more start item randomization.

### Start Item Settings

These settings control what items will be given to the player when the game begins:
- Start Geo: Gives a random amount of geo between the minimum and maximum inclusive.
- Vertical: Controls whether the player starts with Mantis Claw, Monarch Wings, or both.
- Horizontal: Controls whether the player starts with Mothwing Cloak, Crystal Heart, or both.
- Charms: Controls what random equipped charms the player may start with.
- Stags: Controls what unlocked stags the player may start with.
- Misc Items: Controls what various other items the player may start with, including: Dream Nail, Isma's Tear, Vengeful Spirit, Desolate Dive, Howling Wraiths, Cyclone Slash, Great Slash, Dash Slash, Simple Key, Elegant Key, Tram Pass, King's Brand, Love Key, Lumafly Lantern, Shopkeeper's Key, and City Crest.

### Misc Settings

Settings which do not fit into the other classifications.
- Randomize Notch Costs: This setting randomizes the costs of all 40 charms uniformly so that the costs add to a total between 70 and 110. The notch cost will subsequently be displayed as part of the charm name. This does not affect future cost modifications, such as when Carefree Melody or Void Heart are obtained.
- Extra Platforms: Adds small platforms near locations where the player would otherwise softlock, mainly in areas like Royal Waterways or Ancient Basin where vertical movement is often needed.
- Salubra Notches: Allows the player to choose whether Salubra Charm Notches are randomized as part of the Charm Notches pool, are always vanilla or random, or should behave as in Randomizer 2 and 3, where each notch is automatically given when the player collects 5, 10, 18, and 25 notches.
- Mask Shards: Allows replacing randomized mask shards with double or quadruple shards, adding up to the same number of masks.
- Vessel Fragments: Allows replacing randomized vessel fragments with double or triple fragments, adding up to the same number of vessels.
    - These last two settings may be desired to reduce the number of items, particularly in opposition to Duplicate Items.


### Cursed Settings

- Longer Progression Chains: Applies a random priority penalty to each of Mothwing Cloak, Mantis Claw, Crystal Heart, Monarch Wings, Shade Cloak, Dream Nail, Vengeful Spirit, Desolate Dive, Howling Wraiths, Void Heart, Split Cloak items, and Split Claw items, making them less likely to be early progression.
- Replace Junk With One Geo: Replaces randomized Mask Shards, Vessel Fragments, Pale Ore, Charm Notches, Geo, Rancid Eggs (when Egg Shop is not randomized), Relics, and Soul with One Geo pickups. The removed items are then unobtainable, making the resulting seed much more difficult.
- Remove Spell Upgrades: Removes Shade Soul, Descending Dark, and Abyss Shriek. Additionally, modifies spells so that they cannot be upgraded beyond the first level, even with duplicate items.
- Cursed Masks: The Knight starts with only one mask, and an additional 16 mask shards are randomized. Be warned that this setting interacts with Replace Junk With One Geo to remove the added mask shards, if both are active.
- Cursed Notches: The Knight starts with only one charm notch, and an additional 2 charm notches are randomized. Be warned that this setting interacts with Replace Junk With One Geo to remove the added charm notches, if both are active.
- Randomize Mimics: Add the four non-colo mimic locations to the randomization pool (some grubs may turn into mimics). With grubs unrandomized, instead randomize the mimics and grubs among themselves.
- Maximum Grubs Replaced By Mimics: When Randomize Mimics is enabled, this setting allows the randomizer to replace grubs with mimics. The maximum here is inclusive. Grub replacement can happen whether grubs are randomized or not. The number of replaced grubs will be clamped to 46 - CostSettings.MaximumGrubCost - CostSettings.GrubTolerance, to ensure all grub costs remain payable.

### Transition Settings

Settings which control which transitions between rooms are randomized. There are three main transition randomizer modes:
- None: no transitions are randomized
- Map Area Randomizer: transitions between map areas are randomized.
    - Maps refer to the maps purchased from Cornifer or Iselda, and the initial map of King's Pass and Dirtmouth.
- Full Area Randomizer: transitions between titled areas are randomized. 
    - A titled area is defined to be an area with title text, i.e. a name which pops up as onscreen text.
    - Transitions between titled areas agree with the transitions between map areas where possible. The remaining transitions are chosen subjectively, though in most cases the obvious choice is taken.
- Room Randomizer: almost all room transitions are randomized, excluding:
    - Warps of any kind, including those entering dream areas
    - Trams and elevators
    - Transitions within Godhome, the Shrine of Believers and the Black Egg Temple
    - The transitions leading to Sly's storeroom, Bretta's basement, or to any trial of the colosseum

There are additional settings which control the randomization of transitions:
- Area Constraint: Forces the randomizer to connect transitions to the same area if possible. This does not guarantee that areas are connected, but it gives a high likelihood that a transition will stay within the same area.
- Transition Matching: Controls how transition directions affect randomization. Note that independent of this setting, one-way transitions always map to one-way transitions.
    - Matching Directions: left transitions must map to right transitions, top transitions must map to bottom transitions, and so on.
        - Door transitions are assigned directions so that the transition count is balanced. Since there are two more doors and right transitions total than left transitions (due to the door in Dirtmouth leading to a right transition in Bretta's room), one door will act as a left transition and all others will act as a right transition.
    - Matching Transitions and no Door to Door: This settings is the same as the above, except that additionally there can be no door to door transitions.
    - Nonmatching Directions: Directions are not considered in randomization, and one transition can map to any other.
- Coupled: Determines whether transitions must be reversible, in the sense that entering a transition and then going back returns to the original place. With decoupled transitions, going back can lead to a new transition.

Some important Room Randomizer tips:
- The nightmare lantern must be lit to check Grimmchild
- Sly must be rescued to use his shop

### Progression Depth Settings

These settings control how item-location placements are determined at the end of the randomizer:
- Multi Location Penalty: for each multi location (e.g. shops, Grubfather, Seer), moves all but one of its placements to the end of the location list, to strongly reduce its chances of having multiple progression items.
- Duplicate Item Penalty: for each item invisible to logic (e.g. those added by Duplicate Items), moves it to the front of the item list. This means that these items are placed immediately after progression, generally in deep non-shop locations.

The remaining settings relate to the depth priority transform. Briefly, this determines whether locations which became reachable later in randomization should be weighted more strongly than other locations. For example, a Transform Type of Linear and a Coefficient of 3 imply that a location placed one step later in randomization should be advanced past 3% of the locations relative to the original order.

TODO: A more complete explanation of the settings.

### Duplicate Item Settings

These settings control which important items are duplicated. Duplicate items generated by the randomizer are modified to be invisible to logic, and may be penalized in randomization depending on the Progression Depth Settings. When there are not enough locations for the number of items, extra locations are created from the randomized shops.

However, there is one important exception: the default Simple Key Handling is to create two Simple Keys which are identical to normal Simple Keys. This increases the odds of Simple Key progression, to make up for the fact that all Simple Key locks require 4 Simple Keys in logic in Randomizer 4.


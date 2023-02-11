A connection is a mod which integrates with rando to add new features, such as logic edits, custom items and locations, or special randomization requirements.

## The Checklist

This section contains a checklist of steps to create a connection mod. Additional details on the options for each step are given in subsequent sections.

* Define any added [items](#defining-custom-items) and [locations](#defining-custom-locations) in ItemChanger and Randomizer.
  - Add the items and locations to the randomization request ([example](https://github.com/flibber-hk/HollowKnight.RandomizableLevers/blob/c016cfc93dc37c5f4b16dd279c16a2f6fe0d9c66/RandomizableLevers/Rando/RequestModifier.cs#L163-L167)). 
  In particular, this is how to inform [RandoMapMod](https://github.com/syyePhenomenol/RandoMapMod/blob/master/RandoMapMod/Pins/InteropProperties.cs) of the locations ([example](https://github.com/flibber-hk/HollowKnight.RandoPlus/blob/95305e4642bdd2535d683f33438180f701be6254/RandoPlus/GhostEssence/ICInterop.cs#L45-L49)).
  - Add metadata to items to be read by [CMICore](https://github.com/BadMagic100/ConnectionMetadataInjector) and its consumers ([example](https://github.com/flibber-hk/HollowKnight.RandoPlus/blob/95305e4642bdd2535d683f33438180f701be6254/RandoPlus/MrMushroom/ICInterop.cs#L27)).
  - Apply a group resolver for the randomized items and locations ([easy example](https://github.com/flibber-hk/HollowKnight.RandoPlus/blob/95305e4642bdd2535d683f33438180f701be6254/RandoPlus/MrMushroom/RequestMaker.cs#L118-L134)) ([complex example](https://github.com/flibber-hk/HollowKnight.RandomizableLevers/blob/c016cfc93dc37c5f4b16dd279c16a2f6fe0d9c66/RandomizableLevers/Rando/RequestModifier.cs#L96-L113)).
  - Make added items/locations respect base rando settings ([example](https://github.com/flibber-hk/HollowKnight.RandomizableLevers/blob/c016cfc93dc37c5f4b16dd279c16a2f6fe0d9c66/RandomizableLevers/Rando/RequestModifier.cs#L27-L40)).
  At time of writing, the only settings treated specially are WP rando and Deranged.
  - Add important items/locations to the condensed spoiler logger ([example](https://github.com/flibber-hk/HollowKnight.RandomizableLevers/blob/c016cfc93dc37c5f4b16dd279c16a2f6fe0d9c66/RandomizableLevers/Rando/RandoInterop.cs#L26-L28)).
This is done with MonoMod.ModInterop - copy over the file from [here](https://github.com/flibber-hk/HollowKnight.RandomizableLevers/blob/main/RandomizableLevers/Rando/CondensedSpoilerLogImport.cs) to safely do this, so as to not take on a runtime dependency on condensed spoiler logger.
  - Interop with [RandoVanillaTracker](https://github.com/syyePhenomenol/HollowKnight.RandoVanillaTracker) to allow the user to track vanilla placed items ([example](https://github.com/flibber-hk/HollowKnight.RandomizableLevers/blob/c016cfc93dc37c5f4b16dd279c16a2f6fe0d9c66/RandomizableLevers/Rando/RandoInterop.cs#L35)).

* Change any [existing logic](#editing-existing-logic) that conflicts with your connection.

* Create a menu page to allow the player to manage settings ([simple example](https://github.com/flibber-hk/HollowKnight.RandomizableLevers/blob/3f509d51de4758a9b21c5effa573762dc126a3a5/RandomizableLevers/Rando/RandoMenuPage.cs#L26)) ([complex example](https://github.com/BadMagic100/TheRealJournalRando/blob/6890f9e6b5ae30777c0043139302b4f2762da9ae/TheRealJournalRando/Rando/ConnectionMenu.cs)).
  - For other ways to construct the menu, look at examples from [base rando](https://github.com/homothetyhk/RandomizerMod/blob/master/RandomizerMod/Menu/RandomizerMenu.cs) and the [MenuChanger documentation](https://github.com/homothetyhk/HollowKnight.MenuChanger).
  - Colour the menu button according to whether the settings are active ([example](https://github.com/homothetyhk/BenchRando/blob/a5d1d9fa95aed08f1d7500e319369f21350a9ffb/BenchRando/Rando/ConnectionMenu.cs#L42)).
  - Interop with [Rando Settings Manager](https://badmagic100.github.io/RandoSettingsManager/) to allow players to share and manage settings more easily ([example](https://github.com/BadMagic100/TheRealJournalRando/blob/6890f9e6b5ae30777c0043139302b4f2762da9ae/TheRealJournalRando/Rando/RandoInterop.cs#L20-L23)).
  - Add settings to the settings log ([simple example](https://github.com/flibber-hk/HollowKnight.RandomizableLevers/blob/c016cfc93dc37c5f4b16dd279c16a2f6fe0d9c66/RandomizableLevers/Rando/RandoInterop.cs#L23)) ([complex example](https://github.com/dplochcoder/HollowKnight.MoreDoors/blob/582b56a6093fbaf1ad43022e5a6b4d1c2411fbb6/MoreDoors/Rando/RandoInterop.cs#L27-L28)).
  - Explicitly modify the hash if making changes that should be noted but don't affect placements ([example](https://github.com/BadMagic100/MajorItemByAreaTracker/blob/994891502230c91d23cbdc78c6cc567c7a3eb0eb/SemiSpoilerLogger/MajorItemByAreaTracker.cs#L46)).
* Do any other things you need to in order to make your stuff work!

## Defining Custom Items
Items need to be defined in logic for the randomizer to use, and in ItemChanger to be given in game. This step is **mandatory** if custom items will be used.

### Defining Items in ItemChanger

The easiest way to define a custom item in ItemChanger is to use `ItemChanger.Finder.DefineCustomItem` ([Example](https://github.com/flibber-hk/HollowKnight.RandoPlus/blob/main/RandoPlus/MrMushroom/ICInterop.cs)). This works as long as
- You know the names of the items you need in advance.
- The IC implementation of the item is always the same.
- The names of the items in IC are the same as the names of the items as used by the randomizer.

If these conditions do not all hold, there are two options:
- Subscribe to `ItemChanger.Finder.GetItemOverride` with a delegate which constructs the correct item from its name.
- Edit the `realItemCreator` delegate on the `ItemRequestInfo` defined in the `RequestBuilder` for the item.

### Defining Items in RandomizerMod

The easiest way to define a custom item in RandomizerMod is to subscribe to `RCData.RuntimeLogicOverride`, and add the item to the LogicManagerBuilder via `lmb.AddItem`. To provide additional metadata or instance-specific information, edit the `ItemRequestInfo` defined in the `RequestBuilder` for the item.

## Defining Custom Locations
Locations need to be defined in logic for the randomizer to use, and in ItemChanger to place items in game. This step is **mandatory** if custom locations will be used.

### Defining Locations in ItemChanger

The easiest way to define a custom location in ItemChanger is to use `ItemChanger.Finder.DefineCustomLocation` ([Example](https://github.com/flibber-hk/HollowKnight.RandoPlus/blob/main/RandoPlus/MrMushroom/ICInterop.cs)). This works as long as
- You know the names of the locations you need in advance.
- The IC implementation of the location and the placement constructed from the location is always the same.
- The names of the locations in IC are the same as the names of the locations as used by the randomizer.

If these conditions do not all hold, there are two options:
- Subscribe to `ItemChanger.Finder.GetLocationOverride` with a delegate which constructs the correct location from its name.
- Edit the `customPlacementFetch`, `onPlacementFetch`, or `customAddToPlacement` delegates on the `LocationRequestInfo` defined in the `RequestBuilder` for the location.

### Defining Locations in RandomizerMod

The easiest way to define a custom location in RandomizerMod is to subscribe to `RCData.RuntimeLogicOverride`, and add the location to the LogicManagerBuilder via `lmb.AddLogicDef`. To provide additional metadata or instance-specific information, edit the `LocationRequestInfo` defined in the `RequestBuilder` for the item.

If you have a json file of locations, in the same format as the RM locations.json, you can define all logic defs simultaneously through `lmb.DeserializeJson`.

## Editing Existing Logic

There are two easy ways to edit logic. For both, you will need to subscribe to `RandomizerMod.RC.RCData.RuntimeLogicOverride` to access the `LogicManagerBuilder` (`lmb`).
- Call `lmb.DoLogicEdit` to edit logic for locations, transitions, and waypoints. Call `lmb.DoMacroEdit` to edit logic for macros. If you have a json file containing your logic edits, you can provide the file to the `lmb` instead with `lmb.DeserializeJson`.
  - For editing existing logic, you can use the `ORIG` token similarly to a macro. When the logic edit is performed, each instance of `ORIG` will be replaced with the previous logic. A very common pattern for logic edits is to take the form `ORIG | NEWOPTION` where `NEWOPTION` is replaced by an expression containing alternate ways to reach the location added by the connection. This form of edits is highly preferred, since it works well for allowing multiple connections to make edits in a compatible way.
  - Sometimes, on the other hand, you may need to make logic more restrictive or to change something deeply nested within logic. For this, you can use `lmb.DoLogicSubst` or provide a json file of logic substitutions. A substitution replaces a specified token with an expression. You can substitute in constant tokens (`TRUE` and `FALSE`) to delete terms or branches of logic.

A connection is a mod which integrates with rando to add new features, such as logic edits, custom items and locations, or special randomization requirements.

## The Checklist

This section contains a checklist of mandatory and optional steps to create a connection mod. Additional details on the options for each step are given in subsequent sections.

- (mandatory) Define any custom locations and items in itemchanger, using Finder.DefineCustomLocation and Finder.DefineCustomIte ([Example](https://github.com/flibber-hk/HollowKnight.RandoPlus/blob/main/RandoPlus/MrMushroom/ICInterop.cs))
- (mandatory) Define the logic for custom locations ([Example](https://github.com/flibber-hk/HollowKnight.RandoPlus/blob/main/RandoPlus/MrMushroom/LogicAdder.cs#L28))
- (mandatory) Define logic items for custom items ([Example](https://github.com/flibber-hk/HollowKnight.RandoPlus/blob/main/RandoPlus/MrMushroom/LogicAdder.cs#L36))
  - If you're just adding items that you don't want to have an effect on logic, simply call `lmb.AddItem(new EmptyItem(name))` for each added item
- (optional) Edit the logic for existing locations, transitions and waypoints ([Example](https://github.com/flibber-hk/HollowKnight.RandoPlus/blob/main/RandoPlus/RemoveUsefulItems/LogicPatcher.cs#L28))
- (recommended) Edit the LocationRequestInfo and ItemRequestInfo in Rando ([Example](https://github.com/flibber-hk/HollowKnight.RandoPlus/blob/main/RandoPlus/MrMushroom/RequestMaker.cs#L74-L96))
- (optional) Apply a group resolver for the randomized items and locations; if you don't do this then they will be placed in the main item group by default ([Example (easy option - copy another item's group)](https://github.com/flibber-hk/HollowKnight.RandoPlus/blob/main/RandoPlus/MrMushroom/RequestMaker.cs#L98-L114)) ([Example (slightly less easy option - allow the user to create their own group if they like)](https://github.com/flibber-hk/HollowKnight.RandomizableLevers/blob/main/RandomizableLevers/Rando/RequestModifier.cs#L96-L113))
- (mandatory) Add the items to the randomization request ([Example](https://github.com/flibber-hk/HollowKnight.RandomizableLevers/blob/main/RandomizableLevers/Rando/RequestModifier.cs#L163-L167))
- (optional) Make added items/locations respect base rando settings ([Example](https://github.com/flibber-hk/HollowKnight.RandomizableLevers/blob/main/RandomizableLevers/Rando/RequestModifier.cs#L27-L40))
  - At time of writing, the only settings treated specially in the example are WP rando and Deranged.
- (recommended) Create a menu page to allow the player to manage the added items and locations ([Simple Example](https://github.com/flibber-hk/HollowKnight.RandomizableLevers/blob/main/RandomizableLevers/Rando/RandoMenuPage.cs#L26-L30)) ([Complex Example](https://github.com/BadMagic100/TheRealJournalRando/blob/master/TheRealJournalRando/Rando/ConnectionMenu.cs))
 - For other (possibly better) ways to construct the menu, look at examples from [base rando](https://github.com/homothetyhk/RandomizerMod/blob/master/RandomizerMod/Menu/RandomizerMenu.cs) and the [MenuChanger documentation](https://github.com/homothetyhk/HollowKnight.MenuChanger)
- (optional) Add relevant settings to the Randomizer settings log ([Example](https://github.com/flibber-hk/HollowKnight.RandomizableLevers/blob/main/RandomizableLevers/Rando/RandoInterop.cs#L18))
- (optional) Add important items/locations to the condensed spoiler logger ([Example](https://github.com/flibber-hk/HollowKnight.RandomizableLevers/blob/main/RandomizableLevers/Rando/RandoInterop.cs#L21))
  - This is done with MonoMod.ModInterop - copy over the file from [here](https://github.com/flibber-hk/HollowKnight.RandomizableLevers/blob/main/RandomizableLevers/Rando/CondensedSpoilerLogImport.cs) to safely do this, so as to not take on a runtime dependency on condensed spoiler logger.
- (optional) Add `IInteropTag` to items/locations being added as specified by the CustomMetadataInjector so connections such as RandoStats and MapModS will know what to do with them ([Example](https://github.com/flibber-hk/HollowKnight.RandoPlus/blob/main/RandoPlus/SupplementalMetadataTagFactory.cs))
- (optional) Do any other things you need to in order to make your stuff work!

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
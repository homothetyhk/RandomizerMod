This article explains the logic variables defined in the randomizer. Variables are organized by type.

Unless otherwise indicated, all variables in this section have the syntax `PREFIX[P1,P2,...,PN]` where `PREFIX` is a fixed string and `P1` through `PN` are parameters. If there are no parameters, the square brackets can be omitted.

## LogicInts

- NotchCostInt, SafeNotchCostInt
  - Prefix: `$NotchCost`, `$SafeNotchCost`
  - Required Parameters: a sequence of comma-separated integers, representing 1-based charm ids.
  - Optional Parameters: none
  - These LogicInts return the number of notches needed to equip the corresponding combination of charms, minus 1. $NotchCost gives the number allowing for overcharming, while SafeNotchCost gives the number without overcharming. Obsoleted by EquipCharmVariable ($EQUIPCHARM), which has better interoperability.

## StateProviders

- StartLocationDelta
  - Syntax: `$StartLocation[NAME]` where `NAME` is the name of a start def. For example, for the King's Pass start, `NAME` is `King's Pass`.
  - This is a `LogicInt` which is `TRUE` exactly when the current start def matches its argument, and otherwise is false. It provides the default state (`StateManager.StartStateSingleton`) when it is the first state providing term or variable in an expression.

## StateModifiers

- BenchResetVariable
  - Prefix: `$BENCHRESET`
  - Required Parameters: none
  - Optional Parameters: none
  - This is a `StateResetter` which applies the effect of resting at a bench.
    - Field-resetting is opt-out, via a "BenchResetCondition" property on the field which provides single-state infix logic that returns true when the field should be resetted.

- CastSpellVariable
  - Prefix: `$CASTSPELL`
  - Required Parameters: none
  - Optional Parameters:
    - any integer parameters: parses to an array of ints, which represent number of spell casts, where time passes between different entries of the array (i.e. soul reserves can refill, etc).
      - If missing, number of casts is new int[]{1}
    - a parameter beginning with "before:": tries to convert the tail of the parameter to the NearbySoul enum (either by string or int parsing). Represents soul available before any spells are cast.
    - a parameter beginning with "after:": tries to convert the tail of the parameter to the NearbySoul enum (either by string or int parsing). Represents soul available after all spells are cast.
  - Represents the effect on soul of casting spells sequentially.

- EquipCharmVariable, FragileCharmVariable, WhiteFragmentEquipVariable
  - Prefix: `$EQUIPPEDCHARM`
  - Required Parameters:
    - First parameter MUST be either: the name of the charm term (e.g. Gathering_Swarm) or the 1-based charm ID (for Gathering Swarm, 1).
  - Optional Parameters: none
  - Represents the effect of equipping a charm. Uses `MAXNOTCHCOST` to efficiently overcharm if needed. Cannot overcharm if the player has already taken damage. Fragile charms cannot be equipped if the player does not have the ability to repair them, or if they are broken in the current state. Charm 36 (Kingsoul/Void Heart) checks that the player at least 2 white fragments, and uses notch cost 0 if the player has 3 white fragments.

- FlowerProviderVariable
  - Prefix: `$FLOWERGET`
  - Required Parameters: none
  - Optional Parameters: none
  - Sets `NOFLOWER` false, representing the effect of receiving the delicate flower.

- HotSpringResetVariable
  - Prefix: `$HOTSPRINGRESET`
  - Required Parameters: none
  - Optional Parameters: none
  - This is a `StateResetter` which applies the effect of resting in a hot springs.
    - Field-resetting is opt-in, via a "HotSpringResetCondition" property on the field which provides single-state infix logic that returns true when the field should be resetted.

- SaveQuitResetVariable
  - Required Parameters: none
  - Optional Parameters: none
  - This is a `StateResetter` which provides the effect of warping via Benchwarp or savequit, regardless of destination type.
    - Field-resetting is opt-in, via a "SaveQuitResetCondition" property on the field which provides single-state infix logic that returns true when the field should be resetted.

- ShadeStateVariable
  - `$SHADESKIP`
  - Required Parameters: none
  - Optional Parameters:
    - an integer followed by "HITS" (e.g. "2HITS"), denoting the number of nail hits the shade must be able to survive. Defaults to 1.
  - Represents the effect of dying to set up a shade. If the player has and can dream gate, a second path considers the result of dream gating before and after the shade skip.
  - To succeed, requires:
    - The `SHADESKIPS` setting enabled
    - The `CANNOTSHADESKIP` state bool set to false
    - The `USEDSHADE` state bool set to false
    - The state must have enough max HP for the shade health requirement
    - The state must not require more than 66 max soul
  - Sets `NOFLOWER` true and `USEDSHADE` true. Fragile Heart can be equipped to reach a max HP requirement, and can be broken if its conditions are met.

- StagStateVariable
  - Prefix: `$STAGSTATEMODIFIER`
  - Required Parameters: none
  - Optional Parameters: none
  - Represents the effect of riding a stag. Sets `NOFLOWER` true.

- StartRespawnResetVariable
  - Prefix: `$STARTRESPAWN`
  - Required Parameters: none
  - Optional Parameters: none
  - This is a `StateResetter` which provides the effect of the start respawn. Typically used via `$WARPTOSTART`.
    - Field resetting is opt-in, via a "StartRespawnResetCondition" property on the field which provides single-state infix logic that returns true when the field should be resetted.

- TakeDamageVariable
  - Prefix: `$TAKEDAMAGE`
  - Required Parameters:
    - If any parameters are provided, the first should be an int representing the number of 1-damage hits. Defaults to 1.
  - Optional Parameters: none
  - This is a state modifier which represents the effect of taking damage. On the first hit, sets `HASTAKENDAMAGE`, and emits all equippable combinations of `CHARM` and `noCHARM` bools for the following charms: Lifeblood Heart, Lifeblood Core, Joni's Blessing, Fragile Heart. Subsequently, increments `SPENTBLUEHP` and `SPENTHP` accounting for overcharming to handle all damage.

- WarpToBenchResetVariable
  - Prefix: `$WARPTOBENCH`
  - Required Parameters: none
  - Optional Parameters: none
  - Provides the effect of warping to a bench via Benchwarp or savequit. Does not verify whether the player can warp to a bench.
    - Implemented by applying `$SAVEQUITRESET`, then `$BENCHRESET`.

- WarpToStartResetVariable
  - Prefix: `$WARPTOSTART`
  - Required Parameters: none
  - Optional Parameters: none
  - Provides the effect of warping to start via Benchwarp or savequit.
    - Implemented by applying `$SAVEQUITRESET`, then `$STARTRESPAWN`.
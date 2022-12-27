This article explains the logic variables defined in the randomizer. Variables are organized by type.

Unless otherwise indicated, all variables in this section have the syntax `PREFIX[P1,P2,...,PN]` where `PREFIX` is a fixed string and `P1` through `PN` are parameters. If there are no parameters, the square brackets can be omitted.

## LogicInts

- NotchCostInt, SafeNotchCostInt
  - Prefix: `$NotchCost`, `$SafeNotchCost`
  - Required Parameters: a sequence of comma-separated integers, representing 1-based charm ids.
  - Optional Parameters: none
  - These LogicInts return the number of notches needed to equip the corresponding combination of charms, minus 1. $NotchCost gives the number allowing for overcharming, while SafeNotchCost gives the number without overcharming. Obsoleted by EquipCharmVariable ($EQUIPCHARM), which has better interoperability.

## StateVariables

- RequireFlowerVariable
  - Prefix: `$FLOWER`
  - Required Parameters: none
  - Optional Parameters: none
  - Returns true if the input state union contains a state with "NOFLOWER" set to false; in other words, a state with access to the delicate flower.

## StateProviders

- StartLocationDelta
  - Syntax: `$StartLocation[NAME]` where `NAME` is the name of a start def. For example, for the King's Pass start, NAME is `King's Pass`.
  - This is a `LogicInt` which is `TRUE` exactly when the current start def matches its argument, and otherwise is false. It provides the default state (`StateManager.StartStateSingleton`) when it is the first state providing term or variable in an expression.

## StateModifiers

- BenchResetVariable
  - Prefix: `$BENCHRESET`
  - Required Parameters: none
  - Optional Parameters: 
    - "noDG", if it is not possible to place a dream gate near the bench.
  - This is a `StateResetter` which applies the effect of resting at a bench.
    - Field-resetting is opt-out, via a "BenchResetCondition" property on the field which provides single-state infix logic that returns true when the field should be resetted.
    - Also provides the start state, if dream gate can be placed at the bench and the player has dream gate progression.

- CastSpellVariable
  - Prefix: `$CASTSPELL`
  - Required Parameters: none
  - Optional Parameters:
    - any integer parameters: parses to an array of ints, which represent number of spell casts, where time passes between different entries of the array (i.e. soul reserves can refill, etc).
      - If missing, number of casts is new int[]{1}
    - a parameter beginning with "before:": tries to convert the tail of the parameter to the NearbySoul enum (either by string or int parsing). Represents soul available before any spells are cast.
    - a parameter beginning with "after:": tries to convert the tail of the parameter to the NearbySoul enum (either by string or int parsing). Represents soul available after all spells are cast.
    - a parameter equal to "noDG": indicates that dream gate is not possible after the cast.
  - Represents the effect on soul of casting spells sequentially.

- EquipCharmVariable, FragileCharmVariable
  - Prefix: `$EQUIPPEDCHARM`
  - Required Parameters:
    - First parameter MUST be either: the name of the charm term (e.g. Gathering_Swarm) or the 1-based charm ID (for Gathering Swarm, 1).
  - Optional Parameters: none
  - Represents the effect of equipping a charm. Uses MAXNOTCHCOST to efficiently overcharm if needed. Cannot overcharm if the player has already taken damage. Fragile charms cannot be equipped if the player does not have the ability to repair them, or if they are broken in the current state.

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

- ShadeStateVariable
  - Required Parameters: none
  - Optional Parameters:
    - "noDG", if it is not possible to dream gate immediately after the skip
    - an integer followed by "HITS" (e.g. "2HITS"), denoting the number of nail hits the shade must be able to survive. Defaults to 1.
  - Represents the effect of dying to set up a shade. Requires the SHADESKIPS setting to succeed. Resets NOFLOWER. If the player has and can dream gate, has no further effect. Otherwise, sets USEDSHADE.

- StagStateVariable
  - Required Parameters: none
  - Optional Parameters: none
  - Represents the effect of riding a stag. Resets NOFLOWER.

- TakeDamageVariable
  - Required Parameters:
    - If any parameters are provided, the first should be an int representing the number of 1-damage hits. Defaults to 1.
  - Optional Parameters:
    - "noDG", if it is not possible to dream gate immediately after taking damage.
    - "canRegen", if it is possible to pause to regain health between hits.
  - This is a state modifier which represents the effect of taking damage. On the first hit, sets HASTAKENDAMAGE, and emits all equippable combinations of CHARM and noCHARM bools for the following charms: Hiveblood, Lifeblood Heart, Lifeblood Core, Joni's Blessing, Fragile Heart. Subsequently, increments SPENTBLUEHP and SPENTHP accounting for overcharming to handle all damage. If "canRegen" is set, allows Hiveblood recovery between hits.
  
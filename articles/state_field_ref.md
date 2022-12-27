This article explains the state fields defined in the randomizer. Unless otherwise noted, all fields default to `false` or `0`. For detail on more specific properties, see the state.json field in the randomizer resources.

## Bool Fields
- "USEDSHADE"
  - If true, the path to the current point represented by this state requires an intermediate death without any subsequent bench/dream gate.
- "OVERCHARMED"
  - If true, the player is overcharmed.
- "SPENTALLSOUL"
  - If true, the player should be assumed to have no soul, regardless of other state fields.
- "CANNOTREGAINSOUL"
  - If true, the state should not be allowed to regain soul.
- "CANNOTSHADESKIP"
  - If true, the state cannot shade skip.
- "HASTAKENDAMAGE"
  - If true, the player has taken damage. This means that damage-affecting state choices such as overcharming should now be treated as frozen.
- "BROKEHEART"
  - If true, the path to the current point represented by this state requires an intermediate death with Fragile Heart equipped. Breaking fragile charms should only be required in logic if fragile charm repair is accessible. Though fragile charm repair is reachable via benchwarp in standard randomizer, it should not be assumed to be trivially reachable in general, so the fact that charms have been broken should be propagated along paths.
- "BROKEGREED"
  - If true, the path to the current point represented by this state requires an intermediate death with Fragile Greed equipped. Breaking fragile charms should only be required in logic if fragile charm repair is accessible. Though fragile charm repair is reachable via benchwarp in standard randomizer, it should not be assumed to be trivially reachable in general, so the fact that charms have been broken should be propagated along paths.
- "BROKESTRENGTH"
  - If true, the path to the current point represented by this state requires an intermediate death with Fragile Strength equipped. Breaking fragile charms should only be required in logic if fragile charm repair is accessible. Though fragile charm repair is reachable via benchwarp in standard randomizer, it should not be assumed to be trivially reachable in general, so the fact that charms have been broken should be propagated along paths.
- "NOFLOWER"
  - Defaults to `true`
  - If `false`, the path to the current point represented by this state has the unbroken Delicate Flower.
- "CHARM1", ..., "CHARM40" and "noCHARM1", ..., "noCHARM40"
  - These fields correspond to the 40 charms, identified by 1-based id number.
  - For example, the charm with id 1 is Gathering Swarm. If "CHARM1" is true, then Gathering Swarm is equipped. If "noCHARM1" is true, then Gathering Swarm is not equipped. Otherwise, "CHARM1" and "noCHARM1" are both false, and the equip state of Gathering Swarm is ambiguous (i.e. a subsequent state modifier can decide to require it to be equipped or unequipped).

## Int Fields

- "SPENTSOUL"
  - Indicates the amount of soul that has been spent from the soul meter relative to the soul meter's maximum soul.
- "SPENTRESERVESOUL"
  - Indicates the aount of soul that has been spent from the soul vessels relative to the soul vessel's combined maximum soul.
- "SOULLIMITER"
  - Indicates an offset for the soul meter's maximum soul. For example, after dying, this is set to 33, to indicate that the maximum soul is now 99 - 33 = 66.
- "REQUIREDMAXSOUL"
  - Indicates that the soul meter's maximum soul cannot drop below a certain number. For example, if the path contains a triple fireball skip, then this is set to 99, to indicate that the player must be able to cast 3 spells consecutively. Then the path cannot subsequently shade skip, since the preceding fireball skip would no longer be possible.
- "SPENTHP"
  - Indicates the number of masks lost, relative to the current total.
- "SPENTBLUEHP"
  - Indicates the number of blue masks lost, relative to the number given by charms.
- "USEDNOTCHES"
  - Indicates the number of notches currently in use for charms.
- "MAXNOTCHCOST"
  - Indicates the largest notch cost among all equipped charms. Used to determine the optimal overcharming configuration.
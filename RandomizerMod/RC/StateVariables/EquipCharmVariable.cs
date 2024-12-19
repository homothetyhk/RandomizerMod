using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RandomizerMod.RC.StateVariables
{
    /*
     * Prefix: $EQUIPPEDCHARM
     * Required Parameters:
     *   - First parameter MUST be either: the name of the charm term (e.g. Gathering_Swarm) or the 1-based charm ID (for Gathering Swarm, 1).
     * Optional Parameters: none
    */
    public class EquipCharmVariable : StateModifier
    {
        public override string Name { get; }
        protected int CharmID;
        protected Term CharmTerm;
        protected StateBool AnticharmBool;
        protected StateBool OvercharmBool;

        protected readonly StateBool NoPassedCharmEquip;
        protected readonly Term NotchesTerm;
        protected readonly StateBool CharmBool;
        protected readonly StateInt UsedNotchesInt;
        protected readonly StateInt MaxNotchCost;

        public const string Prefix = "$EQUIPPEDCHARM";

        protected EquipCharmVariable(string name, LogicManager lm)
        {
            Name = name;
            try
            {
                NotchesTerm = lm.GetTermStrict("NOTCHES");
                NoPassedCharmEquip = lm.StateManager.GetBoolStrict("NOPASSEDCHARMEQUIP");
                OvercharmBool = lm.StateManager.GetBoolStrict("OVERCHARMED");
                UsedNotchesInt = lm.StateManager.GetIntStrict("USEDNOTCHES");
                MaxNotchCost = lm.StateManager.GetIntStrict("MAXNOTCHCOST");
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error constructing EquipCharmVariable", e);
            }
        }

        public EquipCharmVariable(string name, string charmName, int charmID, LogicManager lm) : this(name, lm)
        {
            this.CharmID = charmID;
            try
            {
                CharmTerm = lm.GetTermStrict(charmName);
                CharmBool = lm.StateManager.GetBoolStrict("CHARM" + charmID);
                AnticharmBool = lm.StateManager.GetBoolStrict("noCHARM" + charmID);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error constructing EquipCharmVariable", e);
            }
        }

        public static string GetName(string charmTermName)
        {
            return $"{Prefix}[{charmTermName}]";
        }

        public static string GetName(int charmID)
        {
            return $"{Prefix}[{charmID}]";
        }

        public static bool TryMatch(LogicManager lm, string term, out LogicVariable variable)
        {
            if (VariableResolver.TryMatchPrefix(term, Prefix, out string[] parameters))
            {
                int charmID;
                string charmName;

                if (!int.TryParse(parameters[0], out charmID))
                {
                    charmID = LogicConstUtil.GetCharmID(charmName = parameters[0]);
                }
                else
                {
                    charmName = LogicConstUtil.GetCharmTerm(charmID);
                }

                EquipCharmVariable ecv;
                if (23 <= charmID && charmID <= 25)
                {
                    ecv = new FragileCharmVariable(term, charmName, charmID, lm);
                }
                else if (charmID == 36)
                {
                    ecv = new WhiteFragmentEquipVariable(term, charmName, lm);
                }
                else 
                {
                    ecv = new EquipCharmVariable(term, charmName, charmID, lm);
                }

                variable = ecv;
                return true;
            }
            variable = default;
            return false;
        }


        public override IEnumerable<Term> GetTerms()
        {
            yield return CharmTerm;
            yield return NotchesTerm;
        }

        public enum EquipResult
        {
            None,
            Overcharm,
            Nonovercharm
        }

        public virtual int GetNotchCost<T>(ProgressionManager pm, T state) where T : IState
        {
            return ((RandoModContext)pm.ctx).notchCosts[CharmID - 1];
        }

        public virtual bool HasCharmProgression(ProgressionManager pm) => pm.Has(CharmTerm);

        /// <summary>
        /// Given that pm.HasCharmProgression returned true, this should determine whether the particular state supports equipping the charm, ignoring notch cost.
        /// </summary>
        public virtual bool HasStateRequirements<T>(ProgressionManager pm, T state) where T : IState
        {
            if (state.GetBool(NoPassedCharmEquip) || state.GetBool(AnticharmBool)) return false;
            return true;
        }

        /// <summary>
        /// Determines whether the charm can be equipped with or without overcharming for the given state. Does not check progression or state requirements.
        /// </summary>
        public EquipResult HasNotchRequirements<T>(ProgressionManager pm, T state) where T : IState
        {
            if (IsEquipped(state))
            {
                return state.GetBool(OvercharmBool) ? EquipResult.Overcharm : EquipResult.Nonovercharm; // Already equipped
            }

            int notchCost = GetNotchCost(pm, state);

            if (notchCost <= 0)
            {
                return state.GetBool(OvercharmBool) ? EquipResult.Overcharm : EquipResult.Nonovercharm; // free to equip
            }

            int netNotches = pm.Get(NotchesTerm) - state.GetInt(UsedNotchesInt) - notchCost;

            if (netNotches >= 0)
            {
                return EquipResult.Nonovercharm;
            }

            int overcharmSave = Math.Max(state.GetInt(MaxNotchCost), notchCost);
            
            if (netNotches + overcharmSave > 0)
            {
                return EquipResult.Overcharm; // charm is not 0 notches, so it requires an open notch to overcharm
            }

            return EquipResult.None;
        }

        public bool CanEquipNonovercharm<T>(ProgressionManager pm, T state) where T : IState
        {
            return HasCharmProgression(pm) && HasStateRequirements(pm, state) && HasNotchRequirements(pm, state) == EquipResult.Nonovercharm;
        }

        public bool CanEquipOvercharm<T>(ProgressionManager pm, T state) where T : IState
        {
            return HasCharmProgression(pm) && HasStateRequirements(pm, state) && HasNotchRequirements(pm, state) != EquipResult.None;
        }

        public EquipResult CanEquip(ProgressionManager pm, StateUnion? localState)
        {
            if (localState is null || !HasCharmProgression(pm)) return EquipResult.None;
            bool overcharm = false;
            for (int i = 0; i < localState.Count; i++)
            {
                if (!HasStateRequirements(pm, localState[i])) continue;
                switch (HasNotchRequirements(pm, localState[i]))
                {
                    case EquipResult.None: continue;
                    case EquipResult.Overcharm: overcharm = true; continue;
                    case EquipResult.Nonovercharm: return EquipResult.Nonovercharm;
                }
            }

            return overcharm ? EquipResult.Overcharm : EquipResult.None;
        }

        /// <summary>
        /// Checks whether the charm can be equipped. Does not modify the state--for that, use <see cref="ModifyState(object, ProgressionManager, LazyStateBuilder)"/>.
        /// </summary>
        public EquipResult CanEquip<T>(ProgressionManager pm, T state) where T : IState
        {
            if (!HasCharmProgression(pm) || !HasStateRequirements(pm, state)) return EquipResult.None;
            return HasNotchRequirements(pm, state);
        }

        public override IEnumerable<LazyStateBuilder> ModifyState(object? sender, ProgressionManager pm, LazyStateBuilder state)
        {
            if (TryEquip(sender, pm, ref state)) yield return state;
        }

        public bool TryEquip(object? sender, ProgressionManager pm, ref LazyStateBuilder state)
        {
            if (IsEquipped(state))
            {
                return true;
            }

            if (CanEquip(pm, state) != EquipResult.None)
            {
                DoEquipCharm(pm, GetNotchCost(pm, state), ref state);
                return true;
            }
            return false;
        }

        public bool TryEquip(object? sender, ProgressionManager pm, in LazyStateBuilder state, out LazyStateBuilder newState)
        {
            if (IsEquipped(state))
            {
                return true;
            }

            if (CanEquip(pm, state) != EquipResult.None)
            {
                newState = new(state);
                DoEquipCharm(pm, GetNotchCost(pm, state), ref newState);
                return true;
            }
            return false;
        }

        protected virtual void DoEquipCharm(ProgressionManager pm, int notchCost, ref LazyStateBuilder state)
        {
            state.Increment(UsedNotchesInt, notchCost);
            state.SetBool(CharmBool, true);
            state.SetInt(MaxNotchCost, Math.Max(state.GetInt(MaxNotchCost), notchCost));
            if (state.GetInt(UsedNotchesInt) > pm.Get(NotchesTerm)) state.SetBool(OvercharmBool, true);
        }

        public bool IsEquipped(LazyStateBuilder state) => state.GetBool(CharmBool);
        public bool IsEquipped<T>(T state) where T : IState => state.GetBool(CharmBool);
        public void SetUnequippable(ref LazyStateBuilder state) => state.SetBool(AnticharmBool, true);
        public bool IsDetermined<T>(T state) where T : IState => state.GetBool(CharmBool) || state.GetBool(AnticharmBool);
        public int GetAvailableNotches(ProgressionManager pm, LazyStateBuilder state)
        {
            return pm.Get(NotchesTerm) - state.GetInt(UsedNotchesInt);
        }

        public IEnumerable<LazyStateBuilder> DecideCharm(ProgressionManager pm, LazyStateBuilder state)
        {
            if (IsDetermined(state))
            {
                yield return state;
                yield break;
            }
            else
            {
                LazyStateBuilder lsb = new(state);
                SetUnequippable(ref lsb);
                yield return lsb;
                if (TryEquip(null, pm, ref state))
                {
                    yield return state;
                }
            }
        }

        /// <summary>
        /// Enumerates states for all equippable subsets of the provided set of charms.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if length of charm list is greater than 30.</exception>
        public static IEnumerable<LazyStateBuilder> GenerateCharmCombinations(ProgressionManager pm, LazyStateBuilder state, IEnumerable<EquipCharmVariable> charmList)
        {
            EquipCharmVariable[] charms = charmList.Where(c =>
                !c.IsDetermined(state)
                && c.HasCharmProgression(pm)
                && c.HasStateRequirements(pm, state)
                && c.HasNotchRequirements(pm, state) != EquipResult.None)
                .ToArray();
            int len = charms.Length;
            if (len == 0)
            {
                yield return state;
                yield break;
            }
            else if (len > 30)
            {
                throw new ArgumentOutOfRangeException(nameof(charmList));
            }

            int p = 1 << len;
            for (int i = 0; i < p; i++)
            {
                LazyStateBuilder next = new(state);
                for (int j = 0; j < len; j++)
                {
                    int f = 1 << j;
                    if ((i & f) == f) // equip
                    {
                        if (!charms[j].TryEquip(null, pm, ref next)) // should only fail due to out of notches
                        {
                            goto end_of_outer_loop;
                        }
                    }
                    else // do not equip
                    {
                        charms[j].SetUnequippable(ref next);
                    }
                }
                yield return next;
            end_of_outer_loop: continue;
            }
        }
    }
}

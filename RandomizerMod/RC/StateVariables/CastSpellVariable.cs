using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RandomizerMod.RC.StateVariables
{
    /*
     * Prefix: $CASTSPELL
     * Required Parameters: none
     * Optional Parameters:
     *   - any integer parameters: parses to an array of ints, which represent number of spell casts, where time passes between different entries of the array (i.e. soul reserves can refill, etc).
     *                             If missing, number of casts is new int[]{1}
     *   - a parameter beginning with "before:": tries to convert the tail of the parameter to the NearbySoul enum (either by string or int parsing). Represents soul available before any spells are cast.
     *   - a parameter beginning with "after:": tries to convert the tail of the parameter to the NearbySoul enum (either by string or int parsing). Represents soul available after all spells are cast.
    */
    public class CastSpellVariable : StateModifier
    {
        public enum NearbySoul
        {
            NONE,
            ITEMSOUL,
            MAPAREASOUL,
            AREASOUL, 
            ROOMSOUL,
        }

        public override string Name { get; }
        protected readonly int[] SpellCasts;
        protected readonly NearbySoul BeforeSoul;
        protected readonly NearbySoul AfterSoul;
        protected readonly StateInt SpentSoul;
        protected readonly StateInt SpentReserveSoul;
        protected readonly StateInt SoulLimiter;
        protected readonly StateInt MaxRequiredSoul;
        protected readonly StateInt UsedNotches;
        protected readonly StateBool CannotRegainSoul;
        protected readonly StateBool SpentAllSoul;
        protected readonly Term VesselFragments;
        protected readonly Term ItemRando;
        protected readonly Term MapAreaRando;
        protected readonly Term AreaRando;
        protected readonly Term RoomRando;
        protected readonly EquipCharmVariable EquipSpellTwister;
        public const string Prefix = "$CASTSPELL";

        public CastSpellVariable(string name, LogicManager lm, int[] spellCasts, bool canDreamgate, NearbySoul beforeSoul, NearbySoul afterSoul)
        {
            Name = name;
            this.SpellCasts = spellCasts;
            this.BeforeSoul = beforeSoul;
            this.AfterSoul = afterSoul;
            try
            {
                SpentSoul = lm.StateManager.GetIntStrict("SPENTSOUL");
                SpentReserveSoul = lm.StateManager.GetIntStrict("SPENTRESERVESOUL");
                SoulLimiter = lm.StateManager.GetIntStrict("SOULLIMITER");
                MaxRequiredSoul = lm.StateManager.GetIntStrict("REQUIREDMAXSOUL");
                UsedNotches = lm.StateManager.GetIntStrict("USEDNOTCHES");
                CannotRegainSoul = lm.StateManager.GetBoolStrict("CANNOTREGAINSOUL");
                SpentAllSoul = lm.StateManager.GetBoolStrict("SPENTALLSOUL");
                VesselFragments = lm.GetTermStrict("VESSELFRAGMENTS");
                ItemRando = lm.GetTermStrict("ITEMRANDO");
                MapAreaRando = lm.GetTermStrict("MAPAREARANDO");
                AreaRando = lm.GetTermStrict("FULLAREARANDO");
                RoomRando = lm.GetTermStrict("ROOMRANDO");
                EquipSpellTwister = (EquipCharmVariable)lm.GetVariableStrict(EquipCharmVariable.GetName("Spell_Twister"));
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error constructing CastSpellVariable", e);
            }
        }

        public static bool TryMatch(LogicManager lm, string term, out LogicVariable variable)
        {
            if (VariableResolver.TryMatchPrefix(term, Prefix, out string[] parameters))
            {
                List<int> spellCasts = new();
                bool canDreamgate = true;
                NearbySoul beforeSoul = NearbySoul.NONE;
                NearbySoul afterSoul = NearbySoul.NONE;
                for (int i = 0; i < parameters.Length; i++)
                {
                    if (int.TryParse(parameters[i], out int castNum)) spellCasts.Add(castNum);
                    else if (parameters[i] == "noDG") canDreamgate = false;
                    else if (parameters[i].StartsWith("before:"))
                    {
                        string arg = parameters[i].Substring(7);
                        if (!Enum.TryParse(arg, out beforeSoul))
                        {
                            if (int.TryParse(arg, out int k)) beforeSoul = (NearbySoul)k;
                            else throw new ArgumentException($"Could not parse {parameters[i]} to CastSpellVariable argument.");
                        }
                    }
                    else if (parameters[i].StartsWith("after:"))
                    {
                        string arg = parameters[i].Substring(6);
                        if (!Enum.TryParse(arg, out afterSoul))
                        {
                            if (int.TryParse(arg, out int k)) afterSoul = (NearbySoul)k;
                            else throw new ArgumentException($"Could not parse {parameters[i]} to CastSpellVariable argument.");
                        }
                    }
                    else throw new ArgumentException($"Could not parse {parameters[i]} to CastSpellVariable argument.");
                }
                if (spellCasts.Count == 0) spellCasts.Add(1);
                variable = new CastSpellVariable(term, lm, spellCasts.ToArray(), canDreamgate, beforeSoul, afterSoul);
                return true;
            }
            variable = default;
            return false;
        }

        public override IEnumerable<Term> GetTerms()
        {
            yield return VesselFragments;
            yield return ItemRando;
            yield return MapAreaRando;
            yield return AreaRando;
            yield return RoomRando;
            foreach (Term t in EquipSpellTwister.GetTerms()) yield return t;
        }

        /// <summary>
        /// Applies the cast spell transformation without accounting for potential dream gate resets before and after.
        /// </summary>
        public override IEnumerable<LazyStateBuilder> ModifyState(object? sender, ProgressionManager pm, LazyStateBuilder state)
        {
            int soul;
            int reserves;
            int maxSoul = GetMaxSoul(state);

            if (!state.GetBool(CannotRegainSoul) && NearbySoulToBool(BeforeSoul, pm))
            {
                soul = GetMaxSoul(state);
                reserves = GetReserves(pm, state);
            }
            else if (state.GetBool(SpentAllSoul))
            {
                soul = 0;
                reserves = 0;
            }
            else
            {
                soul = GetSoul(state);
                reserves = GetReserves(pm, state);
            }

            if (EquipSpellTwister.IsEquipped(state) && TryCastAll(33, maxSoul, reserves, soul))
            {
                LazyStateBuilder state33 = new(state);
                DoAllCasts(33, reserves, ref state33);
                if (!state33.GetBool(CannotRegainSoul) && NearbySoulToBool(AfterSoul, pm))
                {
                    RecoverSoul(SpellCasts.Sum() * 33, ref state33);
                }
                yield return state33;
            }

            LazyStateBuilder STstate = state;
            if (TryCastAll(24, maxSoul, reserves, soul) && EquipSpellTwister.TryEquip(sender, pm, ref STstate))
            {
                DoAllCasts(24, reserves, ref state);
                if (!state.GetBool(CannotRegainSoul) && NearbySoulToBool(AfterSoul, pm))
                {
                    RecoverSoul(SpellCasts.Sum() * 33, ref state); // recover the same amount of soul as in the normal path
                }
                yield return state;
            }
        }

        public void DoAllCasts(int costPerCast, int currentReserve, ref LazyStateBuilder state)
        {
            for (int i = 0; i < SpellCasts.Length; i++) SpendSoul(costPerCast * SpellCasts[i], ref currentReserve, ref state);
        }

        public void SpendSoul(int amount, ref int currentReserve, ref LazyStateBuilder state)
        {
            if (currentReserve >= amount)
            {
                state.Increment(SpentReserveSoul, amount);
                currentReserve -= amount;
            }
            else if (currentReserve > 0)
            {
                state.Increment(SpentReserveSoul, currentReserve);
                state.Increment(SpentSoul, amount - currentReserve);
                currentReserve = 0;
            }
            else
            {
                state.Increment(SpentSoul, amount);
            }
            if (amount > state.GetInt(MaxRequiredSoul))
            {
                state.SetInt(MaxRequiredSoul, amount);
            }
        }

        private bool TrySpendSoul(int amount, int maxSoul, ref int reserves, ref int soul)
        {
            if (soul < amount) return false;
            soul -= amount;
            int transferAmt = Math.Min(reserves, maxSoul - soul);
            soul += transferAmt;
            reserves -= transferAmt;
            return true;
        }

        private bool TryCastAll(int costPerCast, int maxSoul, int reserves, int soul)
        {
            for (int i = 0; i < SpellCasts.Length; i++)
            {
                if (!TrySpendSoul(costPerCast * SpellCasts[i], maxSoul, ref reserves, ref soul)) return false;
            }
            return true;
        }

        public void RecoverSoul(int amount, ref LazyStateBuilder state)
        {
            int soulDiff = state.GetInt(SpentSoul);
            if (soulDiff >= amount)
            {
                state.Increment(SpentSoul, -amount);
            }
            else if (soulDiff > 0)
            {
                state.SetInt(SpentSoul, 0);
                amount -= soulDiff;
            }
            int reserveDiff = state.GetInt(SpentReserveSoul);
            if (reserveDiff >= amount)
            {
                state.Increment(SpentReserveSoul, -amount);
            }
            else if (reserveDiff > 0)
            {
                state.SetInt(SpentReserveSoul, 0);
                amount -= reserveDiff;
            }
        }

        private bool NearbySoulToBool(NearbySoul soul, ProgressionManager pm)
        {
            if (soul > NearbySoul.NONE && soul <= NearbySoul.ROOMSOUL)
            {
                NearbySoul mode = GetMode(pm);
                return mode > NearbySoul.NONE && mode <= soul;
            }
            return false;
        }

        private NearbySoul GetMode(ProgressionManager pm)
        {
            if (pm.Has(RoomRando)) return NearbySoul.ROOMSOUL;
            else if (pm.Has(AreaRando)) return NearbySoul.AREASOUL;
            else if (pm.Has(MapAreaRando)) return NearbySoul.MAPAREASOUL;
            else if (pm.Has(ItemRando)) return NearbySoul.ITEMSOUL;
            else return NearbySoul.NONE;
        }

        private int GetMaxSoul<T>(T state) where T : IState
        {
            return 99 - state.GetInt(SoulLimiter);
        }

        private int GetSoul<T>(T state) where T : IState
        {
            return GetMaxSoul(state) - state.GetInt(SpentSoul);
        }

        private int GetMaxReserves(ProgressionManager pm)
        {
            return (pm.Get(VesselFragments) / 3) * 33;
        }

        private int GetReserves<T>(ProgressionManager pm, T state) where T : IState
        {
            return GetMaxReserves(pm) - state.GetInt(SpentReserveSoul);
        }
    }
}

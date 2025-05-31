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
        public readonly int[] SpellCasts;
        protected readonly NearbySoul BeforeSoul;
        protected readonly NearbySoul AfterSoul;
        protected readonly ISoulStateManager SSM;
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
                SSM = (ISoulStateManager)lm.GetVariableStrict(SoulStateManager.Prefix);
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
            yield return ItemRando;
            yield return MapAreaRando;
            yield return AreaRando;
            yield return RoomRando;
            foreach (Term t in EquipSpellTwister.GetTerms()) yield return t;
            foreach (Term t in SSM.GetTerms(ISoulStateManager.SSMOperation.SpendSoul)) yield return t;
        }

        /// <summary>
        /// Applies the cast spell transformation without accounting for potential dream gate resets before and after.
        /// </summary>
        public override IEnumerable<LazyStateBuilder> ModifyState(object? sender, ProgressionManager pm, LazyStateBuilder state)
        {
            if (NearbySoulToBool(BeforeSoul, pm))
            {
                SSM.TryRestoreAllSoul(pm, ref state, restoreReserves: true);
            }

            if (!EquipSpellTwister.IsDetermined(state))
            {
                if (EquipSpellTwister.TryEquip(sender, pm, in state, out LazyStateBuilder STstate))
                {
                    EquipSpellTwister.SetUnequippable(ref state);
                    if (TryCast(pm, ref STstate, 24))
                    {
                        yield return STstate;
                        if (TryCast(pm, ref state, 33)) yield return state;
                        yield break;
                    }
                    else
                    {
                        yield break; // 24 failed, so 33 will also fail
                    }
                }
                else
                {
                    EquipSpellTwister.SetUnequippable(ref state);
                    if (TryCast(pm, ref state, 33)) yield return state;
                    yield break;
                }
            }
            else
            {
                if (EquipSpellTwister.IsEquipped(state))
                {
                    if (TryCast(pm, ref state, 24)) yield return state;
                    yield break;
                }
                else
                {
                    if (TryCast(pm, ref state, 33)) yield return state;
                    yield break;
                }
            }
        }



        public bool TryCast(ProgressionManager pm, ref LazyStateBuilder state, int amountPerCast)
        {
            if (!SSM.TrySpendSoulSequence(pm, ref state, amountPerCast, SpellCasts))
            {
                return false;
            }
            if (NearbySoulToBool(AfterSoul, pm))
            {
                SSM.TryRestoreSoul(pm, ref state, SpellCasts.Sum() * 33); // recover the same amount of soul in all paths to respect the state ordering
            }
            return true;
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
    }
}

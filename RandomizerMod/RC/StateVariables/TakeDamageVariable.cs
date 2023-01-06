using ItemChanger.Extensions;
using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RandomizerMod.RC.StateVariables
{
    /*
     * Prefix: $TAKEDAMAGE
     * Required Parameters:
         - If any parameters are provided, the first parameter must parse to int to give the damage amount. If absent, defaults to 1.
     * Optional Parameters: none
    */
    public class TakeDamageVariable : StateModifier
    {
        public override string Name { get; }
        protected readonly int Amount; // TODO: properly separate into int[] of amount per hit
        protected readonly StateBool Overcharmed;
        protected readonly StateBool HasTakenDamage;
        protected readonly StateBool NoFlower;
        protected readonly StateBool NoPassedCharmEquip;
        protected readonly StateInt SpentHP;
        protected readonly StateInt SpentBlueHP;
        protected readonly Term MaskShards;
        protected readonly EquipCharmVariable LifebloodHeart;
        protected readonly EquipCharmVariable LifebloodCore;
        protected readonly EquipCharmVariable JonisBlessing;
        protected readonly EquipCharmVariable FragileHeart;
        // not supported: hiveblood, focus, deep focus, grubsong

        public const string Prefix = "$TAKEDAMAGE";

        public TakeDamageVariable(string name, LogicManager lm, int amount)
        {
            Name = name;
            this.Amount = amount;
            try
            {
                Overcharmed = lm.StateManager.GetBoolStrict("OVERCHARMED");
                HasTakenDamage = lm.StateManager.GetBoolStrict("HASTAKENDAMAGE");
                NoFlower = lm.StateManager.GetBoolStrict("NOFLOWER");
                NoPassedCharmEquip = lm.StateManager.GetBoolStrict("NOPASSEDCHARMEQUIP");
                SpentHP = lm.StateManager.GetIntStrict("SPENTHP");
                SpentBlueHP = lm.StateManager.GetIntStrict("SPENTBLUEHP");
                MaskShards = lm.GetTermStrict("MASKSHARDS");
                LifebloodHeart = (EquipCharmVariable)lm.GetVariableStrict(EquipCharmVariable.GetName("Lifeblood_Heart"));
                LifebloodCore = (EquipCharmVariable)lm.GetVariableStrict(EquipCharmVariable.GetName("Lifeblood_Core"));
                JonisBlessing = (EquipCharmVariable)lm.GetVariableStrict(EquipCharmVariable.GetName("Joni's_Blessing"));
                FragileHeart = (EquipCharmVariable)lm.GetVariableStrict(EquipCharmVariable.GetName("Fragile_Heart"));
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error constructing TakeDamageVariable", e);
            }
        }

        public static bool TryMatch(LogicManager lm, string term, out LogicVariable variable)
        {
            if (VariableResolver.TryMatchPrefix(term, Prefix, out string[] parameters))
            {
                int amount = parameters.Length == 0 ? 1 : int.Parse(parameters[0]);
                variable = new TakeDamageVariable(term, lm, amount);
                return true;
            }

            variable = default;
            return false;
        }

        public override IEnumerable<Term> GetTerms()
        {
            yield return MaskShards;
            foreach (Term t in LifebloodHeart.GetTerms()) yield return t;
            foreach (Term t in LifebloodCore.GetTerms()) yield return t;
            foreach (Term t in JonisBlessing.GetTerms()) yield return t;
            foreach (Term t in FragileHeart.GetTerms()) yield return t;
        }

        public override IEnumerable<LazyStateBuilder> ModifyState(object? sender, ProgressionManager pm, LazyStateBuilder state)
        {
            if (state.GetBool(HasTakenDamage) || state.GetBool(NoPassedCharmEquip))
            {
                if (TakeDamage(pm, ref state, Amount))
                {
                    DisableUnequippedHealthCharms(ref state);
                    return state.Yield();
                }
                return null;
            }
            else
            {
                return GenerateCharmLoadouts(pm, state);
            }
        }

        private void DisableUnequippedHealthCharms(ref LazyStateBuilder state)
        {
            if (!LifebloodHeart.IsEquipped(state)) LifebloodHeart.SetUnequippable(ref state);
            if (!LifebloodCore.IsEquipped(state)) LifebloodCore.SetUnequippable(ref state);
            if (!JonisBlessing.IsEquipped(state)) JonisBlessing.SetUnequippable(ref state);
            if (!FragileHeart.IsEquipped(state)) FragileHeart.SetUnequippable(ref state);
        }

        public IEnumerable<LazyStateBuilder> GenerateCharmLoadouts(ProgressionManager pm, LazyStateBuilder state)
        {
            int availableNotches = LifebloodCore.GetAvailableNotches(pm, state);
            if (availableNotches <= 0) yield break;

            List<int> notchCosts = ((RandoModContext)pm.ctx).notchCosts;
            List<EquipCharmVariable> helper = new();
            List<LazyStateBuilder> lsbHelper = new();
            AddECV(LifebloodHeart);
            AddECV(FragileHeart);
            AddECV(LifebloodCore);
            AddECV(JonisBlessing);
            helper.Sort((p, q) => notchCosts[p.CharmID - 1] - notchCosts[q.CharmID - 1]);

            int pow = 1 << helper.Count;
            for (int i = 0; i < pow; i++)
            {
                lsbHelper.Clear();
                lsbHelper.Add(new(state));
                for (int j = 0; j < helper.Count; j++)
                {
                    if ((i & (1 << j)) == (1 << j))
                    {
                        helper[j].ModifyAll(null, pm, lsbHelper);
                        if (lsbHelper.Count == 0) break;
                    }
                }
                for (int j = 0; j < lsbHelper.Count; j++)
                {
                    LazyStateBuilder next = lsbHelper[j];
                    if (TakeDamage(pm, ref next, Amount))
                    {
                        DisableUnequippedHealthCharms(ref next);
                        yield return next;
                    }
                }
            }

            void AddECV(EquipCharmVariable ecv)
            {
                if (ecv.HasCharmProgression(pm))
                {
                    helper.Add(ecv);
                }
            }
        }

        public bool TakeDamage(ProgressionManager pm, ref LazyStateBuilder state, int damage)
        {
            bool oc = state.GetBool(Overcharmed);

            int blueHP = -state.GetInt(SpentBlueHP) + (LifebloodHeart.IsEquipped(state) ? 2 : 0) + (LifebloodCore.IsEquipped(state) ? 4 : 0);
            int hp = pm.Get(MaskShards) / 4 + (FragileHeart.IsEquipped(state) ? 2 : 0);
            if (JonisBlessing.IsEquipped(state)) hp = (int)(hp * 1.4f);
            int hits = oc ? blueHP / 2 + (hp - 1) / 2 : blueHP + hp - 1;

            if (!oc && blueHP >= damage || blueHP >= 2 * damage)
            {
                state.Increment(SpentBlueHP, !oc ? damage : 2 * damage);
                state.SetBool(HasTakenDamage, true);
                state.SetBool(NoFlower, true);
                return true;
            }
            
            if (hits >= damage)
            {
                if (blueHP > 0)
                {
                    state.Increment(SpentBlueHP, blueHP);
                    damage -= oc ? blueHP / 2 : blueHP;
                    hits -= oc ? blueHP / 2 : blueHP;
                }
                state.Increment(SpentHP, !oc ? damage : 2 * damage);
                state.SetBool(HasTakenDamage, true);
                state.SetBool(NoFlower, true);
                return true;
            }
            return false;
        }
    }
}

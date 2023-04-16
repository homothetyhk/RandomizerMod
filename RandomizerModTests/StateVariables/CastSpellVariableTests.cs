using RandomizerCore.Logic.StateLogic;
using RandomizerCore.Logic;

namespace RandomizerModTests.StateVariables
{
    [Collection("Logic Collection")]
    public class CastSpellVariableTests
    {
        LogicFixture Fix { get; }

        public CastSpellVariableTests(LogicFixture fix)
        {
            Fix = fix;
        }

        public static Dictionary<string, int> CharmStateBase => new() { ["NOPASSEDCHARMEQUIP"] = 0 };

        [Fact]
        public void CastSpellSucceedsWith3Casts()
        {
            StateModifier sm = (StateModifier)Fix.LM.GetVariableStrict("$CASTSPELL[3]");
            ProgressionManager pm = Fix.GetProgressionManager(new());
            LazyStateBuilder lsb = Fix.GetState(new());

            IEnumerable<LazyStateBuilder> result = sm.ModifyState(null, pm, lsb);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void CastSpellFailsWith3CastsBeforeOrAfterAShadeSkip()
        {
            StateModifier sm = (StateModifier)Fix.LM.GetVariableStrict("$SHADESKIP");
            StateModifier sm2 = (StateModifier)Fix.LM.GetVariableStrict("$CASTSPELL[3]");
            ProgressionManager pm = Fix.GetProgressionManager(new());
            LazyStateBuilder lsb = Fix.GetState(new());

            IEnumerable<LazyStateBuilder> result = sm.ModifyState(null, pm, new(lsb)).SelectMany(s => sm2.ModifyState(null, pm, s));
            Assert.Empty(result);
            result = sm2.ModifyState(null, pm, new(lsb)).SelectMany(s => sm.ModifyState(null, pm, s));
            Assert.Empty(result);
        }

        [Fact]
        public void CastSpellFailsWith4CastsAndASoulVessel()
        {
            StateModifier sm = (StateModifier)Fix.LM.GetVariableStrict("$CASTSPELL[4]");
            ProgressionManager pm = Fix.GetProgressionManager(new());
            LazyStateBuilder lsb = Fix.GetState(new());

            for (int i = 0; i < 3; i++) pm.Add(Fix.LM.GetItemStrict("Vessel_Fragment"));

            IEnumerable<LazyStateBuilder> result = sm.ModifyState(null, pm, lsb);
            Assert.Empty(result);
        }

        [Fact]
        public void CastSpellSucceedsWith4CastsAndSpellTwister()
        {
            StateModifier sm = (StateModifier)Fix.LM.GetVariableStrict("$CASTSPELL[4]");
            ProgressionManager pm = Fix.GetProgressionManager(new());
            LazyStateBuilder lsb = Fix.GetState(CharmStateBase);

            pm.Add(Fix.LM.GetItemStrict("Spell_Twister"));
            pm.Set("NOTCHES", 6);

            IEnumerable<LazyStateBuilder> result = sm.ModifyState(null, pm, lsb);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void CastSpellSucceedsWith3to1CastsAndASoulVessel()
        {
            StateModifier sm = (StateModifier)Fix.LM.GetVariableStrict("$CASTSPELL[3,1]");
            ProgressionManager pm = Fix.GetProgressionManager(new());
            LazyStateBuilder lsb = Fix.GetState(new());

            for (int i = 0; i < 3; i++) pm.Add(Fix.LM.GetItemStrict("Vessel_Fragment"));

            IEnumerable<LazyStateBuilder> result = sm.ModifyState(null, pm, lsb);
            Assert.NotEmpty(result);
        }
    }
}

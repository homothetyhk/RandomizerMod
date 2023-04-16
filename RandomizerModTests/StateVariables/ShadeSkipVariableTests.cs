using RandomizerCore.Logic.StateLogic;
using RandomizerCore.Logic;

namespace RandomizerModTests.StateVariables
{
    [Collection("Logic Collection")]
    public class ShadeSkipVariableTests
    {
        public LogicFixture Fix { get; }

        public ShadeSkipVariableTests(LogicFixture fix)
        {
            Fix = fix;
        }

        public static Dictionary<string, int> ShadeskipPMBase => new() { ["SHADESKIPS"] = 1, };
        public static Dictionary<string, int> CharmStateBase => new() { ["NOPASSEDCHARMEQUIP"] = 0 };

        [Fact]
        public void CannotShadeSkipWithoutSetting()
        {
            StateModifier sm = (StateModifier)Fix.LM.GetVariableStrict("$SHADESKIP");
            ProgressionManager pm = Fix.GetProgressionManager(new());
            LazyStateBuilder lsb = Fix.GetState(new());

            IEnumerable<LazyStateBuilder> result = sm.ModifyState(null, pm, lsb);
            Assert.Empty(result);
        }

        [Fact]
        public void CannotShadeSkipWithUsedState()
        {
            StateModifier sm = (StateModifier)Fix.LM.GetVariableStrict("$SHADESKIP");
            ProgressionManager pm = Fix.GetProgressionManager(new());
            LazyStateBuilder lsb = Fix.GetState(new() { ["USEDSHADE"] = 1 });

            IEnumerable<LazyStateBuilder> result = sm.ModifyState(null, pm, lsb);
            Assert.Empty(result);
        }

        [Fact]
        public void CannotShadeSkipWithCharm36()
        {
            StateModifier sm = (StateModifier)Fix.LM.GetVariableStrict("$SHADESKIP");
            ProgressionManager pm = Fix.GetProgressionManager(new());
            LazyStateBuilder lsb = Fix.GetState(new() { ["CHARM36"] = 1 });

            IEnumerable<LazyStateBuilder> result = sm.ModifyState(null, pm, lsb);
            Assert.Empty(result);
        }

        [Fact]
        public void CannotShadeSkipWithMaxSoulRequirement()
        {
            StateModifier sm = (StateModifier)Fix.LM.GetVariableStrict("$SHADESKIP");
            ProgressionManager pm = Fix.GetProgressionManager(new());
            LazyStateBuilder lsb = Fix.GetState(new() { ["REQUIREDMAXSOUL"] = 67 });

            IEnumerable<LazyStateBuilder> result = sm.ModifyState(null, pm, lsb);
            Assert.Empty(result);
        }

        [Fact]
        public void CanShadeSkipWithSetting()
        {
            StateModifier sm = (StateModifier)Fix.LM.GetVariableStrict("$SHADESKIP");
            ProgressionManager pm = Fix.GetProgressionManager(ShadeskipPMBase);
            LazyStateBuilder lsb = Fix.GetState(new());

            IEnumerable<LazyStateBuilder> result = sm.ModifyState(null, pm, lsb);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void Cannot2HitShadeSkipWith1HP()
        {
            StateModifier sm = (StateModifier)Fix.LM.GetVariableStrict("$SHADESKIP[2HITS]");
            ProgressionManager pm = Fix.GetProgressionManager(ShadeskipPMBase);
            LazyStateBuilder lsb = Fix.GetState(new());

            pm.Set("MASKSHARDS", 4);

            IEnumerable<LazyStateBuilder> result = sm.ModifyState(null, pm, lsb);
            Assert.Empty(result);
        }

        [Fact]
        public void Can2HitShadeSkipWith4HP()
        {
            StateModifier sm = (StateModifier)Fix.LM.GetVariableStrict("$SHADESKIP[2HITS]");
            ProgressionManager pm = Fix.GetProgressionManager(ShadeskipPMBase);
            LazyStateBuilder lsb = Fix.GetState(new());

            pm.Set("MASKSHARDS", 16);

            IEnumerable<LazyStateBuilder> result = sm.ModifyState(null, pm, lsb);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void Can2HitShadeSkipWith2HPFragileHeartLegEater()
        {
            StateModifier sm = (StateModifier)Fix.LM.GetVariableStrict("$SHADESKIP[2HITS]");
            ProgressionManager pm = Fix.GetProgressionManager(ShadeskipPMBase);
            LazyStateBuilder lsb = Fix.GetState(CharmStateBase);

            pm.Set("MASKSHARDS", 8);
            pm.Set("NOTCHES", 6);
            pm.Set("Can_Repair_Fragile_Charms", 1);
            pm.Add(Fix.LM.GetItemStrict("Fragile_Heart"));

            IEnumerable<LazyStateBuilder> result = sm.ModifyState(null, pm, lsb);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void Can2HitShadeSkipWith2HPUnbreakableHeart()
        {
            StateModifier sm = (StateModifier)Fix.LM.GetVariableStrict("$SHADESKIP[2HITS]");
            ProgressionManager pm = Fix.GetProgressionManager(ShadeskipPMBase);
            LazyStateBuilder lsb = Fix.GetState(CharmStateBase);

            pm.Set("MASKSHARDS", 8);
            pm.Set("NOTCHES", 6);
            pm.Add(Fix.LM.GetItemStrict("Fragile_Heart"));
            pm.Add(Fix.LM.GetItemStrict("Unbreakable_Heart"));

            IEnumerable<LazyStateBuilder> result = sm.ModifyState(null, pm, lsb);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void Cannot2HitShadeSkipWith2HPBrokenFragileHeart()
        {
            StateModifier sm = (StateModifier)Fix.LM.GetVariableStrict("$SHADESKIP[2HITS]");
            ProgressionManager pm = Fix.GetProgressionManager(ShadeskipPMBase);
            LazyStateBuilder lsb = Fix.GetState(CharmStateBase);

            pm.Set("MASKSHARDS", 8);
            pm.Set("NOTCHES", 6);
            pm.Set("Can_Repair_Fragile_Charms", 1);
            pm.Add(Fix.LM.GetItemStrict("Fragile_Heart"));
            lsb.SetBool(Fix.LM.StateManager.GetBoolStrict("BROKEHEART"), true);

            IEnumerable<LazyStateBuilder> result = sm.ModifyState(null, pm, lsb);
            Assert.Empty(result);
        }
    }
}

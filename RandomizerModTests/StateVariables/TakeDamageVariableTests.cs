using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RandomizerModTests.StateVariables
{
    [Collection("Logic Collection")]
    public class TakeDamageVariableTests
    {
        LogicFixture Fix { get; }

        public TakeDamageVariableTests(LogicFixture fix)
        {
            Fix = fix;
        }

        public Dictionary<string, int> CharmStateBase => new() { ["NOPASSEDCHARMEQUIP"] = 0 };

        [Fact]
        public void TakeDamageFailsOn1HP()
        {
            StateModifier sm = (StateModifier)Fix.LM.GetVariableStrict("$TAKEDAMAGE");
            ProgressionManager pm = Fix.GetProgressionManager(new());
            LazyStateBuilder lsb = Fix.GetState(new());

            pm.Set("MASKSHARDS", 4);

            IEnumerable<LazyStateBuilder> result = sm.ModifyState(null, pm, lsb);
            Assert.Empty(result);
        }

        [Fact]
        public void TakeDamageSucceedsOn2HP()
        {
            StateModifier sm = (StateModifier)Fix.LM.GetVariableStrict("$TAKEDAMAGE");
            ProgressionManager pm = Fix.GetProgressionManager(new());
            LazyStateBuilder lsb = Fix.GetState(new());

            pm.Set("MASKSHARDS", 8);

            IEnumerable<LazyStateBuilder> result = sm.ModifyState(null, pm, lsb);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void TakeDamageTwiceFailsOn2HP()
        {
            StateModifier sm = (StateModifier)Fix.LM.GetVariableStrict("$TAKEDAMAGE");
            ProgressionManager pm = Fix.GetProgressionManager(new());
            LazyStateBuilder lsb = Fix.GetState(new());

            pm.Set("MASKSHARDS", 8);

            IEnumerable<LazyStateBuilder> result = sm.ModifyState(null, pm, lsb).SelectMany(s => sm.ModifyState(null, pm, s));
            Assert.Empty(result);
        }

        [Fact]
        public void TakeDamageTwiceSucceedsOn2HPWithFocus()
        {
            StateModifier sm = (StateModifier)Fix.LM.GetVariableStrict("$TAKEDAMAGE");
            ProgressionManager pm = Fix.GetProgressionManager(new());
            LazyStateBuilder lsb = Fix.GetState(new());

            pm.Set("MASKSHARDS", 8);
            pm.Set("FOCUS", 1);

            IEnumerable<LazyStateBuilder> result = sm.ModifyState(null, pm, lsb).SelectMany(s => sm.ModifyState(null, pm, s));
            Assert.NotEmpty(result);
        }

        [Fact]
        public void TakeDamageTwiceSucceedsWithLifebloodHeart()
        {
            StateModifier sm = (StateModifier)Fix.LM.GetVariableStrict("$TAKEDAMAGE");
            ProgressionManager pm = Fix.GetProgressionManager(new());
            LazyStateBuilder lsb = Fix.GetState(CharmStateBase);

            pm.Set("MASKSHARDS", 4);
            pm.Add(Fix.LM.GetItemStrict("Lifeblood_Heart"));
            pm.Set("NOTCHES", 6);

            IEnumerable<LazyStateBuilder> result = sm.ModifyState(null, pm, lsb).SelectMany(s => sm.ModifyState(null, pm, s));
            Assert.NotEmpty(result);
        }

        [Fact]
        public void TakeDamageTwiceFailsWith1HPLifebloodHeartOvercharmed()
        {
            StateModifier sm = (StateModifier)Fix.LM.GetVariableStrict("$TAKEDAMAGE");
            ProgressionManager pm = Fix.GetProgressionManager(new());
            LazyStateBuilder lsb = Fix.GetState(CharmStateBase);

            pm.Set("MASKSHARDS", 4);
            pm.Add(Fix.LM.GetItemStrict("Lifeblood_Heart"));
            pm.Set("NOTCHES", 1);

            IEnumerable<LazyStateBuilder> result = sm.ModifyState(null, pm, lsb).SelectMany(s => sm.ModifyState(null, pm, s));
            Assert.Empty(result);
        }

        [Fact]
        public void TakeDoubleDamageSingleDamageSucceedsWith2HPLifebloodHeart()
        {
            StateModifier sm = (StateModifier)Fix.LM.GetVariableStrict("$TAKEDAMAGE");
            StateModifier sm2 = (StateModifier)Fix.LM.GetVariableStrict("$TAKEDAMAGE[2]");
            ProgressionManager pm = Fix.GetProgressionManager(new());
            LazyStateBuilder lsb = Fix.GetState(CharmStateBase);

            pm.Set("MASKSHARDS", 8);
            pm.Add(Fix.LM.GetItemStrict("Lifeblood_Heart"));
            pm.Set("NOTCHES", 6);

            IEnumerable<LazyStateBuilder> result = sm2.ModifyState(null, pm, lsb).SelectMany(s => sm.ModifyState(null, pm, s));
            Assert.NotEmpty(result);
        }

        [Fact]
        public void TakeSingleDamageDoubleDamageFailsWith2HPLifebloodHeart()
        {
            StateModifier sm = (StateModifier)Fix.LM.GetVariableStrict("$TAKEDAMAGE");
            StateModifier sm2 = (StateModifier)Fix.LM.GetVariableStrict("$TAKEDAMAGE[2]");
            ProgressionManager pm = Fix.GetProgressionManager(new());
            LazyStateBuilder lsb = Fix.GetState(CharmStateBase);

            pm.Set("MASKSHARDS", 8);
            pm.Add(Fix.LM.GetItemStrict("Lifeblood_Heart"));
            pm.Set("NOTCHES", 6);

            IEnumerable<LazyStateBuilder> result = sm.ModifyState(null, pm, lsb).SelectMany(s => sm2.ModifyState(null, pm, s));
            Assert.Empty(result);
        }

        [Fact]
        public void TakeDamageTwiceSucceedsWith3HPLifebloodHeartOvercharmed()
        {
            StateModifier sm = (StateModifier)Fix.LM.GetVariableStrict("$TAKEDAMAGE");
            ProgressionManager pm = Fix.GetProgressionManager(new());
            LazyStateBuilder lsb = Fix.GetState(CharmStateBase);

            pm.Set("MASKSHARDS", 12);
            pm.Add(Fix.LM.GetItemStrict("Lifeblood_Heart"));
            pm.Set("NOTCHES", 1);

            IEnumerable<LazyStateBuilder> result = sm.ModifyState(null, pm, lsb).SelectMany(s => sm.ModifyState(null, pm, s));
            Assert.NotEmpty(result);
        }

        [Fact]
        public void TakeDamageTwiceSucceedsWithLifebloodCore()
        {
            StateModifier sm = (StateModifier)Fix.LM.GetVariableStrict("$TAKEDAMAGE");
            ProgressionManager pm = Fix.GetProgressionManager(new());
            LazyStateBuilder lsb = Fix.GetState(CharmStateBase);

            pm.Set("MASKSHARDS", 4);
            pm.Add(Fix.LM.GetItemStrict("Lifeblood_Heart"));
            pm.Set("NOTCHES", 6);

            IEnumerable<LazyStateBuilder> result = sm.ModifyState(null, pm, lsb).SelectMany(s => sm.ModifyState(null, pm, s));
            Assert.NotEmpty(result);
        }

        [Fact]
        public void TakeDamageTwiceSucceedsWithLifebloodCoreOvercharmed()
        {
            StateModifier sm = (StateModifier)Fix.LM.GetVariableStrict("$TAKEDAMAGE");
            ProgressionManager pm = Fix.GetProgressionManager(new());
            LazyStateBuilder lsb = Fix.GetState(CharmStateBase);

            pm.Set("MASKSHARDS", 4);
            pm.Add(Fix.LM.GetItemStrict("Lifeblood_Heart"));
            pm.Set("NOTCHES", 1);

            IEnumerable<LazyStateBuilder> result = sm.ModifyState(null, pm, lsb).SelectMany(s => sm.ModifyState(null, pm, s));
            Assert.Empty(result);
        }

        [Fact]
        public void TakeDamageTwiceSucceedsWith2HPHiveblood()
        {
            StateModifier sm = (StateModifier)Fix.LM.GetVariableStrict("$TAKEDAMAGE");
            ProgressionManager pm = Fix.GetProgressionManager(new());
            LazyStateBuilder lsb = Fix.GetState(CharmStateBase);

            pm.Set("MASKSHARDS", 8);
            pm.Add(Fix.LM.GetItemStrict("Hiveblood"));
            pm.Set("NOTCHES", 6);

            IEnumerable<LazyStateBuilder> result = sm.ModifyState(null, pm, lsb).SelectMany(s => sm.ModifyState(null, pm, s));
            Assert.NotEmpty(result);
        }

        [Fact]
        public void TakeDoubleDamageFourTimesOn3HPSucceedsWithDeepFocus()
        {
            StateModifier sm = (StateModifier)Fix.LM.GetVariableStrict("$TAKEDAMAGE[2]");
            ProgressionManager pm = Fix.GetProgressionManager(new());
            LazyStateBuilder lsb = Fix.GetState(CharmStateBase);

            pm.Set("MASKSHARDS", 12);
            pm.Add(Fix.LM.GetItemStrict("Deep_Focus"));
            pm.Set("NOTCHES", 6);
            pm.Set("FOCUS", 1);

            IEnumerable<LazyStateBuilder> result = sm.ModifyState(null, pm, lsb).SelectMany(s => sm.ModifyState(null, pm, s)).SelectMany(s => sm.ModifyState(null, pm, s)).SelectMany(s => sm.ModifyState(null, pm, s));
            Assert.NotEmpty(result);
        }

        [Fact]
        public void TakeDoubleDamageFiveTimesOn3HPFailsWithDeepFocus()
        {
            StateModifier sm = (StateModifier)Fix.LM.GetVariableStrict("$TAKEDAMAGE[2]");
            ProgressionManager pm = Fix.GetProgressionManager(new());
            LazyStateBuilder lsb = Fix.GetState(CharmStateBase);

            pm.Set("MASKSHARDS", 12);
            pm.Add(Fix.LM.GetItemStrict("Deep_Focus"));
            pm.Set("NOTCHES", 6);
            pm.Set("FOCUS", 1);

            IEnumerable<LazyStateBuilder> result = sm.ModifyState(null, pm, lsb)
                .SelectMany(s => sm.ModifyState(null, pm, s)).SelectMany(s => sm.ModifyState(null, pm, s))
                .SelectMany(s => sm.ModifyState(null, pm, s)).SelectMany(s => sm.ModifyState(null, pm, s));
            Assert.Empty(result);
        }
    }
}

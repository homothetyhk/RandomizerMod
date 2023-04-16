using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RandomizerModTests.StateVariables
{
    [Collection("Logic Collection")]
    public class ShriekPogoVariableTests : IClassFixture<LogicFixture>
    {
        public LogicFixture Fix { get; }

        public ShriekPogoVariableTests(LogicFixture fixture)
        {
            Fix = fixture;
        }


        public static string[] ShriekPogoVariableNames => new string[]
        {
            "$SHRIEKPOGO", "$SHRIEKPOGO[3,1,NOSTALL,before:ROOMSOUL,after:ROOMSOUL]", "$SHRIEKPOGO[7]"
        };

        public static Dictionary<string, int> ShriekPogoPMBase => new() { ["WINGS"] = 1, ["SCREAM"] = 2, ["SHRIEKPOGOSKIPS"] = 1, };
        public static Dictionary<string, int> DifficultShriekPogoPMBase => new() { ["WINGS"] = 1, ["SCREAM"] = 2, ["SHRIEKPOGOSKIPS"] = 1, ["DIFFICULTSKIPS"] = 1, };

        public static Dictionary<string, int> CharmStateBase => new() { ["NOPASSEDCHARMEQUIP"] = 0 };

        public static Dictionary<string, int>[] MissingWingsOrShriekPMData => new Dictionary<string, int>[]
        {
            new(), new(){["WINGS"] = 1}, new(){["SCREAM"] = 2}, new(){["SHRIEKPOGOSKIPS"] = 1},
            new(){["WINGS"] = 1, ["SCREAM"] = 2}, new(){["SCREAM"] = 2, ["SHRIEKPOGOSKIPS"] = 1}, new(){["WINGS"] = 1, ["SHRIEKPOGOSKIPS"] = 1},
        };

        public static Dictionary<string, int>[] DefaultStateData => new Dictionary<string, int>[] { new() };

        public static IEnumerable<object[]> MissingWingsOrShriekData => new object[][] { ShriekPogoVariableNames, MissingWingsOrShriekPMData, DefaultStateData }.Product();






        [Theory]
        [MemberData(nameof(MissingWingsOrShriekData))]
        public void ShriekPogoFailsIfNoWingsOrNoShriek(string shriekPogoVariableName, Dictionary<string, int> pmFieldValues, Dictionary<string, int> stateFieldValues)
        {
            StateModifier sm = (StateModifier)Fix.LM.GetVariableStrict(shriekPogoVariableName);
            ProgressionManager pm = Fix.GetProgressionManager(pmFieldValues);
            LazyStateBuilder lsb = Fix.GetState(stateFieldValues);

            IEnumerable<LazyStateBuilder> result = sm.ModifyState(null, pm, lsb);
            Assert.Empty(result);
            result = sm.ProvideState(null, pm);
            Assert.Null(result);
        }

        [Fact]
        public void ShriekPogoSucceedsWith3Casts()
        {
            StateModifier sm = (StateModifier)Fix.LM.GetVariableStrict("$SHRIEKPOGO[3]");
            ProgressionManager pm = Fix.GetProgressionManager(ShriekPogoPMBase);
            LazyStateBuilder lsb = Fix.GetState(new());

            IEnumerable<LazyStateBuilder> result = sm.ModifyState(null, pm, lsb);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void ShriekPogoFailsWith4Casts()
        {
            StateModifier sm = (StateModifier)Fix.LM.GetVariableStrict("$SHRIEKPOGO[4]");
            ProgressionManager pm = Fix.GetProgressionManager(DifficultShriekPogoPMBase);
            LazyStateBuilder lsb = Fix.GetState(new());

            IEnumerable<LazyStateBuilder> result = sm.ModifyState(null, pm, lsb);
            Assert.Empty(result);
        }

        [Fact]
        public void ShriekPogoSucceedsWith4CastsAndSpellTwister()
        {
            StateModifier sm = (StateModifier)Fix.LM.GetVariableStrict("$SHRIEKPOGO[4]");
            ProgressionManager pm = Fix.GetProgressionManager(DifficultShriekPogoPMBase);
            LazyStateBuilder lsb = Fix.GetState(CharmStateBase);

            pm.Add(Fix.LM.GetItemStrict("Spell_Twister"));
            pm.Set(Fix.LM.GetTermStrict("NOTCHES"), 6);
            pm.Set("DIFFICULTSKIPS", 1);

            IEnumerable<LazyStateBuilder> result = sm.ModifyState(null, pm, lsb);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void ShriekPogoFailsWith4CastsAndSpellTwisterWithoutDifficultSkips()
        {
            StateModifier sm = (StateModifier)Fix.LM.GetVariableStrict("$SHRIEKPOGO[4]");
            ProgressionManager pm = Fix.GetProgressionManager(ShriekPogoPMBase);
            LazyStateBuilder lsb = Fix.GetState(CharmStateBase);

            pm.Add(Fix.LM.GetItemStrict("Spell_Twister"));
            pm.Set("NOTCHES", 6);
            pm.Set("DIFFICULTSKIPS", 1);

            IEnumerable<LazyStateBuilder> result = sm.ModifyState(null, pm, lsb);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void ShriekPogoFailsWith4CastsAndASoulVessel()
        {
            StateModifier sm = (StateModifier)Fix.LM.GetVariableStrict("$SHRIEKPOGO[4]");
            ProgressionManager pm = Fix.GetProgressionManager(DifficultShriekPogoPMBase);
            LazyStateBuilder lsb = Fix.GetState(new());

            for (int i = 0; i < 3; i++) pm.Add(Fix.LM.GetItemStrict("Vessel_Fragment"));

            IEnumerable<LazyStateBuilder> result = sm.ModifyState(null, pm, lsb);
            Assert.Empty(result);
        }

        [Fact]
        public void ShriekPogoSucceedsWith3to1CastsAndASoulVessel()
        {
            StateModifier sm = (StateModifier)Fix.LM.GetVariableStrict("$SHRIEKPOGO[3,1]");
            ProgressionManager pm = Fix.GetProgressionManager(DifficultShriekPogoPMBase);
            LazyStateBuilder lsb = Fix.GetState(new());

            for (int i = 0; i < 3; i++) pm.Add(Fix.LM.GetItemStrict("Vessel_Fragment"));

            IEnumerable<LazyStateBuilder> result = sm.ModifyState(null, pm, lsb);
            Assert.NotEmpty(result);
        }

        [Fact]
        public void ShriekPogoSucceedsWith4CastsAndASoulVesselAndADash()
        {
            StateModifier sm = (StateModifier)Fix.LM.GetVariableStrict("$SHRIEKPOGO[4]");
            ProgressionManager pm = Fix.GetProgressionManager(DifficultShriekPogoPMBase);
            LazyStateBuilder lsb = Fix.GetState(new());

            for (int i = 0; i < 3; i++) pm.Add(Fix.LM.GetItemStrict("Vessel_Fragment"));
            pm.Add(Fix.LM.GetItemStrict("Mothwing_Cloak"));

            IEnumerable<LazyStateBuilder> result = sm.ModifyState(null, pm, lsb);
            Assert.NotEmpty(result);
        }

    }
}
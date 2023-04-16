using RandomizerCore.Logic.StateLogic;
using RandomizerCore.Logic;

namespace RandomizerModTests.StateVariables
{
    [Collection("Logic Collection")]
    public class SlopeballVariableTests
    {
        public LogicFixture Fix { get; }

        public SlopeballVariableTests(LogicFixture fix)
        {
            Fix = fix;
        }

        public static Dictionary<string, int> SlopeballPMBase => new() { ["FIREBALL"] = 1, ["SLOPEBALLSKIPS"] = 1 };

        public static object[][] InsufficientSlopeballPMBase => new Dictionary<string, int>[] { new(), new() { ["FIREBALL"] = 1 }, new(){ ["SLOPEBALLSKIPS"] = 1 } }.Select(d => new object[] {d}).ToArray();

        public static Dictionary<string, int> InsufficientSoulState => new() { ["SPENTSOUL"] = 99 };

        [Theory]
        [MemberData(nameof(InsufficientSlopeballPMBase))]
        public void CannotCastWithoutProgressionRequirements(Dictionary<string, int> pmFields)
        {
            StateModifier sm = (StateModifier)Fix.LM.GetVariableStrict("$SLOPEBALL");
            ProgressionManager pm = Fix.GetProgressionManager(pmFields);
            LazyStateBuilder lsb = Fix.GetState(new());

            IEnumerable<LazyStateBuilder> result = sm.ModifyState(null, pm, lsb);
            Assert.Empty(result);
        }

        [Fact]
        public void CannotCastWithoutSoul()
        {
            StateModifier sm = (StateModifier)Fix.LM.GetVariableStrict("$SLOPEBALL");
            ProgressionManager pm = Fix.GetProgressionManager(SlopeballPMBase);
            LazyStateBuilder lsb = Fix.GetState(InsufficientSoulState);

            IEnumerable<LazyStateBuilder> result = sm.ModifyState(null, pm, lsb);
            Assert.Empty(result);
        }

        [Fact]
        public void CanCastOnceWithRequirements()
        {
            StateModifier sm = (StateModifier)Fix.LM.GetVariableStrict("$SLOPEBALL");
            ProgressionManager pm = Fix.GetProgressionManager(SlopeballPMBase);
            LazyStateBuilder lsb = Fix.GetState(new());

            IEnumerable<LazyStateBuilder> result = sm.ModifyState(null, pm, lsb);
            Assert.NotEmpty(result);
        }

    }
}

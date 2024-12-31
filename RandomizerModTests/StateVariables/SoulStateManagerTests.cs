using FluentAssertions;
using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;
using RandomizerMod.RC.StateVariables;

namespace RandomizerModTests.StateVariables
{
    [Collection("Logic Collection")]
    public class SoulStateManagerTests(LogicFixture Fix)
    {
        public LazyStateBuilder Default => Fix.GetState([]);
        public LazyStateBuilder BenchFlower => Fix.GetState(new() { { "NOPASSEDCHARMEQUIP", 0 }, { "NOFLOWER", 0 } });
        public SoulStateManager SSM => (SoulStateManager)Fix.LM.GetVariableStrict(SoulStateManager.Prefix);
        public StateManager SM => Fix.LM.StateManager;
        public StateInt SpentSoul => SM.GetIntStrict("SPENTSOUL");
        public StateInt SpentReserveSoul => SM.GetIntStrict("SPENTRESERVESOUL");
        public StateInt SoulLimiter => SM.GetIntStrict("SOULLIMITER");
        public StateInt RequiredMaxSoul => SM.GetIntStrict("REQUIREDMAXSOUL");

        private readonly record struct ExpectedSoul(int SpentSoul, int SpentReserveSoul, int RequiredMaxSoul, int SoulLimiter);

        private void Check(ExpectedSoul soul, LazyStateBuilder state)
        {
            state.GetInt(SpentSoul).Should().Be(soul.SpentSoul);
            state.GetInt(SpentReserveSoul).Should().Be(soul.SpentReserveSoul);
            state.GetInt(RequiredMaxSoul).Should().Be(soul.RequiredMaxSoul);
            state.GetInt(SoulLimiter).Should().Be(soul.SoulLimiter);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void SpendSoulTheory(int testNum)
        {
            ProgressionManager pm = Fix.GetProgressionManager();
            List<LazyStateBuilder> states;
            ExpectedSoul[][] expectedSpend;
            switch (testNum)
            {
                default:
                case 0:
                    states = [Default];
                    expectedSpend = [[new(33, 0, 33, 0)], [new(66, 0, 66, 0)], [new(99, 0, 99, 0)], []];
                    break;
                case 1:
                    states = [Default];
                    pm.Set("VESSELFRAGMENTS", 3);
                    expectedSpend = [[new(0, 33, 33, 0)], [new(33, 33, 33, 0)], [new(66, 33, 66, 0)], [new(99, 33, 99, 0)], []];
                    break;
                case 2:
                    states = SSM.LimitSoul(pm, Default, 33, true).ToList();
                    expectedSpend = [[new(33, 0, 33, 33)], [new(66, 0, 66, 33)], []];
                    break;
            }
            
            for (int i = 0; i < expectedSpend.Length; i++)
            {
                states = states.SelectMany(s => SSM.SpendSoul(pm, s, 33)).ToList();
                states.Should().HaveCount(expectedSpend[i].Length);
                for (int j = 0; j < expectedSpend[i].Length; j++)
                {
                    Check(expectedSpend[i][j], states[j]);
                }
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void FullRestoreTheory(int testNum)
        {
            ProgressionManager pm = Fix.GetProgressionManager();
            ExpectedSoul expected;
            List<LazyStateBuilder> states;
            switch (testNum)
            {
                default:
                case 0:
                    states = [Default];
                    expected = new(0, 0, 66, 0);
                    break;
                case 1:
                    pm.Set("VESSELFRAGMENTS", 3);
                    states = [Default];
                    expected = new(0, 0, 66, 0);
                    break;
                case 2:
                    states = SSM.LimitSoul(pm, Default, 33, true).ToList();
                    expected = new(0, 0, 66, 33);
                    break;
            }
            List<LazyStateBuilder> states2 = states.Select(s => new LazyStateBuilder(s)).ToList();

            states = states.SelectMany(s => SSM.SpendSoul(pm, s, 66)).SelectMany(s => SSM.RestoreAllSoul(pm, s, true)).ToList();
            states.Should().ContainSingle();
            Check(expected, states[0]);

            states2 = states2.SelectMany(s => SSM.SpendAllSoul(pm, s)).SelectMany(s => SSM.RestoreAllSoul(pm, s, true)).ToList();
            states2.Should().ContainSingle();
            Check(expected with { RequiredMaxSoul = SSM.GetSoulInfo(pm, states2[0]).MaxSoul }, states2[0]);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void LimitSoulRoundtripTheory(int testNum)
        {
            ProgressionManager pm = Fix.GetProgressionManager();
            List<LazyStateBuilder> states;
            ExpectedSoul? expected;
            switch (testNum)
            {
                default:
                case 0:
                    states = [Default];
                    expected = new(33, 0, 33, 0);
                    break;
                case 1:
                    pm.Set("VESSELFRAGMENTS", 3);
                    states = [Default];
                    expected = new(0, 33, 33, 0);
                    break;
                case 2:
                    states = SSM.SpendSoul(pm, Default, 67).SelectMany(s => SSM.RestoreAllSoul(pm, s, true)).ToList();
                    expected = null;
                    break;
            }

            states = states.SelectMany(s => SSM.LimitSoul(pm, s, 33, true)).SelectMany(s => SSM.LimitSoul(pm, s, 0, false)).ToList();
            if (!expected.HasValue) states.Should().BeEmpty();
            else
            {
                states.Should().ContainSingle();
                Check(expected.Value, states[0]);
            }
        }

    }
}

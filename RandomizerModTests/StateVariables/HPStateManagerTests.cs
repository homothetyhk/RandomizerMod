using FluentAssertions;
using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;
using RandomizerMod.RC.StateVariables;
using Xunit.Abstractions;

namespace RandomizerModTests.StateVariables
{
    [Collection("Logic Collection")]
    public class HPStateManagerTests(LogicFixture Fix, ITestOutputHelper Output)
    {
        public LazyStateBuilder Default => Fix.GetState([]);
        public LazyStateBuilder BenchFlower => Fix.GetState(new() { { "NOPASSEDCHARMEQUIP", 0 }, { "NOFLOWER", 0 } });
        public HPStateManager HPSM => (HPStateManager)Fix.LM.GetVariableStrict(HPStateManager.Prefix);
        public StateManager SM => Fix.LM.StateManager;
        public StateInt LazySpentHP => SM.GetIntStrict("LAZYSPENTHP");
        public StateInt SpentHP => SM.GetIntStrict("SPENTHP");
        public StateInt SpentBlueHP => SM.GetIntStrict("SPENTBLUEHP");
        public StateBool Overcharmed => SM.GetBoolStrict("OVERCHARMED");
        public StateBool CannotOvercharm => SM.GetBoolStrict("CANNOTOVERCHARM");


        [Fact]
        public void EarlyStrictDamage()
        {
            ProgressionManager pm = Fix.GetProgressionManager([]);
            List<LazyStateBuilder> states = HPSM.DetermineHP(pm, Default).ToList();
            states.Should().HaveCount(2).And.AllSatisfy(s => HPSM.IsHPDetermined(s));

            LazyStateBuilder oc = states.First(s => s.GetBool(Overcharmed));
            LazyStateBuilder noc = states.First(s => s.GetBool(CannotOvercharm));
            oc.GetBool(CannotOvercharm).Should().BeFalse();
            noc.GetBool(Overcharmed).Should().BeFalse();

            List<LazyStateBuilder> oc_damage = HPSM.TakeDamage(pm, oc, 1).ToList();
            List<LazyStateBuilder> noc_damage = HPSM.TakeDamage(pm, noc, 1).ToList();
            oc_damage.Should().ContainSingle()
                .Subject.GetInt(SpentHP).Should().Be(2);
            noc_damage.Should().ContainSingle()
                .Subject.GetInt(SpentHP).Should().Be(1);
        }

        [Fact]
        public void LazyToStrictSwitchFiveMasks()
        {
            ProgressionManager pm = Fix.GetProgressionManager([]);
            List<LazyStateBuilder> states = [Default];
            
            for (int i = 0; i < 2; i++)
            {
                states = states.SelectMany(s => HPSM.TakeDamage(pm, s, 1)).ToList();
                states.Should().ContainSingle($" after taking {i + 1} damage.");
                states[0].GetInt(LazySpentHP).Should().Be(i + 1, $" after taking {i + 1} damage.");
                states[0].GetInt(SpentHP).Should().Be(0, $" after taking {i + 1} damage.");
                HPSM.IsHPDetermined(states[0]).Should().Be(false, $" after taking {i + 1} damage.");
            }
            for (int i = 2; i < 4; i++)
            {
                states = states.SelectMany(s => HPSM.TakeDamage(pm, s, 1)).ToList();
                states.Should().ContainSingle($" after taking {i + 1} damage.");
                states[0].GetInt(LazySpentHP).Should().Be(int.MaxValue, $" after taking {i + 1} damage.");
                states[0].GetInt(SpentHP).Should().Be(i + 1, $" after taking {i + 1} damage.");
                HPSM.IsHPDetermined(states[0]).Should().Be(true, $" after taking {i + 1} damage.");
            }
            {
                states = states.SelectMany(s => HPSM.TakeDamage(pm, s, 1)).ToList();
                states.Should().BeEmpty(" after taking 5 damage");
            }
        }

        [Fact]
        public void JoniHPInfo()
        {
            ProgressionManager pm = Fix.GetProgressionManager();
            pm.Set("Joni's_Blessing", 1);
            pm.Set("NOTCHES", 4);
            LazyStateBuilder state = BenchFlower;
            ((EquipCharmVariable)Fix.LM.GetVariableStrict(EquipCharmVariable.GetName("Joni's_Blessing"))).TryEquip(null, pm, ref state).Should().BeTrue();
            state.SetBool(SM.GetBool("CANNOTOVERCHARM"), true);
            state = HPSM.DetermineHP(pm, state).Single();
            IHPStateManager.StrictHPInfo hp = HPSM.GetHPInfo(pm, state);
            hp.MaxWhiteHP.Should().Be(7);
            hp.CurrentWhiteHP.Should().Be(7);
            hp.CurrentBlueHP.Should().Be(0);
        }

    }
}

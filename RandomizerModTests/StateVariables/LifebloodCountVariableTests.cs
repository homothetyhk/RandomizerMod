using FluentAssertions;
using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;
using RandomizerMod.RC.StateVariables;

namespace RandomizerModTests.StateVariables
{
    [Collection("Logic Collection")]
    public class LifebloodCountVariableTests(LogicFixture Fix)
    {
        public LazyStateBuilder BenchFlower => Fix.GetState(new() { { "NOPASSEDCHARMEQUIP", 0 }, { "NOFLOWER", 0 } });

        [Fact]
        public void FailsOnDefaultProgression()
        {
            ProgressionManager pm = Fix.GetProgressionManager();
            LifebloodCountVariable lcv = (LifebloodCountVariable)Fix.LM.GetVariableStrict(LifebloodCountVariable.Prefix);
            List<LazyStateBuilder> states = [BenchFlower];
            lcv.ModifyAll(null, pm, states);
            states.Should().BeEmpty("since pm has no lifeblood charms.");
        }

        [Fact]
        public void SucceedsWithAnyOneLifebloodCharm()
        {
            string[] charmNames = ["Lifeblood_Heart", "Lifeblood_Core", "Joni's_Blessing"];
            EquipCharmVariable[] charms = charmNames.Select(s => Fix.LM.GetVariableStrict(EquipCharmVariable.GetName(s)))
                .Cast<EquipCharmVariable>().ToArray();
            for (int i = 0; i < 3; i++)
            {
                ProgressionManager pm = Fix.GetProgressionManager();
                pm.Set(charmNames[i], 1);
                pm.Set("NOTCHES", 4);
                LifebloodCountVariable lcv = (LifebloodCountVariable)Fix.LM.GetVariableStrict(LifebloodCountVariable.Prefix);
                List<LazyStateBuilder> states = [BenchFlower];
                lcv.ModifyAll(null, pm, states);
                states.Should().NotBeEmpty($"pm contains {charmNames[i]}.");
            }
        }

        [Fact]
        public void FailsWithLifebloodHeartAfter2Damage()
        {
            ProgressionManager pm = Fix.GetProgressionManager();
            pm.Set("Lifeblood_Heart", 1);
            pm.Set("NOTCHES", 4);
            LifebloodCountVariable lcv = (LifebloodCountVariable)Fix.LM.GetVariableStrict(LifebloodCountVariable.Prefix);
            TakeDamageVariable tdv = (TakeDamageVariable)Fix.LM.GetVariableStrict(TakeDamageVariable.Prefix);
            List<LazyStateBuilder> states = [BenchFlower];
            tdv.ModifyAll(null, pm, states);
            tdv.ModifyAll(null, pm, states);
            lcv.ModifyAll(null, pm, states);
            states.Should().BeEmpty($"Lifeblood Heart is exhausted by 2 damage.");
        }

        [Fact]
        public void SucceedsWithLifebloodCoreOrJoniAfter2Damage()
        {
            string[] charmNames = ["Lifeblood_Core", "Joni's_Blessing"];
            EquipCharmVariable[] charms = charmNames.Select(s => Fix.LM.GetVariableStrict(EquipCharmVariable.GetName(s)))
                .Cast<EquipCharmVariable>().ToArray();

            for (int i = 0; i < charmNames.Length; i++)
            {
                ProgressionManager pm = Fix.GetProgressionManager();
                pm.Set(charmNames[i], 1);
                pm.Set("NOTCHES", 4);
                LifebloodCountVariable lcv = (LifebloodCountVariable)Fix.LM.GetVariableStrict(LifebloodCountVariable.Prefix);
                TakeDamageVariable tdv = (TakeDamageVariable)Fix.LM.GetVariableStrict(TakeDamageVariable.Prefix);
                List<LazyStateBuilder> states = [BenchFlower];
                tdv.ModifyAll(null, pm, states);
                tdv.ModifyAll(null, pm, states);
                lcv.ModifyAll(null, pm, states);
                states.Should().NotBeEmpty($"{charmNames[i]} is not exhausted by 2 damage.");
            }
        }

    }
}

using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;
using RandomizerMod.RandomizerData;
using RandomizerMod.RC;

namespace RandomizerModTests
{
    public class LogicFixture
    {
        public LogicManager LM { get; }
        public RandoModContext Default_CTX { get; }

        public LogicFixture()
        {
            Data.Load();
            Default_CTX = new(new(), Data.GetStartDef("King's Pass"));
            Default_CTX.notchCosts.AddRange(CharmNotchCosts._vanillaCosts);
            LM = Default_CTX.LM;
        }

        public ProgressionManager GetProgressionManager(Dictionary<string, int> pmFieldValues)
        {
            ProgressionManager pm = new(LM, Default_CTX);
            foreach (var kvp in pmFieldValues) pm.Set(kvp.Key, kvp.Value);
            return pm;
        }

        public LazyStateBuilder GetState(Dictionary<string, int> stateFieldValues)
        {
            LazyStateBuilder lsb = new(LM.StateManager.DefaultState);
            foreach (var kvp in stateFieldValues)
            {
                StateField sf = LM.StateManager.FieldLookup[kvp.Key];
                if (sf is StateBool) lsb.SetBool(sf, kvp.Value == 1);
                else lsb.SetInt(sf, kvp.Value);
            }
            return lsb;
        }

    }

    [CollectionDefinition("Logic Collection")]
    public class LMCollection : ICollectionFixture<LogicFixture>
    {

    }
}
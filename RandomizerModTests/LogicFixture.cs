using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;
using RandomizerMod.RandomizerData;
using RandomizerMod.RC;

namespace RandomizerModTests
{
    public class LogicFixture
    {
        public LogicManager LM { get; }

        private RandoModContext Default_CTX { get; }

        public LogicFixture()
        {
            Data.Load();
            Default_CTX = new(new(), Data.GetStartDef("King's Pass"));
            Default_CTX.notchCosts.AddRange(CharmNotchCosts._vanillaCosts);
            LM = Default_CTX.LM;
        }

        public RandoModContext GetContext()
        {
            return new(Default_CTX);
        }

        public ProgressionManager GetProgressionManager()
        {
            return new(LM, new RandoModContext(Default_CTX));
        }

        public ProgressionManager GetProgressionManager(Dictionary<string, int> pmFieldValues)
        {
            ProgressionManager pm = GetProgressionManager();
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

        public LazyStateBuilder GetStateP(params (string, int)[] vals) => GetState(vals.ToDictionary(p => p.Item1, p => p.Item2));

    }

    [CollectionDefinition("Logic Collection")]
    public class LMCollection : ICollectionFixture<LogicFixture>
    {

    }
}
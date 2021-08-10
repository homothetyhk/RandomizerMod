using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerMod.Settings
{
    [Serializable]
    public class GenerationSettings : ICloneable
    {
        public int Seed;
        public TransitionSettings TransitionSettings = new TransitionSettings();
        public SkipSettings SkipSettings = new SkipSettings();
        public PoolSettings PoolSettings = new PoolSettings();
        public CursedSettings CursedSettings = new CursedSettings();
        public GrubCostRandomizerSettings GrubCostRandomizerSettings = new GrubCostRandomizerSettings();
        public EssenceCostRandomizerSettings EssenceCostRandomizerSettings = new EssenceCostRandomizerSettings();
        public LongLocationSettings LongLocationSettings = new LongLocationSettings();
        public StartLocationSettings StartLocationSettings = new StartLocationSettings();
        public StartItemSettings StartItemSettings = new StartItemSettings();
        public MiscSettings MiscSettings = new MiscSettings();

        private object[] serializationFields => new object[]
        {
            TransitionSettings,
            SkipSettings,
            PoolSettings,
            CursedSettings,
            GrubCostRandomizerSettings,
            EssenceCostRandomizerSettings,
            LongLocationSettings,
            StartLocationSettings,
            StartItemSettings,
            MiscSettings
        };

        public string Serialize()
        {
            return string.Join(BinaryFormatting.CLASS_SEPARATOR.ToString(), serializationFields.Select(o => BinaryFormatting.Serialize(o)).ToArray());
        }

        public static GenerationSettings Deserialize(string code)
        {
            GenerationSettings gs = new GenerationSettings();
            string[] pieces = code.Split(BinaryFormatting.CLASS_SEPARATOR);
            object[] fields = gs.serializationFields;

            if (pieces.Length != fields.Length)
            {
                LogHelper.LogWarn("Invalid settings code: not enough pieces.");
                return null;
            }

            for (int i = 0; i < fields.Length; i++)
            {
                BinaryFormatting.Deserialize(pieces[i], fields[i]);
            }

            return gs;
        }

        public object Clone()
        {
            return new GenerationSettings
            {
                Seed = Seed,
                TransitionSettings = TransitionSettings.Clone() as TransitionSettings,
                SkipSettings = SkipSettings.Clone() as SkipSettings,
                PoolSettings = PoolSettings.Clone() as PoolSettings,
                GrubCostRandomizerSettings = GrubCostRandomizerSettings.Clone() as GrubCostRandomizerSettings,
                EssenceCostRandomizerSettings = EssenceCostRandomizerSettings.Clone() as EssenceCostRandomizerSettings,
                LongLocationSettings = LongLocationSettings.Clone() as LongLocationSettings,
                CursedSettings = CursedSettings.Clone() as CursedSettings,
                StartLocationSettings = StartLocationSettings.Clone() as StartLocationSettings,
                StartItemSettings = StartItemSettings.Clone() as StartItemSettings,
                MiscSettings = MiscSettings.Clone() as MiscSettings,
            };
        }
    }
}

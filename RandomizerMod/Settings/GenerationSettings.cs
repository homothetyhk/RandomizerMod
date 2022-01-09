using System.Reflection;

namespace RandomizerMod.Settings
{
    [Serializable]
    public class GenerationSettings : ICloneable
    {
        public int Seed;
        public TransitionSettings TransitionSettings = Presets.TransitionPresetData.None.Clone() as TransitionSettings;
        public SkipSettings SkipSettings = Presets.SkipPresetData.Casual.Clone() as SkipSettings;
        public PoolSettings PoolSettings = Presets.PoolPresetData.Standard.Clone() as PoolSettings;
        public NoveltySettings NoveltySettings = Presets.NoveltyPresetData.Basic.Clone() as NoveltySettings;
        public CostSettings CostSettings = Presets.CostPresetData.Standard.Clone() as CostSettings;
        public CursedSettings CursedSettings = Presets.CursePresetData.None.Clone() as CursedSettings;
        public LongLocationSettings LongLocationSettings = Presets.LongLocationPresetData.Standard.Clone() as LongLocationSettings;
        public StartLocationSettings StartLocationSettings = Presets.StartLocationPresetData.KingsPass.Clone() as StartLocationSettings;
        public StartItemSettings StartItemSettings = Presets.StartItemPresetData.EarlyGeo.Clone() as StartItemSettings;
        public MiscSettings MiscSettings = Presets.MiscPresetData.Standard.Clone() as MiscSettings;
        public ProgressionDepthSettings ProgressionDepthSettings = new();
        public DuplicateItemSettings DuplicateItemSettings = Presets.DuplicateItemPresetData.DuplicateMajorItems.Clone() as DuplicateItemSettings;
        public SplitGroupSettings SplitGroupSettings = Presets.SplitGroupPresetData.Disabled.Clone() as SplitGroupSettings;

        public GenerationSettings()
        {
        }

        private SettingsModule[] modules => moduleFields.Select(f => f.GetValue(this) as SettingsModule).ToArray();
        private static readonly FieldInfo[] moduleFields = typeof(GenerationSettings).GetFields().Where(f => f.FieldType.IsSubclassOf(typeof(SettingsModule)))
            .OrderBy(f => f.Name).ToArray();

        public string Serialize()
        {
            return RandomizerMod.Version + string.Join(BinaryFormatting.CLASS_SEPARATOR.ToString(), modules.Select(o => BinaryFormatting.Serialize(o)).ToArray());
        }

        public static GenerationSettings Deserialize(string code)
        {
            if (!code.StartsWith(RandomizerMod.Version))
            {
                throw new ArgumentException("Invalid settings code: outdated RandomizerMod version.");
            }
            else code = code.Substring(RandomizerMod.Version.Length);

            GenerationSettings gs = new();
            string[] pieces = code.Split(BinaryFormatting.CLASS_SEPARATOR);
            object[] fields = gs.modules;

            if (pieces.Length != fields.Length)
            {
                throw new ArgumentException("Invalid settings code: not enough pieces.");
            }

            for (int i = 0; i < fields.Length; i++)
            {
                BinaryFormatting.Deserialize(pieces[i], fields[i]);
            }

            return gs;
        }

        public void Randomize(Random rng)
        {
            Seed = rng.Next(1000000000);
            foreach (SettingsModule m in modules) m.Randomize(rng);
            Clamp();
        }

        public void Clamp()
        {
            foreach (SettingsModule m in modules) m.Clamp(this);
        }

        public object Clone()
        {
            GenerationSettings gs = MemberwiseClone() as GenerationSettings;
            foreach (FieldInfo f in moduleFields) f.SetValue(gs, (f.GetValue(this) as SettingsModule).Clone());
            return gs;
        }

        public void CopyTo(GenerationSettings target)
        {
            foreach (FieldInfo fi in moduleFields)
            {
                ((SettingsModule)fi.GetValue(this)).CopyTo((SettingsModule)fi.GetValue(target));
            }
        }

        // fields for hard-coding the path of a logic setting
        private const bool True = true;
        private const bool False = false;
    }
}

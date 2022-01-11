namespace RandomizerMod.Settings
{
    public class GlobalSettings
    {
        public GenerationSettings DefaultMenuSettings = new();
        public List<MenuProfile> Profiles = new(){ null };

        public static bool IsInvalid(GlobalSettings value)
        {
            return value is null || value.Profiles is null || value.DefaultMenuSettings is null;
        }
    }

    public class MenuProfile
    {
        public string name;
        public GenerationSettings settings;
        public override string ToString()
        {
            return name;
        }
    }
}

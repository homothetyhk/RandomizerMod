namespace RandomizerMod.Settings.Presets
{
    public static class SkipPresetData
    {
        public static SkipSettings Casual;
        public static SkipSettings Experienced;
        public static SkipSettings Advanced;
        public static SkipSettings Fearless;
        public static SkipSettings Foolish;
        public static Dictionary<string, SkipSettings> SkipPresets;

        static SkipPresetData()
        {
            Casual = new SkipSettings
            {
                PreciseMovement = false,
                ProficientCombat = false,
                BackgroundObjectPogos = false,
                EnemyPogos = false,
                ObscureSkips = false,
                ShadeSkips = false,
                InfectionSkips = false,
                FireballSkips = false,
                SpikeTunnels = false,
                AcidSkips = false,
                DamageBoosts = false,
                DangerousSkips = false,
                DarkRooms = false,
                ComplexSkips = false,
                DifficultSkips = false,
            };

            Experienced = new SkipSettings
            {
                PreciseMovement = true,
                ProficientCombat = true,
                BackgroundObjectPogos = true,
                EnemyPogos = true,
                ObscureSkips = true,
                ShadeSkips = false,
                InfectionSkips = false,
                FireballSkips = false,
                SpikeTunnels = false,
                AcidSkips = false,
                DamageBoosts = false,
                DangerousSkips = false,
                DarkRooms = false,
                ComplexSkips = false,
                DifficultSkips = false,
            };

            Advanced = new SkipSettings
            {
                PreciseMovement = true,
                ProficientCombat = true,
                BackgroundObjectPogos = true,
                EnemyPogos = true,
                ObscureSkips = true,
                ShadeSkips = true,
                InfectionSkips = true,
                FireballSkips = true,
                SpikeTunnels = true,
                AcidSkips = true,
                DamageBoosts = false,
                DangerousSkips = false,
                DarkRooms = false,
                ComplexSkips = false,
                DifficultSkips = false,
            };

            Fearless = new SkipSettings
            {
                PreciseMovement = true,
                BackgroundObjectPogos = true,
                EnemyPogos = true,
                ObscureSkips = true,
                ShadeSkips = true,
                InfectionSkips = true,
                FireballSkips = true,
                SpikeTunnels = true,
                AcidSkips = true,
                DamageBoosts = true,
                DangerousSkips = true,
                DarkRooms = false,
                ComplexSkips = false,
                DifficultSkips = false,
            };

            Foolish = new SkipSettings
            {
                PreciseMovement = true,
                BackgroundObjectPogos = true,
                EnemyPogos = true,
                ObscureSkips = true,
                ShadeSkips = true,
                InfectionSkips = true,
                FireballSkips = true,
                SpikeTunnels = true,
                AcidSkips = true,
                DamageBoosts = true,
                DangerousSkips = true,
                DarkRooms = true,
                ComplexSkips = true,
                DifficultSkips = true,
            };

            SkipPresets = new Dictionary<string, SkipSettings>
            {
                { "Casual", Casual },
                { "Experienced", Experienced },
                { "Advanced", Advanced },
                { "Fearless", Fearless },
                { "Foolish", Foolish }
            };
        }
    }
}

namespace RandomizerMod.Settings.Presets
{
    public static class StartItemPresetData
    {
        public static StartItemSettings EarlyGeo;
        public static StartItemSettings GeoAndStartItems;
        public static StartItemSettings None;

        public static Dictionary<string, StartItemSettings> StartItemPresets;




        static StartItemPresetData()
        {
            EarlyGeo = new StartItemSettings
            {
                MinimumStartGeo = 300,
                MaximumStartGeo = 600,
            };

            GeoAndStartItems = new StartItemSettings
            {
                MinimumStartGeo = 300,
                MaximumStartGeo = 600,
                Stags = StartItemSettings.StartStagType.ZeroOrMoreRandomStags,
                Charms = StartItemSettings.StartCharmType.ZeroOrMore,
                HorizontalMovement = StartItemSettings.StartHorizontalType.ZeroOrMore,
                VerticalMovement = StartItemSettings.StartVerticalType.ZeroOrMore,
                MiscItems = StartItemSettings.StartMiscItems.ZeroOrMore,
            };

            None = new StartItemSettings
            {
                MinimumStartGeo = 0,
                MaximumStartGeo = 0,
            };

            StartItemPresets = new Dictionary<string, StartItemSettings>
            {
                { "Early Geo", EarlyGeo },
                { "Geo and Start Items", GeoAndStartItems },
                { "None", None },
            };
        }
    }
}

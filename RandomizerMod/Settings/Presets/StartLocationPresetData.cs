namespace RandomizerMod.Settings.Presets
{
    public static class StartLocationPresetData
    {
        public static StartLocationSettings KingsPass;
        public static StartLocationSettings RandomNoKP;
        public static StartLocationSettings RandomWithKP;

        public static Dictionary<string, StartLocationSettings> StartLocationPresets;

        static StartLocationPresetData()
        {
            KingsPass = new StartLocationSettings
            {
                StartLocationType = StartLocationSettings.RandomizeStartLocationType.Fixed,
                StartLocation = "King's Pass",
            };

            RandomNoKP = new StartLocationSettings
            {
                StartLocationType = StartLocationSettings.RandomizeStartLocationType.RandomExcludingKP,
                StartLocation = null,
            };

            RandomWithKP = new StartLocationSettings
            {
                StartLocationType = StartLocationSettings.RandomizeStartLocationType.Random,
                StartLocation = null,
            };

            StartLocationPresets = new Dictionary<string, StartLocationSettings>
            {
                { "King's Pass", KingsPass },
                { "Random (no King's Pass)", RandomNoKP },
                { "Random (allow King's Pass)", RandomWithKP },
            };
        }
    }
}

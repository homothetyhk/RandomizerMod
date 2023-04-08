namespace RandomizerMod.Settings
{
    public class SkipSettings : SettingsModule
    {
        public bool PreciseMovement;
        public bool ProficientCombat;
        public bool BackgroundObjectPogos;
        public bool EnemyPogos;
        public bool ObscureSkips;
        public bool ShadeSkips;
        public bool InfectionSkips;
        public bool FireballSkips;
        public bool SpikeTunnels;
        public bool AcidSkips;
        public bool DamageBoosts;
        public bool DangerousSkips;
        public bool DarkRooms;
        public bool Slopeballs;
        public bool ShriekPogos;
        public bool ComplexSkips;
        public bool DifficultSkips;
        public override void Clamp(GenerationSettings gs)
        {
            base.Clamp(gs);
            if (gs.MiscSettings.FireballUpgrade != MiscSettings.ToggleableFireballSetting.Toggleable) Slopeballs = false;
            if (gs.MiscSettings.SteelSoul) ShadeSkips = false;
        }
    }
}

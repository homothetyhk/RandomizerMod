using MenuChanger.Attributes;
using RandomizerMod.RandomizerData;
using System.Reflection;

namespace RandomizerMod.Settings
{
    public class SplitGroupSettings : SettingsModule
    {
        [MenuRange(-1, 99)]
        public int Dreamers;
        [MenuRange(-1, 99)]
        public int Skills;
        [MenuRange(-1, 99)]
        public int Charms;
        [MenuRange(-1, 99)]
        public int Keys;
        [MenuRange(-1, 99)]
        public int MaskShards;
        [MenuRange(-1, 99)]
        public int VesselFragments;
        [MenuRange(-1, 99)]
        public int CharmNotches;
        [MenuRange(-1, 99)]
        public int PaleOre;
        [MenuRange(-1, 99)]
        public int GeoChests;
        [MenuRange(-1, 99)]
        public int RancidEggs;
        [MenuRange(-1, 99)]
        public int Relics;
        [MenuRange(-1, 99)]
        public int WhisperingRoots;
        [MenuRange(-1, 99)]
        public int BossEssence;
        [MenuRange(-1, 99)]
        public int Grubs;
        [MenuRange(-1, 99)]
        public int Mimics;
        [MenuRange(-1, 99)]
        public int Maps;
        [MenuRange(-1, 99)]
        public int Stags;
        [MenuRange(-1, 99)]
        public int LifebloodCocoons;
        [MenuRange(-1, 99)]
        public int GrimmkinFlames;
        [MenuRange(-1, 99)]
        public int JournalEntries;
        [MenuRange(-1, 99)]
        public int GeoRocks;
        [MenuRange(-1, 99)]
        public int BossGeo;
        [MenuRange(-1, 99)]
        public int SoulTotems;
        [MenuRange(-1, 99)]
        public int LoreTablets;

        public static readonly Dictionary<string, FieldInfo> Fields = typeof(SplitGroupSettings)
            .GetFields(BindingFlags.Instance | BindingFlags.Public)
            .ToDictionary(fi => fi.Name);

        public bool TryGetValue(PoolDef def, out int value)
        {
            if (def.Group != null && Fields.TryGetValue(def.Group, out FieldInfo fi))
            {
                int i = (int)fi.GetValue(this);
                if (i >= 0)
                {
                    value = i;
                    return true;
                }
            }
            value = -1;
            return false;
        }

    }
}

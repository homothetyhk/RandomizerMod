using ItemChanger;
using RandomizerMod.Settings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomizerMod.IC
{
    public static class Shops
    {
        public static DefaultShopItems GetDefaultShopItems(GenerationSettings gs)
        {
            DefaultShopItems items = DefaultShopItems.None;

            items |= DefaultShopItems.IseldaMapPins;
            items |= DefaultShopItems.IseldaMapMarkers;
            items |= DefaultShopItems.LegEaterRepair;

            if (!gs.PoolSettings.Keys)
            {
                items |= DefaultShopItems.SlyLantern;
                items |= DefaultShopItems.SlySimpleKey;
                items |= DefaultShopItems.SlyKeyElegantKey;
            }

            if (!gs.PoolSettings.Charms)
            {
                items |= DefaultShopItems.SlyCharms;
                items |= DefaultShopItems.SlyKeyCharms;
                items |= DefaultShopItems.IseldaCharms;
                items |= DefaultShopItems.SalubraCharms;
                items |= DefaultShopItems.LegEaterCharms;
            }

            if (!gs.PoolSettings.Maps)
            {
                items |= DefaultShopItems.IseldaQuill;
                items |= DefaultShopItems.IseldaMaps;
            }

            if (!gs.PoolSettings.MaskShards)
            {
                items |= DefaultShopItems.SlyMaskShards;
            }

            if (!gs.PoolSettings.VesselFragments)
            {
                items |= DefaultShopItems.SlyVesselFragments;
            }

            if (!gs.PoolSettings.RancidEggs)
            {
                items |= DefaultShopItems.SlyRancidEgg;
            }

            if (!gs.PoolSettings.CharmNotches && gs.MiscSettings.SalubraNotches == MiscSettings.SalubraNotchesSetting.GroupedWithCharmNotchesPool
                || gs.MiscSettings.SalubraNotches == MiscSettings.SalubraNotchesSetting.Vanilla
                || gs.MiscSettings.SalubraNotches == MiscSettings.SalubraNotchesSetting.AutoGivenAtCharmThreshold)
            {
                items |= DefaultShopItems.SalubraNotches;
                items |= DefaultShopItems.SalubraBlessing;
            }

            return items;
        }
    }
}

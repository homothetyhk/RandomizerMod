using ItemChanger;
using RandomizerMod.Settings;
using RandomizerMod.RC;
using ItemChanger.Locations;

namespace RandomizerMod.IC
{
    public static class Seer
    {
        public static AbstractPlacement GetSeerPlacement(ICFactory factory)
        {
            ContainerLocation chest = (ContainerLocation)factory.MakeLocation(LocationNames.Vessel_Fragment_Seer);
            PlaceableLocation tablet = (PlaceableLocation)factory.MakeLocation(LocationNames.Hallownest_Seal_Seer);
            if (chest == null || tablet == null)
            {
                throw new InvalidOperationException("Error constructing Seer location!");
            }

            chest.name = tablet.name = "Seer";
            var p = new ItemChanger.Placements.CostChestPlacement("Seer")
            {
                chestLocation = chest,
                tabletLocation = tablet,
            };
            p.AddTag<ItemChanger.Tags.DestroySeerRewardTag>().destroyRewards = GetRandomizedSeerRewards(factory.gs);
            return p;
        }


        public static SeerRewards GetRandomizedSeerRewards(GenerationSettings gs)
        {
            SeerRewards sr = SeerRewards.None;
            if (gs.PoolSettings.Relics)
            {
                sr |= SeerRewards.HallownestSeal | SeerRewards.ArcaneEgg;
            }
            if (gs.PoolSettings.PaleOre)
            {
                sr |= SeerRewards.PaleOre;
            }
            if (gs.PoolSettings.Charms)
            {
                sr |= SeerRewards.DreamWielder;
            }
            if (gs.PoolSettings.VesselFragments)
            {
                sr |= SeerRewards.VesselFragment;
            }
            if (gs.PoolSettings.Skills)
            {
                sr |= SeerRewards.DreamGate | SeerRewards.AwokenDreamNail;
            }
            if (gs.PoolSettings.MaskShards)
            {
                sr |= SeerRewards.MaskShard;
            }

            return sr;
        }
    }
}

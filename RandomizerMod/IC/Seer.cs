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
            AbstractPlacement p = factory.MakeLocation(LocationNames.Seer).Wrap();
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

using ItemChanger;
using RandomizerMod.Settings;
using RandomizerMod.RC;
using ItemChanger.Locations;

namespace RandomizerMod.IC
{
    public static class Grubfather
    {
        public static AbstractPlacement GetGrubfatherPlacement(ICFactory factory)
        {
            AbstractPlacement p = factory.MakeLocation(LocationNames.Grubfather).Wrap();
            p.AddTag<ItemChanger.Tags.DestroyGrubRewardTag>().destroyRewards = GetRandomizedGrubRewards(factory.gs);
            return p;
        }

        public static GrubfatherRewards GetRandomizedGrubRewards(GenerationSettings gs)
        {
            GrubfatherRewards gr = GrubfatherRewards.None;
            if (gs.PoolSettings.Charms)
            {
                gr |= GrubfatherRewards.Grubsong | GrubfatherRewards.GrubberflysElegy;
            }
            if (gs.PoolSettings.MaskShards)
            {
                gr |= GrubfatherRewards.MaskShard;
            }
            if (gs.PoolSettings.PaleOre)
            {
                gr |= GrubfatherRewards.PaleOre;
            }
            if (gs.PoolSettings.Relics)
            {
                gr |= GrubfatherRewards.HallownestSeal | GrubfatherRewards.KingsIdol;
            }
            if (gs.PoolSettings.RancidEggs)
            {
                gr |= GrubfatherRewards.RancidEgg;
            }

            return gr;
        }
    }
}

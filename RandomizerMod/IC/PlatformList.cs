using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ItemChanger;
using RandomizerCore;
using RandomizerMod.RC;
using RandomizerMod.Settings;

namespace RandomizerMod.IC
{
    public static class PlatformList
    {
        public static IBool hasWalljump = new PDBool(nameof(PlayerData.hasWalljump));
        public static IBool hasWalljumpLeft = new PDBool(nameof(ItemChanger.Modules.SplitClaw.hasWalljumpLeft));
        public static IBool hasWalljumpRight = new PDBool(nameof(ItemChanger.Modules.SplitClaw.hasWalljumpRight));
        public static IBool hasDoubleJump = new PDBool(nameof(PlayerData.hasDoubleJump));

        public static IBool lacksLeftClaw = new Negation(new Disjunction(hasWalljump, hasWalljumpLeft));
        public static IBool lacksLeftVertical = new Negation(new Disjunction(hasWalljump, hasDoubleJump, hasWalljumpLeft));
        public static IBool lacksRightClaw = new Negation(new Disjunction(hasWalljump, hasWalljumpRight));
        public static IBool lacksRightVertical = new Negation(new Disjunction(hasWalljump, hasDoubleJump, hasWalljumpRight));
        public static IBool lacksAnyClaw = new Negation(new Disjunction(hasWalljump, hasWalljumpLeft, hasWalljumpRight));
        public static IBool lacksAnyVertical = new Negation(new Disjunction(hasWalljump, hasDoubleJump, hasWalljumpLeft, hasWalljumpRight));

        public static List<SmallPlatform> GetPlatformList(GenerationSettings gs, RandoModContext ctx)
        {
            HashSet<string> targetNames = new(ctx.transitionPlacements?.Select(x => x.target.Name) ?? Enumerable.Empty<string>());

            List<SmallPlatform> plats = new();

            // TODO: extra platform settings check

            // Platforms to climb out from basin wanderer's journal
            plats.Add(new() { SceneName = SceneNames.Abyss_02, X = 128.3f, Y = 7f, Test = lacksLeftClaw });
            plats.Add(new() { SceneName = SceneNames.Abyss_02, X = 128.3f, Y = 11f, Test = lacksLeftClaw });

            // Platforms to climb up to tram in basin from left with no items
            if (!targetNames.Contains($"{SceneNames.Abyss_03}[bot1]"))
            {
                plats.Add(new() { SceneName = SceneNames.Abyss_03, X = 34f, Y = 7f, Test = lacksRightVertical });
            }

            // Platform to climb out of Abyss with only wings
            plats.Add(new() { SceneName = SceneNames.Abyss_06_Core, X = 88.6f, Y = 263f, Test = lacksLeftClaw });

            // Platforms to climb back up from pale ore with no items
            plats.Add(new() { SceneName = SceneNames.Abyss_17, X = 164.7f, Y = 30f, Test = lacksRightVertical });
            plats.Add(new() { SceneName = SceneNames.Abyss_17, X = 99.5f, Y = 12.5f, Test = lacksRightVertical });
            plats.Add(new() { SceneName = SceneNames.Abyss_17, X = 117.7f, Y = 18.8f, Test = lacksAnyClaw });
            plats.Add(new() { SceneName = SceneNames.Abyss_17, X = 114.3f, Y = 23f, Test = lacksAnyClaw });
            plats.Add(new() { SceneName = SceneNames.Abyss_17, X = 117.7f, Y = 7f, Test = lacksAnyClaw });
            plats.Add(new() { SceneName = SceneNames.Abyss_17, X = 117.7f, Y = 10.8f, Test = lacksAnyClaw });

            // Platforms to remove softlock with wings at simple key in basin
            plats.Add(new() { SceneName = SceneNames.Abyss_20, X = 26.5f, Y = 13f, Test = lacksAnyClaw });

            // Platform for returning to Gorb landing
            plats.Add(new() { SceneName = SceneNames.Cliffs_02, X = 32.3f, Y = 27.7f, Test = lacksAnyVertical });

            // Platform to return from Deepnest mimic grub room
            if (!targetNames.Contains($"{SceneNames.Deepnest_01b}[right2]")
                && !targetNames.Contains($"{SceneNames.Deepnest_02}[left1]")
                && !targetNames.Contains($"{SceneNames.Deepnest_02}[right1]"))
            {
                plats.Add(new() { SceneName = SceneNames.Deepnest_01b, X = 48.3f, Y = 40f, Test = lacksAnyVertical });
            }

            // Platforms to return from the Deepnest_02 geo rocks without vertical
            plats.Add(new() { SceneName = SceneNames.Deepnest_02, X = 26f, Y = 12f, Test = lacksAnyClaw });
            plats.Add(new() { SceneName = SceneNames.Deepnest_02, X = 26f, Y = 16f, Test = lacksAnyClaw });

            // Platform to escape the Deepnest mimic room when mimics may not be present
            if (gs.CursedSettings.RandomizeMimics)
            {
                plats.Add(new() { SceneName = SceneNames.Deepnest_36, X = 26f, Y = 11f, Test = lacksLeftVertical });
            }

            // Platforms to climb back up from Mantis Lords with only wings
            if (!targetNames.Contains($"{SceneNames.Fungus2_15}[left1]") 
                && !targetNames.Contains($"{SceneNames.Fungus2_25}[top1]") 
                && !targetNames.Contains($"{SceneNames.Fungus2_25}[top2]"))
            {
                for (int i = 0; i < 2; i++)
                {
                    plats.Add(new() { SceneName = SceneNames.Fungus2_15, X = 48f + 2 * i, Y = 15f + 10 * i, Test = lacksRightClaw });
                }
            }

            // Platforms to prevent softlock on lever on the way to love key.
            for (int i = 0; i < 2; i++)
            {
               plats.Add(new() { SceneName = SceneNames.Fungus3_05, X = 65.7f, Y = 11f + 4.5f * i, Test = lacksRightClaw });
            }

            // Move the load in colo downward to prevent bench soft lock
            if (!targetNames.Contains($"{SceneNames.Room_Colosseum_02}[top2]")
                && !targetNames.Contains($"{SceneNames.Room_Colosseum_Spectate}[right1]"))
            {
                plats.Add(new() { SceneName = SceneNames.Room_Colosseum_02, X = 43.5f, Y = 45f, Test = lacksAnyClaw });
                plats.Add(new() { SceneName = SceneNames.Room_Colosseum_02, X = 43.5f, Y = 49.5f, Test = lacksAnyClaw });
            }

            // Platform to escape from the geo rock above Lemm
            plats.Add(new() { SceneName = SceneNames.Ruins1_05c, X = 26.6f, Y = 73.2f, Test = lacksAnyVertical });

            // Platforms to climb back up to King's Pass with no items
            if (!targetNames.Contains($"{SceneNames.Town}[right1]") && gs.StartLocationSettings.StartLocation == "King's Pass")
            {
                for (int i = 0; i < 6; i++)
                {
                    plats.Add(new() { SceneName = SceneNames.Town, X = 20f - 2 * (i % 2), Y = 5f * i + 15f, Test = lacksLeftClaw });
                }
            }

            // Platforms to prevent itemless softlock when checking left waterways
            if (!targetNames.Contains($"{SceneNames.Waterways_04}[left1]")
                && !targetNames.Contains($"{SceneNames.Waterways_04}[left2]")
                && !targetNames.Contains($"{SceneNames.Waterways_04b}[left1]")
                && !targetNames.Contains($"{SceneNames.Waterways_09}[left1]")
                && gs.StartLocationSettings.StartLocation != "West Waterways")
            {
                plats.Add(new() { SceneName = SceneNames.Waterways_04, X = 148f, Y = 23.1f, Test = lacksAnyVertical });
                plats.Add(new() { SceneName = SceneNames.Waterways_04, X = 139f, Y = 32f, Test = lacksAnyVertical });
                plats.Add(new() { SceneName = SceneNames.Waterways_04, X = 107f, Y = 10f,
                    Test = gs.NoveltySettings.RandomizeSwim ? new PDBool("canSwim") : null });
                plats.Add(new() { SceneName = SceneNames.Waterways_04, X = 107f, Y = 15f,
                    Test = gs.NoveltySettings.RandomizeSwim ? new PDBool("canSwim") : null });
            }

            return plats;
        }
    }
}

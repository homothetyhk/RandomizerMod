using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ItemChanger;
using RandomizerMod.Settings;

namespace RandomizerMod.IC
{
    public static class PlatformList
    {
        public static List<SmallPlatform> GetPlatformList(GenerationSettings gs)
        {
            List<SmallPlatform> plats = new();

            // TODO: extra platform settings check

            // Platforms to climb out from basin wanderer's journal
            plats.Add(new() { SceneName = SceneNames.Abyss_02, X = 128.3f, Y = 7f });
            plats.Add(new() { SceneName = SceneNames.Abyss_02, X = 128.3f, Y = 11f });

            // Platforms to climb up to tram in basin from left with no items
            if (gs.TransitionSettings.Mode != TransitionSettings.TransitionMode.RoomRandomizer)
            {
                plats.Add(new() { SceneName = SceneNames.Abyss_03, X = 34f, Y = 7f });
            }

            // Platform to climb out of Abyss with only wings
            plats.Add(new() { SceneName = SceneNames.Abyss_06_Core, X = 88.6f, Y = 263f });

            // Platforms to climb back up from pale ore with no items
            plats.Add(new() { SceneName = SceneNames.Abyss_17, X = 164.7f, Y = 30f });
            plats.Add(new() { SceneName = SceneNames.Abyss_17, X = 99.5f, Y = 12.5f });
            plats.Add(new() { SceneName = SceneNames.Abyss_17, X = 117.7f, Y = 18.8f });
            plats.Add(new() { SceneName = SceneNames.Abyss_17, X = 114.3f, Y = 23f });
            plats.Add(new() { SceneName = SceneNames.Abyss_17, X = 117.7f, Y = 7f });
            plats.Add(new() { SceneName = SceneNames.Abyss_17, X = 117.7f, Y = 10.8f });

            // Platforms to remove softlock with wings at simple key in basin
            plats.Add(new() { SceneName = SceneNames.Abyss_20, X = 26.5f, Y = 13f });

            // Platform for returning to Gorb landing
            plats.Add(new() { SceneName = SceneNames.Cliffs_02, X = 32.3f, Y = 27.7f });

            // Platform to return from Deepnest mimic grub room
            if (gs.TransitionSettings.Mode != TransitionSettings.TransitionMode.RoomRandomizer)
            {
                plats.Add(new() { SceneName = SceneNames.Deepnest_01b, X = 48.3f, Y = 40f });
            }

            // Platforms to return from the Deepnest_02 geo rocks without vertical
            plats.Add(new() { SceneName = SceneNames.Deepnest_02, X = 26f, Y = 12f });
            plats.Add(new() { SceneName = SceneNames.Deepnest_02, X = 26f, Y = 16f });

            // Platform to escape the Deepnest mimic room when mimics may not be present
            if (gs.CursedSettings.RandomizeMimics)
            {
                plats.Add(new() { SceneName = SceneNames.Deepnest_36, X = 26f, Y = 11f });
            }

            // Platforms to climb back up from Mantis Lords with only wings
            if (gs.TransitionSettings.Mode == TransitionSettings.TransitionMode.None)
            {
                for (int i = 0; i < 2; i++)
                {
                    plats.Add(new() { SceneName = SceneNames.Fungus2_15, X = 48f + 2 * i, Y = 15f + 10 * i });
                }
            }

            // Platforms to prevent softlock on lever on the way to love key.
            for (int i = 0; i < 2; i++)
            {
               plats.Add(new() { SceneName = SceneNames.Fungus3_05, X = 65.7f, Y = 11f + 4.5f * i });
            }

            // Move the load in colo downward to prevent bench soft lock
            if (gs.TransitionSettings.Mode == TransitionSettings.TransitionMode.None)
            {

            }

            // Platform to escape from the geo rock above Lemm
            plats.Add(new() { SceneName = SceneNames.Ruins1_05c, X = 26.6f, Y = 73.2f });

            // Platforms to climb back up to King's Pass with no items
            if (gs.TransitionSettings.Mode == TransitionSettings.TransitionMode.None && gs.StartLocationSettings.StartLocation == "King's Pass")
            {
                for (int i = 0; i < 6; i++)
                {
                    plats.Add(new() { SceneName = SceneNames.Town, X = 20f - 2 * (i % 2), Y = 5f * i + 15f });
                }
            }

            // Platforms to prevent itemless softlock when checking left waterways
            if (gs.TransitionSettings.Mode == TransitionSettings.TransitionMode.None && gs.StartLocationSettings.StartLocation != "West Waterways")
            {
                plats.Add(new() { SceneName = SceneNames.Waterways_04, X = 148f, Y = 23.1f });
                plats.Add(new() { SceneName = SceneNames.Waterways_04, X = 139f, Y = 32f });
                plats.Add(new() { SceneName = SceneNames.Waterways_04, X = 107f, Y = 10f,
                    Test = gs.NoveltySettings.RandomizeSwim ? new PDBool { boolName = "canSwim" } : null });
                plats.Add(new() { SceneName = SceneNames.Waterways_04, X = 107f, Y = 15f,
                    Test = gs.NoveltySettings.RandomizeSwim ? new PDBool { boolName = "canSwim" } : null });
            }

            return plats;
        }
    }
}

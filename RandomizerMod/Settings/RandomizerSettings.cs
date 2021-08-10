using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Modding;
using static RandomizerMod.LogHelper;
using System.Reflection;

namespace RandomizerMod.Settings
{
    public class RandomizerSettings
    {
        public bool Randomizer;
        public QoLSettings GameSettings = new QoLSettings();
        public GenerationSettings GenerationSettings = new GenerationSettings();
    }

    public class QoLSettings
    {
        public bool RealGeoRocks;
        public bool PreloadGeoRocks;

        public bool JinnAppearsWithJiji;
        public bool PreloadJinn;

        public bool NPCItemDialogue;
        public bool RealGrubJars;

        public bool SalubraNotches;
    }
}

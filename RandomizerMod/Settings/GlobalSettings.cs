using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Modding;
using Modding.Patches;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using Newtonsoft.Json;
using MonoMod;

namespace RandomizerMod.Settings
{
    public class GlobalSettings
    {
        public GenerationSettings DefaultMenuSettings = new GenerationSettings();
        public List<MenuProfile> Profiles = new List<MenuProfile> { null };
    }

    public class MenuProfile
    {
        public string name;
        public GenerationSettings settings;
        public override string ToString()
        {
            return name;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerMod.RandomizerData
{
    public class PoolDef
    {
        public string Name { get; init; }
        public string Path { get; init; }
        public string[] IncludeItems { get; init; }
        public string[] IncludeLocations { get; init; }
        public StringILP[] Vanilla { get; init; }

        public readonly struct StringILP
        {
            public readonly string item;
            public readonly string location;

            [Newtonsoft.Json.JsonConstructor]
            public StringILP(string item, string location)
            {
                this.item = item;
                this.location = location;
            }
        }

        public bool IsIncluded(Settings.GenerationSettings gs)
        {
            if (!string.IsNullOrEmpty(Path)
                && Settings.Util.Get(gs, Path) is bool value
                && value) return true;
            else return false;
        }

        public bool IsVanilla(Settings.GenerationSettings gs)
        {
            if (!string.IsNullOrEmpty(Path)
                && Settings.Util.Get(gs, Path) is bool value
                && !value) return true;
            else return false;
        }

    }
}

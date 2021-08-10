using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RandomizerMod.RandomizerData
{
    public class PoolDef
    {
        public string name;
        public string path;
        public string[] includeItems;
        public string[] includeLocations;
        public StringILP[] vanilla;

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
            if (!string.IsNullOrEmpty(path)
                && Settings.Util.Get(gs, path) is bool value
                && value) return true;
            else return false;
        }

        public bool IsVanilla(Settings.GenerationSettings gs)
        {
            if (!string.IsNullOrEmpty(path)
                && Settings.Util.Get(gs, path) is bool value
                && !value) return true;
            else return false;
        }

    }
}

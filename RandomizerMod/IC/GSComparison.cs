using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ItemChanger;
using RandomizerMod.Settings;

namespace RandomizerMod.IC
{
    /// <summary>
    /// Compare a settings field to a given value.
    /// </summary>
    public class GSComparison<T> : IBool where T : IComparable
    {
        /// <summary>
        /// Dot separators: e.g. SkipSettings.MildSkips
        /// </summary>
        public string FieldPath { get; init; }

        public T Target { get; init; }

        public ComparisonOperator Op { get; init; } = ComparisonOperator.Eq;

        public bool Value => ItemChanger.Extensions.Extensions.Compare((T)RandomizerMod.RS.GenerationSettings.Get(FieldPath), Op, Target);

        public IBool Clone() => (IBool)MemberwiseClone();

        public GSComparison() { }

        public GSComparison(string fieldName)
        {
            FieldPath = Util.GetPath(fieldName);
        }
    }
}

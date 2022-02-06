using RandomizerCore.Logic;

namespace RandomizerMod.RC.LogicInts
{
    /// <summary>
    /// LogicInt which is true exactly when the GenerationSettings StartLocation equals its argument.
    /// </summary>
    public class StartLocationDelta : LogicInt
    {
        public StartLocationDelta(string location)
        {
            Location = location;
            Name = $"$StartLocation[{location}]";
        }

        public override string Name { get; }
        public string Location { get; }

        public override IEnumerable<Term> GetTerms()
        {
            return Enumerable.Empty<Term>();
        }

        public override int GetValue(object sender, ProgressionManager pm)
        {
            return ((RandoModContext)pm.ctx).GenerationSettings.StartLocationSettings.StartLocation == Location ? 1 : 0;
        }
    }
}

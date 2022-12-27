﻿using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RandomizerMod.RC.LogicInts
{
    /// <summary>
    /// LogicInt which is true exactly when the GenerationSettings StartLocation equals its argument.
    /// </summary>
    public class StartLocationDelta : StateProvider
    {
        public StartLocationDelta(string location)
        {
            Location = location;
            Name = $"$StartLocation[{location}]";
        }

        public override string Name { get; }
        public string Location { get; }

        public override StateUnion? GetInputState(object? sender, ProgressionManager pm)
        {
            return pm.lm.StateManager.StartStateSingleton;
        }

        public override IEnumerable<Term> GetTerms()
        {
            return Enumerable.Empty<Term>();
        }

        public override int GetValue(object? sender, ProgressionManager pm)
        {
            return ((RandoModContext)pm.ctx).GenerationSettings.StartLocationSettings.StartLocation == Location ? TRUE : FALSE;
        }
    }
}
using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;

namespace RandomizerMod.RC.StateVariables
{
    /// <summary>
    /// Base class which handles passing the parameters in a given state modifier name to an inner state modifier.
    /// </summary>
    public abstract class StateModifierWrapper<T> : StateModifier where T : StateModifier
    {
        protected readonly T InnerVariable;
        public override string Name { get; }
        protected abstract string InnerPrefix { get; }

        protected StateModifierWrapper(string name, LogicManager lm) 
        {
            Name = name;
            try
            {
                string innerName = name.IndexOf('[') is int i && i >= 0 ? $"{InnerPrefix}{name.Substring(i)}" : InnerPrefix;
                InnerVariable = (T)lm.GetVariableStrict(innerName);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error constructing " + GetType().Name, e);
            }
        }
    }
}

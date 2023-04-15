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
                int i = name.IndexOf('[');
                string innerName;
                if (i >= 0 && VariableResolver.TryMatchPrefix(name.Substring(0, i), name, out string[] parameters))
                {
                    if (parameters.Length > 0) parameters = parameters.Where(p => !Consume(p)).ToArray();
                    innerName = parameters.Length > 0 ? $"{InnerPrefix}[{string.Join(",", parameters)}]" : InnerPrefix;
                }
                else
                {
                    innerName = InnerPrefix;
                }
                InnerVariable = (T)lm.GetVariableStrict(innerName);
            }
            catch (Exception e)
            {
                throw new InvalidOperationException("Error constructing " + GetType().Name, e);
            }
        }

        /// <summary>
        /// Indicates that the parameter should not be passed to the inner variable.
        /// </summary>
        protected virtual bool Consume(string parameter) => false;
    }
}

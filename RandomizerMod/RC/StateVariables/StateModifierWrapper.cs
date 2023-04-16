using RandomizerCore.Logic;
using RandomizerCore.Logic.StateLogic;
using System.Security.Cryptography;

namespace RandomizerMod.RC.StateVariables
{
    /// <summary>
    /// Base class which handles passing the parameters in a given state modifier name to an inner state modifier.
    /// </summary>
    public abstract class StateModifierWrapper<T> : StateModifier where T : StateModifier
    {
        public readonly T InnerVariable;
        public override string Name { get; }
        protected abstract string InnerPrefix { get; }
        /// <summary>
        /// The parameters which were not consumed, and thus were passed to the inner variable.
        /// </summary>
        protected readonly string[] InnerParameters;

        protected StateModifierWrapper(string name, LogicManager lm) 
        {
            Name = name;
            try
            {
                int i = name.IndexOf('[');
                string innerName;
                if (i >= 0 && VariableResolver.TryMatchPrefix(name, name.Substring(0, i), out string[] parameters))
                {
                    InnerParameters = parameters.Where(p => !Consume(p)).ToArray();
                    innerName = InnerParameters.Length > 0 ? $"{InnerPrefix}[{string.Join(",", InnerParameters)}]" : InnerPrefix;
                }
                else
                {
                    InnerParameters = Array.Empty<string>();
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

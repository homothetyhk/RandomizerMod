using RandomizerCore;
using RandomizerCore.Logic;
using RandomizerMod.RandomizerData;

namespace RandomizerMod.RC
{
    public class RandoModTransition : RandoTransition
    {
        /// <summary>
        /// The TransitionDef associated with the location. Preferred over Data.GetTransitionDef, since this preserves modified transition data.
        /// <br/>This field is serialized, and is safe to use after reloading the game.
        /// </summary>
        public TransitionDef TransitionDef;
        public RandoModTransition(LogicTransition lt, TransitionDef transitionDef)
            : base(lt) 
        {
            TransitionDef = transitionDef ?? throw new ArgumentNullException($"Null TransitionDef passed into RandoModTransition for {lt.Name}");
        }
    }
}

using Newtonsoft.Json;
using RandomizerCore;
using RandomizerCore.Logic;
using RandomizerMod.RandomizerData;

namespace RandomizerMod.RC
{
    public class RandoModTransition : RandoTransition
    {
        /// <summary>
        /// The TransitionRequestInfo associated with the transition. May be null if the transition does not require modification.
        /// <br/>This field is not serialized and will be null upon reloading the game.
        /// </summary>
        [JsonIgnore] public TransitionRequestInfo? info;
        /// <summary>
        /// The TransitionDef associated with the location. Preferred over Data.GetTransitionDef, since this preserves modified transition data.
        /// <br/>This field is serialized, and is safe to use after reloading the game.
        /// </summary>
        public TransitionDef TransitionDef;
        public RandoModTransition(LogicTransition lt) : base(lt) { }
    }
}

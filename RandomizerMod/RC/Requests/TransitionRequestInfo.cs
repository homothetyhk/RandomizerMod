using ItemChanger;
using RandomizerCore;
using RandomizerMod.RandomizerData;

namespace RandomizerMod.RC
{
    public class TransitionRequestInfo
    {
        public Func<RandoFactory, RandoModTransition>? randoTransitionCreator;
        public Action<RandoFactory, RandoModTransition>? onRandoTransitionCreation;
        public Action<RandoPlacement>? onRandomizerFinish;
        public Func<RequestBuilder, RandoPlacement, ITransition>? realTargetCreator;
        public Func<RequestBuilder, RandoPlacement, Transition> realSourceCreator;
        public Func<TransitionDef>? getTransitionDef;

        public void AddGetTransitionDefModifier(string name, Func<TransitionDef, TransitionDef> modifier)
        {
            Func<TransitionDef> get = getTransitionDef ?? (() => Data.GetTransitionDef(name));
            getTransitionDef = () => modifier(get());
        }

        public TransitionRequestInfo Clone()
        {
            return new TransitionRequestInfo
            {
                randoTransitionCreator = (Func<RandoFactory, RandoModTransition>)randoTransitionCreator?.Clone(),
                onRandoTransitionCreation = (Action<RandoFactory, RandoModTransition>)onRandoTransitionCreation?.Clone(),
                onRandomizerFinish = (Action<RandoPlacement>)onRandomizerFinish?.Clone(),
                realTargetCreator = (Func<RequestBuilder, RandoPlacement, ITransition>)realTargetCreator?.Clone(),
                realSourceCreator = (Func<RequestBuilder, RandoPlacement, Transition>)realSourceCreator?.Clone(),
                getTransitionDef = (Func<TransitionDef>)getTransitionDef?.Clone()
            };
        }

        public void AppendTo(TransitionRequestInfo info)
        {
            if (randoTransitionCreator != null) info.randoTransitionCreator = randoTransitionCreator;
            info.onRandoTransitionCreation += onRandoTransitionCreation;
            info.onRandomizerFinish += onRandomizerFinish;
            if (realTargetCreator != null) info.realTargetCreator = realTargetCreator;
            if (realSourceCreator != null) info.realSourceCreator = realSourceCreator;
            if (getTransitionDef != null) info.getTransitionDef = getTransitionDef;
        }
    }
}

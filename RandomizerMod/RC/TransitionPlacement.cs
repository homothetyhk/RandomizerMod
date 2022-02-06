using RandomizerCore;

namespace RandomizerMod.RC
{
    public record struct TransitionPlacement(RandoModTransition Target, RandoModTransition Source)
    {
        public void Deconstruct(out RandoModTransition target, out RandoModTransition source)
        {
            target = this.Target;
            source = this.Source;
        }

        public static implicit operator GeneralizedPlacement(TransitionPlacement p) => new(p.Target, p.Source);
        public static explicit operator TransitionPlacement(GeneralizedPlacement p) => new((RandoModTransition)p.Item, (RandoModTransition)p.Location);
    }
}

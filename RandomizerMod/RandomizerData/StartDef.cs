using GlobalEnums;
using RandomizerCore;
using RandomizerCore.Logic;

namespace RandomizerMod.RandomizerData
{
    public record StartDef
    {
        public string Name { get; init; }

        // respawn marker properties
        public string SceneName { get; init; }
        public float X { get; init; }
        public float Y { get; init; }
        public MapZone Zone { get; init; }

        // logic info
        public string Transition { get; init; }

        // Primitive logic -- check SettingsPM
        public string Logic { get; init; }

        public virtual IEnumerable<TermValue> GetStartLocationProgression(LogicManager lm)
        {
            yield return new(lm.GetTerm(Transition), 1);
        }

        public virtual ItemChanger.StartDef ToItemChangerStartDef()
        {
            return new ItemChanger.StartDef
            {
                SceneName = SceneName,
                X = X,
                Y = Y,
                MapZone = (int)Zone,
                RespawnFacingRight = true,
                SpecialEffects = ItemChanger.SpecialStartEffects.Default | ItemChanger.SpecialStartEffects.SlowSoulRefill,
            };
        }
    }
}

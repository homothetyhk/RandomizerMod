using GlobalEnums;
using RandomizerCore;
using RandomizerCore.Logic;
using RandomizerMod.Settings;

namespace RandomizerMod.RandomizerData
{
    public record StartDef
    {
        /// <summary>
        /// The name of the start. Names should be unique.
        /// </summary>
        public string Name { get; init; }
        /// <summary>
        /// The scene of the start location in-game.
        /// </summary>
        public string SceneName { get; init; }
        /// <summary>
        /// The x-coordinate of the start location in-game.
        /// </summary>
        public float X { get; init; }
        /// <summary>
        /// The y-coordinate of the start location in-game.
        /// </summary>
        public float Y { get; init; }
        /// <summary>
        /// The map zone of the start location in-game.
        /// </summary>
        public MapZone Zone { get; init; }

        /// <summary>
        /// The transition which is used as the initial logical progression for this start location.
        /// </summary>
        public string Transition { get; init; }

        /// <summary>
        /// Logic evaluated by the SettingsPM to determine whether the start can be selected in the menu. Must not be null.
        /// </summary>
        public string Logic { get; init; }
        /// <summary>
        /// Logic evaluated by the SettingsPM to determine whether the start can be randomly selected. If null, Logic is used instead.
        /// </summary>
        public string RandoLogic { get; init; }
        /// <summary>
        /// Flag which determines whether the start is given a button in the Start Locations menu. Hidden starts can still be randomly selected.
        /// </summary>
        public bool ExcludeFromMenu { get; init; }

        public virtual bool CanBeSelected(SettingsPM pm)
        {
            return pm.Evaluate(Logic);
        }

        public virtual bool CanBeRandomized(SettingsPM pm)
        {
            return pm.Evaluate(RandoLogic ?? Logic);
        }

        public virtual bool DisplayInMenu(SettingsPM pm)
        {
            return !ExcludeFromMenu;
        }

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

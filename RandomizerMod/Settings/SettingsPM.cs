using RandomizerCore.StringLogic;

namespace RandomizerMod.Settings
{
    /// <summary>
    /// ProgressionManager for interpreting logic for GenerationSettings
    /// </summary>
    public class SettingsPM : StringPM
    {
        public GenerationSettings GS;

        public SettingsPM(GenerationSettings gs) => GS = gs;

        public override bool Evaluate(TermToken token)
        {
            if (token is ConstToken bt)
            {
                return bt.Value;
            }
            else if (token is SimpleToken st)
            {
                return GetBool(st.Name);
            }
            else if (token is ComparisonToken ct)
            {
                return ct.ComparisonType switch
                {
                    ComparisonType.LT => GetInt(ct.Left) < GetInt(ct.Right),
                    ComparisonType.GT => GetInt(ct.Left) > GetInt(ct.Right),
                    _ => GetInt(ct.Left) == GetInt(ct.Right)
                };
            }
            else if (token is MacroToken mt)
            {
                return Evaluate(mt.Value);
            }
            else throw new ArgumentException($"Unable to evaluate token: " + token);
        }

        public int GetInt(string name)
        {
            if (int.TryParse(name, out int value)) return value;
            return name switch
            {
                _ => throw new NotImplementedException(),
            };
        }

        public bool GetBool(string name)
        {
            return name switch
            {
                "MILDSKIPS" => GS.SkipSettings.MildSkips,
                "SHADESKIPS" => GS.SkipSettings.ShadeSkips,
                "ACIDSKIPS" => GS.SkipSettings.AcidSkips,
                "FIREBALLSKIPS" => GS.SkipSettings.FireballSkips,
                "SPIKETUNNELS" => GS.SkipSettings.SpikeTunnels,
                "DARKROOMS" => GS.SkipSettings.DarkRooms,
                "SPICYSKIPS" => GS.SkipSettings.SpicySkips,

                "ITEMRANDO" => GS.TransitionSettings.Mode == TransitionSettings.TransitionMode.None,
                "AREARANDO" => GS.TransitionSettings.Mode == TransitionSettings.TransitionMode.FullAreaRandomizer 
                || GS.TransitionSettings.Mode == TransitionSettings.TransitionMode.MapAreaRandomizer,
                "ROOMRANDO" => GS.TransitionSettings.Mode == TransitionSettings.TransitionMode.RoomRandomizer,
                
                "SWIM" => !GS.NoveltySettings.RandomizeSwim,
                "ELEVATOR" => !GS.NoveltySettings.RandomizeElevatorPass,

                "2MASKS" => !GS.CursedSettings.CursedMasks,
                
                "VERTICAL" => false,
                _ => throw new NotImplementedException()
            };
        }
    }
}

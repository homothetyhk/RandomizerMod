using RandomizerCore.StringLogic;

namespace RandomizerMod.Settings
{
    /// <summary>
    /// ProgressionManager for interpreting logic for GenerationSettings
    /// </summary>
    public class SettingsPM : StringPM
    {
        public readonly GenerationSettings GS;

        public SettingsPM(GenerationSettings gs) => GS = gs;

        public delegate bool BoolTermResolver(string term, out bool result);
        public static event BoolTermResolver OnResolveBoolTerm;
        public delegate bool IntTermResolver(string term, out int result);
        public static event IntTermResolver OnResolveIntTerm;

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
            if (OnResolveIntTerm != null)
            {
                foreach (IntTermResolver d in OnResolveIntTerm.GetInvocationList())
                {
                    if (d(name, out int result)) return result;
                }
            }

            return name switch
            {
                _ => throw new NotImplementedException(),
            };
        }

        public bool GetBool(string name)
        {
            if (OnResolveBoolTerm != null)
            {
                foreach (BoolTermResolver d in OnResolveBoolTerm.GetInvocationList())
                {
                    if (d(name, out bool result)) return result;
                }
            }

            return name switch
            {
                "PRECISEMOVEMENT" => GS.SkipSettings.PreciseMovement,
                "PROFICIENTCOMBAT" => GS.SkipSettings.ProficientCombat,
                "BACKGROUNDPOGOS" => GS.SkipSettings.BackgroundObjectPogos,
                "ENEMYPOGOS" => GS.SkipSettings.EnemyPogos,
                "OBSCURESKIPS" => GS.SkipSettings.ObscureSkips,
                "SHADESKIPS" => GS.SkipSettings.ShadeSkips,
                "INFECTIONSKIPS" => GS.SkipSettings.InfectionSkips,
                "ACIDSKIPS" => GS.SkipSettings.AcidSkips,
                "FIREBALLSKIPS" => GS.SkipSettings.FireballSkips,
                "SPIKETUNNELS" => GS.SkipSettings.SpikeTunnels,
                "DARKROOMS" => GS.SkipSettings.DarkRooms,

                "DAMAGEBOOSTS" => GS.SkipSettings.DamageBoosts,
                "DANGEROUSSKIPS" => GS.SkipSettings.DangerousSkips,
                "COMPLEXSKIPS" => GS.SkipSettings.ComplexSkips,
                "DIFFICULTSKIPS" => GS.SkipSettings.DifficultSkips,

                "ITEMRANDO" => GS.TransitionSettings.Mode == TransitionSettings.TransitionMode.None,
                "MAPAREARANDO" => GS.TransitionSettings.Mode == TransitionSettings.TransitionMode.MapAreaRandomizer,
                "FULLAREARANDO" => GS.TransitionSettings.Mode == TransitionSettings.TransitionMode.FullAreaRandomizer,
                "AREARANDO" => GS.TransitionSettings.Mode == TransitionSettings.TransitionMode.FullAreaRandomizer 
                || GS.TransitionSettings.Mode == TransitionSettings.TransitionMode.MapAreaRandomizer,
                "ROOMRANDO" => GS.TransitionSettings.Mode == TransitionSettings.TransitionMode.RoomRandomizer,
                
                "SWIM" => !GS.NoveltySettings.RandomizeSwim,
                "ELEVATOR" => !GS.NoveltySettings.RandomizeElevatorPass,

                "2MASKS" => GS.CursedSettings.CursedMasks < 4,
                
                "VERTICAL" => GS.StartItemSettings.VerticalMovement != StartItemSettings.StartVerticalType.None 
                    && GS.StartItemSettings.VerticalMovement != StartItemSettings.StartVerticalType.ZeroOrMore,
                _ => throw new ArgumentException($"Unrecognized term in SettingsPM: {name}"),
            };
        }
    }
}

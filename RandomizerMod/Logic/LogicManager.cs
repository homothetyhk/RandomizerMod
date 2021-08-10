using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static RandomizerMod.LogHelper;
using RandomizerMod.Extensions;
using RandomizerMod.RandomizerData;

namespace RandomizerMod.Logic
{
    public class LogicManager
    {
        public readonly int TermCount;
        public readonly LogicWaypoint[] Waypoints;

        Dictionary<string, LogicDef> logicDefs = new Dictionary<string, LogicDef>();

        // Data structures dynamically constructed to correspond to logic
        Dictionary<string, int> itemToIndex = new Dictionary<string, int>();
        readonly string[] terms;

        readonly LogicProcessor LP;

        public LogicManager(IEnumerable<string> terms, RawLogicDef[] waypoints, RawLogicDef[] logic)
        {
            LP = LogicProcessor.DefaultProcessor;

            this.terms = terms.ToArray();
            TermCount = this.terms.Length;
            itemToIndex = this.terms.Select((s, i) => (s, i)).ToDictionary(p => p.s, p => p.i);

            Waypoints = waypoints
                .Select(w => new LogicWaypoint(itemToIndex[w.name], new LogicDef 
                {
                    name = w.name,
                    logic = Process(w)
                }))
                .ToArray();

            logicDefs = logic.ToDictionary(r => r.name, r => new LogicDef
            {
                name = r.name,
                logic = Process(r)
            });
        }

        public LogicDef GetLogicDef(string name)
        {
            if (!logicDefs.TryGetValue(name, out LogicDef def))
            {
                LogWarn($"Unable to find logic for {name}.");
                return null;
            }

            return def;
        }

        public bool EvaluateLogic(string name, ProgressionManager pm)
        {
            if (!logicDefs.TryGetValue(name, out LogicDef def))
            {
                LogWarn($"Unable to find logic for {name}.");
                return false;
            }

            return def.CanGet(pm);
        }

        public int GetIndex(string item)
        {
            return itemToIndex[item];
        }

        public bool TryGetIndex(string item, out int index)
        {
            if (itemToIndex.TryGetValue(item, out index))
            {
                return true;
            }
            return false;
        }

        public string GetItem(int id)
        {
            return terms[id];
        }

        public LogicDef FromString(string str, string name = null)
        {
            return new LogicDef
            {
                name = name,
                logic = Process(new RawLogicDef { logic = str, name = name }),
            };
        }

        public LogicDef FromString(RawLogicDef def)
        {
            return new LogicDef
            {
                name = def.name,
                logic = Process(def),
            };
        }


        private int[] Process(RawLogicDef def)
        {
            try
            {
                return Process(def.logic).ToArray();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error in processing logic for {def.name}:\n{e.Message}");
                return null;
            }
        }

        private IEnumerable<int> Process(string logic)
        {
            foreach (string s in LP.Shunt(logic))
            {
                if (operators.TryGetValue(s, out int i))
                {
                    yield return i;
                }
                else if (itemToIndex.TryGetValue(s, out i))
                {
                    yield return i;
                }
                else if (int.TryParse(s, out i))
                {
                    yield return i;
                }
                else
                {
                    throw new ArgumentException($"Unknown token {s} found in logic");
                }
            }
        }

        static Dictionary<string, int> operators = new Dictionary<string, int>
        {
            { "NONE", (int)LogicOperators.NONE },
            { "ANY", (int)LogicOperators.ANY },
            { "|", (int)LogicOperators.OR },
            { "+", (int)LogicOperators.AND },
            { ">", (int)LogicOperators.GTR },
            { "!", (int)LogicOperators.NOT },
        };
    }
}

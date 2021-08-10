using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static RandomizerMod.LogHelper;

namespace RandomizerMod.Logic
{
    public class LogicDef
    {
        public string name;
        public int[] logic;

        public static int logicEvaluations = 0;
        public static int flagReads = 0;
        public static System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();

        // recycling the stack saves approx 0.03s in 129000 calls of CanGet (i.e. standard preset)
        static readonly Stack<bool> stack = new Stack<bool>();

        public bool CanGet(int[] obtained)
        {
            logicEvaluations++;
            if (logic == null || logic.Length == 0) return true;
            stopwatch.Start();

            //Stack<bool> stack = new Stack<bool>();

            for (int i = 0; i < logic.Length; i++)
            {
                switch (logic[i])
                {
                    case (int)LogicOperators.AND:
                        stack.Push(stack.Pop() & stack.Pop());
                        break;
                    case (int)LogicOperators.OR:
                        stack.Push(stack.Pop() | stack.Pop());
                        break;
                    case (int)LogicOperators.NONE:
                        stack.Push(false);
                        break;
                    case (int)LogicOperators.ANY:
                        stack.Push(true);
                        break;
                    case (int)LogicOperators.GTR:
                        int type = logic[++i];
                        int amt = logic[++i];
                        stack.Push(obtained[type] > amt);
                        flagReads++;
                        break;
                    case (int)LogicOperators.NOT:
                        stack.Push(!stack.Pop());
                        break;
                    default:
                        stack.Push(obtained[logic[i]] > 0);
                        flagReads++;
                        break;
                }
            }
            stopwatch.Stop();

            return stack.Pop();
        }

        public bool CanGet(ProgressionManager pm)
        {
            if (logic == null || logic.Length == 0) return true;

            Stack<bool> stack = new Stack<bool>();

            for (int i = 0; i < logic.Length; i++)
            {
                switch (logic[i])
                {
                    case (int)LogicOperators.AND:
                        stack.Push(stack.Pop() & stack.Pop());
                        break;
                    case (int)LogicOperators.OR:
                        stack.Push(stack.Pop() | stack.Pop());
                        break;
                    case (int)LogicOperators.NONE:
                        stack.Push(false);
                        break;
                    case (int)LogicOperators.ANY:
                        stack.Push(true);
                        break;
                    case (int)LogicOperators.GTR:
                        int type = logic[++i];
                        int amt = logic[++i];
                        stack.Push(pm.Has(type, amt));
                        break;
                    case (int)LogicOperators.NOT:
                        stack.Push(!stack.Pop());
                        break;
                    default:
                        stack.Push(pm.Has(logic[i]));
                        break;
                }
            }

            return stack.Pop();
        }
    }
}

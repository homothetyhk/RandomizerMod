using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RandomizerCore;
using RandomizerCore.Logic;
using RandomizerMod.RandomizerData;
using RandomizerMod.Settings;

namespace RandomizerMod.RC
{
    public class RandoFactory
    {
        public RandoFactory(RequestBuilder rb)
        {
            this.rb = rb;
            this.lm = rb.lm;
            this.rng = rb.rng;
            this.gs = rb.gs;
        }

        public readonly RequestBuilder rb;
        public readonly LogicManager lm;
        public readonly Random rng;
        public readonly GenerationSettings gs;

        public IRandoItem MakeItem(string name)
        {
            if (rb.TryGetItemDef(name, out ItemRequestInfo info) && info.randoItemCreator != null)
            {
                return info.randoItemCreator(this);
            }
            else return MakeItemInternal(name);
        }

        public RandoItem MakeItemInternal(string name)
        {
            return new RandoItem()
            {
                item = lm.GetItem(name),
            };
        }

        public IRandoLocation MakeLocation(string name)
        {
            if (rb.TryGetLocationDef(name, out LocationRequestInfo info) && info.randoLocationCreator != null)
            {
                return info.randoLocationCreator(this);
            }
            else return MakeLocationInternal(name);
        }

        public RandoLocation MakeLocationInternal(string name)
        {
            RandoLocation rl = new()
            {
                logic = lm.GetLogicDef(name),
            };

            if (Data.TryGetCost(name, out CostDef def))
            {
                switch (def.term)
                {
                    case "ESSENCE":
                    case "GRUBS":
                    case "SIMPLE":
                        rl.AddCost(new SimpleCost(lm.GetTerm("SIMPLE"), 1));
                        break;
                    case "Spore_Shroom":
                        rl.AddCost(new SimpleCost(lm.GetTerm("Spore_Shroom"), 1));
                        break;
                    case "GEO":
                        rl.AddCost(new LogicGeoCost(lm, def.amount));
                        break;
                    default:
                        rl.AddCost(new SimpleCost(lm.GetTerm(def.term), def.amount));
                        break;
                }
            }

            switch (name)
            {
                case "Grubfather":
                    rl.AddCost(new SimpleCost(lm.GetTerm("GRUBS"), rng.Next(gs.CostSettings.MinimumGrubCost, gs.CostSettings.MaximumGrubCost + 1)));
                    break;
                case "Seer":
                    rl.AddCost(new SimpleCost(lm.GetTerm("ESSENCE"), rng.Next(gs.CostSettings.MinimumEssenceCost, gs.CostSettings.MaximumEssenceCost + 1)));
                    break;
                case "Egg_Shop":
                    rl.AddCost(new SimpleCost(lm.GetTerm("RANCIDEGGS"), rng.Next(gs.CostSettings.MinimumEggCost, gs.CostSettings.MaximumEggCost + 1)));
                    break;
                case "Salubra_(Requires_Charms)":
                    rl.AddCost(new SimpleCost(lm.GetTerm("CHARMS"), rng.Next(gs.CostSettings.MinimumCharmCost, gs.CostSettings.MaximumCharmCost + 1)));
                    goto case "Salubra";
                case "Sly":
                case "Sly_(Key)":
                case "Iselda":
                case "Salubra":
                case "Leg_Eater":
                    rl.AddCost(new LogicGeoCost(lm, -1));
                    break;
            }

            return rl;
        }

        public IRandoCouple MakeTransition(string name)
        {
            return new RandoTransition(lm.GetTransition(name));
        }
    }
}

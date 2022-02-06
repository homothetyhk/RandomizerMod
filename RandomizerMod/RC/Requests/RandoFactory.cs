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

        /// <summary>
        /// Makes a PlaceholderItem. The item will be exported identically to the result of MakeItem, but will be treated by logic as an EmptyItem.
        /// </summary>
        public RandoModItem MakeWrappedItem(string name)
        {
            RandoModItem item = MakeItem(name);
            PlaceholderItem wrapper = new(item);
            return wrapper;
        }

        public RandoModItem MakeItem(string name)
        {
            RandoModItem ri;
            if (!rb.TryGetItemRequest(name, out ItemRequestInfo info))
            {
                ri = MakeItemInternal(name);
                ri.ItemDef = Data.GetItemDef(name);
            }
            else
            {
                ri = info.randoItemCreator != null ? info.randoItemCreator(this) : MakeItemInternal(name);
                info.AppendTo(ri.info ??= new());
                ri.ItemDef = info.getItemDef != null ? info.getItemDef() : Data.GetItemDef(name);
                info.onRandoItemCreation?.Invoke(this, ri);
            }
            
            return ri;
        }

        public RandoModItem MakeItemInternal(string name)
        {
            RandoModItem item = new()
            {
                item = lm.GetItem(name),
            };

            return item;
        }

        public RandoModLocation MakeLocation(string name)
        {
            RandoModLocation rl;
            if (!rb.TryGetLocationRequest(name, out LocationRequestInfo info))
            {
                rl = MakeLocationInternal(name);
                rl.LocationDef = Data.GetLocationDef(name);
            }
            else
            {
                rl = info.randoLocationCreator?.Invoke(this) ?? MakeLocationInternal(name);
                info.AppendTo(rl.info ??= new());
                rl.LocationDef = info?.getLocationDef != null ? info.getLocationDef() : Data.GetLocationDef(name);
                info.onRandoLocationCreation?.Invoke(this, rl);
            }
            
            return rl;
        }

        private RandoModLocation MakeLocationInternal(string name)
        {
            RandoModLocation rl = new()
            {
                logic = lm.GetLogicDef(name),
            };

            if (Data.TryGetCost(name, out CostDef def))
            {
                switch (def.Term)
                {
                    case "ESSENCE":
                    case "GRUBS":
                        break;
                    case "SIMPLE":
                        rl.AddCost(new SimpleCost(lm.GetTerm("SIMPLE"), 1));
                        break;
                    case "Spore_Shroom":
                        rl.AddCost(new SimpleCost(lm.GetTerm("Spore_Shroom"), 1));
                        break;
                    case "GEO":
                        rl.AddCost(new LogicGeoCost(lm, def.Amount));
                        break;
                    default:
                        rl.AddCost(new SimpleCost(lm.GetTerm(def.Term), def.Amount));
                        break;
                }
            }

            return rl;
        }

        public GeneralizedPlacement MakeVanillaPlacement(VanillaDef def)
        {
            if (lm.TransitionLookup.TryGetValue(def.Item, out LogicTransition target))
            {
                LogicTransition source = lm.GetTransition(def.Location);
                return new(target, source);
            }

            LogicItem li = lm.GetItem(def.Item);
            RandoLocation rl = new() { logic = lm.GetLogicDef(def.Location) };
            void ApplyCost(CostDef cost)
            {
                switch (cost.Term)
                {
                    case "GEO":
                        rl.AddCost(new LogicGeoCost(lm, cost.Amount));
                        break;
                    default:
                        rl.AddCost(new SimpleCost(lm.GetTerm(cost.Term), cost.Amount));
                        break;
                }
            }

            if (Data.TryGetCost(def.Location, out CostDef baseCost))
            {
                ApplyCost(baseCost);
            }
            if (def.Costs != null)
            {
                foreach (CostDef cost in def.Costs)
                {
                    ApplyCost(cost);
                }
            }

            return rl.costs == null ? new(li, rl.logic) : new(li, rl);
        }

        public IRandoCouple MakeTransition(string name)
        {
            return new RandoModTransition(lm.GetTransition(name), Data.GetTransitionDef(name));
        }
    }
}

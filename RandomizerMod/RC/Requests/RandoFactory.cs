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
            RandoModItem item;
            if (rb.TryGetItemDef(name, out ItemRequestInfo info) && info.randoItemCreator != null)
            {
                item = info.randoItemCreator(this);
            }
            else
            {
                item = MakeItemInternal(name);
            }

            if (info != null)
            {
                item.onRandomizerFinish += info.onRandomizerFinish;
                item.realItemCreator += info.realItemCreator;
                info.onRandoItemCreation?.Invoke(this, item);
            }

            return item;
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
            if (!rb.TryGetLocationDef(name, out LocationRequestInfo info))
            {
                return MakeLocationInternal(name);
            }
            else
            {
                RandoModLocation rl = info.randoLocationCreator?.Invoke(this) ?? MakeLocationInternal(name);
                info.onRandoLocationCreation?.Invoke(this, rl);
                return rl;
            }
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
           
            if (rb.TryGetLocationDef(name, out LocationRequestInfo info))
            {
                rl.onRandomizerFinish += info.onRandomizerFinish;
                rl.customPlacementFetch += info.customPlacementFetch;
                rl.onPlacementFetch += info.onPlacementFetch;
                rl.customAddToPlacement += info.customAddToPlacement;
            }

            return rl;
        }

        public IRandoCouple MakeTransition(string name)
        {
            return new RandoModTransition(lm.GetTransition(name));
        }
    }
}

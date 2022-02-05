using ItemChanger;
using ItemChanger.Extensions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UObject = UnityEngine.Object;
using Module = ItemChanger.Modules.Module;
using System.Collections.ObjectModel;

namespace RandomizerMod.IC
{
    public class RandomizerModule : Module
    {
        /// <summary>
        /// Event which is invoked during ItemChanger.Events.OnEnterGame, to allow access to the item and placement lookups after all items and placements have loaded.
        /// </summary>
        public static event Action OnLoadComplete;

        /// <summary>
        /// Item lookup indexed parallel to the RandoContext item placement list. Refreshed on entering the game.
        /// </summary>
        public static ReadOnlyDictionary<int, AbstractItem> Items { get; } = new(_items = new());
        private static readonly Dictionary<int, AbstractItem> _items;

        /// <summary>
        /// Placement lookup indexed parallel to the RandoContext item placement list. Refreshed on entering the game.
        /// </summary>
        public static ReadOnlyDictionary<int, AbstractPlacement> Placements { get; } = new(_placements = new());
        private static readonly Dictionary<int, AbstractPlacement> _placements;

        public override void Initialize()
        {
            AbstractItem.ModifyRedundantItemGlobal += ModifyRedundantItem;
            ToggleSceneHooks(true);
            Events.OnEnterGame += InvokeOnLoadComplete;
            RandoItemTag.OnLoad += RecordItem;
            RandoPlacementTag.OnLoad += RecordPlacement;

            _items.Clear();
            _placements.Clear();
        }

        public override void Unload()
        {
            AbstractItem.ModifyRedundantItemGlobal -= ModifyRedundantItem;
            ToggleSceneHooks(false);
            Events.OnEnterGame -= InvokeOnLoadComplete;
            RandoItemTag.OnLoad -= RecordItem;
            RandoPlacementTag.OnLoad -= RecordPlacement;

            _items.Clear();
            _placements.Clear();
        }

        private static void InvokeOnLoadComplete()
        {
            try
            {
                OnLoadComplete?.Invoke();
            }
            catch (Exception e)
            {
                LogError($"Error invoking RandomizerModule.OnLoadComplete:\n{e}");
            }
        }

        private static void RecordItem(AbstractItem item, RandoItemTag tag)
        {
            _items[tag.id] = item;
        }

        private static void RecordPlacement(AbstractPlacement placement, RandoPlacementTag tag)
        {
            if (tag.ids != null)
            {
                foreach (int id in tag.ids) _placements[id] = placement;
            }
        }

        private static void ModifyRedundantItem(GiveEventArgs args)
        {
            args.Item = new ItemChanger.Items.AddGeoItem
            {
                amount = 100,
                name = $"100_Geo-{args.Orig.name}",
                UIDef = DupeUIDef.Convert(100, args.Orig.UIDef),
            };
        }

        private static void ToggleSceneHooks(bool toggle)
        {
            Settings.GenerationSettings gs = RandomizerMod.RS.GenerationSettings;

            switch (gs.StartLocationSettings.StartLocation)
            {
                case "Ancestral Mound":
                    if (gs.NoveltySettings.RandomizeNail)
                    {
                        if (toggle) Events.AddSceneChangeEdit(SceneNames.Crossroads_ShamanTemple, DestroyPlanksForAncestralMoundStart);
                        else Events.RemoveSceneChangeEdit(SceneNames.Crossroads_ShamanTemple, DestroyPlanksForAncestralMoundStart);
                    }
                    break;

                case "Fungal Core":
                    if (toggle) Events.AddSceneChangeEdit(SceneNames.Fungus2_30, CreateBounceShroomsForFungalCoreStart);
                    else Events.RemoveSceneChangeEdit(SceneNames.Fungus2_30, CreateBounceShroomsForFungalCoreStart);
                    break;
            }


        }

        // Destroy planks in cursed nail mode because we can't slash them
        private static void DestroyPlanksForAncestralMoundStart(Scene to)
        {
            foreach ((_, GameObject go) in to.Traverse())
            {
                if (go.name.StartsWith("Plank")) UObject.Destroy(go);
            }
        }

        private static void CreateBounceShroomsForFungalCoreStart(Scene to)
        {
            GameObject bounceShroom = to.FindGameObjectByName("Bounce Shroom C");

            GameObject s0 = UObject.Instantiate(bounceShroom);
            s0.transform.SetPosition3D(12.5f, 26f, 0f);
            s0.SetActive(true);

            GameObject s1 = UObject.Instantiate(bounceShroom);
            s1.transform.SetPosition3D(12.5f, 54f, 0f);
            s1.SetActive(true);

            GameObject s2 = UObject.Instantiate(bounceShroom);
            s2.transform.SetPosition3D(21.7f, 133f, 0f);
            s2.SetActive(true);
        }
    }
}

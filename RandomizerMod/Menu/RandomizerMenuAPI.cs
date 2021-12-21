﻿using MenuChanger;
using MenuChanger.MenuElements;
using RandomizerMod.RC;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RandomizerMod.Menu
{
    public delegate void OnRandomizerMenuConstructionHandler(MenuPage landingPage);
    public delegate bool TryGetMenuButtonHandler(MenuPage landingPage, out SmallButton button);
    public delegate bool OverrideRandomizerStartHandler(RandoController rc, MenuPage landingPage, out BaseButton button);
    internal readonly record struct RandoMenuPageConstructor
            (OnRandomizerMenuConstructionHandler ConstructionHandler, TryGetMenuButtonHandler ButtonHandler);
    internal readonly record struct RandoStartOverride
        (OnRandomizerMenuConstructionHandler ConstructionHandler, OverrideRandomizerStartHandler StartHandler);
    public static class RandomizerMenuAPI
    {
        public static RandomizerMenu Menu => RandomizerMenuConstructor.menu;

        internal static List<RandoMenuPageConstructor> randoMenuPageConstructors = new();
        public static void AddMenuPage(OnRandomizerMenuConstructionHandler constructionHandler, TryGetMenuButtonHandler buttonHandler)
        {
            randoMenuPageConstructors.Add(new(constructionHandler, buttonHandler));
            Menu?.RebuildConnectionsPanel();
        }

        public static void RemoveMenuPage(OnRandomizerMenuConstructionHandler constructionHandler, TryGetMenuButtonHandler buttonHandler)
        {
            randoMenuPageConstructors.Remove(new(constructionHandler, buttonHandler));
            Menu?.RebuildConnectionsPanel();
        }

        internal static List<RandoStartOverride> randoStartOverrides = new();
        public static void AddStartGameOverride(OnRandomizerMenuConstructionHandler constructionHandler, OverrideRandomizerStartHandler startHandler)
        {
            randoStartOverrides.Add(new(constructionHandler, startHandler));
        }

        public static void RemoveStartGameOverride(OnRandomizerMenuConstructionHandler constructionHandler, OverrideRandomizerStartHandler startHandler)
        {
            randoStartOverrides.Remove(new(constructionHandler, startHandler));
        }
    }

    /*
    public class TestPage
    {
        public static void Test()
        {
            for (int i = 0; i < 100; i++)
            {
                TestPage tp = new();
                AddMenuPage(tp.OnConstruct, tp.TryGetButton);
            }
        }

        public ToggleButton button;

        public void OnConstruct(MenuPage page)
        {
            button = new(page, "Test");
        }

        public bool TryGetButton(MenuPage page, out SmallButton button)
        {
            button = this.button;
            if (button == null) throw new InvalidOperationException();
            return true;
        }
    }
    */
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using MenuChanger;
using MenuChanger.MenuPanels;
using MenuChanger.MenuElements;
using Random = System.Random;
using System.Threading;
using UnityEngine.UI;
using RandomizerMod.Extensions;
using RandomizerMod.Settings;
using RandomizerMod.Settings.Presets;
using static RandomizerMod.LogHelper;
using RandomizerMod.RandomizerData;
using RandomizerMod.RC;
using MenuChanger.Extensions;
using RandomizerCore.Extensions;

namespace RandomizerMod.Menu
{
    public class RandomizerMenuConstructor : ModeMenuConstructor
    {
        internal static RandomizerMenu menu;
        internal static bool finished = false;

        public override bool TryGetModeButton(MenuPage modeMenu, out BigButton button)
        {
            button = menu.EntryButton;
            return true;
        }

        public override void OnEnterMainMenu(MenuPage modeMenu)
        {
            menu = new RandomizerMenu(modeMenu);
            foreach (var entry in RandomizerMenuAPI.randoMenuPageConstructors)
            {
                try
                {
                    entry.ConstructionHandler(menu.ConnectionsPage);
                }
                catch (Exception e)
                {
                    LogError($"Error constructing external menu:\n{e}");
                }
            }
            foreach (var entry in RandomizerMenuAPI.randoStartOverrides)
            {
                try
                {
                    entry.ConstructionHandler(menu.FinalPage);
                }
                catch (Exception e)
                {
                    LogError($"Error constructing external menu:\n{e}");
                }
            }
            finished = true;
        }

        public override void OnExitMainMenu()
        {
            menu = null;
            finished = false;
        }
    }

    public class RandomizerMenu
    {
        public BigButton EntryButton;
        GenerationSettings Settings => RandomizerMod.GS.DefaultMenuSettings;
        List<MenuProfile> Profiles => RandomizerMod.GS.Profiles;
        readonly StartDef[] StartDefs;

        readonly MenuPage ModePage;
        MenuPage ResumePage;
        readonly SettingsPM pm;
        readonly Random rng = new();
            
        #region Start

        MenuPage StartPage;

        BigButton GenerateButton;
        NumericEntryField<int> SeedEntryField;
        SmallButton RandomSeedButton;


        MenuPreset<PoolSettings> PoolPreset;
        MenuPreset<SkipSettings> SkipPreset;
        MenuPreset<NoveltySettings> NoveltyPreset;
        MenuPreset<CostSettings> CostPreset;
        MenuPreset<StartItemSettings> StartItemPreset;
        MenuPreset<StartLocationSettings> StartLocationPreset;
        MenuPreset<LongLocationSettings> LongLocationPreset;
        MenuPreset<CursedSettings> CursedPreset;
        MenuPreset<TransitionSettings> TransitionPreset;
        MenuPreset<DuplicateItemSettings> DuplicateItemPreset;

        SmallButton[] PresetButtons => new SmallButton[]
        {
            PoolPreset,
            SkipPreset,
            NoveltyPreset,
            CostPreset,
            StartItemPreset,
            StartLocationPreset,
            LongLocationPreset,
            CursedPreset,
            TransitionPreset,
            DuplicateItemPreset,
        };
        GridItemPanel StartGIP;


        SmallButton JumpToJumpPageButton;
        SmallButton JumpToConnectionsButton;
        ToggleButton ToggleCaptionsButton;

        SmallButton[] StartCornerButtons => new SmallButton[]
        {
            JumpToJumpPageButton,
            JumpToConnectionsButton,
            ToggleCaptionsButton,
        };
        VerticalItemPanel StartCornerVIP;

        #endregion

        #region Jump

        MenuPage JumpPage;
        SmallButton[] JumpButtons;
        GridItemPanel JumpPanel;

        SmallButton DefaultSettingsButton;
        SmallButton ToManageSettingsPageButton;

        #endregion

        #region Manage Settings

        MenuPage ManageSettingsPage;

        TextEntryField SettingsCodeField;
        SmallButton GenerateCodeButton;
        SmallButton ApplyCodeButton;
        IMenuElement[] CodeElements => new IMenuElement[]
        {
            GenerateCodeButton,
            ApplyCodeButton,
            SettingsCodeField,
        };
        VerticalItemPanel CodeVIP;

        TextEntryField ProfileNameField;
        MenuItem<MenuProfile> ProfileSwitch;
        SmallButton OverwriteProfileButton;
        SmallButton DeleteProfileButton;
        SmallButton SaveAsNewProfileButton;
        SmallButton ApplyProfileButton;
        IMenuElement[] ProfileElements => new IMenuElement[]
        {
            ProfileSwitch,
            OverwriteProfileButton,
            DeleteProfileButton,
            SaveAsNewProfileButton,
            ApplyProfileButton,
            ProfileNameField,
        };
        VerticalItemPanel ProfileVIP;

        #endregion

        #region Advanced Settings

        MenuPage AdvancedSettingsPage;

        OrderedItemViewer AdvancedSettingsViewer;

        Subpage[] AdvancedSettingsSubpages => new Subpage[]
        {
            PoolSubpage,
            SkipSubpage,
            NoveltySubpage,
            CostSubpage,
            LongLocationSubpage,
            StartLocationSubpage,
            StartItemSubpage,
            MiscSubpage,
            CursedSubpage,
            TransitionSubpage,
            ProgressionDepthSubpage,
            DuplicateItemSubpage
        };

        Subpage PoolSubpage;
        MenuElementFactory<PoolSettings> poolMEF;
        GridItemPanel poolGIP;

        Subpage SkipSubpage;
        MenuElementFactory<SkipSettings> skipMEF;
        GridItemPanel skipPanel;

        Subpage NoveltySubpage;
        MenuElementFactory<NoveltySettings> novMEF;
        VerticalItemPanel novVIP;

        Subpage CostSubpage;
        MenuElementFactory<CostSettings> costMEF;
        GridItemPanel costGIP;

        Subpage LongLocationSubpage;
        MenuElementFactory<LongLocationSettings> longLocationMEF;
        VerticalItemPanel longVIP;

        Subpage StartLocationSubpage;
        MenuItem<StartLocationSettings.RandomizeStartLocationType> startLocationTypeSwitch;
        SmallButton randomFixedStartButton;
        RadioSwitch startLocationSwitch;
        VerticalItemPanel startLocationVIP;
        GridItemPanel startLocationGIP;

        Subpage StartItemSubpage;
        MenuElementFactory<StartItemSettings> startItemMEF;
        GridItemPanel startItemGIP;

        Subpage MiscSubpage;
        MenuElementFactory<MiscSettings> miscMEF;
        VerticalItemPanel miscVIP;

        Subpage CursedSubpage;
        MenuElementFactory<CursedSettings> cursedMEF;
        VerticalItemPanel cursedVIP;

        Subpage TransitionSubpage;
        MenuElementFactory<TransitionSettings> transitionMEF;
        VerticalItemPanel transitionPanel;

        Subpage ProgressionDepthSubpage;
        MenuElementFactory<ProgressionDepthSettings> progressionDepthMEF;
        VerticalItemPanel progressionDepthPanel;

        Subpage DuplicateItemSubpage;
        MenuElementFactory<DuplicateItemSettings> duplicateItemMEF;
        GridItemPanel duplicateItemPanel;

        #endregion

        #region Game Settings

        public MenuPage ConnectionsPage;
        MultiGridItemPanel connectionsPanel;
        MenuLabel emptyConnectionsPanelLabel;
        #endregion

        #region Final

        public MenuPage FinalPage;

        BigButton StartButton;

        MenuLabel InfoPanelTitle;
        CounterLabel AttemptCounter;
        TimerLabel RandomizationTimer;
        MenuLabel OutputLabel;

        IMenuElement[] InfoElements => new IMenuElement[]
        {
            InfoPanelTitle,
            RandomizationTimer,
            AttemptCounter,
            OutputLabel,
        };
        VerticalItemPanel generationInfoVIP;

        MenuLabel[] HashLabels;
        VerticalItemPanel HashVIP;

        #endregion


        public RandomizerMenu(MenuPage modePage)
        {
            ModePage = modePage;

            StartDefs = Data.GetStartNames().Select(s => Data.GetStartDef(s)).ToArray();
            pm = new(Settings);

            MakeMenuPages();
            MakeMenuElements();
            MakePanels();
            AddEvents();
            Arrange();

            ResumeMenu.AddResumePage("Randomizer", ResumePage);
            SeedEntryField.SetValue(rng.Next(0, 999999999));
            ApplySettingsToMenu(Settings);
        }

        private void MakeMenuPages()
        {
            StartPage = new MenuPage("Randomizer Setting Select Page", ModePage);
            JumpPage = new MenuPage("Randomizer Jump To Advanced Settings Page", StartPage);
            AdvancedSettingsPage = new MenuPage("Randomizer Advanced Settings Page", JumpPage);
            ManageSettingsPage = new MenuPage("Randomizer Manage Settings Page", JumpPage);
            ConnectionsPage = new MenuPage("Randomizer Game Settings Page", StartPage);
            FinalPage = new MenuPage("Randomizer Final Page", StartPage);
            FinalPage.backButton.Text.text = "Abort";
            FinalPage.backButton.OnClick += Abort;

            ResumePage = new MenuPage("Randomizer Resume Page");
            new BigButton(ResumePage, "Resume").OnClick += () =>
            {
                MenuChangerMod.HideAllMenuPages();
                UIManager.instance.ContinueGame();
                GameManager.instance.ContinueGame();
            };
        }

        private void MakeMenuElements()
        {
            EntryButton = new BigButton(ModePage, SpriteManager.GetSprite("logo"), "Randomizer");
            // Start Page

            JumpToJumpPageButton = new SmallButton(StartPage, "More Randomizer Settings");
            JumpToConnectionsButton = new SmallButton(StartPage, "Connections");
            ToggleCaptionsButton = new ToggleButton(StartPage, "Toggle Menu Captions");

            GenerateButton = new BigButton(StartPage, "Begin Randomization");
            SeedEntryField = new NumericEntryField<int>(StartPage, "Seed");

            RandomSeedButton = new SmallButton(StartPage, "Random");

            // The AdvancedSettingsPage Elements must be constructed before the StartPage preset buttons.
            poolMEF = new MenuElementFactory<PoolSettings>(AdvancedSettingsPage, Settings.PoolSettings);
            skipMEF = new MenuElementFactory<SkipSettings>(AdvancedSettingsPage, Settings.SkipSettings);
            novMEF = new MenuElementFactory<NoveltySettings>(AdvancedSettingsPage, Settings.NoveltySettings);
            costMEF = new MenuElementFactory<CostSettings>(AdvancedSettingsPage, Settings.CostSettings);
            longLocationMEF = new MenuElementFactory<LongLocationSettings>(AdvancedSettingsPage, Settings.LongLocationSettings);
            
            startLocationTypeSwitch = new MenuEnum<StartLocationSettings.RandomizeStartLocationType>(AdvancedSettingsPage,
                nameof(StartLocationSettings.StartLocationType).FromCamelCase());
            startLocationTypeSwitch.Bind(Settings.StartLocationSettings,
                typeof(StartLocationSettings).GetField(nameof(StartLocationSettings.StartLocationType)));
            randomFixedStartButton = new SmallButton(AdvancedSettingsPage, "Random Fixed Start");

            startLocationSwitch = new RadioSwitch(AdvancedSettingsPage, StartDefs.Select(def => def.Name).ToArray());
            startItemMEF = new MenuElementFactory<StartItemSettings>(AdvancedSettingsPage, Settings.StartItemSettings);

            miscMEF = new MenuElementFactory<MiscSettings>(AdvancedSettingsPage, Settings.MiscSettings);
            cursedMEF = new MenuElementFactory<CursedSettings>(AdvancedSettingsPage, Settings.CursedSettings);
            transitionMEF = new MenuElementFactory<TransitionSettings>(AdvancedSettingsPage, Settings.TransitionSettings);
            progressionDepthMEF = new MenuElementFactory<ProgressionDepthSettings>(AdvancedSettingsPage, Settings.ProgressionDepthSettings);
            duplicateItemMEF = new MenuElementFactory<DuplicateItemSettings>(AdvancedSettingsPage, Settings.DuplicateItemSettings);

            PoolPreset = new MenuPreset<PoolSettings>(StartPage, "Randomized Items", 
                PoolPresetData.PoolPresets, Settings.PoolSettings,
                (ps) => string.Join(", ", typeof(PoolSettings).GetFields().Where(f => (bool)f.GetValue(ps)).Select(f => f.Name.FromCamelCase()).ToArray()),
                poolMEF);
            SkipPreset = new MenuPreset<SkipSettings>(StartPage, "Required Skips", 
                SkipPresetData.SkipPresets, Settings.SkipSettings,
                (sk) => string.Join(", ", typeof(SkipSettings).GetFields().Where(f => (bool)f.GetValue(sk)).Select(f => f.Name.FromCamelCase()).ToArray()),
                skipMEF);
            NoveltyPreset = new MenuPreset<NoveltySettings>(StartPage, "Novelties", NoveltyPresetData.NoveltyPresets, Settings.NoveltySettings, Captions.Caption, novMEF);
            CostPreset = new MenuPreset<CostSettings>(StartPage, "Cost Randomization", CostPresetData.CostPresets, Settings.CostSettings, Captions.Caption, costMEF);
            StartItemPreset = new MenuPreset<StartItemSettings>(StartPage, "Start Items",
                StartItemPresetData.StartItemPresets, Settings.StartItemSettings, si => si.Caption(), startItemMEF);
            StartLocationPreset = new MenuPreset<StartLocationSettings>(StartPage, "Start Location",
                StartLocationPresetData.StartLocationPresets, Settings.StartLocationSettings, Captions.Caption);
            StartLocationPreset.Pair(startLocationTypeSwitch, typeof(StartLocationSettings).GetField(nameof(StartLocationSettings.StartLocationType)));

            LongLocationPreset = new MenuPreset<LongLocationSettings>(StartPage, "Long Locations",
                LongLocationPresetData.LongLocationPresets, Settings.LongLocationSettings,
                ll => ll.Caption(Settings),
                longLocationMEF);
            
            CursedPreset = new MenuPreset<CursedSettings>(StartPage, "Curses",
                CursePresetData.CursedPresets, Settings.CursedSettings,
                cs => cs.Caption(),
                cursedMEF);

            TransitionPreset = new MenuPreset<TransitionSettings>(StartPage, "Transition Randomizer",
                TransitionPresetData.TransitionPresets, Settings.TransitionSettings,
                ts => ts.Caption(),
                transitionMEF);

            DuplicateItemPreset = new MenuPreset<DuplicateItemSettings>(StartPage, "Duplicate Items",
                DuplicateItemPresetData.Presets, Settings.DuplicateItemSettings, ds => ds.Caption(), duplicateItemMEF);

            DefaultSettingsButton = new SmallButton(JumpPage, "Restore Default Settings");
            ToManageSettingsPageButton = new SmallButton(JumpPage, "Manage Settings Profiles");

            GenerateCodeButton = new SmallButton(ManageSettingsPage, "Refresh Code");
            ApplyCodeButton = new SmallButton(ManageSettingsPage, "Apply Code To Menu");
            SettingsCodeField = new TextEntryField(ManageSettingsPage, "Shareable Settings Code");

            ProfileSwitch = new MenuItem<MenuProfile>(ManageSettingsPage, "Profile", Profiles);
            OverwriteProfileButton = new SmallButton(ManageSettingsPage, "Overwrite Selected Profile");
            DeleteProfileButton = new SmallButton(ManageSettingsPage, "Delete Selected Profile");
            SaveAsNewProfileButton = new SmallButton(ManageSettingsPage, "Save As New Profile");
            ApplyProfileButton = new SmallButton(ManageSettingsPage, "Apply Profile");
            ProfileNameField = new TextEntryField(ManageSettingsPage, "Profile Name");

            // Final Page
            InfoPanelTitle = new MenuLabel(FinalPage, "Randomizer Progress");
            AttemptCounter = new CounterLabel(FinalPage, "Attempts");
            RandomizationTimer = new TimerLabel(FinalPage, "Time Elapsed");
            OutputLabel = new MenuLabel(FinalPage, "", new Vector2(800, 800));
            OutputLabel.Hide();

            StartButton = new BigButton(FinalPage, SpriteManager.GetSprite("logo"), "Start Game");
            StartButton.Hide();

            int hashLength = 1 + 4;
            HashLabels = new MenuLabel[hashLength];
            HashLabels[0] = new MenuLabel(FinalPage, "Hash");
            for (int i = 1; i < hashLength; i++)
            {
                HashLabels[i] = new MenuLabel(FinalPage, "", MenuLabel.Style.Body);
                HashLabels[i].Text.alignment = TextAnchor.UpperCenter;
            }
        }

        private void MakePanels()
        {
            StartGIP = new GridItemPanel(StartPage, new Vector2(0, 450), 2, 125, 960, true, PresetButtons);

            PoolSubpage = new Subpage(AdvancedSettingsPage, "Randomized Items");
            poolGIP = new GridItemPanel(AdvancedSettingsPage, new Vector2(0, 300), 4, 50f, 400f, false, poolMEF.Elements);
            PoolSubpage.Add(poolGIP);

            SkipSubpage = new Subpage(AdvancedSettingsPage, "Required Skips");
            skipPanel = new GridItemPanel(AdvancedSettingsPage, new Vector2(0, 300), 2, 50f, 800f,  false, skipMEF.Elements);
            SkipSubpage.Add(skipPanel);

            NoveltySubpage = new Subpage(AdvancedSettingsPage, "Novelties");
            novVIP = new VerticalItemPanel(AdvancedSettingsPage, new Vector2(0, 300), 50f, false, novMEF.Elements.ToArray());
            NoveltySubpage.Add(novVIP);

            CostSubpage = new Subpage(AdvancedSettingsPage, "Cost Randomization");
            costGIP = new GridItemPanel(AdvancedSettingsPage, new Vector2(0, 300), 3, 200f, 400f, false, costMEF.Elements.ToArray());
            CostSubpage.Add(costGIP);

            LongLocationSubpage = new Subpage(AdvancedSettingsPage, "Long Location Options");
            longVIP = new VerticalItemPanel(AdvancedSettingsPage, new Vector2(0, 300), 50f, false, longLocationMEF.Elements);
            LongLocationSubpage.Add(longVIP);

            StartLocationSubpage = new Subpage(AdvancedSettingsPage, "Start Location");
            startLocationGIP = new GridItemPanel(AdvancedSettingsPage, new Vector2(0, 150), 3, 50f, 600f, false, startLocationSwitch.Elements);
            startLocationVIP = new VerticalItemPanel(AdvancedSettingsPage, new Vector2(0, 300), 75f, false, startLocationTypeSwitch, randomFixedStartButton, startLocationGIP);
            StartLocationSubpage.Add(startLocationVIP);

            StartItemSubpage = new Subpage(AdvancedSettingsPage, "Start Items");
            startItemGIP = new GridItemPanel(AdvancedSettingsPage, new Vector2(0, 300), 2, 150f, 800f, false, startItemMEF.Elements);
            StartItemSubpage.Add(startItemGIP);

            MiscSubpage = new Subpage(AdvancedSettingsPage, "Miscellaneous");
            miscVIP = new VerticalItemPanel(AdvancedSettingsPage, new Vector2(0, 300), 50f, false, miscMEF.Elements);
            MiscSubpage.Add(miscVIP);

            CursedSubpage = new Subpage(AdvancedSettingsPage, "Curse Options");
            cursedVIP = new VerticalItemPanel(AdvancedSettingsPage, new Vector2(0, 300), 50f, false, cursedMEF.Elements);
            CursedSubpage.Add(cursedVIP);

            TransitionSubpage = new Subpage(AdvancedSettingsPage, "Transition Randomizer");
            transitionPanel = new VerticalItemPanel(AdvancedSettingsPage, new Vector2(0, 300), 50f, false, transitionMEF.Elements);
            TransitionSubpage.Add(transitionPanel);

            ProgressionDepthSubpage = new Subpage(AdvancedSettingsPage, "Progression Depth");
            progressionDepthPanel = new VerticalItemPanel(AdvancedSettingsPage, new Vector2(0f, 300f), 75f, false, progressionDepthMEF.Elements);
            ProgressionDepthSubpage.Add(progressionDepthPanel);

            DuplicateItemSubpage = new Subpage(AdvancedSettingsPage, "Duplicate Items");
            duplicateItemPanel = new GridItemPanel(AdvancedSettingsPage, new Vector2(0, 300), 2, 50f, 800f, false, duplicateItemMEF.Elements);
            DuplicateItemSubpage.Add(duplicateItemPanel);

            AdvancedSettingsViewer = new OrderedItemViewer(AdvancedSettingsPage, AdvancedSettingsSubpages);

            JumpButtons = AdvancedSettingsSubpages.Select(p =>
            {
                SmallButton b = new(JumpPage, p.TitleLabel.Text.text);
                b.OnClick += () =>
                {
                    JumpPage.TransitionTo(AdvancedSettingsPage);
                    b.Button.ForceDeselect();
                    AdvancedSettingsViewer.JumpTo(p);
                    AdvancedSettingsPage.nav.SelectDefault();
                };
                return b;
            }).ToArray();
            JumpPanel = new GridItemPanel(JumpPage, new Vector2(0, 300), 2, 60f, 800f, true, JumpButtons);

            StartCornerVIP = new VerticalItemPanel(StartPage, new Vector2(-650, -350), 50f, false, StartCornerButtons);

            emptyConnectionsPanelLabel = new MenuLabel(ConnectionsPage, "This page is currently empty. " +
                "Mods connected to the randomizer can link menus here.", MenuLabel.Style.Title);
            emptyConnectionsPanelLabel.MoveTo(new(0, 400));
            connectionsPanel = new MultiGridItemPanel(ConnectionsPage, 6, 4, 50f, 400f, new(0, 300), Array.Empty<IMenuElement>());
            // note - connection entries are constructed after menu construction!

            CodeVIP = new VerticalItemPanel(ManageSettingsPage, new Vector2(-400, 300), 100, true, CodeElements);
            ProfileVIP = new VerticalItemPanel(ManageSettingsPage, new Vector2(400, 300), 100, true, ProfileElements);

            generationInfoVIP = new VerticalItemPanel(FinalPage, new Vector2(-400, 300), 50, true, InfoElements);
            HashVIP = new VerticalItemPanel(FinalPage, new Vector2(400, 300), 50, true, HashLabels);
            HashVIP.Hide();
        }

        private void AddEvents()
        {
            EntryButton.AddHideAndShowEvent(ModePage, StartPage);

            SeedEntryField.ValueChanged += (i) => Settings.Seed = i;

            GenerateButton.AddHideAndShowEvent(StartPage, FinalPage);
            GenerateButton.OnClick += Randomize;

            RandomSeedButton.OnClick += () => SeedEntryField.SetValue(rng.Next(0, 1000000000));

            StartLocationPreset.SelfChanged += (self) => UpdateStartLocationPreset();

            JumpToJumpPageButton.AddHideAndShowEvent(StartPage, JumpPage);
            JumpToConnectionsButton.OnClick += RebuildConnectionsPanel;
            JumpToConnectionsButton.AddHideAndShowEvent(StartPage, ConnectionsPage);

            ToggleCaptionsButton.SetValue(true);
            ToggleCaptionsButton.SelfChanged += (self) =>
            {
                foreach (SmallButton button in PresetButtons)
                {
                    if (button is IMenuPreset preset) preset.Label?.SetVisibleByAlpha(((ToggleButton)self).Value);
                }
            };

            skipMEF.ElementLookup[nameof(SkipSettings.MildSkips)].SelfChanged += UpdateStartLocation;
            skipMEF.ElementLookup[nameof(SkipSettings.MildSkips)].SelfChanged += UpdateStartLocation;
            skipMEF.ElementLookup[nameof(SkipSettings.ShadeSkips)].SelfChanged += UpdateStartLocation;
            skipMEF.ElementLookup[nameof(SkipSettings.AcidSkips)].SelfChanged += UpdateStartLocation;
            skipMEF.ElementLookup[nameof(SkipSettings.FireballSkips)].SelfChanged += UpdateStartLocation;
            skipMEF.ElementLookup[nameof(SkipSettings.SpikeTunnels)].SelfChanged += UpdateStartLocation;
            skipMEF.ElementLookup[nameof(SkipSettings.DarkRooms)].SelfChanged += UpdateStartLocation;
            skipMEF.ElementLookup[nameof(SkipSettings.SpicySkips)].SelfChanged += UpdateStartLocation;

            transitionMEF.ElementLookup[nameof(TransitionSettings.Mode)].SelfChanged += UpdateStartLocation;

            novMEF.ElementLookup[nameof(NoveltySettings.RandomizeSwim)].SelfChanged += UpdateStartLocation;
            novMEF.ElementLookup[nameof(NoveltySettings.RandomizeElevatorPass)].SelfChanged += UpdateStartLocation;
            cursedMEF.ElementLookup[nameof(CursedSettings.CursedMasks)].SelfChanged += UpdateStartLocation;

            startLocationSwitch.Changed += Settings.StartLocationSettings.SetStartLocation;
            startLocationSwitch.Changed += (s) => UpdateStartLocationPreset();
            startLocationTypeSwitch.ValueChanged += UpdateStartLocationSwitch;
            randomFixedStartButton.OnClick += () =>
            {
                startLocationTypeSwitch.SetValue(StartLocationSettings.RandomizeStartLocationType.Fixed);
                startLocationSwitch.ChangeSelection(rng.NextWhere(startLocationSwitch.Elements, e => !e.Locked));
            };
            UpdateStartLocation();

            ToManageSettingsPageButton.AddHideAndShowEvent(JumpPage, ManageSettingsPage);
            DefaultSettingsButton.OnClick += () => ApplySettingsToMenu(new GenerationSettings()); // Proper defaults please!


            GenerateCodeButton.OnClick += () => SettingsCodeField.SetValue(Settings.Serialize());
            ApplyCodeButton.OnClick += () =>
            {
                if (GenerationSettings.Deserialize(SettingsCodeField.Value) is GenerationSettings gs)
                {
                    ApplySettingsToMenu(gs);
                }
            };

            ProfileSwitch.SelfChanged += (element) =>
            {
                MenuItem<MenuProfile> self = (MenuItem<MenuProfile>)element;
                ProfileNameField.SetValue(self.Value?.ToString() ?? string.Empty);
                if (self.Value == null)
                {
                    OverwriteProfileButton.Lock();
                    DeleteProfileButton.Lock();
                    ApplyProfileButton.Lock();
                }
                else
                {
                    OverwriteProfileButton.Unlock();
                    DeleteProfileButton.Unlock();
                    ApplyProfileButton.Unlock();
                }
            };
            OverwriteProfileButton.Lock();
            DeleteProfileButton.Lock();
            ApplyProfileButton.Lock();

            OverwriteProfileButton.OnClick += () =>
            {
                MenuProfile mp = new MenuProfile
                {
                    name = ProfileNameField.Value,
                    settings = Settings.Clone() as GenerationSettings
                };
                ProfileSwitch.OverwriteCurrent(mp);
            };
            SaveAsNewProfileButton.OnClick += () =>
            {
                MenuProfile mp = new MenuProfile
                {
                    name = ProfileNameField.Value,
                    settings = Settings.Clone() as GenerationSettings
                };
                ProfileSwitch.AddItem(mp);
                ProfileSwitch.SetValue(mp);
            };
            DeleteProfileButton.OnClick += () =>
            {
                ProfileSwitch.RemoveCurrent();
            };
            ApplyProfileButton.OnClick += () =>
            {
                if (ProfileSwitch.Value is MenuProfile mp)
                {
                    ApplySettingsToMenu(mp.settings);
                }
            };

            StartButton.AddSetResumeKeyEvent("Randomizer");
            StartButton.OnClick += StartRandomizerGame;
        }

        private void Arrange()
        {
            GenerateButton.MoveTo(new Vector2(0, -350));
            SeedEntryField.MoveTo(new Vector2(650, -350));
            RandomSeedButton.MoveTo(new Vector2(650, -400));

            DefaultSettingsButton.MoveTo(new Vector2(-400, -300));
            ToManageSettingsPageButton.MoveTo(new Vector2(400, -300));

            StartButton.MoveTo(new Vector2(0, -200));

            // buttons not in panels need navigation
            GenerateButton.SymSetNeighbor(Neighbor.Up, StartGIP);
            GenerateButton.SymSetNeighbor(Neighbor.Right, RandomSeedButton);
            GenerateButton.SymSetNeighbor(Neighbor.Left, StartCornerVIP);
            GenerateButton.SymSetNeighbor(Neighbor.Down, StartPage.backButton);

            RandomSeedButton.SymSetNeighbor(Neighbor.Up, SeedEntryField);
            RandomSeedButton.SymSetNeighbor(Neighbor.Right, StartCornerVIP);

            StartCornerVIP.SetNeighbor(Neighbor.Up, StartGIP);

            StartPage.backButton.SetNeighbor(Neighbor.Right, RandomSeedButton);
            StartPage.backButton.SetNeighbor(Neighbor.Left, StartCornerVIP);

            DefaultSettingsButton.SymSetNeighbor(Neighbor.Up, JumpPanel);
            ToManageSettingsPageButton.SetNeighbor(Neighbor.Up, JumpPanel);
            DefaultSettingsButton.SymSetNeighbor(Neighbor.Down, JumpPage.backButton);
            ToManageSettingsPageButton.SetNeighbor(Neighbor.Down, JumpPage.backButton);
            DefaultSettingsButton.SymSetNeighbor(Neighbor.Left, ToManageSettingsPageButton);
            DefaultSettingsButton.SymSetNeighbor(Neighbor.Right, ToManageSettingsPageButton);
        }

        private void UpdateStartLocationSwitch(StartLocationSettings.RandomizeStartLocationType type, string loc)
        {
            switch (type)
            {
                default:
                case StartLocationSettings.RandomizeStartLocationType.Fixed:
                    startLocationSwitch.DeselectAll(button => !CanSelectStart(button.Name));
                    if (!startLocationSwitch.TrySelect(loc))
                    {
                        startLocationSwitch.SelectFirst();
                    }
                    break;
                case StartLocationSettings.RandomizeStartLocationType.Random:
                    startLocationSwitch.MatchPredicateAndLock(button => CanSelectStart(button.Name));
                    break;
                case StartLocationSettings.RandomizeStartLocationType.RandomExcludingKP:
                    startLocationSwitch.MatchPredicateAndLock(button => button.Name != "King's Pass" && CanSelectStart(button.Name));
                    break;
            }
        }

        private void UpdateStartLocationSwitch(StartLocationSettings.RandomizeStartLocationType type)
        {
            UpdateStartLocationSwitch(
                type,
                Settings.StartLocationSettings.StartLocation);
        }

        private void UpdateStartLocationSwitch(MenuItem<StartLocationSettings.RandomizeStartLocationType> startLocationTypeSwitch)
        {
            UpdateStartLocationSwitch(
                startLocationTypeSwitch.Value,
                Settings.StartLocationSettings.StartLocation);
        }

        private void UpdateStartLocationSwitch()
        {
            UpdateStartLocationSwitch(Settings.StartLocationSettings.StartLocationType, Settings.StartLocationSettings.StartLocation);
        }

        private void UpdateStartLocationPreset()
        {
            StartLocationPreset.UpdatePreset();
            if (Settings.StartLocationSettings.StartLocationType == StartLocationSettings.RandomizeStartLocationType.Fixed
                && StartLocationPresetData.StartLocationPresets.TryGetValue(StartLocationPreset.Value, out StartLocationSettings preset)
                && Settings.StartLocationSettings.StartLocation != preset.StartLocation)
            {
                StartLocationPreset.SetValue("Custom");
            }
            StartLocationPreset.UpdateCaption();
        }

        private void UpdateStartLocation()
        {
            UpdateStartLocationSwitch();
            UpdateStartLocationPreset();
        }

        private void UpdateStartLocation(object o)
        {
            UpdateStartLocation();
        }

        private void ApplySettingsToMenu(GenerationSettings settings)
        {
            poolMEF.SetMenuValues(settings.PoolSettings);
            skipMEF.SetMenuValues(settings.SkipSettings);
            costMEF.SetMenuValues(settings.CostSettings);
            longLocationMEF.SetMenuValues(settings.LongLocationSettings);
            startLocationTypeSwitch.SetValue(settings.StartLocationSettings.StartLocationType);
            if (settings.StartLocationSettings.StartLocationType == StartLocationSettings.RandomizeStartLocationType.Fixed)
            {
                startLocationSwitch.TrySelect(settings.StartLocationSettings.StartLocation);
            }
            startItemMEF.SetMenuValues(settings.StartItemSettings);
            miscMEF.SetMenuValues(settings.MiscSettings);
            cursedMEF.SetMenuValues(settings.CursedSettings);
            transitionMEF.SetMenuValues(settings.TransitionSettings);

            foreach (IMenuPreset preset in PresetButtons) preset.UpdatePreset();
        }


        Thread RandomizerThread;
        RandoController rc;
        
        private void Randomize()
        {
            AttemptCounter.Set(0);

            RandomizerCore.RandoMonitor rm = new();
            rm.OnSendEvent += (t, m) =>
            {
                Log($"[{t}]  {m}");
                if (t == RandomizerCore.RandoEventType.NewAttempt) ThreadSupport.BeginInvoke(() => AttemptCounter.Incr());
                else if (t == RandomizerCore.RandoEventType.Finished)
                {

                }
                if (!string.IsNullOrEmpty(m))
                {
                    ThreadSupport.BeginInvoke(() =>
                    {
                        OutputLabel.Text.text = m;
                    });
                }
            };

            RandomizationTimer.Reset();
            RandomizationTimer.Start();

            RandomizerThread = new Thread(() =>
            {
                try
                {
                    rc = new(Settings, pm, rm);
                    rc.Run();
                    ThreadSupport.BeginInvoke(() =>
                    {
                        RandomizationTimer.Stop();
                        OutputLabel.Text.color = Color.white;
                        OutputLabel.Text.text = "Randomization completed successfully!";
                        OutputLabel.Text.alignment = TextAnchor.UpperCenter;
                        OutputLabel.Show();

                        string[] hash = Hash.GetHash(rc.Hash());
                        for (int i = 0; i < Hash.Length; i++)
                        {
                            HashLabels[1 + i].Text.text = hash[i];
                        }
                        HashVIP.Show();

                        var startButton = ResolveStartButton();
                        startButton.Show();
                        startButton.MoveTo(new(0f, -200f));
                        FinalPage.backButton.SetNeighbor(Neighbor.Up, startButton);
                        startButton.SetNeighbor(Neighbor.Down, FinalPage.backButton);
                    });
                }
                catch (Exception e)
                {
                    ThreadSupport.BeginInvoke(() =>
                    {
                        RandomizationTimer.Stop();
                        OutputLabel.Text.color = Color.red;
                        OutputLabel.Text.text = "Randomization terminated due to error:\n" + e;
                        OutputLabel.Text.alignment = TextAnchor.UpperLeft;
                        OutputLabel.Show();
                        Log("Randomization terminated due to error:\n" + e);
                    });
                }
            });
            RandomizerThread.Start();
        }

        private void StartRandomizerGame()
        {
            try
            {
                rc.Save();
                MenuChangerMod.HideAllMenuPages();
                UIManager.instance.StartNewGame();
            }
            catch (Exception e)
            {
                Log("Start Game terminated due to error:\n" + e);
                FinalPage.Show();
                OutputLabel.Text.color = Color.red;
                OutputLabel.Text.text = "Start Game terminated due to error:\n" + e;
                OutputLabel.Text.alignment = TextAnchor.UpperLeft;
                OutputLabel.Show();
            }
        }

        private void Abort()
        {
            RandomizerThread?.Abort();
            RandomizerThread?.Join();
            OutputLabel.Hide();
            HashVIP.Hide();
            StartButton.Hide();
            FinalPage.backButton.SetNeighbor(Neighbor.Up, null);
            StartButton.SetNeighbor(Neighbor.Down, null);
        }

        private bool CanSelectStart(string name)
        {
            StartDef def = Data.GetStartDef(name);
            return pm.Evaluate(def.Logic);
        }

        public void RebuildConnectionsPanel()
        {
            if (!RandomizerMenuConstructor.finished) return;

            List<SmallButton> buttons = new();
            foreach (var entry in RandomizerMenuAPI.randoMenuPageConstructors)
            {
                if (entry.ButtonHandler(ConnectionsPage, out SmallButton button))
                {
                    buttons.Add(button);
                }
            }
            connectionsPanel.Clear();
            if (buttons.Count > 0)
            {
                connectionsPanel.AddRange(buttons);
                emptyConnectionsPanelLabel.Hide();
            }
            else
            {
                emptyConnectionsPanelLabel.Show();
            }
        }

        private BaseButton ResolveStartButton()
        {
            foreach (var entry in RandomizerMenuAPI.randoStartOverrides)
            {
                if (entry.StartHandler(rc, FinalPage, out BaseButton button)) return button;
            }
            return StartButton;
        }

    }
}

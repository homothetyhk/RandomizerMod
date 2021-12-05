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
        private RandomizerMenu menu;

        public override bool TryGetModeButton(MenuPage modeMenu, out BigButton button)
        {
            button = menu.EntryButton;
            return true;
        }

        public override void OnEnterMainMenu(MenuPage modeMenu)
        {
            menu = new RandomizerMenu(modeMenu);
        }

        public override void OnExitMainMenu()
        {
            menu = null;
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
        IntEntryField SeedEntryField;
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
        };
        GridItemPanel StartGIP;


        SmallButton JumpToJumpPageButton;
        SmallButton JumpToGameSettingsButton;
        ToggleButton ToggleCaptionsButton;

        SmallButton[] StartCornerButtons => new SmallButton[]
        {
            JumpToJumpPageButton,
            JumpToGameSettingsButton,
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
        };

        Subpage PoolSubpage;
        MenuElementFactory<PoolSettings> poolMEF;
        GridItemPanel poolGIP;

        Subpage SkipSubpage;
        MenuElementFactory<SkipSettings> skipMEF;
        VerticalItemPanel skipVIP;

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

        #endregion

        #region Game Settings

        MenuPage GameSettingsPage;
        //MenuElementFactory<QoLSettings> gameSettingsMEF;

        //GridItemPanel gameSettingsPanel;
        #endregion

        #region Final

        MenuPage FinalPage;

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
            SeedEntryField.InputValue = rng.Next(0, 999999999);
            ApplySettingsToMenu(Settings);
        }

        private void MakeMenuPages()
        {
            StartPage = new MenuPage("Randomizer Setting Select Page", ModePage);
            JumpPage = new MenuPage("Randomizer Jump To Advanced Settings Page", StartPage);
            AdvancedSettingsPage = new MenuPage("Randomizer Advanced Settings Page", JumpPage);
            ManageSettingsPage = new MenuPage("Randomizer Manage Settings Page", JumpPage);
            GameSettingsPage = new MenuPage("Randomizer Game Settings Page", StartPage);
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
            JumpToGameSettingsButton = new SmallButton(StartPage, "Game Settings");
            ToggleCaptionsButton = new ToggleButton(StartPage, "Toggle Menu Captions");

            GenerateButton = new BigButton(StartPage, "Begin Randomization");
            SeedEntryField = new IntEntryField(StartPage, "Seed");

            RandomSeedButton = new SmallButton(StartPage, "Random");

            // The AdvancedSettingsPage Elements must be constructed before the StartPage preset buttons.
            poolMEF = new MenuElementFactory<PoolSettings>(AdvancedSettingsPage, Settings.PoolSettings);
            skipMEF = new MenuElementFactory<SkipSettings>(AdvancedSettingsPage, Settings.SkipSettings);
            novMEF = new MenuElementFactory<NoveltySettings>(AdvancedSettingsPage, Settings.NoveltySettings);
            costMEF = new MenuElementFactory<CostSettings>(AdvancedSettingsPage, Settings.CostSettings);
            longLocationMEF = new MenuElementFactory<LongLocationSettings>(AdvancedSettingsPage, Settings.LongLocationSettings);
            
            startLocationTypeSwitch = new MenuItem<StartLocationSettings.RandomizeStartLocationType>(AdvancedSettingsPage,
                nameof(StartLocationSettings.StartLocationType).FromCamelCase(),
                Enum.GetValues(typeof(StartLocationSettings.RandomizeStartLocationType)).Cast<StartLocationSettings.RandomizeStartLocationType>().ToArray());
            startLocationTypeSwitch.Format += (_, p, c, r) => (p, c, r.FromCamelCase());
            startLocationTypeSwitch.Bind(Settings.StartLocationSettings,
                typeof(StartLocationSettings).GetField(nameof(StartLocationSettings.StartLocationType)));
            randomFixedStartButton = new SmallButton(AdvancedSettingsPage, "Random Fixed Start");

            startLocationSwitch = new RadioSwitch(AdvancedSettingsPage, StartDefs.Select(def => def.name).ToArray());
            startItemMEF = new MenuElementFactory<StartItemSettings>(AdvancedSettingsPage, Settings.StartItemSettings);

            miscMEF = new MenuElementFactory<MiscSettings>(AdvancedSettingsPage, Settings.MiscSettings);
            cursedMEF = new MenuElementFactory<CursedSettings>(AdvancedSettingsPage, Settings.CursedSettings);
            transitionMEF = new MenuElementFactory<TransitionSettings>(AdvancedSettingsPage, Settings.TransitionSettings);

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

            //gameSettingsMEF = new MenuElementFactory<QoLSettings>(GameSettingsPage, GameSettings);

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
            //StartVIP = new VerticalItemPanel(StartPage, new Vector2(-480, 450), 125f, PresetButtons);
            StartGIP = new GridItemPanel(StartPage, new Vector2(0, 450), 2, 125, 960, true, PresetButtons);

            PoolSubpage = new Subpage(AdvancedSettingsPage, "Randomized Items");
            poolGIP = new GridItemPanel(AdvancedSettingsPage, new Vector2(0, 300), 4, 50f, 400f, false, poolMEF.Elements);
            PoolSubpage.Add(poolGIP);

            SkipSubpage = new Subpage(AdvancedSettingsPage, "Required Skips");
            skipVIP = new VerticalItemPanel(AdvancedSettingsPage, new Vector2(0, 300), 50f, false, skipMEF.Elements);
            SkipSubpage.Add(skipVIP);

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

            /*
            gameSettingsPanel = new GridItemPanel(GameSettingsPage, new Vector2(0, 300), 2, 80, 550, gameSettingsMEF.Elements);
            */

            CodeVIP = new VerticalItemPanel(ManageSettingsPage, new Vector2(-400, 300), 100, true, CodeElements);
            ProfileVIP = new VerticalItemPanel(ManageSettingsPage, new Vector2(400, 300), 100, true, ProfileElements);

            generationInfoVIP = new VerticalItemPanel(FinalPage, new Vector2(-400, 300), 50, true, InfoElements);
            HashVIP = new VerticalItemPanel(FinalPage, new Vector2(400, 300), 50, true, HashLabels);
            HashVIP.Hide();
        }

        private void AddEvents()
        {
            EntryButton.Button.AddHideAndShowEvent(ModePage, StartPage);

            SeedEntryField.Changed += (i) => Settings.Seed = i;

            GenerateButton.Button.AddHideAndShowEvent(StartPage, FinalPage);
            GenerateButton.Button.AddEvent(Randomize);

            RandomSeedButton.Button.AddEvent(() =>
            {
                SeedEntryField.InputValue = rng.Next(0, 1000000000);
            });

            StartLocationPreset.Changed += (self) => UpdateStartLocationPreset();

            JumpToJumpPageButton.Button.AddHideAndShowEvent(StartPage, JumpPage);
            JumpToGameSettingsButton.Button.AddHideAndShowEvent(StartPage, GameSettingsPage);

            ToggleCaptionsButton.SetSelection(true);
            ToggleCaptionsButton.Changed += (self) =>
            {
                foreach (SmallButton button in PresetButtons)
                {
                    if (button is IMenuPreset preset) preset.Label?.SetVisibleByAlpha(self.CurrentSelection);
                }
            };

            skipMEF.BoolFields[nameof(SkipSettings.MildSkips)].Changed += UpdateStartLocation;
            skipMEF.BoolFields[nameof(SkipSettings.MildSkips)].Changed += UpdateStartLocation;
            skipMEF.BoolFields[nameof(SkipSettings.ShadeSkips)].Changed += UpdateStartLocation;
            skipMEF.BoolFields[nameof(SkipSettings.AcidSkips)].Changed += UpdateStartLocation;
            skipMEF.BoolFields[nameof(SkipSettings.FireballSkips)].Changed += UpdateStartLocation;
            skipMEF.BoolFields[nameof(SkipSettings.SpikeTunnels)].Changed += UpdateStartLocation;
            skipMEF.BoolFields[nameof(SkipSettings.DarkRooms)].Changed += UpdateStartLocation;
            skipMEF.BoolFields[nameof(SkipSettings.SpicySkips)].Changed += UpdateStartLocation;

            transitionMEF.EnumFields[nameof(TransitionSettings.Mode)].Changed += UpdateStartLocation;

            novMEF.BoolFields[nameof(NoveltySettings.RandomizeSwim)].Changed += UpdateStartLocation;
            novMEF.BoolFields[nameof(NoveltySettings.RandomizeElevatorPass)].Changed += UpdateStartLocation;
            cursedMEF.BoolFields[nameof(CursedSettings.CursedMasks)].Changed += UpdateStartLocation;

            startLocationSwitch.Changed += Settings.StartLocationSettings.SetStartLocation;
            startLocationSwitch.Changed += (s) => UpdateStartLocationPreset();
            startLocationTypeSwitch.Changed += UpdateStartLocationSwitch;
            randomFixedStartButton.OnClick += () =>
            {
                startLocationTypeSwitch.SetSelection(StartLocationSettings.RandomizeStartLocationType.Fixed);
                startLocationSwitch.ChangeSelection(rng.NextWhere(startLocationSwitch.Elements, e => !e.Locked));
            };
            UpdateStartLocation();

            ToManageSettingsPageButton.Button.AddHideAndShowEvent(JumpPage, ManageSettingsPage);
            DefaultSettingsButton.Button.AddEvent(() => ApplySettingsToMenu(new GenerationSettings())); // Proper defaults please!


            GenerateCodeButton.Button.AddEvent(() =>
            {
                SettingsCodeField.InputValue = Settings.Serialize();
            });
            ApplyCodeButton.Button.AddEvent(() =>
            {
                if (GenerationSettings.Deserialize(SettingsCodeField.InputValue) is GenerationSettings gs)
                {
                    ApplySettingsToMenu(gs);
                }
            });

            ProfileSwitch.Changed += (self) =>
            {
                ProfileNameField.InputValue = self.CurrentSelection?.ToString() ?? string.Empty;
                if (self.CurrentSelection == null)
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
                    name = ProfileNameField.InputValue,
                    settings = Settings.Clone() as GenerationSettings
                };
                ProfileSwitch.OverwriteCurrent(mp);
            };
            SaveAsNewProfileButton.OnClick += () =>
            {
                MenuProfile mp = new MenuProfile
                {
                    name = ProfileNameField.InputValue,
                    settings = Settings.Clone() as GenerationSettings
                };
                ProfileSwitch.AddItem(mp);
                ProfileSwitch.SetSelection(mp);
            };
            DeleteProfileButton.OnClick += () =>
            {
                ProfileSwitch.RemoveCurrent();
            };
            ApplyProfileButton.OnClick += () =>
            {
                if (ProfileSwitch.CurrentSelection is MenuProfile mp)
                {
                    ApplySettingsToMenu(mp.settings);
                }
            };

            StartButton.Button.AddSetResumeKeyEvent("Randomizer");
            StartButton.Button.AddEvent(StartRandomizerGame);
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

        private void UpdateStartLocationSwitch(MenuItem<StartLocationSettings.RandomizeStartLocationType> startLocationTypeSwitch)
        {
            UpdateStartLocationSwitch(
                startLocationTypeSwitch.CurrentSelection,
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
                && StartLocationPresetData.StartLocationPresets.TryGetValue(StartLocationPreset.CurrentSelection, out StartLocationSettings preset)
                && Settings.StartLocationSettings.StartLocation != preset.StartLocation)
            {
                StartLocationPreset.SetSelection("Custom");
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
            poolMEF.SetMenuValues(settings.PoolSettings, Settings.PoolSettings);
            skipMEF.SetMenuValues(settings.SkipSettings, Settings.SkipSettings);
            costMEF.SetMenuValues(settings.CostSettings, Settings.CostSettings);
            longLocationMEF.SetMenuValues(settings.LongLocationSettings, Settings.LongLocationSettings);
            startLocationTypeSwitch.SetSelection(settings.StartLocationSettings.StartLocationType);
            if (settings.StartLocationSettings.StartLocationType == StartLocationSettings.RandomizeStartLocationType.Fixed)
            {
                startLocationSwitch.TrySelect(settings.StartLocationSettings.StartLocation);
            }
            startItemMEF.SetMenuValues(settings.StartItemSettings, Settings.StartItemSettings);
            miscMEF.SetMenuValues(settings.MiscSettings, Settings.MiscSettings);
            cursedMEF.SetMenuValues(settings.CursedSettings, Settings.CursedSettings);
            transitionMEF.SetMenuValues(settings.TransitionSettings, Settings.TransitionSettings);

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

                        StartButton.Show();
                        FinalPage.backButton.SetNeighbor(Neighbor.Up, StartButton);
                        StartButton.SetNeighbor(Neighbor.Down, FinalPage.backButton);
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
            return pm.Evaluate(def.logic);
        }
    }
}

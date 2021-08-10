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

namespace RandomizerMod.Menu
{
    public class RandomizerMenuConstructor : ModeMenuConstructor
    {
        private RandomizerMenu menu;

        public override BigButton GetModeButton(MenuPage modeMenu)
        {
            return menu.EntryButton;
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

        GameSettings GameSettings = new GameSettings();

        readonly MenuPage ModePage;
        MenuPage ResumePage;
        MiniPM pm = new MiniPM();

        #region Start

        MenuPage StartPage;

        BigButton GenerateButton;
        IntEntryField SeedEntryField;
        SmallButton RandomSeedButton;


        MenuPreset<PoolSettings> PoolPreset;
        MenuPreset<SkipSettings> SkipPreset;
        MenuPreset<GrubCostRandomizerSettings> GrubCostPreset;
        MenuPreset<EssenceCostRandomizerSettings> EssenceCostPreset;
        MenuPreset<StartItemSettings> StartItemPreset;
        MenuPreset<StartLocationSettings> StartLocationPreset;
        MenuPreset<LongLocationSettings> LongLocationPreset;
        MenuPreset<CursedSettings> CursedPreset;
        MenuPreset<TransitionSettings> TransitionPreset;

        SmallButton[] PresetButtons => new SmallButton[]
        {
            PoolPreset,
            SkipPreset,
            GrubCostPreset,
            EssenceCostPreset,
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

        Subpage CostSubpage;
        MenuElementFactory<GrubCostRandomizerSettings> grubCostMEF;
        MenuElementFactory<EssenceCostRandomizerSettings> essenceCostMEF;
        GridItemPanel costGIP;

        Subpage LongLocationSubpage;
        MenuElementFactory<LongLocationSettings> longLocationMEF;
        VerticalItemPanel longVIP;

        Subpage StartLocationSubpage;
        MenuItem<StartLocationSettings.RandomizeStartLocationType> startLocationTypeSwitch;
        RadioSwitch startLocationSwitch;
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
        MenuElementFactory<GameSettings> gameSettingsMEF;
        MenuLabel preloadExplanationLabel;

        GridItemPanel gameSettingsPanel;
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

            MakeMenuPages();
            Log("Made Randomizer menu pages");
            MakeMenuElements();
            Log("Made Randomizer menu elements");
            MakePanels();
            Log("Made Randomizer menu panels");
            AddEvents();
            AddMiniPMEvents();
            Log("Made Randomizer menu events");
            Arrange();
            Log("Arranged Randomizer menu");

            ResumeMenu.AddResumePage("Randomizer", ResumePage);
            SeedEntryField.InputValue = new System.Random().Next(0, 999999999);
            ApplySettingsToMenu(Settings);
            Log("Constructed Randomizer menu successfully");
        }

        private void MakeMenuPages()
        {
            StartPage = new MenuPage("Randomizer Setting Select Page", ModePage);
            JumpPage = new MenuPage("Randomizer Jump To Advanced Settings Page", StartPage);
            AdvancedSettingsPage = new MenuPage("Randomizer Advanced Settings Page", JumpPage);
            ManageSettingsPage = new MenuPage("Randomizer Manage Settings Page", JumpPage);
            GameSettingsPage = new MenuPage("Randomizer Game Settings Page", StartPage);
            FinalPage = new MenuPage("Randomizer Final Page", StartPage);
            FinalPage.backButton.transform.Find("Text").GetComponent<Text>().text = "Abort";
            FinalPage.backButton.AddEvent(Abort);

            ResumePage = new MenuPage("Randomizer Resume Page");
        }

        private void MakeMenuElements()
        {
            Log("Mode Page? " + (ModePage != null));
            EntryButton = new BigButton(ModePage, MenuChanger.SpriteManager.GetSprite("logo"), "Randomizer");
            Log("Made EntryButton");
            // Start Page

            JumpToJumpPageButton = new SmallButton(StartPage, "More Randomizer Settings");
            JumpToGameSettingsButton = new SmallButton(StartPage, "Game Settings");
            ToggleCaptionsButton = new ToggleButton(StartPage, "Toggle Menu Captions");

            GenerateButton = new BigButton(StartPage, "Begin Randomization"); // TODO: constructor for default sprite, custom text
            SeedEntryField = new IntEntryField(StartPage, "Seed");

            RandomSeedButton = new SmallButton(StartPage, "Random");

            // The AdvancedSettingsPage Elements must be constructed before the StartPage preset buttons.
            poolMEF = new MenuElementFactory<PoolSettings>(AdvancedSettingsPage, Settings.PoolSettings);
            skipMEF = new MenuElementFactory<SkipSettings>(AdvancedSettingsPage, Settings.SkipSettings);
            grubCostMEF = new MenuElementFactory<GrubCostRandomizerSettings>(AdvancedSettingsPage, Settings.GrubCostRandomizerSettings);
            essenceCostMEF = new MenuElementFactory<EssenceCostRandomizerSettings>(AdvancedSettingsPage, Settings.EssenceCostRandomizerSettings);
            longLocationMEF = new MenuElementFactory<LongLocationSettings>(AdvancedSettingsPage, Settings.LongLocationSettings);
            
            startLocationTypeSwitch = new MenuItem<StartLocationSettings.RandomizeStartLocationType>(AdvancedSettingsPage,
                nameof(StartLocationSettings.StartLocationType).FromCamelCase(),
                Enum.GetValues(typeof(StartLocationSettings.RandomizeStartLocationType)).Cast<StartLocationSettings.RandomizeStartLocationType>().ToArray());
            startLocationTypeSwitch.Format += (_, p, c, r) => (p, c, r.FromCamelCase());
            startLocationTypeSwitch.Bind(Settings.StartLocationSettings,
                typeof(StartLocationSettings).GetField(nameof(StartLocationSettings.StartLocationType)));

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
            GrubCostPreset = new MenuPreset<GrubCostRandomizerSettings>(StartPage, "Grub Cost Randomization",
                GrubCostPresetData.GrubCostPresets, Settings.GrubCostRandomizerSettings,
                gc => gc.Caption(),
                grubCostMEF);
            EssenceCostPreset = new MenuPreset<EssenceCostRandomizerSettings>(StartPage, "Essence Cost Randomization",
                EssenceCostPresetData.EssencePresets, Settings.EssenceCostRandomizerSettings,
                ec => ec.Caption(),
                essenceCostMEF);
            StartItemPreset = new MenuPreset<StartItemSettings>(StartPage, "Start Items",
                StartItemPresetData.StartItemPresets, Settings.StartItemSettings, si => si.Caption(), startItemMEF);
            StartLocationPreset = new MenuPreset<StartLocationSettings>(StartPage, "Start Location",
                StartLocationPresetData.StartLocationPresets, Settings.StartLocationSettings, sl => sl.Caption());
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

            gameSettingsMEF = new MenuElementFactory<GameSettings>(GameSettingsPage, GameSettings);

            // Final Page
            InfoPanelTitle = new MenuLabel(FinalPage, "Randomizer Progress");
            AttemptCounter = new CounterLabel(FinalPage, "Attempts");
            RandomizationTimer = new TimerLabel(FinalPage, "Time Elapsed");
            OutputLabel = new MenuLabel(FinalPage, "", new Vector2(800, 800));
            OutputLabel.Hide();

            StartButton = new BigButton(FinalPage, MenuChanger.SpriteManager.GetSprite("logo"), "Start Game");
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
            StartGIP = new GridItemPanel(StartPage, new Vector2(0, 450), 2, 125, 960, PresetButtons);

            PoolSubpage = new Subpage(AdvancedSettingsPage, "Randomized Items");
            poolGIP = new GridItemPanel(AdvancedSettingsPage, new Vector2(0, 300), 4, 50f, 400f, poolMEF.Elements);
            PoolSubpage.Add(poolGIP);

            SkipSubpage = new Subpage(AdvancedSettingsPage, "Required Skips");
            skipVIP = new VerticalItemPanel(AdvancedSettingsPage, new Vector2(0, 300), 50f, skipMEF.Elements);
            SkipSubpage.Add(skipVIP);

            CostSubpage = new Subpage(AdvancedSettingsPage, "Cost Randomization");
            costGIP = new GridItemPanel(AdvancedSettingsPage, new Vector2(0, 300), 4, 200f, 400f, grubCostMEF.Elements.Concat(essenceCostMEF.Elements).ToArray());
            CostSubpage.Add(costGIP);

            LongLocationSubpage = new Subpage(AdvancedSettingsPage, "Long Location Options");
            longVIP = new VerticalItemPanel(AdvancedSettingsPage, new Vector2(0, 300), 50f, longLocationMEF.Elements);
            LongLocationSubpage.Add(longVIP);

            StartLocationSubpage = new Subpage(AdvancedSettingsPage, "Start Location");
            startLocationTypeSwitch.MoveTo(new Vector2(0, 300));
            StartLocationSubpage.Add(startLocationTypeSwitch);
            startLocationGIP = new GridItemPanel(AdvancedSettingsPage, new Vector2(0, 150), 3, 50f, 600f, startLocationSwitch.Elements);
            StartLocationSubpage.Add(startLocationGIP);

            StartItemSubpage = new Subpage(AdvancedSettingsPage, "Start Items");
            startItemGIP = new GridItemPanel(AdvancedSettingsPage, new Vector2(0, 300), 2, 150f, 800f, startItemMEF.Elements);
            StartItemSubpage.Add(startItemGIP);

            MiscSubpage = new Subpage(AdvancedSettingsPage, "Miscellaneous");
            miscVIP = new VerticalItemPanel(AdvancedSettingsPage, new Vector2(0, 300), 50f, miscMEF.Elements);
            MiscSubpage.Add(miscVIP);

            CursedSubpage = new Subpage(AdvancedSettingsPage, "Curse Options");
            cursedVIP = new VerticalItemPanel(AdvancedSettingsPage, new Vector2(0, 300), 50f, cursedMEF.Elements);
            CursedSubpage.Add(cursedVIP);

            TransitionSubpage = new Subpage(AdvancedSettingsPage, "Transition Randomizer");
            transitionPanel = new VerticalItemPanel(AdvancedSettingsPage, new Vector2(0, 300), 50f, transitionMEF.Elements);
            TransitionSubpage.Add(transitionPanel);

            AdvancedSettingsViewer = new OrderedItemViewer(AdvancedSettingsPage, AdvancedSettingsSubpages);

            JumpButtons = AdvancedSettingsSubpages.Select(p =>
            {
                SmallButton b = new SmallButton(JumpPage, p.TitleLabel.Text.text);
                b.Button.AddHideAndShowEvent(JumpPage, AdvancedSettingsPage);
                b.Button.AddEvent(() => AdvancedSettingsViewer.JumpTo(p));
                return b;
            }).ToArray();
            JumpPanel = new GridItemPanel(JumpPage, new Vector2(0, 300), 2, 60f, 800f, JumpButtons);

            StartCornerVIP = new VerticalItemPanel(StartPage, new Vector2(-650, -350), 50f, StartCornerButtons);

            preloadExplanationLabel = new MenuLabel(GameSettingsPage,
                "Disabling preloads allows the game to use less memory, and may help to prevent glitches such as " +
                "infinite loads or invisible bosses. Changes to this setting apply to all files, " +
                "but only take effect after restarting the game.",
                MenuLabel.Style.Body);
            preloadExplanationLabel.MoveTo(new Vector2(0, -200));
            gameSettingsPanel = new GridItemPanel(GameSettingsPage, new Vector2(0, 300), 2, 80, 550, gameSettingsMEF.Elements);


            CodeVIP = new VerticalItemPanel(ManageSettingsPage, new Vector2(-400, 300), 100, CodeElements);
            ProfileVIP = new VerticalItemPanel(ManageSettingsPage, new Vector2(400, 300), 100, ProfileElements);

            generationInfoVIP = new VerticalItemPanel(FinalPage, new Vector2(-400, 300), 50, InfoElements);
            HashVIP = new VerticalItemPanel(FinalPage, new Vector2(400, 300), 50, HashLabels);
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
                SeedEntryField.InputValue = new System.Random().Next(0, 1000000000);
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

            // terrible interaction code
            grubCostMEF.IntFields[nameof(GrubCostRandomizerSettings.MinimumGrubCost)].SetClamp(0, 46);
            grubCostMEF.IntFields[nameof(GrubCostRandomizerSettings.MaximumGrubCost)].SetClamp(0, 46);
            grubCostMEF.IntFields[nameof(GrubCostRandomizerSettings.GrubTolerance)].SetClamp(-46, 46);
            grubCostMEF.IntFields[nameof(GrubCostRandomizerSettings.MinimumGrubCost)].Modify +=
                (s) => Math.Min(s, Settings.GrubCostRandomizerSettings.MaximumGrubCost);
            grubCostMEF.IntFields[nameof(GrubCostRandomizerSettings.MaximumGrubCost)].Modify +=
                (s) => Math.Max(s, Settings.GrubCostRandomizerSettings.MinimumGrubCost);
            grubCostMEF.IntFields[nameof(GrubCostRandomizerSettings.GrubTolerance)].Modify +=
                (s) => Math.Min(s, 46 - Settings.GrubCostRandomizerSettings.MaximumGrubCost);
            grubCostMEF.IntFields[nameof(GrubCostRandomizerSettings.MaximumGrubCost)].Changed +=
                (s) =>
                {
                    if (46 - Settings.GrubCostRandomizerSettings.GrubTolerance < s)
                    {
                        grubCostMEF.IntFields[nameof(GrubCostRandomizerSettings.GrubTolerance)].InputValue = 46 - s;
                    }
                };

            essenceCostMEF.IntFields[nameof(EssenceCostRandomizerSettings.MinimumEssenceCost)].SetClamp(0, 3100);
            essenceCostMEF.IntFields[nameof(EssenceCostRandomizerSettings.MaximumEssenceCost)].SetClamp(0, 3100);
            essenceCostMEF.IntFields[nameof(EssenceCostRandomizerSettings.EssenceTolerance)].SetClamp(-3100, 3100);
            essenceCostMEF.IntFields[nameof(EssenceCostRandomizerSettings.MinimumEssenceCost)].Modify +=
                (s) => Math.Min(s, Settings.EssenceCostRandomizerSettings.MaximumEssenceCost);
            essenceCostMEF.IntFields[nameof(EssenceCostRandomizerSettings.MaximumEssenceCost)].Modify +=
                (s) => Math.Max(s, Settings.EssenceCostRandomizerSettings.MinimumEssenceCost);
            essenceCostMEF.IntFields[nameof(EssenceCostRandomizerSettings.EssenceTolerance)].Modify +=
                (s) => Math.Min(s, 3100 - Settings.EssenceCostRandomizerSettings.MaximumEssenceCost);
            essenceCostMEF.IntFields[nameof(EssenceCostRandomizerSettings.MaximumEssenceCost)].Changed +=
                (s) =>
                {
                    if (3100 - Settings.EssenceCostRandomizerSettings.EssenceTolerance < s)
                    {
                        essenceCostMEF.IntFields[nameof(EssenceCostRandomizerSettings.EssenceTolerance)].InputValue = 3100 - s;
                    }
                };

            startItemMEF.IntFields[nameof(StartItemSettings.MinimumStartGeo)].Modify +=
                (s) => Math.Min(s, Settings.StartItemSettings.MinimumStartGeo);
            startItemMEF.IntFields[nameof(StartItemSettings.MaximumStartGeo)].Modify +=
                (s) => Math.Max(s, Settings.StartItemSettings.MaximumStartGeo);

            startLocationSwitch.Changed += Settings.StartLocationSettings.SetStartLocation;
            startLocationSwitch.Changed += (s) => UpdateStartLocationPreset();
            startLocationTypeSwitch.Changed += UpdateStartLocationSwitch;
            UpdateStartLocationSwitch();

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

            StartButton.Button.AddHideAllMenuPagesEvent();
            StartButton.Button.AddSetResumeKeyEvent("Randomizer");
            StartButton.Button.AddEvent(() => GameManager.instance.ContinueGame());
        }

        private void AddMiniPMEvents()
        {
            skipMEF.BoolFields[nameof(SkipSettings.MildSkips)].Changed += b => pm.SetBool("MILDSKIPS", b.CurrentSelection);
            skipMEF.BoolFields[nameof(SkipSettings.ShadeSkips)].Changed += b => pm.SetBool("SHADESKIPS", b.CurrentSelection);
            skipMEF.BoolFields[nameof(SkipSettings.AcidSkips)].Changed += b => pm.SetBool("ACIDSKIPS", b.CurrentSelection);
            skipMEF.BoolFields[nameof(SkipSettings.FireballSkips)].Changed += b => pm.SetBool("FIREBALLSKIPS", b.CurrentSelection);
            skipMEF.BoolFields[nameof(SkipSettings.SpikeTunnels)].Changed += b => pm.SetBool("SPIKETUNNELS", b.CurrentSelection);
            skipMEF.BoolFields[nameof(SkipSettings.DarkRooms)].Changed += b => pm.SetBool("DARKROOMS", b.CurrentSelection);
            skipMEF.BoolFields[nameof(SkipSettings.SpicySkips)].Changed += b => pm.SetBool("SPICYSKIPS", b.CurrentSelection);

            transitionMEF.EnumFields[nameof(TransitionSettings.Mode)].Changed += b =>
            {
                pm.SetBool("ITEMRANDO", Equals(b.CurrentSelection, TransitionSettings.TransitionMode.None));
                pm.SetBool("AREARANDO", Equals(b.CurrentSelection, TransitionSettings.TransitionMode.AreaRandomizer));
                pm.SetBool("ROOMRANDO", Equals(b.CurrentSelection, TransitionSettings.TransitionMode.RoomRandomizer));
            };

            cursedMEF.BoolFields[nameof(CursedSettings.RandomizeSwim)].Changed += b => pm.SetBool("SWIM", !b.CurrentSelection);
            cursedMEF.BoolFields[nameof(CursedSettings.CursedMasks)].Changed += b => pm.SetBool("2MASKS", !b.CurrentSelection);

            pm.SetBool("VERTICAL", false);

        }

        private void Arrange()
        {
            GenerateButton.MoveTo(new Vector2(0, -350));
            SeedEntryField.MoveTo(new Vector2(650, -350));
            RandomSeedButton.MoveTo(new Vector2(650, -400));

            DefaultSettingsButton.MoveTo(new Vector2(-400, -300));
            ToManageSettingsPageButton.MoveTo(new Vector2(400, -300));

            StartButton.MoveTo(new Vector2(0, -200));
        }

        private void UpdateStartLocationSwitch(StartLocationSettings.RandomizeStartLocationType type, string loc)
        {
            Log("UpdateStartLocationSwitch called");
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

        private void ApplySettingsToMenu(GenerationSettings settings)
        {
            poolMEF.SetMenuValues(settings.PoolSettings, Settings.PoolSettings);
            skipMEF.SetMenuValues(settings.SkipSettings, Settings.SkipSettings);
            grubCostMEF.SetMenuValues(settings.GrubCostRandomizerSettings, Settings.GrubCostRandomizerSettings);
            essenceCostMEF.SetMenuValues(settings.EssenceCostRandomizerSettings, Settings.EssenceCostRandomizerSettings);
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
        private void Randomize()
        {
            AttemptCounter.Set(0);
            RandomizationTimer.Reset();
            RandomizationTimer.Start();

            RandomizerThread = new Thread(() =>
            {
                try
                {
                    Logic.LogicManager lm = RandomizerMod.ItemLogicManager;
                    var items = Data.GetRandomizedItems(Settings, lm);
                    var locations = Data.GetRandomizedLocations(Settings, lm).Select(ld => new Logic.RandoLocation { logic = ld, multi = Data.GetLocationDef(ld.name).multi }).ToArray();
                    Data.PoolTest(Settings, lm);
                    Logic.Randomizer r = new Logic.Randomizer(items, locations, lm, Settings);

                    ThreadSupport.BeginInvoke(() => AttemptCounter.Incr());
                    r.Randomize(Settings.Seed);

                    ThreadSupport.BeginInvoke(() =>
                    {
                        RandomizationTimer.Stop();
                        OutputLabel.Text.color = Color.white;
                        OutputLabel.Text.text = "Randomization completed successfully!";
                        OutputLabel.Text.alignment = TextAnchor.UpperCenter;
                        OutputLabel.Show();

                        int hashSeed = 0;
                        unchecked
                        {
                            foreach (var (item, location) in r.placements)
                            {
                                hashSeed += item.name.GetStableHashCode() * location.name.GetStableHashCode();
                            }
                        }
                        string[] hash = Hash.GetHash(hashSeed);
                        for (int i = 0; i < Hash.Length; i++)
                        {
                            HashLabels[1 + i].Text.text = hash[i];
                        }
                        HashVIP.Show();

                        StartButton.Show();
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
                    });
                }
            });
            RandomizerThread.Start();
        }

        private void Abort()
        {
            RandomizerThread?.Abort();
            RandomizerThread?.Join();
            OutputLabel.Hide();
            HashVIP.Hide();
            StartButton.Hide();
        }

        private bool CanSelectStart(string name)
        {
            StartDef def = Data.GetStartDef(name);
            return pm.Evaluate(def.logic);
        }
    }
}

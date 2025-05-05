using ItemChanger;
using ItemChanger.Modules;

namespace RandomizerMod.IC
{
    /* From LOGIC_README.md:
    - With skips allowed, the player is advised to take care to not get locked out of certain required pogos. Obtain:
    - No more than 1 nail upgrade before claw or wings
    - No more than 3 nail upgrades before claw
    - In Split Claw mode, instead obtain no more than 2 nail upgrades with only a single claw piece, and any number of nail upgrades with a single claw piece as well as wings
    */

    public class NailUpgradeWarningModule : Module
    {
        public override void Initialize()
        {
            ToggleLanguageHook(true);
        }

        public override void Unload()
        {
            ToggleLanguageHook(false);
        }

        private void ToggleLanguageHook(bool toggle)
        {
            if (toggle)
            {
                Events.AddLanguageEdit(new("Nailsmith", "NAILSMITH_OFFER_ORE"), NailUpgradeWarning);
            }
            else
            {
                Events.RemoveLanguageEdit(new("Nailsmith", "NAILSMITH_OFFER_ORE"), NailUpgradeWarning);
            }
        }

        private static void NailUpgradeWarning(ref string s)
        {
            bool claw = PlayerData.instance.GetBool(nameof(PlayerData.hasWalljump));
            bool wings = PlayerData.instance.GetBool(nameof(PlayerData.hasDoubleJump));
            SplitClaw? scm = ItemChangerMod.Modules.Get<SplitClaw>();
            int level = PlayerData.instance.GetInt(nameof(PlayerData.nailSmithUpgrades));
            const int repetitions = 10;

            switch (level + 1)
            {
                case >= 2 when !(claw || wings || scm is not null && scm.hasWalljumpAny):
                    CreateMessage(
                    Localize("WARNING -- obtaining more than one nail upgrade before collecting one of Mantis Claw or Monarch Wings may lock out required enemy pogos!"),
                    repetitions, ref s);
                    break;
                case >= 3 when scm is not null && !(scm.hasWalljumpAny && wings || scm.hasWalljumpBoth):
                    CreateMessage(
                    Localize("WARNING -- obtaining more than two nail upgrades before collecting two out of three of Left Mantis Claw, Right Mantis Claw, and Monarch Wings may lock out required enemy pogos!"),
                    repetitions, ref s);
                    break;
                case >= 4 when !(claw && wings):
                    CreateMessage(
                    Localize("WARNING -- obtaining more than three nail upgrades before collecting both Mantis Claw and Monarch Wings may lock out required enemy pogos!"),
                    repetitions, ref s);
                    break;
            }
        }

        private static void CreateMessage(string warning, int times, ref string result)
        {
            result = string.Empty;
            for (int i = 0; i < times - 1; i++)
            {
                result += warning + $" ({i + 1}/{times})" + "<page>";
            }
            result += warning + $" ({times}/{times})";
        }

    }
}

using MenuChanger.MenuElements;
using MenuChanger.MenuPanels;
using Newtonsoft.Json;
using RandomizerMod.RandomizerData;
using UnityEngine.UI;

namespace RandomizerMod
{
    public static class LocalizationData
    {
        public static Dictionary<string, string> textLookup;

        public static void Load()
        {
            try
            {
                string path = Path.Combine(RandomizerMod.Folder, "language.json");
                if (File.Exists(path))
                {
                    using FileStream fs = File.OpenRead(path);
                    using StreamReader sr = new(fs);
                    JsonTextReader jtr = new(sr);
                    textLookup = JsonUtil._js.Deserialize<Dictionary<string, string>>(jtr);
                }
            }
            catch (Exception e)
            {
                LogError(e);
            }

            textLookup ??= new();
        }
    }

    public class LocalizedMenuItemFormatter : MenuItemFormatter
    {
        readonly MenuItemFormatter orig;

        public LocalizedMenuItemFormatter(MenuItemFormatter orig)
        {
            this.orig = orig;
        }

        public override string GetText(string prefix, object value)
        {
            return Localize(orig.GetText(prefix, value));
        }
    }

    public static class Localization
    {
        public static string Localize(string value)
        {
            if (value != null)
            {
                if (LocalizationData.textLookup.TryGetValue(value, out string result))
                {
                    return result;
                }
                else
                {
                    LocalizationData.textLookup[value] = value;
                }
            }
            return value;
        }

        public static void Localize(Text t)
        {
            string text = t.text;
            if (string.IsNullOrWhiteSpace(text)) return;
            bool newLineTerminated = text[text.Length - 1] == '\n';
            if (newLineTerminated) text = text.Substring(0, text.Length - 1);
            text = Localize(text);
            if (newLineTerminated) text += '\n';
            t.text = text;
        }

        public static void Localize(SmallButton sb)
        {
            if (sb is MenuItem mi) mi.Formatter = new LocalizedMenuItemFormatter(mi.Formatter);
            Localize(sb.Text);
        }

        public static void Localize(BigButton bb)
        {
            Text title = bb.Button.transform.Find("Text").GetComponent<Text>();
            Localize(title);
            Text desc = bb.Button.transform.Find("DescriptionText").GetComponent<Text>();
            Localize(desc);
        }

        public static void Localize(EntryField ef)
        {
            Localize(ef.Label.Text);
        }

        public static void Localize(RadioSwitch rs)
        {
            foreach (MenuItem mi in rs.Elements) Localize(mi);
        }

        public static void Localize(Subpage sp)
        {
            Localize(sp.TitleLabel);
        }

        public static void Localize(MenuLabel ml)
        {
            Localize(ml.Text);
        }

        public static void Localize<T>(MenuElementFactory<T> mef)
        {
            foreach (IMenuElement e in mef.Elements)
            {
                if (e is MenuItem mi) Localize(mi);
                else if (e is EntryField ef) Localize(ef);
            }
        } 
    }
}

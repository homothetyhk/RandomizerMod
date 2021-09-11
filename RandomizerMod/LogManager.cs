using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using RandomizerCore;
using RandomizerMod.Settings;
using RandomizerMod.Logging;
using static RandomizerMod.LogHelper;

namespace RandomizerMod
{
    public abstract class RandoLogger
    {
        public abstract void Log(string directory, LogArguments args);
        internal void DoLog(string directory, LogArguments args)
        {
            try
            {
                Log(directory, args);
            }
            catch (Exception)
            {
                
            }
        }
    }

    public class LogArguments
    {
        public object randomizer { get; init; }
        public GenerationSettings gs { get; init; }
        public RandoContext ctx { get; init; }
    }

    public class LogManager
    {
        public readonly string directory;
        public static readonly string RecentDirectory = Path.Combine(Application.persistentDataPath, "Randomizer 4", "Recent");
        private static readonly List<RandoLogger> loggers = new()
        {
            new ItemSpoilerLog(),
            new TransitionSpoilerLog(),
        };

        internal LogManager(int saveSlot)
        {
            directory = Path.Combine(Application.persistentDataPath, "Randomizer 4", "user" + saveSlot);
        }

        internal void WriteLogs(LogArguments args)
        {
            DirectoryInfo di;
            try
            {
                Directory.CreateDirectory(directory);
                di = new(directory);
            }
            catch (Exception e)
            {
                Log($"Error creating logging directory:\n{e}");
                return;
            }

            try
            {
                foreach (FileInfo fi in di.EnumerateFiles())
                {
                    fi.Delete();
                }
            }
            catch (Exception e)
            {
                Log($"Error clearing logging directory:\n{e}");
            }

            loggers.AsParallel().ForAll(l => l.DoLog(directory, args));

            DirectoryInfo rdi;
            try
            {
                Directory.CreateDirectory(RecentDirectory);
                rdi = new(RecentDirectory);
            }
            catch (Exception e)
            {
                Log($"Error creating recent directory:\n{e}");
                return;
            }

            try
            {
                foreach (FileInfo fi in rdi.EnumerateFiles())
                {
                    fi.Delete();
                }
            }
            catch (Exception e)
            {
                Log($"Error clearing recent directory:\n{e}");
            }

            di.Refresh();
            foreach (FileInfo fi in di.EnumerateFiles())
            {
                try
                {
                    fi.CopyTo(Path.Combine(RecentDirectory, fi.Name), true);
                }
                catch (Exception e)
                {
                    Log($"Error copying file {fi.Name} to recent directory:\n{e}");
                }
            }
        }

    }
}

using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;

namespace RandomizerMod.Logging
{
    public static class LogManager
    {
        public static readonly string R4Directory = Path.Combine(Application.persistentDataPath, "Randomizer 4");
        public static readonly string RecentDirectory = Path.Combine(Application.persistentDataPath, "Randomizer 4", "Recent");
        public static string UserDirectory => Path.Combine(Application.persistentDataPath, "Randomizer 4", "user" + RandomizerMod.RS.ProfileID);

        /// <summary>
        /// Loggers which are activated when the save is created.
        /// </summary>
        private static readonly List<RandoLogger> loggers = new()
        {
            new ItemSpoilerLog(),
            new TransitionSpoilerLog(),
        };

        public static void AddLogger(RandoLogger rl)
        {
            loggers.Add(rl);
        }

        public static void RemoveLogger(RandoLogger rl)
        {
            loggers.Remove(rl);
        }

        internal static void Initialize()
        {
            logRequestConsumer = new Thread(() =>
            {
                foreach (Action a in logRequests.GetConsumingEnumerable())
                {
                    try
                    {
                        a?.Invoke();
                    }
                    catch (Exception e)
                    {
                        LogError($"Error in log request:\n{e}");
                    }
                }
            })
            { IsBackground = true, Priority = System.Threading.ThreadPriority.Lowest };
            logRequestConsumer.Start();

            Modding.ModHooks.ApplicationQuitHook += CloseLogRequests;
        }

        private static void CloseLogRequests()
        {
            try
            {
                logRequests.CompleteAdding();
                logRequestConsumer.Join();
                logRequests.Dispose();
            }
            catch (Exception e)
            {
                LogError($"Error disposing LogManager:\n{e}");
            }
        }

        private static readonly BlockingCollection<Action> logRequests = new();
        private static Thread logRequestConsumer;

        public static void Write(string contents, string fileName)
        {
            void WriteLog()
            {
                try
                {
                    string userPath = Path.Combine(UserDirectory, fileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(userPath));
                    File.WriteAllText(userPath, contents);
                }
                catch (Exception e)
                {
                    LogError($"Error printing log request to {UserDirectory}:\n{e}");
                }
                try
                {
                    string recentPath = Path.Combine(RecentDirectory, fileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(recentPath));
                    File.WriteAllText(recentPath, contents);
                }
                catch (Exception e)
                {
                    LogError($"Error printing log request to {RecentDirectory}:\n{e}");
                }
            }

            logRequests.Add(WriteLog);
        }

        public static void Write(Action<TextWriter> a, string fileName)
        {
            void WriteLog()
            {
                try
                {
                    string userPath = Path.Combine(UserDirectory, fileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(userPath));
                    using FileStream fs = File.OpenWrite(userPath);
                    using StreamWriter sr = new(fs);
                    a?.Invoke(sr);

                    string recentPath = Path.Combine(RecentDirectory, fileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(recentPath));
                    File.Copy(userPath, recentPath, true);
                }
                catch (Exception e)
                {
                    LogError($"Error printing log request for {fileName}:\n{e}");
                }
            }

            logRequests.Add(WriteLog);
        }

        public static void Append(string contents, string fileName)
        {
            void AppendLog()
            {
                try
                {
                    string userPath = Path.Combine(UserDirectory, fileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(userPath));
                    File.AppendAllText(userPath, contents);
                }
                catch (Exception e)
                {
                    LogError($"Error appending log request to {UserDirectory}:\n{e}");
                }
                try
                {
                    string recentPath = Path.Combine(RecentDirectory, fileName);
                    Directory.CreateDirectory(Path.GetDirectoryName(recentPath));
                    File.AppendAllText(recentPath, contents);
                }
                catch (Exception e)
                {
                    LogError($"Error appending log request to {RecentDirectory}:\n{e}");
                }
            }

            logRequests.Add(AppendLog);
        }

        internal static void WriteLogs(LogArguments args)
        {
            DirectoryInfo userDI;
            try
            {
                userDI = Directory.CreateDirectory(UserDirectory);
                foreach (FileInfo fi in userDI.EnumerateFiles())
                {
                    fi.Delete();
                }
            }
            catch (Exception e)
            {
                Log($"Error initializing user logging directory:\n{e}");
                return;
            }

            DirectoryInfo recentDI;
            try
            {
                recentDI = Directory.CreateDirectory(RecentDirectory);
                foreach (FileInfo fi in recentDI.EnumerateFiles())
                {
                    fi.Delete();
                }
            }
            catch (Exception e)
            {
                Log($"Error initializing recent logging directory:\n{e}");
                return;
            }

            System.Diagnostics.Stopwatch sw = new();
            sw.Start();
            foreach (var rl in loggers) logRequests.Add(() => rl.DoLog(args));
            logRequests.Add(() =>
            {
                sw.Stop();
                Log($"Printed new game logs in {sw.Elapsed.TotalSeconds} seconds.");
            });
        }

        internal static void UpdateRecent(int profileID)
        {
            void MoveFiles()
            {
                try
                {
                    DirectoryInfo recentDI = Directory.CreateDirectory(RecentDirectory);
                    DirectoryInfo userDI = Directory.CreateDirectory(Path.Combine(R4Directory, "user" + profileID));
                    foreach (FileInfo fi in recentDI.EnumerateFiles())
                    {
                        fi.Delete();
                    }
                    foreach (FileInfo fi in userDI.EnumerateFiles())
                    {
                        fi.CopyTo(Path.Combine(recentDI.FullName, fi.Name), true);
                    }
                }
                catch (Exception e)
                {
                    LogError($"Error overwriting recent log directory:\n{e}");
                }
            }

            logRequests.Add(MoveFiles);
        }
    }
}

namespace RandomizerMod.Logging
{
    public abstract class RandoLogger
    {
        public abstract void Log(LogArguments args);
        internal void DoLog(LogArguments args)
        {
            try
            {
                Log(args);
            }
            catch (Exception e)
            {
                LogError($"Error in RandoLogger {GetType().Name}:\n{e}");
            }
        }
    }
}

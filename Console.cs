namespace BackupService
{
    public static class Console
    {
        public static void WriteLineInColor(string line, System.ConsoleColor color)
        {
            System.ConsoleColor originalColor = System.Console.ForegroundColor;
            System.Console.ForegroundColor = color;
            System.Console.WriteLine(line);
            System.Console.ForegroundColor = originalColor;
        }
        public static void WriteLine(string line)
        {
            WriteLineInColor(line, System.ConsoleColor.White);
        }
        public static void WriteLine()
        {
            WriteLine("");
        }
        public static void WriteRawWarning(string rawWarning)
        {
            WriteLineInColor(rawWarning, System.ConsoleColor.DarkYellow);
        }
        public static void WriteWarning(string warning)
        {
            WriteRawWarning($"Warning: {warning}.");
        }
        public static void WriteWarning(System.Exception warning)
        {
            WriteWarning(warning.Message);
        }
        public static void WriteRawError(string rawError)
        {
            WriteLineInColor(rawError, System.ConsoleColor.Red);
        }
        public static void WriteError(string error)
        {
            WriteRawError($"Error: {error}.");
        }
        public static void WriteError(System.Exception error)
        {
            WriteError(error.Message);
        }
        public static void WriteFatalError(string fatalError)
        {
            WriteRawError($"Fatal Error: {fatalError}.");
            PressAnyKeyToExit();
        }
        public static void WriteFatalError(System.Exception error)
        {
            WriteFatalError(error.Message);
        }
        public static void PressAnyKeyToExit()
        {
            WriteLine();
            WriteLine("Press Any Key To Exit...");
            System.Diagnostics.Stopwatch bufferDisposalStopwatch = new System.Diagnostics.Stopwatch();
            bufferDisposalStopwatch.Start();
            while (true)
            {
                System.Console.ReadKey(true);
                if (bufferDisposalStopwatch.ElapsedTicks >= 10000000)
                {
                    break;
                }
            }
            try
            {
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
            catch
            {

            }
            try
            {
                System.Environment.Exit(0);
            }
            catch
            {

            }
            return;
        }
    }
}
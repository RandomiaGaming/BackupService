namespace BackupService
{
    public static class History
    {
        public const string HistoryPath = "backupHistory.ini";
        private static System.Collections.Generic.List<HistoryData> CurrentHistory = LoadInternal();
        public static System.Collections.Generic.List<HistoryData> LoadInternal()
        {
            if (!System.IO.File.Exists(HistoryPath))
            {
                System.IO.File.WriteAllText(HistoryPath, "");
                return new System.Collections.Generic.List<HistoryData>();
            }
            else
            {
                string[] lines = System.IO.File.ReadAllLines(HistoryPath);
                HistoryData[] historyData = new HistoryData[lines.Length];

                for (int i = 0; i < lines.Length; i++)
                {

                }

                return new System.Collections.Generic.List<HistoryData>(historyData);
            }
        }
        public static void MarkBackup(string path)
        {
            long time = System.DateTime.Now.Ticks;

            for (int i = 0; i < data.Count; i++)
            {
                if (data[i].Item2 == path)
                {
                    data[i].Item1 = time;

                }
            }
        }
        public static long CheckBackup(string path)
        {
            for (int i = 0; i < CurrentHistory.; i++)
            {

            }
        }
        private static void Save()
        {

        }
        private sealed class HistoryData
        {
            public string FilePath;
            public long LastBackupTime;
        }
    }
}
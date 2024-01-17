namespace BackupService
{
    public static class Config
    {
        public const string ConfigPath = "config.ini";
        public static ConfigData CurrentConfig = LoadInternal();
        public static void Load()
        {
            CurrentConfig = LoadInternal();
        }
        private static ConfigData LoadInternal()
        {
            if (!System.IO.File.Exists(ConfigPath))
            {
                CurrentConfig = new ConfigData();
                Save();
                return CurrentConfig;
            }
            else
            {
                string json = System.IO.File.ReadAllText(ConfigPath);
                return Newtonsoft.Json.JsonConvert.DeserializeObject<ConfigData>(json);
            }
        }
        public static void Save()
        {
            Newtonsoft.Json.JsonSerializerSettings serializationSettings = new Newtonsoft.Json.JsonSerializerSettings
            {
                DefaultValueHandling = Newtonsoft.Json.DefaultValueHandling.Include,
                Formatting = Newtonsoft.Json.Formatting.Indented,
                DateFormatHandling = Newtonsoft.Json.DateFormatHandling.IsoDateFormat,
                NullValueHandling = Newtonsoft.Json.NullValueHandling.Include,
                DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc
            };

            string json = Newtonsoft.Json.JsonConvert.SerializeObject(CurrentConfig, serializationSettings);

            System.IO.File.WriteAllText(ConfigPath, json);
        }
    }
    public sealed class ConfigData
    {
        public System.Collections.Generic.List<string> BackupSources = new System.Collections.Generic.List<string>();
        public string BackupDestination = "E:\\Backup";
        public System.TimeSpan BackupInterval = new System.TimeSpan(24, 0, 0);
        public long AlwaysBackupSize = -1;
        public long HeaderCheckSize = -1;
        public int BlockSize = 1048576;
        public bool IncludeHidden = false;
        public bool IncludeSystem = false;
        public bool ExcludeSymLinks = true;
        public ConfigData()
        {
            BackupSources = new System.Collections.Generic.List<string>();
            BackupDestination = "E:\\Backup";
            BackupInterval = new System.TimeSpan(24, 0, 0);
            AlwaysBackupSize = -1;
            HeaderCheckSize = -1;
            BlockSize = 1048576;
            IncludeHidden = false;
            IncludeSystem = false;
            ExcludeSymLinks = true;
        }
    }
}
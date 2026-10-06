using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAllocation.Persistence.Models;

namespace FluentAllocation.Persistence.Services
{
    // the disk layer under the saved settings:
    // callers hand in plain data and get plain data back, and the folder is handed in, so nothing here depends
    // on the app
    public class PersistenceService
    {
        // === fields ===

        public const string SettingsFileName = "settings.json";

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() }
        };

        private readonly string _settingsPath;


        // === singleton instance ===

        // set once by App with the folder of this build; the tests make their own
        public static PersistenceService Instance { get; private set; } = null!;

        public static void Initialize(string rootFolder)
        {
            Instance = new PersistenceService(rootFolder);
        }


        // === constructor ===

        public PersistenceService(string rootFolder)
        {
            RootFolder = rootFolder;
            _settingsPath = Path.Combine(rootFolder, SettingsFileName);
        }


        // === public api ===

        public string RootFolder { get; }

        // a missing or unreadable file gives the defaults
        public AppSettingsData LoadSettings()
        {
            try
            {
                if (!File.Exists(_settingsPath)) return new AppSettingsData();

                return JsonSerializer.Deserialize<AppSettingsData>(File.ReadAllText(_settingsPath), JsonOptions)
                    ?? new AppSettingsData();
            }
            catch
            {
                return new AppSettingsData();
            }
        }

        // written whole and at once; a settings change is a single click, so there is nothing to debounce
        // (through a temp file, so a crash mid write cannot leave half a file behind)
        public void SaveSettings(AppSettingsData data)
        {
            try
            {
                Directory.CreateDirectory(RootFolder);

                string tempPath = _settingsPath + ".tmp";
                File.WriteAllText(tempPath, JsonSerializer.Serialize(data, JsonOptions));
                File.Move(tempPath, _settingsPath, overwrite: true);
            }
            catch { /* the setting still applies for this session, it just will not survive a restart */ }
        }
    }
}

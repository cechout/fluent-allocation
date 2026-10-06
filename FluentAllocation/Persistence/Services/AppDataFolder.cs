using System;
using System.IO;
using FluentAllocation.Distribution;

namespace FluentAllocation.Persistence.Services
{
    // where the json files live:
    // %LocalAppData%\FluentAllocation for the installer build, a Persistence folder next to the exe for the
    // portable build
    public static class AppDataFolder
    {
        // === fields ===

        private const string LocalFolderName = "FluentAllocation";

        // next to the exe, so the whole state travels with the folder; (portable build only)
        private const string PortableFolderName = "Persistence";


        // === public api ===

        public static string Resolve()
        {
            string localAppData = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), LocalFolderName);

            if (!AppDistribution.IsPortableBuild) return localAppData;

            try
            {
                string? appFolder = Path.GetDirectoryName(Environment.ProcessPath);
                if (string.IsNullOrEmpty(appFolder)) return localAppData;

                // created on the spot as the write check; a zip unpacked somewhere read only would drop every save
                string portableFolder = Path.Combine(appFolder, PortableFolderName);
                Directory.CreateDirectory(portableFolder);
                return portableFolder;
            }
            catch
            {
                // an unusable app folder; the per user folder then
                return localAppData;
            }
        }
    }
}

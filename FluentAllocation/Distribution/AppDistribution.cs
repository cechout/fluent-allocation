using System;
using System.IO;

namespace FluentAllocation.Distribution
{
    // the distribution channel:
    // installer or portable, the two GitHub downloads; they differ only in where the state lives
    public static class AppDistribution
    {
        // === fields ===

        // portable mode: this marker next to the exe moves the state from %LocalAppData% into the app
        // folder; (only in the portable zip)
        public const string PortableMarkerFileName = "portable.txt";

        private static readonly Lazy<bool> _isPortableBuild = new Lazy<bool>(DetectPortableBuild);


        // === public api ===

        public static bool IsPortableBuild => _isPortableBuild.Value;


        // === private helpers ===

        private static bool DetectPortableBuild()
        {
            try
            {
                string? appFolder = Path.GetDirectoryName(Environment.ProcessPath);
                if (string.IsNullOrEmpty(appFolder)) return false;

                return File.Exists(Path.Combine(appFolder, PortableMarkerFileName));
            }
            catch
            {
                // an unreadable app folder counts as installed (%LocalAppData%)
                return false;
            }
        }
    }
}

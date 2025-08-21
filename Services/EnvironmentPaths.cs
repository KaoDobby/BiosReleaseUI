using System;
using System.IO;

namespace BiosReleaseUI.Services
{
    public static class EnvironmentPaths
    {
        public static string GetProjectRoot()
        {
            string? fullPath = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.Parent?.FullName;
            return fullPath ?? string.Empty;
        }

        public static string GetPreDumpPath()
        {
            return Path.Combine(GetProjectRoot(), "Pre_Dump");
        }

        public static string GetBackgroundImagePath()
        {
            return Path.Combine(GetProjectRoot(), "BiosReleaseUI", "bg.jpg");
        }
    }
}

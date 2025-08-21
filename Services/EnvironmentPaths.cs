using System;
using System.IO;

namespace BiosReleaseUI.Services
{
    public static class EnvironmentPaths
    {
        public static string GetProjectRoot()
        {
            return AppDomain.CurrentDomain.BaseDirectory;
        }

        public static string GetPreDumpPath()
        {
            return Path.Combine(GetProjectRoot(), "Pre_Dump");
        }

        public static string GetBackgroundImagePath()
        {
            return Path.Combine(GetProjectRoot(), "bg.jpg");
        }
    }
}

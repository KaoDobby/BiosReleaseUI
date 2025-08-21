using System;
using System.IO;


namespace BiosReleaseUI.Modular
{
public class DefaultPathProvider : IPathProvider
{
public string ProjectRoot { get; }
public string PreDumpPath { get; }


public DefaultPathProvider()
{
string? fullPath = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.Parent?.FullName;
ProjectRoot = fullPath ?? string.Empty;
PreDumpPath = Path.Combine(ProjectRoot, "Pre_Dump");
}
}
}
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;


namespace BiosReleaseUI.Modular
{
public class ReleaseNoteOpener : IReleaseNoteOpener
{
public bool TryOpen(string preDumpPath, string? detectedArch, Action<string> log)
{
// 依你的原邏輯調整，如：不同 ARCH 選不同檔名 pattern
string pattern = "*BIOS*.xlsm";
var matches = Directory.GetFiles(preDumpPath, pattern);
if (matches.Length == 0) { log("Release note not found."); return false; }


string note = matches.First();
try
{
Process.Start(new ProcessStartInfo { FileName = note, UseShellExecute = true });
log($"Open release note: {Path.GetFileName(note)}");
return true;
}
catch (Exception ex)
{
log($"Failed to open: {ex.Message}");
return false;
}
}
}
}
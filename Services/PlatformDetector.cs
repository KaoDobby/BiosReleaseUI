namespace BiosReleaseUI.Modular
{
public class PlatformDetector : IPlatformDetector
{
public string? TryParseArch(string line)
{
if (line.StartsWith("ARCH=", System.StringComparison.OrdinalIgnoreCase))
return line.Substring(5).Trim().ToUpperInvariant();
return null;
}
}
}
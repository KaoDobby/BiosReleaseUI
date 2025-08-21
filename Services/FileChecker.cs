using System.IO;
using System.Linq;
using System.Text.RegularExpressions;


namespace BiosReleaseUI.Modular
{
public class FileChecker : IFileChecker
{
public FileCheckResult Check(string preDumpPath)
{
var material = Path.Combine(preDumpPath, "Material");
if (!Directory.Exists(material))
return new FileCheckResult(false, false, System.Array.Empty<string>());


bool hasTxt = Directory.GetFiles(material, "*.txt").Length == 1;
bool hasBin = Directory.GetFiles(material, "*.bin").Length == 1;


var xmlDir = Path.Combine(material, "XML");
var platforms = Directory.Exists(xmlDir)
? Directory.GetFiles(xmlDir, "*.xml")
.Select(f => Path.GetFileNameWithoutExtension(f))
.Where(name => Regex.IsMatch(name ?? string.Empty, @"^[A-Z]\d{2}$"))
.Distinct()
.OrderBy(n => n)
.ToArray()
: System.Array.Empty<string>();


return new FileCheckResult(hasTxt, hasBin, platforms);
}
}
}


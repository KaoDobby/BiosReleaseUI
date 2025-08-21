using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace BiosReleaseUI.Services
{
    public class MaterialCheckResult
    {
        public bool AllExist { get; set; }
        public List<string> Platforms { get; set; } = new();
        public List<string> Messages { get; set; } = new();
    }

    public class MaterialChecker
    {
        public MaterialCheckResult CheckMaterials(string preDumpPath)
        {
            var result = new MaterialCheckResult { AllExist = true };
            string materialFolder = Path.Combine(preDumpPath, Constants.MaterialFolder);
            if (!Directory.Exists(materialFolder))
            {
                result.Messages.Add("\u2718 Missing folder: Material");
                result.AllExist = false;
            }
            else
            {
                string[] txtFiles = Directory.GetFiles(materialFolder, "*.txt");
                string[] binFiles = Directory.GetFiles(materialFolder, "*.bin");

                if (txtFiles.Length != 1)
                {
                    result.Messages.Add($"\u2718 Found {txtFiles.Length} TXT files. Exactly 1 expected, please check Material folder");
                    result.AllExist = false;
                }
                else
                {
                    result.Messages.Add($"\u2714 Found {txtFiles.Length} TXT file");
                }

                if (binFiles.Length != 1)
                {
                    result.Messages.Add($"\u2718 Found {binFiles.Length} BIN files. Exactly 1 expected, please check Material folder");
                    result.AllExist = false;
                }
                else
                {
                    result.Messages.Add($"\u2714 Found {binFiles.Length} BIN file");
                }

                string xmlFolder = Path.Combine(materialFolder, Constants.XmlFolder);
                string[] xmlFiles = Directory.Exists(xmlFolder) ? Directory.GetFiles(xmlFolder, "*.xml") : System.Array.Empty<string>();
                if (xmlFiles.Length > 0)
                {
                    result.Messages.Add($"\u2714 Found {xmlFiles.Length} XML files");
                    var platforms = xmlFiles.Select(f => Path.GetFileNameWithoutExtension(f))
                                            .Where(name => Regex.IsMatch(name, @"^[A-Z]\d{2}$"))
                                            .Distinct()
                                            .ToList();
                    result.Platforms.AddRange(platforms);
                }
                else
                {
                    result.Messages.Add("\u2718 No XML files found");
                    result.AllExist = false;
                }
            }
            return result;
        }
    }
}

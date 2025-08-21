using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace BiosReleaseUI.Services
{
    public class ScriptExecutor
    {
        public async Task RunScriptAsync(string scriptPath, Action<string> onOutput)
        {
            var psi = new ProcessStartInfo
            {
                FileName = scriptPath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = Path.GetDirectoryName(scriptPath) ?? string.Empty,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
                CreateNoWindow = true
            };

            using var proc = new Process { StartInfo = psi };
            proc.Start();

            while (!proc.StandardOutput.EndOfStream)
            {
                var line = await proc.StandardOutput.ReadLineAsync();
                if (line != null)
                    onOutput(line);
            }

            while (!proc.StandardError.EndOfStream)
            {
                var line = await proc.StandardError.ReadLineAsync();
                if (line != null)
                    onOutput(line);
            }

            await proc.WaitForExitAsync();
        }
    }
}

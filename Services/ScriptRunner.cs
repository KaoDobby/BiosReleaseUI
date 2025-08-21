using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;


namespace BiosReleaseUI.Modular
{
public class ScriptRunner : IScriptRunner
{
public async Task<int> RunAsync(string fileName,
string workingDir,
Action<string> onStdOut,
Action<string> onStdErr,
CancellationToken ct = default)
{
var psi = new ProcessStartInfo
{
FileName = fileName,
WorkingDirectory = workingDir,
UseShellExecute = false,
RedirectStandardOutput = true,
RedirectStandardError = true,
CreateNoWindow = true,
StandardOutputEncoding = Encoding.UTF8,
StandardErrorEncoding = Encoding.UTF8
};


using var proc = new Process { StartInfo = psi };
proc.Start();


var tOut = Task.Run(async () =>
{
while (!proc.StandardOutput.EndOfStream)
{
var line = await proc.StandardOutput.ReadLineAsync();
if (line != null) onStdOut(line);
}
}, ct);


var tErr = Task.Run(async () =>
{
while (!proc.StandardError.EndOfStream)
{
var line = await proc.StandardError.ReadLineAsync();
if (line != null) onStdErr(line);
}
}, ct);


await Task.WhenAll(tOut, tErr);
await proc.WaitForExitAsync(ct);
return proc.ExitCode;
}
}
}
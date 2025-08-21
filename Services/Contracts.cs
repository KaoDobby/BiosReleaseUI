using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;


namespace BiosReleaseUI.Modular
{
public record FileCheckResult(bool HasTxt, bool HasBin, IReadOnlyList<string> Platforms);


public interface IPathProvider
{
string ProjectRoot { get; }
string PreDumpPath { get; }
}


public interface IFileChecker
{
FileCheckResult Check(string preDumpPath);
}


public interface IScriptRunner
{
Task<int> RunAsync(string fileName,
string workingDir,
Action<string> onStdOut,
Action<string> onStdErr,
CancellationToken ct = default);
}


public interface IPlatformDetector
{
string? TryParseArch(string line);
}


public interface IReleaseNoteOpener
{
bool TryOpen(string preDumpPath, string? detectedArch, Action<string> log);
}
}


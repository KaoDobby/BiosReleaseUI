using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;


namespace BiosReleaseUI.Modular
{
    public class MainPresenter
    {
        private readonly IPathProvider _paths;
        private readonly IFileChecker _checker;
        private readonly IScriptRunner _runner;
        private readonly IPlatformDetector _detector;
        private readonly IReleaseNoteOpener _note;
        private readonly ILogSink _log;
        private readonly ReleaseViewModel _vm;


        public MainPresenter(IPathProvider paths,
        IFileChecker checker,
        IScriptRunner runner,
        IPlatformDetector detector,
        IReleaseNoteOpener note,
        ILogSink log,
        ReleaseViewModel vm)
        {
            _paths = paths; _checker = checker; _runner = runner; _detector = detector; _note = note; _log = log; _vm = vm;
        }

        public string PreDumpPath => _paths.PreDumpPath;


        public FileCheckResult CheckFiles()
        {
            var result = _checker.Check(_paths.PreDumpPath);
            _vm.AllFilesExist = result.HasTxt && result.HasBin && result.Platforms.Count > 0;
            _log.AppendInfo(_vm.AllFilesExist ? "All files found" : "Missing files under Pre_Dump/Material");
            return result;
        }


        public void ConfirmPlatform(string platform)
        {
            _vm.SelectedPlatform = platform;
            _vm.ConfirmedPlatform = platform;
            _log.AppendInfo($"Confirmed platform: {platform}");
        }

        public async Task RunAsync(CancellationToken ct = default)
        {
            if (!_vm.CanRun) { _log.AppendWarn("Not ready to run."); return; }


            string batPath = Path.Combine(_paths.ProjectRoot, "Pre_Dump", "Make_csv_file.bat");
            if (!File.Exists(batPath))
            {
                _log.AppendError($"Script not found: {batPath}");
                return;
            }


            _log.AppendInfo($"Running: {batPath}");
            _vm.DetectedArch = null;


            int code = await _runner.RunAsync(
            fileName: batPath,
            workingDir: _paths.PreDumpPath,
            onStdOut: line =>
            {
                _log.Append(line);
                var arch = _detector.TryParseArch(line);
                if (!string.IsNullOrEmpty(arch))
                {
                    _vm.DetectedArch = arch;
                    _log.AppendInfo($"Detected ARCH={arch}");
                }
            },
            onStdErr: line => _log.AppendError(line),
            ct: ct
            );


            _log.AppendInfo($"Process exit code: {code}");
        }
        public void OpenReleaseNote()
        {
            if (!_vm.CanOpenNote) { _log.AppendWarn("ARCH not detected yet."); return; }
            _note.TryOpen(_paths.PreDumpPath, _vm.DetectedArch, s => _log.Append(s));
        }
    }
}

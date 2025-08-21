namespace BiosReleaseUI.Modular
{
public class ReleaseViewModel
{
public bool AllFilesExist { get; set; }
public string? SelectedPlatform { get; set; }
public string? ConfirmedPlatform { get; set; }
public string? DetectedArch { get; set; }


public bool CanChoosePlatform => AllFilesExist;
public bool CanRun => AllFilesExist && !string.IsNullOrEmpty(ConfirmedPlatform);
public bool CanOpenNote => !string.IsNullOrEmpty(DetectedArch);
}
}
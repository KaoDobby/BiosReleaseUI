namespace BiosReleaseUI.Services
{
    public interface ILogService
    {
        void Append(string message);
        void Clear();
        string GetText();
    }
}

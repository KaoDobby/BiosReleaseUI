namespace BiosReleaseUI.Modular
{
    public interface ILogSink
    {
        void Append(string text);
        void AppendInfo(string text) => Append($"[INFO] {text}");
        void AppendWarn(string text) => Append($"[WARN] {text}");
        void AppendError(string text) => Append($"[ERR ] {text}");
        void Clear();
    }
}
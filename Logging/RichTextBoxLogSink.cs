using System;
using System.Windows.Forms;


namespace BiosReleaseUI.Modular
{
    // 把你原本 Form 上的 RichTextBox 傳進來，不改 UI，只改呼叫方式
    public class RichTextBoxLogSink : ILogSink
    {
        private readonly RichTextBox _rtb;
        private readonly Control _owner; // 用於 Invoke 回 UI 執行緒


        public RichTextBoxLogSink(RichTextBox rtb)
        {
            _rtb = rtb;
            _owner = rtb;
        }


        public void Append(string text)
        {
            if (_owner.InvokeRequired)
            {
                _owner.BeginInvoke(new Action<string>(Append), text);
                return;
            }
            _rtb.AppendText(text + Environment.NewLine);
            _rtb.ScrollToCaret();
        }


        public void Clear()
        {
            if (_owner.InvokeRequired)
            {
                _owner.BeginInvoke(new Action(Clear));
                return;
            }
            _rtb.Clear();
        }
    }
}
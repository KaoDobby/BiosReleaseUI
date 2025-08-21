using WpfControls = System.Windows.Controls;
using WpfDocuments = System.Windows.Documents;
using WpfMedia = System.Windows.Media;

namespace BiosReleaseUI.Services
{
    public class RichTextBoxLogService : ILogService
    {
        private readonly WpfControls.RichTextBox _textBox;

        public RichTextBoxLogService(WpfControls.RichTextBox textBox)
        {
            _textBox = textBox;
        }

        public void Append(string message)
        {
            var paragraph = new WpfDocuments.Paragraph { Margin = new System.Windows.Thickness(0) };
            var brush = WpfMedia.Brushes.Black;
            if (message.Contains("\u2714")) brush = WpfMedia.Brushes.Green;
            else if (message.Contains("\u2718") || message.Contains("ERROR")) brush = WpfMedia.Brushes.Brown;
            else if (message.StartsWith("[")) brush = WpfMedia.Brushes.Blue;
            paragraph.Inlines.Add(new WpfDocuments.Run(message) { Foreground = brush });
            paragraph.Inlines.Add(new WpfDocuments.LineBreak());
            _textBox.Document.Blocks.Add(paragraph);
            _textBox.ScrollToEnd();
        }

        public void Clear()
        {
            _textBox.Document.Blocks.Clear();
        }

        public string GetText()
        {
            var textRange = new WpfDocuments.TextRange(_textBox.Document.ContentStart, _textBox.Document.ContentEnd);
            return textRange.Text;
        }
    }
}

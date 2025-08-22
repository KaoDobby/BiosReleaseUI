using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

// WinForms using naming
using WinForms = System.Windows.Forms;
using WinFormsIntegration = System.Windows.Forms.Integration;
using Drawing = System.Drawing;

// WPF using naming
using WpfControls = System.Windows.Controls;
using WpfDocuments = System.Windows.Documents;
using WpfMedia = System.Windows.Media;

namespace BiosReleaseUI
{
    public partial class MainForm
    {
        private void BindEvents()
        {
            checkFilesButton.Click += CheckFilesButton_Click;
            runMainCodeButton.Click += RunMainCodeButton_Click;
            openReleaseNoteButton.Click += OpenReleaseNoteButton_Click;
            aboutButton.Click += AboutButton_Click;
            clearLogButton.Click += ClearLogButton_Click;
            saveLogButton.Click += SaveLogButton_Click;
            confirmPlatformButton.Click += ConfirmPlatformButton_Click;
            platformComboBox.DrawItem += PlatformComboBox_DrawItem;
        }

        private void PlatformComboBox_DrawItem(object? sender, WinForms.DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            e.DrawBackground();
            string text = platformComboBox.Items[e.Index]?.ToString() ?? string.Empty;
            using var brush = new Drawing.SolidBrush(e.ForeColor);
            var format = new Drawing.StringFormat
            {
                Alignment = Drawing.StringAlignment.Center,
                LineAlignment = Drawing.StringAlignment.Center
            };
            e.Graphics.DrawString(text, e.Font ?? platformComboBox.Font ?? Drawing.SystemFonts.DefaultFont, brush, e.Bounds, format);
            e.DrawFocusRectangle();
        }

        private void ConfirmPlatformButton_Click(object? sender, EventArgs e)
        {
            if (platformComboBox.SelectedItem != null)
            {
                selectedPlatform = platformComboBox.SelectedItem.ToString() ?? string.Empty;
                selectedPlatformConfirmed = selectedPlatform;
                AppendLog($"[Selected Platform] → {selectedPlatform}");
                UpdateRunButtonState();
            }
            else
            {
                WinForms.MessageBox.Show("Please select a platform.");
            }
        }

        private void ClearLogButton_Click(object? sender, EventArgs e) => wpfLogBox.Document.Blocks.Clear();

        private void UpdateRunButtonState()
        {
            runMainCodeButton.Enabled = allFilesExist && !string.IsNullOrEmpty(selectedPlatformConfirmed);
            platformComboBox.Enabled = allFilesExist;
            confirmPlatformButton.Enabled = allFilesExist;
        }

        private void AppendLog(string message)
        {
            var paragraph = new WpfDocuments.Paragraph { Margin = new System.Windows.Thickness(0) };
            var brush = WpfMedia.Brushes.Black;
            if (message.Contains("\u2714")) brush = WpfMedia.Brushes.Green;
            else if (message.Contains("\u2718") || message.Contains("ERROR")) brush = WpfMedia.Brushes.Brown;
            else if (message.StartsWith("[")) brush = WpfMedia.Brushes.Blue;
            paragraph.Inlines.Add(new WpfDocuments.Run(message) { Foreground = brush });
            paragraph.Inlines.Add(new WpfDocuments.LineBreak());
            wpfLogBox.Document.Blocks.Add(paragraph);
            wpfLogBox.ScrollToEnd();
        }

        private void CheckFilesButton_Click(object? sender, EventArgs e)
        {
            wpfLogBox.Document.Blocks.Clear();
            bool allExist = true;

            string materialFolder = Path.Combine(preDumpPath, "Material");
            if (!Directory.Exists(materialFolder))
            {
                AppendLog("\u2718 Missing folder: Material");
                allExist = false;
            }
            else
            {
                string[] txtFiles = Directory.GetFiles(materialFolder, "*.txt");
                string[] binFiles = Directory.GetFiles(materialFolder, "*.bin");

                if (txtFiles.Length != 1)
                {
                    AppendLog($"\u2718 Found {txtFiles.Length} TXT files. Exactly 1 expected, please check Material folder");
                    allExist = false;
                }
                else
                {
                    AppendLog($"\u2714 Found {txtFiles.Length} TXT file");
                }

                if (binFiles.Length != 1)
                {
                    AppendLog($"\u2718 Found {binFiles.Length} BIN files. Exactly 1 expected, please check Material folder");
                    allExist = false;
                }
                else
                {
                    AppendLog($"\u2714 Found {binFiles.Length} BIN file");
                }

                string XMLFolder = Path.Combine(materialFolder, "XML");
                string[] xmlFiles = Directory.GetFiles(XMLFolder, "*.xml");
                if (xmlFiles.Length > 0)
                {
                    AppendLog($"\u2714 Found {xmlFiles.Length} XML files");
                    var platforms = xmlFiles.Select(f => Path.GetFileNameWithoutExtension(f))
                                            .Where(name => Regex.IsMatch(name, @"^[A-Z]\d{2}$"))
                                            .Distinct()
                                            .ToList();

                    platformComboBox.Items.Clear();
                    platformComboBox.Items.AddRange(platforms.ToArray());
                    if (platforms.Count > 0) platformComboBox.SelectedIndex = 0;
                }
                else
                {
                    AppendLog("\u2718 No XML files found");
                    allExist = false;
                }
            }

            statusLabel.Text = allExist ? "Status: \u2705 All files found" : "Status: \u26a0 Incomplete files";
            allFilesExist = allExist;
            openReleaseNoteButton.Enabled = false;
            runMainCodeButton.Enabled = false;
            platformComboBox.Enabled = allExist;
            confirmPlatformButton.Enabled = allExist;
            UpdateRunButtonState();
        }

        private void RunMainCodeButton_Click(object? sender, EventArgs e)
        {
            string batPath = Path.Combine(preDumpPath, "Make_csv_file.bat");
            if (!File.Exists(batPath)) { WinForms.MessageBox.Show("Make_csv_file.bat not found"); return; }

            runMainCodeButton.Enabled = false;
            openReleaseNoteButton.Enabled = false;
            AppendLog("[Executing Main Script...]");

            Task.Run(() =>
            {
                var psi = new ProcessStartInfo
                {
                    FileName = batPath,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    WorkingDirectory = preDumpPath,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8,
                    CreateNoWindow = true
                };

                using (var proc = Process.Start(psi))
                {
                    if (proc == null) { Invoke(() => AppendLog("\u2718 Failed to start process")); return; }
                    while (!proc.StandardOutput.EndOfStream)
                    {
                        var line = proc.StandardOutput.ReadLine();
                        if (line != null && line.StartsWith("ARCH=", StringComparison.OrdinalIgnoreCase))
                        {
                            string arch = line.Substring(5).Trim().ToUpper();
                            detectedArch = arch;
                            Invoke(() => AppendLog($"[Detected Platform Vender] → {arch}"));
                        }
                        else
                        {
                            Invoke(() => AppendLog(line ?? "[null]"));
                        }
                    }
                }

                Invoke(() => AppendLog($"[Next Excel File Target] → {detectedArch}"));

                Invoke(() =>
                {
                    AppendLog("[DONE]");
                    runMainCodeButton.Enabled = true;
                    openReleaseNoteButton.Enabled = true;
                });
            });
        }

        private void OpenReleaseNoteButton_Click(object? sender, EventArgs e)
        {
            string[] defaultNote = Directory.GetFiles(preDumpPath, "*BIOS*.xlsm");
            AppendLog($"Release note:{string.Join(", ", defaultNote)}");
            string notePath = defaultNote.FirstOrDefault() ?? string.Empty;

            if (!string.IsNullOrEmpty(detectedArch))
            {
                string expectedFile = detectedArch.ToUpper() switch
                {
                    "AMD" => "*BIOS*.xlsm",
                    "INTEL" => "*BIOS*.xlsm",
                    _ => ""
                };

                if (!string.IsNullOrEmpty(expectedFile))
                {
                    string fullPath = Path.Combine(preDumpPath, expectedFile);
                    if (File.Exists(fullPath))
                    {
                        notePath = fullPath;
                    }
                    else
                    {
                        AppendLog($"[Warning] Expected Excel not found: {expectedFile}, fallback to default.");
                    }
                }
            }

            if (File.Exists(notePath))
                Process.Start(new ProcessStartInfo(notePath) { UseShellExecute = true });
            else
                WinForms.MessageBox.Show("Release note file not found.", "Error", WinForms.MessageBoxButtons.OK, WinForms.MessageBoxIcon.Error);
        }

        private void AboutButton_Click(object? sender, EventArgs e)
        {
            WinForms.MessageBox.Show("BIOS Release Tool\nAuthor:\n\tJarvey\n\tWilliam\n\tDobby\n\n\ud83c\udf89 You're awesome for checking the About page! \ud83c\udf89", "About", WinForms.MessageBoxButtons.OK, WinForms.MessageBoxIcon.Information);
        }

        private void SaveLogButton_Click(object? sender, EventArgs e)
        {
            var dialog = new WinForms.SaveFileDialog
            {
                Title = "Save Log As",
                Filter = "Text Files (*.txt)|*.txt",
                DefaultExt = "txt",
                FileName = "BiosReleaseTool_Log_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt"
            };

            if (dialog.ShowDialog() == WinForms.DialogResult.OK)
            {
                try
                {
                    var textRange = new WpfDocuments.TextRange(wpfLogBox.Document.ContentStart, wpfLogBox.Document.ContentEnd);
                    File.WriteAllText(dialog.FileName, textRange.Text, Encoding.UTF8);
                    WinForms.MessageBox.Show("Log saved successfully!", "Success", WinForms.MessageBoxButtons.OK, WinForms.MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    WinForms.MessageBox.Show("Failed to save log:\n" + ex.Message, "Error", WinForms.MessageBoxButtons.OK, WinForms.MessageBoxIcon.Error);
                }
            }
        }
    }
}


using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Linq;
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
    public class BiosReleaseUI : WinForms.Form
    {
        private WinForms.Label statusLabel;
        private WinForms.Button checkFilesButton, runMainCodeButton, openReleaseNoteButton, aboutButton, clearLogButton;
        private WinForms.ComboBox platformComboBox;
        private WinForms.Button confirmPlatformButton;
        private string selectedPlatform = "";
        private string selectedPlatformConfirmed = "";
        private bool allFilesExist = false;
        private string projectRoot, preDumpPath;
        private WinForms.Panel logBackgroundPanel;
        private Drawing.Image? logBackgroundImage;
        private WinFormsIntegration.ElementHost logHost;
        private WpfControls.RichTextBox wpfLogBox;
        private string detectedArch = "";

        public BiosReleaseUI()
        {
            string? fullPath = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.Parent?.FullName;
            projectRoot = fullPath ?? "";
            preDumpPath = Path.Combine(projectRoot, "Pre_Dump");
            string imagePath = Path.Combine(projectRoot, "BiosReleaseUI", "bg.jpg");

            // Set UI STEP HEIGHTS
            int stepBtnHeight = 85; // Step Height
            int groupBoxHeight = 170;
            int stepFontSize = 17;

            if (File.Exists(imagePath))
            {
                try { logBackgroundImage = Drawing.Image.FromFile(imagePath); }
                catch (Exception ex) { WinForms.MessageBox.Show("Background image input failure : " + ex.Message); }
            }

            Text = "BIOS Release Tool";
            Width = 1140;
            Height = 1380;
            Font = new Drawing.Font("Segoe UI", 10);
            BackColor = Drawing.Color.White;

            var statusPanel = new WinForms.Panel
            {
                Dock = WinForms.DockStyle.Top,
                Height = 50,
                BackColor = Drawing.Color.LightSteelBlue
            };
            statusLabel = new WinForms.Label
            {
                Text = "Status: ⚠ Incomplete files",
                AutoSize = false,
                TextAlign = Drawing.ContentAlignment.MiddleLeft,
                Dock = WinForms.DockStyle.Fill,
                ForeColor = Drawing.Color.DarkBlue
            };
            aboutButton = new WinForms.Button
            {
                Text = "ⓘ",
                Width = 50,
                Dock = WinForms.DockStyle.Right,
                FlatStyle = WinForms.FlatStyle.Flat,
                BackColor = Drawing.Color.LightGray,
                Font = new Drawing.Font("Segoe UI", 10, Drawing.FontStyle.Bold)
            };
            aboutButton.FlatAppearance.BorderSize = 0;
            aboutButton.Click += AboutButton_Click;
            statusPanel.Controls.Add(statusLabel);
            statusPanel.Controls.Add(aboutButton);

            var controlPanel = new WinForms.TableLayoutPanel
            {
                Dock = WinForms.DockStyle.Top,
                RowCount = 4,
                ColumnCount = 1,
                Height = 440, // = 90 + 140 + 90 + 90 + Padding
                Padding = new WinForms.Padding(10),
                AutoSize = true
            };
            // Update Height
            controlPanel.RowStyles.Add(new WinForms.RowStyle(WinForms.SizeType.Absolute, stepBtnHeight));
            controlPanel.RowStyles.Add(new WinForms.RowStyle(WinForms.SizeType.Absolute, groupBoxHeight));
            controlPanel.RowStyles.Add(new WinForms.RowStyle(WinForms.SizeType.Absolute, stepBtnHeight));
            controlPanel.RowStyles.Add(new WinForms.RowStyle(WinForms.SizeType.Absolute, stepBtnHeight));

            checkFilesButton = CreateStyledButton("① Check Material Files", Drawing.Color.FromArgb(220, 230, 250), Drawing.Color.DarkBlue, true, stepFontSize);
            runMainCodeButton = CreateStyledButton("③ Execute Make_csv_file.bat", Drawing.Color.FromArgb(230, 250, 230), Drawing.Color.DarkGreen, true, stepFontSize);
            openReleaseNoteButton = CreateStyledButton("④ Open BIOS_RELEASE_NOTE.xlsm", Drawing.Color.FromArgb(250, 240, 200), Drawing.Color.SaddleBrown, true, stepFontSize);

            // Set Height with same
            checkFilesButton.Height = stepBtnHeight;
            runMainCodeButton.Height = stepBtnHeight;
            openReleaseNoteButton.Height = stepBtnHeight;

            runMainCodeButton.Enabled = false;
            openReleaseNoteButton.Enabled = false;

            var platformGroupBox = new WinForms.GroupBox
            {
                Text = "② Platform Selection",
                Height = groupBoxHeight, // Step2 Height
                Dock = WinForms.DockStyle.Top,
                Padding = new WinForms.Padding(10),
                Font = new Drawing.Font("Segoe UI", 12, Drawing.FontStyle.Bold)
            };
            var platformLayout = new WinForms.TableLayoutPanel
            {
                Dock = WinForms.DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1
            };
            platformLayout.ColumnStyles.Add(new WinForms.ColumnStyle(WinForms.SizeType.Percent, 50F));
            platformLayout.ColumnStyles.Add(new WinForms.ColumnStyle(WinForms.SizeType.Percent, 50F));

            platformComboBox = new WinForms.ComboBox
            {
                Dock = WinForms.DockStyle.Fill,
                DropDownStyle = WinForms.ComboBoxStyle.DropDownList,
                Font = new Drawing.Font("Segoe UI", 13),
                Height = stepBtnHeight,
                DrawMode = WinForms.DrawMode.OwnerDrawFixed
            };
            
            platformComboBox.ItemHeight = stepBtnHeight ;
            platformComboBox.DrawItem += (s, e) =>
            {
                if (e.Index < 0) return;
                e.DrawBackground();

                string text = platformComboBox.Items[e.Index]?.ToString() ?? "";
                using var brush = new Drawing.SolidBrush(e.ForeColor);

                // format to center
                var format = new StringFormat
                {
                    Alignment = StringAlignment.Center,     
                    LineAlignment = StringAlignment.Center   
                };
                e.Graphics.DrawString(text, e.Font ?? platformComboBox.Font ?? SystemFonts.DefaultFont, brush, e.Bounds, format);

                e.DrawFocusRectangle();

            };
            
            confirmPlatformButton = new WinForms.Button
            {
                Text = "Confirm Platform",
                Dock = WinForms.DockStyle.Fill,
                Font = new Drawing.Font("Segoe UI", stepFontSize, Drawing.FontStyle.Bold),
                Height = 40,
                BackColor = Drawing.Color.LightGoldenrodYellow,
                FlatStyle = WinForms.FlatStyle.Flat
            };

            confirmPlatformButton.Click += (s, e) =>
            {
                if (platformComboBox.SelectedItem != null)
                {
                    selectedPlatform = platformComboBox.SelectedItem.ToString() ?? "";
                    selectedPlatformConfirmed = selectedPlatform;
                    AppendLog($"[Selected Platform] → {selectedPlatform}");
                    UpdateRunButtonState();
                }
                else
                {
                    WinForms.MessageBox.Show("Please select a platform.");
                }
            };

            platformLayout.Controls.Add(platformComboBox, 0, 0);
            platformLayout.Controls.Add(confirmPlatformButton, 1, 0);
            platformGroupBox.Controls.Add(platformLayout);

            controlPanel.Controls.Add(checkFilesButton, 0, 0);
            controlPanel.Controls.Add(platformGroupBox, 0, 1);
            controlPanel.Controls.Add(runMainCodeButton, 0, 2);
            controlPanel.Controls.Add(openReleaseNoteButton, 0, 3);

            logBackgroundPanel = new WinForms.Panel
            {
                Dock = WinForms.DockStyle.Fill,
                BackgroundImage = logBackgroundImage,
                BackgroundImageLayout = WinForms.ImageLayout.Zoom,
                Padding = new WinForms.Padding(10)
            };

            wpfLogBox = new WpfControls.RichTextBox
            {
                Background = new WpfMedia.SolidColorBrush(WpfMedia.Color.FromArgb(50, 255, 255, 255)),
                Foreground = WpfMedia.Brushes.Black,
                FontFamily = new WpfMedia.FontFamily("Consolas"),
                FontSize = 14,
                IsReadOnly = true,
                BorderThickness = new System.Windows.Thickness(0),
                Padding = new System.Windows.Thickness(4),
            };

            logHost = new WinFormsIntegration.ElementHost
            {
                Dock = WinForms.DockStyle.Fill,
                Child = wpfLogBox,
                BackColorTransparent = true
            };

            var logLayout = new WinForms.TableLayoutPanel
            {
                Dock = WinForms.DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 2,
                BackColor = Drawing.Color.Transparent
            };

            logLayout.RowStyles.Add(new WinForms.RowStyle(WinForms.SizeType.Percent, 100F));
            logLayout.RowStyles.Add(new WinForms.RowStyle(WinForms.SizeType.Absolute, 80F));
            logLayout.Controls.Add(logHost, 0, 0);

            clearLogButton = new WinForms.Button
            {
                Text = "Clear Log",
                Width = 155,
                Height = 45,
                Anchor = WinForms.AnchorStyles.Bottom | WinForms.AnchorStyles.Right,
                BackColor = Drawing.Color.FromArgb(255, 230, 230),
                FlatStyle = WinForms.FlatStyle.Flat
            };
            clearLogButton.Click += (s, e) => wpfLogBox.Document.Blocks.Clear();
            clearLogButton.Margin = new WinForms.Padding(10, 5, 10, 5);

            var saveLogButton = new WinForms.Button
            {
                Text = "Save Log",
                Width = 155,
                Height = 45,
                Anchor = WinForms.AnchorStyles.Bottom | WinForms.AnchorStyles.Right,
                BackColor = Drawing.Color.FromArgb(230, 255, 230),
                FlatStyle = WinForms.FlatStyle.Flat
            };
            saveLogButton.Click += SaveLogButton_Click;
            saveLogButton.Margin = new WinForms.Padding(10, 5, 10, 5);

            var buttonPanel = new WinForms.FlowLayoutPanel
            {
                Dock = WinForms.DockStyle.Right,
                FlowDirection = WinForms.FlowDirection.RightToLeft,
                Padding = new WinForms.Padding(5, 12, 5, 12),
                AutoSize = true
            };
            buttonPanel.Controls.Add(clearLogButton);
            buttonPanel.Controls.Add(saveLogButton);
            logLayout.Controls.Add(buttonPanel, 0, 1);

            logBackgroundPanel.Controls.Add(logLayout);

            checkFilesButton.Click += CheckFilesButton_Click;
            runMainCodeButton.Click += RunMainCodeButton_Click;
            openReleaseNoteButton.Click += OpenReleaseNoteButton_Click;

            Controls.Add(logBackgroundPanel);
            Controls.Add(controlPanel);
            Controls.Add(statusPanel);
        }

        private WinForms.Button CreateStyledButton(string text, Drawing.Color backColor, Drawing.Color foreColor, bool bold = false, int fontSize = 11)
        {
            var button = new WinForms.Button
            {
                Text = text,
                Dock = WinForms.DockStyle.Top,
                Height = 50,
                FlatStyle = WinForms.FlatStyle.Flat,
                BackColor = backColor,
                ForeColor = foreColor,
                Font = new Drawing.Font("Segoe UI", fontSize, bold ? Drawing.FontStyle.Bold : Drawing.FontStyle.Regular)
            };
            button.FlatAppearance.BorderColor = Drawing.Color.Gray;
            button.MouseEnter += (s, e) => button.BackColor = WinForms.ControlPaint.Light(backColor);
            button.MouseLeave += (s, e) => button.BackColor = backColor;
            return button;
        }

        private void UpdateRunButtonState()
        {
            runMainCodeButton.Enabled = allFilesExist && !string.IsNullOrEmpty(selectedPlatformConfirmed);
            platformComboBox.Enabled = allFilesExist;
            confirmPlatformButton.Enabled = allFilesExist;
        }

        private WinForms.Button CreateStyledButton(string text, Drawing.Color backColor)
        {
            var button = new WinForms.Button { Text = text, Dock = WinForms.DockStyle.Top, Height = 40, FlatStyle = WinForms.FlatStyle.Flat, BackColor = backColor };
            button.FlatAppearance.BorderColor = Drawing.Color.Gray;
            button.MouseEnter += (s, e) => button.BackColor = WinForms.ControlPaint.Light(backColor);
            button.MouseLeave += (s, e) => button.BackColor = backColor;
            return button;
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
                // txt , bin check
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

                // XML check
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
            openReleaseNoteButton.Enabled = false; // [MOD] Step4永遠不能跳過前面
            runMainCodeButton.Enabled = false;     // [MOD] Step3 也一樣
            platformComboBox.Enabled = allExist;   // [MOD]
            confirmPlatformButton.Enabled = allExist; // [MOD]
            UpdateRunButtonState(); // [MOD]
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
            string notePath = defaultNote.FirstOrDefault()?? "";

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

        [STAThread]
        static void Main() => WinForms.Application.Run(new BiosReleaseUI());
    }
}

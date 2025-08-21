using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

// WinForms using naming
using WinForms = System.Windows.Forms;
using WinFormsIntegration = System.Windows.Forms.Integration;
using Drawing = System.Drawing;

// WPF using naming
using WpfControls = System.Windows.Controls;
using WpfMedia = System.Windows.Media;

using BiosReleaseUI.Services;

namespace BiosReleaseUI
{
    public class BiosReleaseUI : WinForms.Form
    {
        private WinForms.Label statusLabel = null!;
        private WinForms.Button checkFilesButton = null!, runMainCodeButton = null!, openReleaseNoteButton = null!, aboutButton = null!, clearLogButton = null!, saveLogButton = null!;
        private WinForms.ComboBox platformComboBox = null!;
        private WinForms.Button confirmPlatformButton = null!;
        private string selectedPlatform = "";
        private string selectedPlatformConfirmed = "";
        private bool allFilesExist = false;
        private readonly string projectRoot;
        private readonly string preDumpPath;
        private WinForms.Panel logBackgroundPanel = null!;
        private Drawing.Image? logBackgroundImage;
        private WinFormsIntegration.ElementHost logHost = null!;
        private WpfControls.RichTextBox wpfLogBox = null!;
        private string detectedArch = "";
        private readonly ILogService logService;
        private readonly MaterialChecker materialChecker;
        private readonly ScriptExecutor scriptExecutor;

        public BiosReleaseUI()
        {
            projectRoot = EnvironmentPaths.GetProjectRoot();
            preDumpPath = EnvironmentPaths.GetPreDumpPath();
            string imagePath = EnvironmentPaths.GetBackgroundImagePath();

            if (File.Exists(imagePath))
            {
                try { logBackgroundImage = Drawing.Image.FromFile(imagePath); }
                catch (Exception ex) { WinForms.MessageBox.Show("Background image input failure : " + ex.Message); }
            }
            else
            {
                WinForms.MessageBox.Show("Background image not found. Using default background color.");
            }

            InitializeUi();

            logService = new RichTextBoxLogService(wpfLogBox!);
            materialChecker = new MaterialChecker();
            scriptExecutor = new ScriptExecutor();

            checkFilesButton!.Click += CheckFilesButton_Click;
            runMainCodeButton!.Click += RunMainCodeButton_Click;
            openReleaseNoteButton!.Click += OpenReleaseNoteButton_Click;
            aboutButton!.Click += AboutButton_Click;
            confirmPlatformButton!.Click += ConfirmPlatformButton_Click;
            clearLogButton!.Click += (s, e) => logService.Clear();
            saveLogButton!.Click += SaveLogButton_Click;
        }

        private void InitializeUi()
        {
            int stepFontSize = 17;

            Text = "BIOS Release Tool";
            Width = 1140;
            Height = 1380;
            Font = new Drawing.Font("Segoe UI", 10);
            AutoScaleMode = WinForms.AutoScaleMode.Dpi;
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
                Font = new Drawing.Font("Segoe UI", 10, Drawing.FontStyle.Bold),
            };
            aboutButton.FlatAppearance.BorderSize = 0;
            statusPanel.Controls.Add(statusLabel);
            statusPanel.Controls.Add(aboutButton);

            var controlPanel = new WinForms.TableLayoutPanel
            {
                Dock = WinForms.DockStyle.Top,
                RowCount = 4,
                ColumnCount = 1,
                Padding = new WinForms.Padding(10),
                AutoSize = true,
                AutoSizeMode = WinForms.AutoSizeMode.GrowAndShrink
            };
            controlPanel.RowStyles.Add(new WinForms.RowStyle(WinForms.SizeType.Percent, 25F));
            controlPanel.RowStyles.Add(new WinForms.RowStyle(WinForms.SizeType.Percent, 25F));
            controlPanel.RowStyles.Add(new WinForms.RowStyle(WinForms.SizeType.Percent, 25F));
            controlPanel.RowStyles.Add(new WinForms.RowStyle(WinForms.SizeType.Percent, 25F));

            checkFilesButton = CreateStyledButton("① Check Material Files", Drawing.Color.FromArgb(220, 230, 250), Drawing.Color.DarkBlue, true, stepFontSize);
            runMainCodeButton = CreateStyledButton("③ Execute Make_csv_file.bat", Drawing.Color.FromArgb(230, 250, 230), Drawing.Color.DarkGreen, true, stepFontSize);
            openReleaseNoteButton = CreateStyledButton("④ Open BIOS_RELEASE_NOTE.xlsm", Drawing.Color.FromArgb(250, 240, 200), Drawing.Color.SaddleBrown, true, stepFontSize);

            runMainCodeButton.Enabled = false;
            openReleaseNoteButton.Enabled = false;

            var platformGroupBox = new WinForms.GroupBox
            {
                Text = "② Platform Selection",
                Dock = WinForms.DockStyle.Fill,
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
                DrawMode = WinForms.DrawMode.OwnerDrawFixed
            };
            platformComboBox.ItemHeight = platformComboBox.Font.Height + 10;
            platformComboBox.DrawItem += (s, e) =>
            {
                if (e.Index < 0) return;
                e.DrawBackground();
                string text = platformComboBox.Items[e.Index]?.ToString() ?? "";
                using var brush = new Drawing.SolidBrush(e.ForeColor);
                var format = new Drawing.StringFormat
                {
                    Alignment = Drawing.StringAlignment.Center,
                    LineAlignment = Drawing.StringAlignment.Center
                };
                e.Graphics.DrawString(text, e.Font ?? platformComboBox.Font ?? Drawing.SystemFonts.DefaultFont, brush, e.Bounds, format);
                e.DrawFocusRectangle();
            };

            confirmPlatformButton = new WinForms.Button
            {
                Text = "Confirm Platform",
                Dock = WinForms.DockStyle.Fill,
                Font = new Drawing.Font("Segoe UI", stepFontSize, Drawing.FontStyle.Bold),
                BackColor = Drawing.Color.LightGoldenrodYellow,
                FlatStyle = WinForms.FlatStyle.Flat
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
                BackColor = logBackgroundImage == null ? Drawing.Color.LightGray : Drawing.Color.Transparent,
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
            logLayout.RowStyles.Add(new WinForms.RowStyle(WinForms.SizeType.AutoSize));
            logLayout.Controls.Add(logHost, 0, 0);

            clearLogButton = new WinForms.Button
            {
                Text = "Clear Log",
                AutoSize = true,
                Anchor = WinForms.AnchorStyles.Bottom | WinForms.AnchorStyles.Right,
                BackColor = Drawing.Color.FromArgb(255, 230, 230),
                FlatStyle = WinForms.FlatStyle.Flat
            };
            clearLogButton.Margin = new WinForms.Padding(10, 5, 10, 5);

            saveLogButton = new WinForms.Button
            {
                Text = "Save Log",
                AutoSize = true,
                Anchor = WinForms.AnchorStyles.Bottom | WinForms.AnchorStyles.Right,
                BackColor = Drawing.Color.FromArgb(230, 255, 230),
                FlatStyle = WinForms.FlatStyle.Flat
            };
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

            Controls.Add(logBackgroundPanel);
            Controls.Add(controlPanel);
            Controls.Add(statusPanel);
        }

        private WinForms.Button CreateStyledButton(string text, Drawing.Color backColor, Drawing.Color foreColor, bool bold = false, int fontSize = 11)
        {
            var button = new WinForms.Button
            {
                Text = text,
                Dock = WinForms.DockStyle.Fill,
                FlatStyle = WinForms.FlatStyle.Flat,
                BackColor = backColor,
                ForeColor = foreColor,
                Font = new Drawing.Font("Segoe UI", fontSize, bold ? Drawing.FontStyle.Bold : Drawing.FontStyle.Regular),
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

        private async void RunMainCodeButton_Click(object? sender, EventArgs e)
        {
            string batPath = Path.Combine(preDumpPath, Constants.MakeCsvScript);
            if (!File.Exists(batPath))
            {
                WinForms.MessageBox.Show("Make_csv_file.bat not found");
                return;
            }

            runMainCodeButton.Enabled = false;
            openReleaseNoteButton.Enabled = false;
            logService.Append("[Executing Main Script...]");

            await scriptExecutor.RunScriptAsync(batPath, line =>
            {
                if (line.StartsWith("ARCH=", StringComparison.OrdinalIgnoreCase))
                {
                    string arch = line.Substring(5).Trim().ToUpper();
                    detectedArch = arch;
                    logService.Append($"[Detected Platform Vender] → {arch}");
                }
                else
                {
                    logService.Append(line);
                }
            });

            logService.Append($"[Next Excel File Target] → {detectedArch}");
            logService.Append("[DONE]");
            runMainCodeButton.Enabled = true;
            openReleaseNoteButton.Enabled = true;
        }

        private void CheckFilesButton_Click(object? sender, EventArgs e)
        {
            logService.Clear();
            var result = materialChecker.CheckMaterials(preDumpPath);
            foreach (var msg in result.Messages)
                logService.Append(msg);

            statusLabel.Text = result.AllExist ? "Status: ✅ All files found" : "Status: ⚠ Incomplete files";
            selectedPlatform = "";
            selectedPlatformConfirmed = "";
            allFilesExist = result.AllExist;
            platformComboBox.Items.Clear();
            platformComboBox.Items.AddRange(result.Platforms.ToArray());
            if (result.Platforms.Count > 0) platformComboBox.SelectedIndex = 0;

            openReleaseNoteButton.Enabled = false;
            runMainCodeButton.Enabled = false;
            UpdateRunButtonState();
        }

        private void OpenReleaseNoteButton_Click(object? sender, EventArgs e)
        {
            string[] defaultNote = Directory.GetFiles(preDumpPath, Constants.ReleaseNotePattern);
            logService.Append($"Release note:{string.Join(", ", defaultNote)}");
            string notePath = defaultNote.FirstOrDefault() ?? "";

            if (!string.IsNullOrEmpty(detectedArch))
            {
                string expectedFile = detectedArch.ToUpper() switch
                {
                    "AMD" => Constants.ReleaseNotePattern,
                    "INTEL" => Constants.ReleaseNotePattern,
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
                        logService.Append($"[Warning] Expected Excel not found: {expectedFile}, fallback to default.");
                    }
                }
            }

            if (File.Exists(notePath))
                Process.Start(new ProcessStartInfo(notePath) { UseShellExecute = true });
            else
                WinForms.MessageBox.Show("Release note file not found.", "Error", WinForms.MessageBoxButtons.OK, WinForms.MessageBoxIcon.Error);
        }

        private void ConfirmPlatformButton_Click(object? sender, EventArgs e)
        {
            if (platformComboBox.SelectedItem != null)
            {
                selectedPlatform = platformComboBox.SelectedItem.ToString() ?? "";
                selectedPlatformConfirmed = selectedPlatform;
                logService.Append($"[Selected Platform] → {selectedPlatform}");
                UpdateRunButtonState();
            }
            else
            {
                WinForms.MessageBox.Show("Please select a platform.");
            }
        }

        private void AboutButton_Click(object? sender, EventArgs e)
        {
            WinForms.MessageBox.Show("BIOS Release Tool\nAuthor:\n\tJarvey\n\tWilliam\n\tDobby\n\n🎉 You're awesome for checking the About page! 🎉", "About", WinForms.MessageBoxButtons.OK, WinForms.MessageBoxIcon.Information);
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
                    File.WriteAllText(dialog.FileName, logService.GetText(), Encoding.UTF8);
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

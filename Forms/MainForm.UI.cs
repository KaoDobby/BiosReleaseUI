using System;

// WinForms using naming
using WinForms = System.Windows.Forms;
using WinFormsIntegration = System.Windows.Forms.Integration;
using Drawing = System.Drawing;

// WPF using naming
using WpfControls = System.Windows.Controls;
using WpfMedia = System.Windows.Media;

namespace BiosReleaseUI
{
    public partial class MainForm
    {
        private void InitializeComponent()
        {
            int stepBtnHeight = 85;
            int groupBoxHeight = 170;
            int stepFontSize = 17;

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
            statusPanel.Controls.Add(statusLabel);
            statusPanel.Controls.Add(aboutButton);

            var controlPanel = new WinForms.TableLayoutPanel
            {
                Dock = WinForms.DockStyle.Top,
                RowCount = 4,
                ColumnCount = 1,
                Height = 440,
                Padding = new WinForms.Padding(10),
                AutoSize = true
            };
            controlPanel.RowStyles.Add(new WinForms.RowStyle(WinForms.SizeType.Absolute, stepBtnHeight));
            controlPanel.RowStyles.Add(new WinForms.RowStyle(WinForms.SizeType.Absolute, groupBoxHeight));
            controlPanel.RowStyles.Add(new WinForms.RowStyle(WinForms.SizeType.Absolute, stepBtnHeight));
            controlPanel.RowStyles.Add(new WinForms.RowStyle(WinForms.SizeType.Absolute, stepBtnHeight));

            checkFilesButton = CreateStyledButton("① Check Material Files", Drawing.Color.FromArgb(220, 230, 250), Drawing.Color.DarkBlue, true, stepFontSize);
            runMainCodeButton = CreateStyledButton("③ Execute Make_csv_file.bat", Drawing.Color.FromArgb(230, 250, 230), Drawing.Color.DarkGreen, true, stepFontSize);
            openReleaseNoteButton = CreateStyledButton("④ Open BIOS_RELEASE_NOTE.xlsm", Drawing.Color.FromArgb(250, 240, 200), Drawing.Color.SaddleBrown, true, stepFontSize);

            checkFilesButton.Height = stepBtnHeight;
            runMainCodeButton.Height = stepBtnHeight;
            openReleaseNoteButton.Height = stepBtnHeight;

            runMainCodeButton.Enabled = false;
            openReleaseNoteButton.Enabled = false;

            var platformGroupBox = new WinForms.GroupBox
            {
                Text = "② Platform Selection",
                Height = groupBoxHeight,
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
            platformComboBox.ItemHeight = stepBtnHeight;

            confirmPlatformButton = new WinForms.Button
            {
                Text = "Confirm Platform",
                Dock = WinForms.DockStyle.Fill,
                Font = new Drawing.Font("Segoe UI", stepFontSize, Drawing.FontStyle.Bold),
                Height = 40,
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
            clearLogButton.Margin = new WinForms.Padding(10, 5, 10, 5);

            saveLogButton = new WinForms.Button
            {
                Text = "Save Log",
                Width = 155,
                Height = 45,
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
    }
}


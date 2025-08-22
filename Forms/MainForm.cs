using System;
using System.IO;

// WinForms using naming
using WinForms = System.Windows.Forms;
using WinFormsIntegration = System.Windows.Forms.Integration;
using Drawing = System.Drawing;

// WPF using naming
using WpfControls = System.Windows.Controls;

namespace BiosReleaseUI
{
    public partial class MainForm : WinForms.Form
    {
        private WinForms.Label statusLabel;
        private WinForms.Button checkFilesButton, runMainCodeButton, openReleaseNoteButton, aboutButton, clearLogButton, saveLogButton;
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

        public MainForm()
        {
            string? fullPath = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.Parent?.FullName;
            projectRoot = fullPath ?? string.Empty;
            preDumpPath = Path.Combine(projectRoot, "Pre_Dump");
            string imagePath = Path.Combine(projectRoot, "BiosReleaseUI", "bg.jpg");

            if (File.Exists(imagePath))
            {
                try { logBackgroundImage = Drawing.Image.FromFile(imagePath); }
                catch (Exception ex) { WinForms.MessageBox.Show("Background image input failure : " + ex.Message); }
            }

            InitializeComponent();
            BindEvents();
        }
    }
}


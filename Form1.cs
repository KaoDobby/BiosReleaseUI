using System.IO;
using System;

namespace BiosReleaseUI
{
    public partial class Form1 : Form
    {
        private string preDumpPath;

        public Form1()
        {
            InitializeComponent();

            string projectRoot = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.Parent?.Parent?.FullName ?? "";
            preDumpPath = Path.Combine(projectRoot, "Pre_Dump");
        }

    }
}
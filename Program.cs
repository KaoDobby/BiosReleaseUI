using System;
using System.Windows.Forms;

namespace BiosReleaseUI;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        Application.Run(new MainForm());
    }
}

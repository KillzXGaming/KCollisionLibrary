using System;
using System.Windows.Forms;
using KclLibraryGUI;
using KclLibrary;
using System.IO;
using System.Threading;
using System.Numerics;
using ByamlExt.Byaml;

namespace KclLibraryGUI
{
    public partial class LoadingWindow : Form
    {
        public LoadingWindow()
        {
            InitializeComponent();

            progressBar1.Style = ProgressBarStyle.Marquee;
            DebugLogger.OnDebuggerUpdated += LoggerUpdated;
        }

        private void LoggerUpdated(object sender, EventArgs e) {

            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    consoleLogger.AppendText((string)sender + "\r\n");
                    this.Refresh();
                });
            }
            else
            {
                consoleLogger.AppendText((string)sender + "\r\n");
                this.Refresh();
            }
        }

        private void LoadingWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            DebugLogger.OnDebuggerUpdated -= LoggerUpdated;
        }
    }
}

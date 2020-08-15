using KclLibrary;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KclImporter
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

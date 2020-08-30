using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using KclLibraryGUI;
using KclLibrary;
using System.IO;
using System.Numerics;
using ByamlExt.Byaml;

namespace KclImporter
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            CollisionPresetData.LoadPresets(Directory.GetFiles("CollisionPresets"));
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Supported Formats|*.obj;";
            ofd.Multiselect = true;
            if (ofd.ShowDialog() == DialogResult.OK) {

                foreach (var file in ofd.FileNames) {

                    var thread = new Thread(() =>
                    {
                        try
                        {
                            var result = CollisionLoader.CreateCollisionFromObject(this, file);
                            this.Invoke((MethodInvoker)delegate
                            {
                                if (result.KclFie != null)
                                    CollisionLoader.SaveKCL(result.KclFie, file, result.AttributeByml);
                            });
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.ToString());
                        }
                        CollisionLoader.CloseConsole(this);

                    });
                    thread.Start();
                }
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Supported Formats|*.kcl;";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                KCLFile kcl = new KCLFile(ofd.FileName);

                SaveFileDialog sfd = new SaveFileDialog();
                sfd.Filter = "Supported Formats|*.obj;";
                sfd.FileName = Path.GetFileNameWithoutExtension(ofd.FileName);
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    var obj = kcl.CreateGenericModel();
                    obj.Save(sfd.FileName, true);
                }
            }
        }

        private void ExportKCL()
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}

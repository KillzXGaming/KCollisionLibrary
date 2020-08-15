using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using KCLExt;
using KclLibrary;
using System.IO;
using System.Threading;
using System.Numerics;

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
                foreach (var file in ofd.FileNames)
                    ImportObjectFile(file, ofd.FileNames.Length > 1);
            }
        }


        private void ImportObjectFile(string fileName, bool batchExport)
        {
            var objectFile = new ObjModel(fileName);
            var materials = objectFile.GetMaterialNameList();
            var meshes = objectFile.GetMeshNameList();

            MaterialSetForm form = new MaterialSetForm(materials, meshes);
            if (form.ShowDialog() != DialogResult.OK)
                return;

            LoadingWindow window = new LoadingWindow();
            window.Show(this);

            Thread Thread = new Thread((ThreadStart)(() =>
            {
                var matDictionary = form.Result;
                var endianness = form.GetEndianness;
                var version = form.GetVersion;
                var preset = MaterialSetForm.ActiveGamePreset;
                var settings = new CollisionImportSettings()
                {
                    SphereRadius = preset.SphereRadius,
                    PrisimThickness = preset.PrismThickness,
                    PaddingMax = new Vector3(preset.PaddingMax),
                    PaddingMin = new Vector3(preset.PaddingMin),
                    MaxRootSize = preset.MaxRootSize,
                    MinCubeSize = preset.MinCubeSize,
                    MaxTrianglesInCube = preset.MaxTrianglesInCube,
                    MinRootSize = preset.MinRootSize,
                };

                foreach (var mesh in objectFile.Meshes)
                {
                    for (int f = 0; f < mesh.Faces.Count; f++)
                    {
                        if (form.UseObjectMaterials)
                        {
                            if (matDictionary.ContainsKey(mesh.Faces[f].Material))
                                mesh.Faces[f].SetCollisionAttribute(matDictionary[mesh.Faces[f].Material]);
                        }
                        else if (matDictionary.ContainsKey(mesh.Name))
                            mesh.Faces[f].SetCollisionAttribute(matDictionary[mesh.Name]);
                    }
                }

                var triangles = objectFile.ToTriangles();
                if (version != FileVersion.Version2 && triangles.Count > ushort.MaxValue / 4)
                {
                    MessageBox.Show($"KCL must be below {triangles.Count > ushort.MaxValue / 4} polys!");
                    window.Close();
                    return;
                }

                var kcl = new KCLFile(triangles, version, endianness);

                window.Invoke((MethodInvoker)delegate
                {
                    SaveFileDialog sfd = new SaveFileDialog();
                    sfd.Filter = "Supported Formats|*.kcl;";
                    sfd.FileName = Path.GetFileNameWithoutExtension(fileName);
                    if (sfd.ShowDialog() == DialogResult.OK) {
                        kcl.Save(sfd.FileName);
                    }

                    window?.Close();
                });
            }));
            Thread.Start();
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Supported Formats|*.kcl;";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                if (ofd.FileNames.Length > 1)
                {

                }
                else
                {
                    KCLFile kcl = new KCLFile(ofd.FileName);

                    SaveFileDialog sfd = new SaveFileDialog();
                    sfd.Filter = "Supported Formats|*.obj;";
                    sfd.FileName = Path.GetFileNameWithoutExtension(ofd.FileName);
                    if (sfd.ShowDialog() == DialogResult.OK) {
                        var obj = kcl.CreateGenericModel();
                        obj.Save(sfd.FileName, true);
                    }
                }
            }
        }

        private void ExportKCL()
        {

        }
    }
}

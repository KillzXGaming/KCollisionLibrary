using System;
using System.Windows.Forms;
using KclLibrary;
using System.IO;
using System.Threading.Tasks;
using System.Numerics;
using ByamlExt.Byaml;

namespace KclLibraryGUI
{
    public class CollisionLoader
    {
        public class KclResult
        {
            public BymlFileData AttributeByml = null;
            public KCLFile KclFie = null;
        }

        private static LoadingWindow LoadingWindow = null;

        public static void LoadConsole(Form parent)
        {
            if (LoadingWindow == null || LoadingWindow.IsDisposed)
            {
                parent.Invoke((MethodInvoker)delegate
                {
                    LoadingWindow = new LoadingWindow();
                    LoadingWindow.Show(parent);
                });
            }
        }

        public static void CloseConsole(Form parent)
        {
            if (LoadingWindow != null && !LoadingWindow.IsDisposed)
            {
                parent.Invoke((MethodInvoker)delegate
                {
                    LoadingWindow.Close();
                    LoadingWindow = null;
                });
            }

        }

        public static KclResult CreateCollisionFromObject(Form parent, string fileName)
        {
            var objectFile = new ObjModel(fileName);
            var materials = objectFile.GetMaterialNameList();
            var meshes = objectFile.GetMeshNameList();

            MaterialSetForm form = new MaterialSetForm(materials, meshes);
            if (form.ShowDialog() != DialogResult.OK)
                return new KclResult();

            LoadConsole(parent);

            var kclFile = ImportObjectFile(form, objectFile).Result;
            return kclFile;
        }
     
        static async Task<KclResult> ImportObjectFile(MaterialSetForm form, ObjModel objectFile)
        {
            KclResult kcl = new KclResult();
            await Task.Run(() =>
            {
                kcl.AttributeByml = form.AttributeByml;
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
                                mesh.Faces[f] = new ObjFace(mesh.Faces[f], matDictionary[mesh.Faces[f].Material]);
                        }
                        else if (matDictionary.ContainsKey(mesh.Name))
                            mesh.Faces[f] = new ObjFace(mesh.Faces[f], matDictionary[mesh.Name]);
                    }
                }

                var triangles = objectFile.ToTriangles();
                if (version != FileVersion.Version2 && triangles.Count > ushort.MaxValue / 4) {
                    MessageBox.Show($"Version 1 KCL (Wii, GC, DS, 3DS) must be below {ushort.MaxValue / 4} polys! Poly Count: {triangles.Count}");
                }
                else
                {
                    kcl.KclFie = new KCLFile(triangles, version, endianness);
                }
            });
            return kcl;
        }

        public static void SaveKCL(KCLFile kcl, string fileName, BymlFileData AttributeByml)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Supported Formats|*.kcl;";
            sfd.FileName = Path.GetFileNameWithoutExtension(fileName);
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                kcl.Save(sfd.FileName);

                if (AttributeByml != null)
                {
                    File.WriteAllBytes($"{sfd.FileName.Replace(".kcl", "")}Attribute.byml", ByamlFile.SaveN(AttributeByml));
                }
            }
        }
    }
}

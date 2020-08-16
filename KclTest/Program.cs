using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KclLibrary;
using Syroot.BinaryData;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Numerics;

namespace KclTest
{
    class Program
    {
        static void Main(string[] args)
        {
            bool bigEndian = args.Contains("-be");

            ObjModel obj = new ObjModel("map.obj");
            var settings = new CollisionImportSettings()
            {
                //Octree Settings
                PaddingMax = new System.Numerics.Vector3(1, 1, 1),
                PaddingMin = new System.Numerics.Vector3(-1, -1, -1),
                MaxRootSize = 1024,
                MinRootSize = 128,
                MinCubeSize = 128,
                MaxTrianglesInCube = 50,
                //Model Settings
                PrismThickness = 1,
            };

            KCLFile kclFile = new KCLFile(obj.ToTriangles(), FileVersion.VersionGC, true, settings);
            var collisionHit = kclFile.CheckHit(new Vector3(10, 200.15f, 40.0f));
            //Stores prisim and distance info
            var prisim = collisionHit.Prism;
            var dist = collisionHit.Distance;

            foreach (var file in args)
            {
                string ext = Path.GetExtension(file);
                string name = file.Replace(ext, string.Empty);
                if (ext == ".obj")
                {
                    var obj = new ObjModel(file);
                    var kcl = new KCLFile(obj.ToTriangles(), FileVersion.Version2, bigEndian);
                    kcl.Save($"{name}.kcl");
                }
                if (ext == ".kcl")
                {
                    var kcl = new KCLFile(file);
                    var obj = kcl.CreateGenericModel();
                    obj.Save($"{name}.obj");
                }
            }
        }

        //Exports the collision model as obj and reimports it into a new generated collision file.

        static void ReimportCollisonTPGC(string fileName)
        {
            var kcl = new KCLFile(fileName);

            Dictionary<int, ushort> materialIds = new Dictionary<int, ushort>();
            for (int i = 0; i < kcl.Models[0].Prisms.Length; i++)
            {
                materialIds.Add(i, kcl.Models[0].Prisms[i].CollisionFlags);
            }

            var obj = kcl.CreateGenericModel();
            //Export the kcl to a generic object file
            obj.Save($"{fileName}.obj");


            var settings = new CollisionImportSettings()
            {
                //Octree Settings
                PaddingMax = new System.Numerics.Vector3(1, 1, 1),
                PaddingMin = new System.Numerics.Vector3(-1, -1, -1),
                MaxRootSize = 1024,
                MinRootSize = 128,
                MinCubeSize = 128,
                MaxTrianglesInCube = 50,
                //Model Settings
                PrismThickness = 1,
            };

            var triangles = obj.ToTriangles();
            kcl = new KCLFile(triangles, FileVersion.VersionGC, true, settings);
            for (int i = 0; i < kcl.Models[0].Prisms.Length; i++)
            {
                kcl.Models[0].Prisms[i].CollisionFlags = materialIds[i];
            }

            kcl.Save($"{fileName}.new.kcl");
        }

        static void ReimportCollison(string fileName, FileVersion version, bool bigEndian, CollisionImportSettings settings = null)
        {
            var kcl = new KCLFile(fileName);
            Console.WriteLine($"Smallest Cube Size {kcl.Models[0].GetMinCubeSize()}");
            Console.WriteLine($"Max Tri {kcl.Models[0].GetMaxTriangleCount()}");
            Console.WriteLine($"Padding Size {kcl.Models[0].GetCoordinatePadding()}");
            Console.WriteLine($"Depth {kcl.Models[0].GetMaxOctreeDepth()}");

            var obj = kcl.CreateGenericModel();
            obj.Save($"{fileName}.obj");

            var triangles = obj.ToTriangles();

            kcl = new KCLFile(triangles, version, bigEndian, settings);
            kcl.Save($"{fileName}.new.kcl");
        }
    }
}

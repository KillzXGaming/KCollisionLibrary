using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KclLibrary;
using Syroot.BinaryData;
using System.Diagnostics;
using System.Threading;

namespace KclTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var obj = new ObjModel("course.obj");
            var kcl = new KCLFile(obj.ToTriangles(), FileVersion.Version2, true);
            kcl.Save("course.test.kcl");

            //MKWII Settings
            var settings = new CollisionImportSettings()
            {
                //Octree Settings
                PaddingMax = new System.Numerics.Vector3(250, 250, 250),
                PaddingMin = new System.Numerics.Vector3(-250, -250, -250),
                MaxRootSize = 2048,
                MinCubeSize = 512,
                MaxTrianglesInCube = 60,
                //Model Settings
                PrisimThickness = 300,
                SphereRadius = 250,
            };
            ReimportCollison("CityWorldHomeBuilding022.kcl", FileVersion.Version2, false);
            ReimportCollison("course.kcl", FileVersion.Version2, true);
            return;
        }

        //Exports the collision model as obj and reimports it into a new generated collision file.

        static void ReimportCollisonTPGC(string fileName)
        {
            var kcl = new KCLFile(fileName);

            Dictionary<int, ushort> materialIds = new Dictionary<int, ushort>();
            for (int i = 0; i < kcl.Models[0].Prisims.Length; i++)
            {
                materialIds.Add(i, kcl.Models[0].Prisims[i].CollisionFlags);
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
                PrisimThickness = 1,
            };

            var triangles = obj.ToTriangles();
            kcl = new KCLFile(triangles, FileVersion.VersionGC, true, settings);
            for (int i = 0; i < kcl.Models[0].Prisims.Length; i++)
            {
                kcl.Models[0].Prisims[i].CollisionFlags = materialIds[i];
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

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
            //MKWII Settings
            var settings = new CollisionImportSettings()
            {
                //Octree Settings
                PaddingMax = new System.Numerics.Vector3(400, 400, 400),
                PaddingMin = new System.Numerics.Vector3(-400, -400, -400),
                MaxRootSize = 2048,
                MinCubeSize = 512,
                MaxTrianglesInCube = 30,
                CubeBlow = 400,
                //Model Settings
                PrisimThickness = 300,
                SphereRadius = 250,
            };
            ReimportCollison("course.kcl", FileVersion.VersionWII, true, settings);
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
            kcl.Save($"{fileName}.RB.kcl");

            var obj = kcl.CreateGenericModel();
            var triangles = obj.ToTriangles();

            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();

            kcl = new KCLFile(triangles, version, bigEndian, settings);

            stopwatch.Stop();

            Console.WriteLine("Time elapsed: {0}", stopwatch.Elapsed);

            kcl.Save($"{fileName}.new.kcl");
        }
    }
}

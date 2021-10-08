using System;
using System.Collections.Generic;
using KclLibrary;

namespace KCollisionCLI
{
    class Program
    {
        class CommandLineArguments
        {
            public bool Import { get; set; }
            public bool Export { get; set; }

            public string ObjectFilePath { get; set; } = "Model.obj";
            public string CollisionFile { get; set; } = "Model.kcl";

            public Dictionary<string, ushort> MeshesToAttributes = new Dictionary<string, ushort>();
            public Dictionary<string, ushort> MaterialsToAttributes = new Dictionary<string, ushort>();

            public float PaddingSize = 1;

            public int MaxRootSize = 128;
            public int MinRootSize = 128;
            public int MinCubeSize = 128;
            public int MaxTrianglesInCube = 50;
            //Model Settings
            public int PrismThickness = 40;
            public int SphereRadius = 1;

            public bool BigEndian = true;
            public bool IsDS = true;
            public bool IsGCN = true;
            public bool IsWii = true;
        }

        static void Main(string[] args)
        {
            var cmdArgs = new CommandLineArguments();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-import")     cmdArgs.Import = true;
                if (args[i] == "-export")     cmdArgs.Export = true;
                if (args[i].EndsWith(".obj")) cmdArgs.ObjectFilePath = args[i];
                if (args[i].EndsWith(".kcl")) cmdArgs.CollisionFile = args[i];
                if (args[i] == "-meshAtt")
                {
                    //2 required arguments afterwards
                    cmdArgs.MeshesToAttributes.Add(args[i + 1], ushort.Parse(args[i + 2]));
                }
                if (args[i] == "-matAtt")
                {
                    //2 required arguments afterwards
                    cmdArgs.MaterialsToAttributes.Add(args[i + 1], ushort.Parse(args[i + 2]));
                }

                //Endianness
                if (args[i] == "-le") cmdArgs.BigEndian = false;
                //Platform
                if (args[i] == "-ds") cmdArgs.IsDS = true;
                if (args[i] == "-wii") cmdArgs.IsWii = true;
                if (args[i] == "-gcn") cmdArgs.IsGCN = true;

                //Octree and prism settings
                if (args[i] == "-padding") cmdArgs.PaddingSize = float.Parse(args[i + 1]);
                if (args[i] == "-max_root") cmdArgs.MaxRootSize = int.Parse(args[i + 1]);
                if (args[i] == "-min_root") cmdArgs.MinRootSize = int.Parse(args[i + 1]);
                if (args[i] == "-min_cube") cmdArgs.MinCubeSize = int.Parse(args[i + 1]);
                if (args[i] == "-thickness") cmdArgs.PrismThickness = int.Parse(args[i + 1]);
                if (args[i] == "-radius") cmdArgs.SphereRadius = int.Parse(args[i + 1]);
            }

            if (cmdArgs.Export && !string.IsNullOrEmpty(cmdArgs.CollisionFile))
            {
                Console.WriteLine($"Exporting OBJ from KCL {cmdArgs.CollisionFile} obj {cmdArgs.ObjectFilePath}");

                var kcl = new KCLFile(cmdArgs.CollisionFile);
                var obj = kcl.CreateGenericModel();
                //Export the kcl to a generic object file
                obj.Save(cmdArgs.ObjectFilePath);
            }
            if (cmdArgs.Import && !string.IsNullOrEmpty(cmdArgs.ObjectFilePath))
            {
                Console.WriteLine($"Importing OBJ to KCL {cmdArgs.ObjectFilePath}");

                var settings = new CollisionImportSettings()
                {
                    //Octree Settings
                    PaddingMax = new System.Numerics.Vector3(cmdArgs.PaddingSize),
                    PaddingMin = new System.Numerics.Vector3(-cmdArgs.PaddingSize),
                    MaxRootSize = cmdArgs.MaxRootSize,
                    MinRootSize = cmdArgs.MinRootSize,
                    MinCubeSize = cmdArgs.MinCubeSize,
                    MaxTrianglesInCube = cmdArgs.MaxTrianglesInCube,
                    //Model Settings
                    PrismThickness = cmdArgs.PrismThickness,
                    SphereRadius = cmdArgs.SphereRadius,
                };

                FileVersion version = FileVersion.Version2;
                if (cmdArgs.IsDS) version = FileVersion.VersionDS;
                if (cmdArgs.IsWii) version = FileVersion.VersionWII;
                if (cmdArgs.IsGCN) version = FileVersion.VersionGC;

                var obj = new ObjModel(cmdArgs.ObjectFilePath);
                var kcl = new KCLFile(obj.ToTriangles(), version, cmdArgs.BigEndian, settings);
                kcl.Save(cmdArgs.CollisionFile);
            }
        }
    }
}

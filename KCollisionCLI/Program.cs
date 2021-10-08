using System;
using System.Linq;
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
            public bool Is3DS = true;

            public void PrintHelp()
            {
                Console.WriteLine("------ KCL Settings ------");
                Console.WriteLine("-import (imports a given .obj into the collision)");
                Console.WriteLine("-export (exports a collision as .obj)");
                Console.WriteLine("Example: KCollisionCLI.exe -import imported.obj model.kcl (injects imported.obj into model.kcl)");
                Console.WriteLine("Example: KCollisionCLI.exe -export model.kcl exported.obj (exports model.kcl as exported.obj)");
                Console.WriteLine("------ Material Settings ------");
                Console.WriteLine("-meshAtt MeshNameHere ## (speicify the mesh name and ## material number to assign to)");
                Console.WriteLine("-matAtt MaterialNameHere ## (speicify the mesh name and ## material number to assign to)");
                Console.WriteLine("------ Platforms ------");
                Console.WriteLine("Wii U outputs default=");
                Console.WriteLine("-le (save as little endian Switch kcl)");
                Console.WriteLine("-ds (save as DS kcl)");
                Console.WriteLine("-wii (save as Wii kcl)");
                Console.WriteLine("-gcn (save as GCN kcl)");
                Console.WriteLine("-3ds (save as 3DS kcl)");
                Console.WriteLine("------ Extra ------");
                Console.WriteLine("-adv (advanced settings)");
            }

            public void PrintAdvanced()
            {
                Console.WriteLine("-padding ## (specify the amount of padding in octrees)");
                Console.WriteLine("-max_root ## (specify the max root size)");
                Console.WriteLine("-min_root ## (specify the min root size)");
                Console.WriteLine("-min_cube ## (specify the min cube size)");
                Console.WriteLine("-thickness ## (specify the prism thickness)");
                Console.WriteLine("-radius ## (specify the sphere radius)");
            }
        }

        static void Main(string[] args)
        {
            var cmdArgs = new CommandLineArguments();
            if (args.Length == 0 || args.Contains("-h"))
            {
                cmdArgs.PrintHelp();
                return;
            }
            if (args.Contains("-adv"))
            {
                cmdArgs.PrintAdvanced();
                return;
            }

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
                if (args[i] == "-3ds") cmdArgs.Is3DS = true;

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
                if (cmdArgs.Is3DS) version = FileVersion.VersionWII;

                if (cmdArgs.Is3DS)
                    cmdArgs.BigEndian = false;

                var obj = new ObjModel(cmdArgs.ObjectFilePath);
                var kcl = new KCLFile(obj.ToTriangles(), version, cmdArgs.BigEndian, settings);
                kcl.Save(cmdArgs.CollisionFile);
            }
        }
    }
}

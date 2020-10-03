using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using Syroot.BinaryData;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace KclLibrary
{
    /// <summary>
    /// Represents the header of a V2 KCL binary collision file.
    /// </summary>
    public class KCLFile
    {
        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        private const int _version2 = 0x02020000;

        private static readonly int MaxModelPrismCount = 65535 / 4;

        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="KclFile"/> class, created from the given
        /// <paramref name="objModel"/>.
        /// </summary>
        public KCLFile(List<Triangle> triangles, FileVersion version, 
            bool isBigEndian, CollisionImportSettings settings = null)
        {
            if (settings == null) settings = new CollisionImportSettings();

            Version = version;
            ByteOrder = isBigEndian ? ByteOrder.BigEndian : ByteOrder.LittleEndian;
            Replace(triangles, settings);
        }

        /// <summary>
        /// Loads the data from the given file.
        /// </summary>
        /// <param name="fileName">The name of the file to load the data from.</param>
        public KCLFile(string fileName) : base()
        {
            using (FileStream stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                Load(stream);
            }
        }

        /// <summary>
        /// Loads the data from the given stream.
        /// </summary>
        /// <param name="fileName">The name of the file to load the data from.</param>
        public KCLFile(Stream stream) : base()
        {
            Load(stream);
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Represents the version of the KCL.
        /// </summary>
        public FileVersion Version { get; set; }

        /// <summary>
        /// Gets the smallest coordinate spanned by the octree in this file.
        /// </summary>
        public Vector3 MinCoordinate { get; private set; }

        /// <summary>
        /// Gets the biggest coordinate spanned by the octree in this file.
        /// </summary>
        public Vector3 MaxCoordinate { get; private set; }

        /// <summary>
        /// Gets the coordinate shift required to compute indices into the octree.
        /// </summary>
        public Vector3U CoordinateShift { get; private set; }

        /// <summary>
        /// Gets the total amount of prisms used in the KCL file.
        /// </summary>
        public int PrismCount { get; private set; }

        /// <summary>
        /// Gets the root node of the model octree.
        /// </summary>
        public ModelOctreeNode ModelOctreeRoot { get; private set; }

        /// <summary>
        /// Gets the list of <see cref="KCLModel"/> instances referenced by the model octree.
        /// </summary>
        public List<KCLModel> Models { get; private set; }

        /// <summary>
        /// Gets or sets the byte order of the KCL file.
        /// </summary>
        public ByteOrder ByteOrder { get; set; }

        /// <summary>
        /// The world transformation of the collision.
        /// </summary>
        public Matrix4x4 Transform = Matrix4x4.Identity;

        // ---- METHODS (PUBLIC) -------------------------------------------------------------------------------------

        /// <summary>
        /// Creates a generic model which can be exported into a .obj file format.
        /// </summary>
        /// <returns></returns>
        public ObjModel CreateGenericModel()
        {
            ObjModel objModel = new ObjModel();

            var mesh = new ObjMesh($"Mesh");
            objModel.Meshes.Add(mesh);

            foreach (KCLModel model in Models) {
                foreach (var face in model.Prisms)
                {
                    var triangle = model.GetTriangle(face);
                    var normal = triangle.Normal;

                    ObjFace objFace = new ObjFace();
                    objFace.Material = $"COL_{face.CollisionFlags.ToString("X")}";
                    objFace.Vertices = new ObjVertex[3];
                    for (int i = 0; i < 3; i++) {
                        objFace.Vertices[i] = new ObjVertex()
                        {
                            Position = triangle.Vertices[i],
                            Normal = normal,
                        };
                    }

                    mesh.Faces.Add(objFace);
                }
            }
            return objModel;
        }

        // ---- METHODS (INTERNAL) -------------------------------------------------------------------------------------

        /// <summary>
        /// Loads the data from the given <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to load the data from.</param>
        /// <param name="leaveOpen"><c>true</c> to leave <paramref name="stream"/> open after loading the instance.
        /// </param>
        public void Load(Stream stream, bool leaveOpen = false)
        {
            using (BinaryDataReader reader = new BinaryDataReader(stream, leaveOpen))
            {
                Read(reader);
            }
        }

        /// <summary>
        /// Saves the data in the given file.
        /// </summary>
        /// <param name="fileName">The name of the file to save the data in.</param>
        public void Save(string fileName)
        {
            using (FileStream stream = new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.Write)) {
                Save(stream);
            }
        }

        /// <summary>
        /// Saves the data from the given <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to load the data from.</param>
        /// </param>
        public void Save(Stream stream)
        {
            using (BinaryDataWriter writer = new BinaryDataWriter(stream))
            {
                Write(writer);
            }
        }

        /// <summary>
        /// Replaces the current collision model from the given
        /// <paramref name="objModel"/>.
        /// </summary>
        /// <param name="objModel">The <see cref="ObjModel"/> to create the collision data from.</param>
        public void Replace(List<Triangle> triangles, CollisionImportSettings settings)
        {
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            // Find the smallest and biggest coordinate (and add padding).
            Vector3 minCoordinate = new Vector3(Single.MaxValue, Single.MaxValue, Single.MaxValue);
            Vector3 maxCoordinate = new Vector3(Single.MinValue, Single.MinValue, Single.MinValue);

            DebugLogger.WriteLine($"Replacing Collision...");

            DebugLogger.WriteLine($"Settings:");
            DebugLogger.WriteLine($"-MaxRootSize {settings.MaxRootSize}");
            DebugLogger.WriteLine($"-MaxTrianglesInCube {settings.MaxTrianglesInCube}");
            DebugLogger.WriteLine($"-MinCubeSize {settings.MinCubeSize}");
            DebugLogger.WriteLine($"-PaddingMax {settings.PaddingMax}");
            DebugLogger.WriteLine($"-PaddingMin {settings.PaddingMin}");
            DebugLogger.WriteLine($"-PrismThickness {settings.PrismThickness}");
            DebugLogger.WriteLine($"-SphereRadius {settings.SphereRadius}");

            DebugLogger.WriteLine($"Calculating bounding sizes...");

            for (int i = 0; i < triangles.Count; i++) {
                for (int v = 0; v < triangles[i].Vertices.Length; v++) {
                    Vector3 position = triangles[i].Vertices[v];
                    minCoordinate.X = Math.Min(position.X, minCoordinate.X);
                    minCoordinate.Y = Math.Min(position.Y, minCoordinate.Y);
                    minCoordinate.Z = Math.Min(position.Z, minCoordinate.Z);
                    maxCoordinate.X = Math.Max(position.X, maxCoordinate.X);
                    maxCoordinate.Y = Math.Max(position.Y, maxCoordinate.Y);
                    maxCoordinate.Z = Math.Max(position.Z, maxCoordinate.Z);
                }
            }

            MinCoordinate = minCoordinate + settings.PaddingMin;
            MaxCoordinate = maxCoordinate + settings.PaddingMax;

            DebugLogger.WriteLine($"MinCoordinate: {MinCoordinate}");
            DebugLogger.WriteLine($"MaxCoordinate: {MaxCoordinate}");

            // Compute square cube size of the world, and with it the coordinate shift for use with the model octree.
            Vector3 size = MaxCoordinate - MinCoordinate;
            int worldLengthExp = Maths.GetNext2Exponent(Math.Min(Math.Min(size.X, size.Y), size.Z));
            int cubeSize = 1 << worldLengthExp;
            Vector3 exponents = new Vector3(
                Maths.GetNext2Exponent(size.X),
                Maths.GetNext2Exponent(size.Y),
                Maths.GetNext2Exponent(size.Z));
            CoordinateShift = new Vector3U(
                (uint)(exponents.X),
                (uint)(exponents.Y),
                (uint)(exponents.Z));

            Models = new List<KCLModel>();
            Vector3 boxSize = new Vector3(
                1 << (int)CoordinateShift.X,
                1 << (int)CoordinateShift.Y,
                1 << (int)CoordinateShift.Z);

            DebugLogger.WriteLine($"Model Octree Bounds: {boxSize}");

            //Create subdivied triangle models
            var modelRoots = CreateModelDivision(MinCoordinate, triangles, boxSize / 2f);

            //For a model octree, we need 8 octrees per division
            ModelOctreeRoot = new ModelOctreeNode();
            ModelOctreeRoot.Children = new ModelOctreeNode[ModelOctreeNode.ChildCount];
            for (int i = 0; i < ModelOctreeNode.ChildCount; i++) {
                ModelOctreeRoot.Children[i] = new ModelOctreeNode();
            }
            Models.Clear();

            //Load all the model data
            CreateModelOctree(modelRoots, ModelOctreeRoot.Children, settings, 0);
            PrismCount = Models.Sum(x => x.Prisms.Length);

            stopWatch.Stop();

            DebugLogger.WriteLine($"Model Octree:");
            PrintModelOctree(ModelOctreeRoot.Children);

            DebugLogger.WriteLine($"Finished Collsion Generation {stopWatch.Elapsed}");
        }

        /// <summary>
        /// Resets the list of prisim and octree data for any that has been previously hit.
        /// </summary>
        public void ResetHits()
        {
            for (int i = 0; i < Models.Count; i++) {
                Models[i].HitPrisms.Clear();
                Models[i].HitOctrees.Clear();
            }
        }

        /// <summary>
        /// Checks if a prism gets hit from a given point and returns hit information.
        /// </summary>
        /// <returns></returns>
        public KCLHit CheckHit(Vector3 point)
        {
            ResetHits();

            point = CollisionHandler.ConvertLocalSpace(Transform, point);
            //Model only has one model to search, check directly instead
            if (Models.Count == 1) return Models[0].CheckHit(point);

            //Check total bounding area
            bool inRange = (MinCoordinate.X < point.X && point.X < MaxCoordinate.X &&
                            MinCoordinate.Y < point.Y && point.Y < MaxCoordinate.Y &&
                            MinCoordinate.Z < point.Z && point.Z < MaxCoordinate.Z);
            if (!inRange)
                return null;

            Vector3 boxSize = new Vector3(
                1 << (int)CoordinateShift.X,
                1 << (int)CoordinateShift.Y,
                1 << (int)CoordinateShift.Z);

            var block = SearchModelBlock(ModelOctreeRoot.Children, point, MinCoordinate, boxSize);
            if (block != null && block.ModelIndex != null)
            {
                var hit = Models[(int)block.ModelIndex].CheckHit(point);
              //  if (hit != null) //Convert local space back to world with the current transformation
                    //      hit.CenterY = Vector3.Transform(new Vector3(0, hit.CenterY, 0), Transform).Y;
                return hit;
            }

            return null;
        }

        // ---- METHODS (PRIVATE) -------------------------------------------------------------------------------------

        private ModelOctreeNode SearchModelBlock(ModelOctreeNode[] children, Vector3 point, Vector3 position, Vector3 boxSize)
        {
            int blockIdx = 0;
            for (int z = 0; z < 2; z++) {
                for (int y = 0; y < 2; y++) {
                    for (int x = 0; x < 2; x++) {
                        Vector3 cubePosition = position + boxSize * new Vector3(x, y, z);
                        Vector3 min = cubePosition - boxSize / 2f;
                        Vector3 max = cubePosition + boxSize / 2f;
                        bool inCube = (min.X < point.X && point.X < max.X &&
                                       min.Y < point.Y && point.Y < max.Y &&
                                       min.Z < point.Z && point.Z < max.Z);

                        if (inCube)
                        {
                            if (children[blockIdx].Children != null)
                                return SearchModelBlock(children[blockIdx].Children, point, cubePosition, boxSize / 2f);
                            else
                                return children[blockIdx];
                        }
                        blockIdx++;
                    }
                }
            }
            return null;
        }

        private void PrintModelOctree(ModelOctreeNode[] children, string indent = "") {
            int index = 0;
            foreach (var octree in children)
            {
                if (octree.ModelIndex.HasValue)
                    DebugLogger.WriteLine($"{indent}index {index} ModelIndex {octree.ModelIndex}");
                else if (octree.Children == null)
                    DebugLogger.WriteLine($"{indent}index {index} Empty Space");

                if (octree.Children != null)
                    PrintModelOctree(octree.Children, indent + "-");

                index++;
            }
        }

        private void CreateModelOctree(List<ModelGroup> modelRoots, ModelOctreeNode[] nodes,
            CollisionImportSettings settings,  uint baseTriCount, int level = 0)
        {
            for (int i = 0; i < modelRoots.Count; i++)
            {
                int nodeIndex = modelRoots[i].BlockIndex;

                if (modelRoots[i].Children.Count > 0)
                {
                    nodes[nodeIndex].Children = new ModelOctreeNode[ModelOctreeNode.ChildCount];
                    for (int j = 0; j < ModelOctreeNode.ChildCount; j++)
                    {
                        nodes[nodeIndex].Children[j] = new ModelOctreeNode();
                    }
                    //Load addtional subidivison models
                    CreateModelOctree(modelRoots[i].Children, nodes[nodeIndex].Children, settings, baseTriCount, level + 1);

                }//If the model has triangle data, add it to our global model list
                else if (modelRoots[i].Triangles.Count > 0)
                {
                    var model = new KCLModel(modelRoots[i].Triangles, baseTriCount, Version, settings);
                    baseTriCount += (uint)modelRoots[i].Triangles.Count;

                    nodes[nodeIndex].ModelIndex = (uint)Models.Count;
                    foreach (var index in modelRoots[i].MergedBlockIndices)
                        nodes[index].ModelIndex = (uint)Models.Count;

                    Models.Add(model);
                }
            }
        }

        private List<ModelGroup> TryMergeModelGroups(List<ModelGroup> modelRoots)
        {
            List<ModelGroup> GlobalList = new List<ModelGroup>();
            //Now go through each model group and merge them if possible in the global list.
            for (int i = 0; i < modelRoots.Count; i++)
            {
                if (modelRoots[i].Children.Count == 0)
                {
                    bool isMerged = false;
                    foreach (var globalModel in GlobalList)
                    {
                        if (modelRoots[i].Triangles.Count + globalModel.Triangles.Count < MaxModelPrismCount)
                        {
                            globalModel.Triangles.AddRange(modelRoots[i].Triangles);
                            globalModel.MergedBlockIndices.Add(i);
                            isMerged = true;
                        }
                    }
                    if (!isMerged)
                        GlobalList.Add(modelRoots[i]);
                }
                else
                {
                    modelRoots[i].Children = TryMergeModelGroups(modelRoots[i].Children);
                    GlobalList.Add(modelRoots[i]);
                }


            }
            return GlobalList;
        }

        //Subdivies a list of triangles into 8 regions.
        //When the max prism count is rearched, it divides again.
        private List<ModelGroup> CreateModelDivision(Vector3 position, List<Triangle> triangles, Vector3 boxSize, int level = 0)
        {
            //Version 1 uses one single model so skip dividing them.
            //Models only split if their poly count is too high, so add a check for it.
            if (Version < FileVersion.Version2 || triangles.Count < MaxModelPrismCount && level == 0) {
                ModelGroup model = new ModelGroup();
                model.Triangles.AddRange(triangles);
                model.BlockIndex = 0;
                model.MergedBlockIndices = new List<int>() { 1, 2, 3, 4, 5, 6, 7 };
                return new List<ModelGroup>() { model };
            }

            //Create a fixed set of 8 model groups.
            ModelGroup[] modelRoots = new ModelGroup[8];
            int index = 0;
            for (int z = 0; z < 2; z++) {
                for (int y = 0; y < 2; y++) {
                    for (int x = 0; x < 2; x++) {
                        //Create a model group for each region
                        ModelGroup model = new ModelGroup();

                        //Get the position of the current region
                        Vector3 cubePosition = position + boxSize * new Vector3(x, y, z);

                        List<Triangle> containedTriangles = new List<Triangle>();
                        for (int i = 0; i < triangles.Count; i++)
                        {
                            //Check for intersecting triangles in the current region.
                            if (TriangleBoxIntersect.TriBoxOverlap(triangles[i], cubePosition + boxSize / 2f, boxSize / 2f))
                                containedTriangles.Add(triangles[i]);
                        }

                        if (containedTriangles.Count >= MaxModelPrismCount)
                            DebugLogger.WriteLine($"Dividing model at {containedTriangles.Count} polygons.");

                        if (level > 2)
                            DebugLogger.WriteError($"Warning! Your KCL has over 3 division levels and may fall through!");

                        //If the children have too many Prisms, divide into 8 more regions as children.
                        if (containedTriangles.Count >= MaxModelPrismCount)
                            model.Children = CreateModelDivision(cubePosition, containedTriangles, boxSize / 2f, level + 1);
                        else //Set the triangle list for this region. If it is empty, it will be skipped later
                            model.Triangles = containedTriangles;

                        model.BlockIndex = index;
                        modelRoots[index] = model;
                        index++;
                    }
                }
            }
            return modelRoots.ToList();
        }

        private class ModelGroup
        {
            //A list of triangles used for the model
            public List<Triangle> Triangles = new List<Triangle>();
            //A list of subdivided models
            public List<ModelGroup> Children = new List<ModelGroup>();
            //The index of the octree block
            public int BlockIndex { get; set; }
            //A list of octree blocks merged into this one.
            public List<int> MergedBlockIndices = new List<int>();
        }

        private void Read(BinaryDataReader reader)
        {
            ModelOctreeRoot = new ModelOctreeNode();
            ModelOctreeRoot.Children = new ModelOctreeNode[ModelOctreeNode.ChildCount];
            Models = new List<KCLModel>();

            reader.ByteOrder = ByteOrder.BigEndian;
            uint value = reader.ReadUInt32();
            Version = (FileVersion)value;

            ByteOrder = CheckByteOrder(reader);
            reader.ByteOrder = this.ByteOrder;

            if ((FileVersion)value != FileVersion.Version2) //Assume the KCL is V1 instead 
            {
                if (value == 56) //Smaller header (GCN)
                    Version = FileVersion.VersionGC;
                else
                {
                    using (reader.TemporarySeek(56, SeekOrigin.Begin)) {
                        if (reader.ReadInt32() == 102400)
                            Version = FileVersion.VersionDS;
                        else
                            Version = FileVersion.VersionWII;
                    }
                }

                reader.Seek(-4); //Seek back and read V1 properly

                //V1 KCL is just a KCL model.
                KCLModel model = new KCLModel();
                model.Read(reader, Version);
                Models.Add(model);

                //Create default model octrees that index the first model
                for (int i = 0; i < ModelOctreeNode.ChildCount; i++) {
                    ModelOctreeRoot.Children[i] = new ModelOctreeNode() { ModelIndex = 0 };
                }

                PrismCount = model.Prisms.Length;
                MinCoordinate = model.MinCoordinate;

                //Todo, auto generate the rest of V2 header data for V1 for cross conversion.
            }
            else
            {
                int octreeOffset = reader.ReadInt32();
                int modelOffsetArrayOffset = reader.ReadInt32();
                int modelCount = reader.ReadInt32();
                MinCoordinate = reader.ReadVector3F();
                MaxCoordinate = reader.ReadVector3F();
                CoordinateShift = reader.ReadVector3U();
                PrismCount = reader.ReadInt32();

                reader.Position = octreeOffset; // Mostly unrequired, data is successive.
                for (int i = 0; i < ModelOctreeNode.ChildCount; i++) {
                    ModelOctreeRoot.Children[i] = new ModelOctreeNode(reader, (uint)octreeOffset);
                }

                // Read the model offsets.
                reader.Position = modelOffsetArrayOffset; // Mostly unrequired, data is successive.
                int[] modelOffsets = reader.ReadInt32s(modelCount);
                Models = new List<KCLModel>(modelCount);
                foreach (int modelOffset in modelOffsets)
                {
                    reader.Position = modelOffset; // Required as loading a model does not position reader at its end.
                    var model = new KCLModel();
                    model.Read(reader, Version);
                    Models.Add(model);
                }
            }
        }

        private ByteOrder CheckByteOrder(BinaryDataReader reader)
        {
            //KCL has no direct way to determine the byte order. 
            //Check the first ofset (octree offset) then check if it's pointed to the end of the header.
            using (reader.TemporarySeek(0, SeekOrigin.Begin))
            {
                reader.ByteOrder = ByteOrder.BigEndian;

                uint value = reader.ReadUInt32();
                if (value == _version2) //V2 KCL
                {
                    if (reader.ReadUInt32() == 56)
                        return ByteOrder.BigEndian;
                    else
                        return ByteOrder.LittleEndian;
                }
                else //V1 KCL
                {
                    if (value == 60 || value == 56) //First uint in V1 header is the octree ofset
                        return ByteOrder.BigEndian;
                    else
                        return ByteOrder.LittleEndian;
                }
            }
        }

        private void Write(BinaryDataWriter writer)
        {
            DebugLogger.WriteLine($"Writing binary {this.ByteOrder} {Version}");

            writer.ByteOrder = this.ByteOrder;
            if (Version == FileVersion.Version2)
                WriteV2(writer);
            else
                WriteV1(writer);
        }

        private void WriteV1(BinaryDataWriter writer) {
            //v1 only has a single model header.
            Models[0].Write(writer, Version);
        }

        private void WriteV2(BinaryDataWriter writer)
        {
            writer.ByteOrder = ByteOrder.BigEndian;
            writer.Write((uint)Version);
            writer.ByteOrder = this.ByteOrder;

            Offset octreeOffset = writer.ReserveOffset();
            Offset modelOffsetArrayOffset = writer.ReserveOffset();
            writer.Write(Models.Count);
            writer.Write(MinCoordinate);
            writer.Write(MaxCoordinate);
            writer.Write(CoordinateShift);
            writer.Write(PrismCount);

            // Write the model octree.
            octreeOffset.Satisfy();
            foreach (ModelOctreeNode rootChild in ModelOctreeRoot) 
                rootChild.Write(writer);

            int branchKey = 8;
            foreach (ModelOctreeNode rootChild in ModelOctreeRoot)
                rootChild.WriteChildren(writer, ref branchKey);

            // Write the model offsets.
            modelOffsetArrayOffset.Satisfy();
            Offset[] modelOffsets = writer.ReserveOffset(Models.Count);

            // Write the models.
            for (int i = 0; i < Models.Count; i++)
            {
                modelOffsets[i].Satisfy();
                Models[i].Write(writer, Version);
                writer.Align(4);
            }
        }
    }
}

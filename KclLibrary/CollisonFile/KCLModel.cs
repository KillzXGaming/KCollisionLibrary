﻿using System;
using System.Collections.Generic;
using System.Linq;
using Syroot.BinaryData;
using System.Runtime.InteropServices;
using System.Numerics;

namespace KclLibrary
{
    public class KCLModel
    {
        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        public KCLModel() { }

        public KCLModel(List<Triangle> triangleList, uint baseTriCount,
            FileVersion version, CollisionImportSettings settings)
        {
            // Transfer the faces to collision faces and find the smallest and biggest coordinates.
            Vector3 minCoordinate = new Vector3(Single.MaxValue, Single.MaxValue, Single.MaxValue);
            Vector3 maxCoordinate = new Vector3(Single.MinValue, Single.MinValue, Single.MinValue);

            List<KclPrisim> prisimList = new List<KclPrisim>();
            Dictionary<ushort, Triangle> triangles = new Dictionary<ushort, Triangle>();

            Positions = new List<Vector3>();
            Normals = new List<Vector3>();
            Prisims = new KclPrisim[0];
            Version = version;

            PrisimThickness = settings.PrisimThickness;
            SphereRadius = settings.SphereRadius;

            Dictionary<int, int> positionHashTable = new Dictionary<int, int>();
            Dictionary<int, int> normalHashTable = new Dictionary<int, int>();

            ushort triindex = 0;
            for (int i = 0; i < triangleList.Count; i++)
            {
                var triangle = triangleList[i];

                Vector3 direction = Vector3.Cross(
                    triangle.Vertices[1] - triangle.Vertices[0],
                    triangle.Vertices[2] - triangle.Vertices[0]);

                if ((direction.X * direction.X + direction.Y * direction.Y + direction.Z * direction.Z) < 0.01) continue;
                direction = Vector3.Normalize(direction);

                // Get the position vectors and find the smallest and biggest coordinates.
                for (int j = 0; j < 3; j++)
                {
                    Vector3 position = triangle.Vertices[j];
                    minCoordinate.X = Math.Min(position.X, minCoordinate.X);
                    minCoordinate.Y = Math.Min(position.Y, minCoordinate.Y);
                    minCoordinate.Z = Math.Min(position.Z, minCoordinate.Z);
                    maxCoordinate.X = Math.Max(position.X, maxCoordinate.X);
                    maxCoordinate.Y = Math.Max(position.Y, maxCoordinate.Y);
                    maxCoordinate.Z = Math.Max(position.Z, maxCoordinate.Z);
                }

                //Calculate the ABC normal values.
                Vector3 normalA = Vector3.Cross(direction,
                    triangle.Vertices[2] - triangle.Vertices[0]);

                Vector3 normalB = (-(Vector3.Cross(direction,
                     triangle.Vertices[1] - triangle.Vertices[0])));

                Vector3 normalC = Vector3.Cross(direction,
                     triangle.Vertices[1] - triangle.Vertices[2]);

                //Normalize the ABC normal values.
                normalA = Vector3.Normalize(normalA);
                normalB = Vector3.Normalize(normalB);
                normalC = Vector3.Normalize(normalC);

                //Create a KCL prisim
                KclPrisim face = new KclPrisim()
                {
                    PositionIndex = (ushort)IndexOfVertex(triangle.Vertices[0], Positions, positionHashTable),
                    DirectionIndex = (ushort)IndexOfVertex(direction, Normals, normalHashTable),
                    Normal1Index = (ushort)IndexOfVertex(normalA, Normals, normalHashTable),
                    Normal2Index = (ushort)IndexOfVertex(normalB, Normals, normalHashTable),
                    Normal3Index = (ushort)IndexOfVertex(normalC, Normals, normalHashTable),
                    GlobalIndex = baseTriCount + (uint)prisimList.Count,
                    CollisionFlags = triangle.Attribute,
                };

                // Compute the face direction (normal) and add it to the normal list.
                triangles.Add((ushort)triindex++, triangle);

                //Compute the length
                float length = Vector3.Dot(triangle.Vertices[1] - triangle.Vertices[0], normalC);
                face.Length = length;

                prisimList.Add(face);
            }

            positionHashTable.Clear();
            normalHashTable.Clear();

            //No triangles found to intersect the current box, return.
            if (prisimList.Count == 0) return;

            //Padd the coordinates
            minCoordinate += settings.PaddingMin;
            maxCoordinate += settings.PaddingMax;

            MinCoordinate = minCoordinate;
            Prisims = prisimList.ToArray();

            // Compute the octree.
            Vector3 size = maxCoordinate - minCoordinate;
            Vector3U exponents = new Vector3U(
                (uint)Maths.GetNext2Exponent(size.X),
                (uint)Maths.GetNext2Exponent(size.Y),
                (uint)Maths.GetNext2Exponent(size.Z));
            int cubeSizePower = Maths.GetNext2Exponent(Math.Min(Math.Min(size.X, size.Y), size.Z));
            if (cubeSizePower > Maths.GetNext2Exponent(settings.MaxRootSize))
                cubeSizePower = Maths.GetNext2Exponent(settings.MaxRootSize);

            int cubeSize = 1 << cubeSizePower;
            CoordinateShift = new Vector3U(
               (uint)cubeSizePower,
               (uint)(exponents.X - cubeSizePower),
               (uint)(exponents.X - cubeSizePower + exponents.Y - cubeSizePower));
            CoordinateMask = new Vector3U(
                (uint)(0xFFFFFFFF << (int)exponents.X),
                (uint)(0xFFFFFFFF << (int)exponents.Y),
                (uint)(0xFFFFFFFF << (int)exponents.Z));
            Vector3U cubeCounts = new Vector3U(
                (uint)Math.Max(1, (1 << (int)exponents.X) / cubeSize),
                (uint)Math.Max(1, (1 << (int)exponents.Y) / cubeSize),
                (uint)Math.Max(1, (1 << (int)exponents.Z) / cubeSize));
            // Generate the root nodes, which are square cubes required to cover all of the model.
            PolygonOctreeRoots = new PolygonOctree[cubeCounts.X * cubeCounts.Y * cubeCounts.Z];

            DebugLogger.WriteLine($"Creating Octrees {cubeCounts}");

            int index = 0;
            for (int z = 0; z < cubeCounts.Z; z++)
            {
                for (int y = 0; y < cubeCounts.Y; y++)
                {
                    for (int x = 0; x < cubeCounts.X; x++)
                    {
                        Vector3 cubePosition = minCoordinate + ((float)cubeSize) * new Vector3(x, y, z);
                        PolygonOctreeRoots[index++] = new PolygonOctree(triangles, cubePosition, cubeSize,
                            settings.MaxTrianglesInCube, settings.MinCubeSize);
                    }
                }
            }
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the smallest coordinate of the cube spanned by the model.
        /// </summary>
        public Vector3 MinCoordinate { get; internal set; }

        /// <summary>
        /// Gets the coordinate mask required to compute indices into the octree.
        /// </summary>
        public Vector3U CoordinateMask { get; internal set; }

        /// <summary>
        /// Gets the coordinate shift required to compute indices into the octree.
        /// </summary>
        public Vector3U CoordinateShift { get; internal set; }

        /// <summary>
        /// Gets the array of vertex positions.
        /// </summary>
        public List<Vector3> Positions { get; internal set; }

        /// <summary>
        /// Gets the array of vertex normals.
        /// </summary>
        public List<Vector3> Normals { get; internal set; }

        /// <summary>
        /// Gets the array of triangles.
        /// </summary>
        public KclPrisim[] Prisims { get; internal set; }

        /// <summary>
        /// Gets or sets the thickness of the prisims.
        /// </summary>
        public float PrisimThickness { get; set; }

        /// <summary>
        /// Gets the root nodes of the model triangle octree. Can be <c>null</c> if no octree was loaded or created yet.
        /// </summary>
        public PolygonOctree[] PolygonOctreeRoots { get; private set; }

        /// <summary>
        /// Gets or sets the thickness of the prisims.
        /// </summary>
        public float SphereRadius { get; set; }

        /// <summary>
        /// Gets the current file version to use in the binary file.
        /// </summary>
        public FileVersion Version { get; private set; } = FileVersion.Version2;

        /// <summary>
        /// A list of prisims which are hit detectedfrom the collision handler.
        /// </summary>
        public List<KclPrisim> HitPrisims { get; internal set; } = new List<KclPrisim>();

        /// <summary>
        /// A list of octrees which are hit detected from the collision handler.
        /// </summary>
        public List<PolygonOctree> HitOctrees { get; internal set; } = new List<PolygonOctree>();

        // ---- METHODS (PUBLIC) -------------------------------------------------------------------------------------

        /// <summary>
        /// Creates a triangle with 3 positions from the given collision prisim.
        /// </summary>
        /// <returns></returns>
        public Triangle GetTriangle(KclPrisim prisim)
        {
            Vector3 A = Positions[prisim.PositionIndex];
            Vector3 CrossA = Vector3.Cross(Normals[prisim.Normal1Index], Normals[prisim.DirectionIndex]);
            Vector3 CrossB = Vector3.Cross(Normals[prisim.Normal2Index], Normals[prisim.DirectionIndex]);
            Vector3 B = A + CrossB * (prisim.Length / Vector3.Dot(CrossB, Normals[prisim.Normal3Index]));
            Vector3 C = A + CrossA * (prisim.Length / Vector3.Dot(CrossA, Normals[prisim.Normal3Index]));
            return new Triangle(A, B, C);
        }

        public KCLHit CheckHit(Vector3 point)
        {
            HitPrisims.Clear();
            var hit = CollisionHandler.CheckPoint(this, point, 1, 1);
            return hit;
        }

        /// <summary>
        /// Gets the maximum triangle count used by an octree.
        /// </summary>
        public uint GetMaxTriangleCount()
        {
            uint maxTriangleCount = uint.MinValue;

            var boundings = GetOctreeBoundings();
            for (int i = 0; i < boundings.Count; i++)
            {
                if (boundings[i].Octree.TriangleIndices == null)
                    continue;

                int size = boundings[i].Octree.TriangleIndices.Count;
                maxTriangleCount = (uint)Math.Max(maxTriangleCount, size);
            }
            return maxTriangleCount;
        }

        /// <summary>
        /// Gets the smallest cube size used for the octrees.
        /// </summary>
        public float GetMinCubeSize()
        {
            float minCubeSize = float.MaxValue;

            var boundings = GetOctreeBoundings();
            for (int i = 0; i < boundings.Count; i++)
            {
                float size = boundings[i].Size;
                minCubeSize = Math.Min(minCubeSize, size);
            }
            return minCubeSize;
        }

        /// <summary>
        /// Gets a global list of octrees with bounding information.
        /// </summary>
        /// <returns></returns>
        public List<OctreeBounding> GetOctreeBoundings()
        {
            List<OctreeBounding> boundings = new List<OctreeBounding>();

            Vector3 cubeCounts = new Vector3(
                      (~(int)CoordinateMask.X >> (int)CoordinateShift.X) + 1,
                      ((~(int)CoordinateMask.Y >> (int)CoordinateShift.X) + 1),
                      ((~(int)CoordinateMask.Z >> (int)CoordinateShift.X) + 1));

            int cubeSize = 1 << (int)CoordinateShift.X;

            int index = 0;
            for (int z = 0; z < cubeCounts.Z; z++)
            {
                for (int y = 0; y < cubeCounts.Y; y++)
                {
                    for (int x = 0; x < cubeCounts.X; x++)
                    {
                        Vector3 cubePosition = MinCoordinate + ((float)cubeSize) * new Vector3(x, y, z);
                        GetOctreeBounding(boundings, PolygonOctreeRoots[index++], cubePosition, (float)cubeSize);
                    }
                }
            }
            return boundings;
        }

        // ---- METHODS (INTERNAL) -------------------------------------------------------------------------------------

        internal void Read(BinaryDataReader reader, FileVersion version)
        {
            Version = version;

            long modelPosition = reader.Position;
            int positionArrayOffset = reader.ReadInt32();
            int normalArrayOffset = reader.ReadInt32();
            int prisimArrayOffset = reader.ReadInt32();
            int octreeOffset = reader.ReadInt32();

            if (version == FileVersion.VersionDS)
            {
                PrisimThickness = reader.ReadFx32();
                MinCoordinate = reader.ReadVector3Fx32();
                CoordinateMask = reader.ReadVector3U();
                CoordinateShift = reader.ReadVector3U();
                SphereRadius = reader.ReadFx32();
                prisimArrayOffset += 0x10;

                // Read the positions.
                reader.Position = modelPosition + positionArrayOffset; // Mostly unrequired, data is successive.
                int positionCount = (normalArrayOffset - positionArrayOffset) / 12;
                Positions = new List<Vector3>(reader.ReadVector3Fx32s(positionCount));

                // Read the normals.
                reader.Position = modelPosition + normalArrayOffset; // Mostly unrequired, data is successive.
                int normalCount = (prisimArrayOffset - normalArrayOffset) / 6;
                Normals = new List<Vector3>(reader.ReadVector3Fx16s(normalCount));

                // Read the prisims.
                reader.Position = modelPosition + prisimArrayOffset; // Mostly unrequired, data is successive.
                int prisimCount = (octreeOffset - prisimArrayOffset) / (version >= FileVersion.Version2 ? 20 : 16);
                Prisims = reader.ReadPrisims(prisimCount, version);
            }
            else
            {
                PrisimThickness = reader.ReadSingle();
                MinCoordinate = reader.ReadVector3F();
                CoordinateMask = reader.ReadVector3U();
                CoordinateShift = reader.ReadVector3U();
                if (positionArrayOffset > 56) //Older versions don't have the sphere radius property
                    SphereRadius = reader.ReadSingle();

                if (version < FileVersion.Version2) //octree triangle indices are -1 indexed. Shift the offset value.
                    prisimArrayOffset += 0x10;

                // Read the positions.
                reader.Position = modelPosition + positionArrayOffset; // Mostly unrequired, data is successive.
                int positionCount = (normalArrayOffset - positionArrayOffset) / 12;
                Positions = new List<Vector3>(reader.ReadVector3Fs(positionCount));

                // Read the normals.
                reader.Position = modelPosition + normalArrayOffset; // Mostly unrequired, data is successive.
                int normalCount = (prisimArrayOffset - normalArrayOffset) / 12;
                Normals = new List<Vector3>(reader.ReadVector3Fs(normalCount));

                // Read the prisims.
                reader.Position = modelPosition + prisimArrayOffset; // Mostly unrequired, data is successive.
                int prisimCount = (octreeOffset - prisimArrayOffset) / (version >= FileVersion.Version2 ? 20 : 16);
                Prisims = reader.ReadPrisims(prisimCount, version);
            }

            reader.Position = modelPosition + octreeOffset; // Mostly unrequired, data is successive.
            int nodeCount
                 = ((~(int)CoordinateMask.X >> (int)CoordinateShift.X) + 1)
                 * ((~(int)CoordinateMask.Y >> (int)CoordinateShift.X) + 1)
                 * ((~(int)CoordinateMask.Z >> (int)CoordinateShift.X) + 1);

            PolygonOctreeRoots = new PolygonOctree[nodeCount];
            for (int i = 0; i < nodeCount; i++)
            {
                PolygonOctreeRoots[i] = new PolygonOctree(reader, modelPosition + octreeOffset, version);
            }
        }

        internal void Write(BinaryDataWriter writer, FileVersion version)
        {
            long modelPosition = writer.Position;

            Offset positionArrayOffset = writer.ReserveOffset();
            Offset normalArrayOffset = writer.ReserveOffset();
            Offset triangleArrayOffset = writer.ReserveOffset();
            Offset octreeOffset = writer.ReserveOffset();
            if (version == FileVersion.VersionDS)
            {
                writer.WriteFx32(PrisimThickness);
                writer.WriteVector3Fx32(MinCoordinate);
                writer.Write(CoordinateMask);
                writer.Write(CoordinateShift);
                writer.WriteFx32(SphereRadius);

                // Write the positions.
                positionArrayOffset.Satisfy((int)(writer.Position - modelPosition));
                writer.WriteVector3Fx32s(Positions.ToArray());

                // Write the normals.
                normalArrayOffset.Satisfy((int)(writer.Position - modelPosition));
                writer.WriteVector3Fx16s(Normals.ToArray());

                // Write the triangles.
                triangleArrayOffset.Satisfy((int)(writer.Position - modelPosition - 0x10));
                writer.Write(Prisims, version);
            }
            else
            {
                writer.Write(PrisimThickness);
                writer.Write(MinCoordinate);
                writer.Write(CoordinateMask);
                writer.Write(CoordinateShift);
                if (version > FileVersion.VersionGC)
                    writer.Write(SphereRadius);

                // Write the positions.
                positionArrayOffset.Satisfy((int)(writer.Position - modelPosition));
                writer.Write(Positions.ToArray());

                // Write the normals.
                normalArrayOffset.Satisfy((int)(writer.Position - modelPosition));
                writer.Write(Normals.ToArray());

                // Write the triangles.
                if (version < FileVersion.Version2)
                    triangleArrayOffset.Satisfy((int)(writer.Position - modelPosition - 0x10));
                else
                    triangleArrayOffset.Satisfy((int)(writer.Position - modelPosition));
                writer.Write(Prisims, version);
            }

            // Write the octree.
            int octreeOffsetValue = (int)(writer.Position - modelPosition);
            octreeOffset.Satisfy(octreeOffsetValue);

            // Write the node keys, and compute the correct offsets into the triangle lists or to child nodes.
            // Nintendo writes child nodes behind the current node, so the children need to be queued.
            // In this implementation, empty triangle lists point to the same terminator behind the last node.
            int triangleListPos = GetNodeCount(PolygonOctreeRoots) * sizeof(uint);
            Queue<PolygonOctree[]> queuedNodes = new Queue<PolygonOctree[]>();
            Dictionary<ushort[], int> indexPool = CreateIndexBuffer(queuedNodes);

            queuedNodes.Enqueue(PolygonOctreeRoots);
            while (queuedNodes.Count > 0)
            {
                PolygonOctree[] nodes = queuedNodes.Dequeue();
                long offset = writer.Position - modelPosition - octreeOffsetValue;
                foreach (PolygonOctree node in nodes)
                {
                    if (node.Children == null)
                    {
                        // Node is a leaf and points to triangle index list.
                        ushort[] indices = node.TriangleIndices.ToArray();
                        int listPos = triangleListPos + indexPool[indices];
                        node.Key = (uint)ModelOctreeNode.Flags.Values | (uint)(listPos - offset - sizeof(ushort));
                    }
                    else
                    {
                        // Node is a branch and points to 8 children.
                        node.Key = (uint)(nodes.Length + queuedNodes.Count * 8) * sizeof(uint);
                        queuedNodes.Enqueue(node.Children);
                    }
                    writer.Write(node.Key);
                }
            }

            foreach (var ind in indexPool)
            {
                //Last value skip. Uses terminator of previous index list
                if (ind.Key.Length == 0)
                    break;

                //Save the index lists and terminator
                if (version < FileVersion.Version2)
                {
                    for (int i = 0; i < ind.Key.Length; i++)
                        writer.Write((ushort)(ind.Key[i] + 1)); //-1 indexed
                    writer.Write((ushort)0); // Terminator
                }
                else
                {
                    writer.Write(ind.Key);
                    writer.Write((ushort)0xFFFF); // Terminator
                }
            }
        }

        // ---- METHODS (PRIVATE) --------------------------------------------------------------------------------------

        //Create an index buffer to find matching index lists
        private Dictionary<ushort[], int> CreateIndexBuffer(Queue<PolygonOctree[]> queuedNodes)
        {
            Dictionary<ushort[], int> indexPool = new Dictionary<ushort[], int>(new IndexEqualityComparer());
            int offset = 0;
            queuedNodes.Enqueue(PolygonOctreeRoots);
            while (queuedNodes.Count > 0)
            {
                PolygonOctree[] nodes = queuedNodes.Dequeue();
                foreach (PolygonOctree node in nodes)
                {
                    if (node.Children == null)
                    {
                        ushort[] indices = node.TriangleIndices.ToArray();
                        if (node.TriangleIndices.Count > 0 && !indexPool.ContainsKey(indices))
                        {
                            indexPool.Add(indices, offset);
                            offset += (node.TriangleIndices.Count + 1) * sizeof(ushort); //+1 to add terminator
                        }
                    }
                    else
                    {
                        // Node is a branch and points to 8 children.
                        queuedNodes.Enqueue(node.Children);
                    }
                }
            }
            //Empty values are last in the buffer using the last terminator
            indexPool.Add(new ushort[0], offset - sizeof(ushort));
            return indexPool;
        }

        private void GetOctreeBounding(List<OctreeBounding> boundings, PolygonOctree octree, Vector3 cubePosition, float cubeSize)
        {
            OctreeBounding bounding = new OctreeBounding();
            bounding.Position = cubePosition;
            bounding.Size = cubeSize;
            bounding.Octree = octree;
            boundings.Add(bounding);

            if (octree.Children != null)
            {
                float childCubeSize = cubeSize / 2f;
                int i = 0;
                for (int z = 0; z < 2; z++)
                {
                    for (int y = 0; y < 2; y++)
                    {
                        for (int x = 0; x < 2; x++)
                        {
                            Vector3 childCubePosition = cubePosition + childCubeSize * new Vector3(x, y, z);
                            GetOctreeBounding(boundings, octree.Children[i++], childCubePosition, childCubeSize);
                        }
                    }
                }
            }
        }

        public class OctreeBounding
        {
            public PolygonOctree Octree;
            public Vector3 Position { get; set; }
            public float Size { get; set; }
        }

        private class IndexEqualityComparer : IEqualityComparer<ushort[]>
        {
            public bool Equals(ushort[] x, ushort[] y)
            {
                if (x.Length != y.Length)
                {
                    return false;
                }
                for (int i = 0; i < x.Length; i++)
                {
                    if (x[i] != y[i])
                    {
                        return false;
                    }
                }
                return true;
            }

            public int GetHashCode(ushort[] obj)
            {
                int result = 17;
                for (int i = 0; i < obj.Length; i++)
                {
                    unchecked
                    {
                        result = result * 23 + obj[i];
                    }
                }
                return result;
            }
        }

        private int GetNodeCount(PolygonOctree[] nodes)
        {
            int count = nodes.Length;
            foreach (PolygonOctree node in nodes)
            {
                if (node.Children != null)
                {
                    count += GetNodeCount(node.Children);
                }
            }
            return count;
        }

        private int IndexOfVertex(Vector3 value, List<Vector3> valueList, Dictionary<int, int> hashTable)
        {
            int hash = value.GetHashCode();
            if (!hashTable.ContainsKey(hash))
            {
                valueList.Add(value);
                hashTable.Add(hash, hashTable.Count);
            }

            return hashTable[hash];
        }
    }
}

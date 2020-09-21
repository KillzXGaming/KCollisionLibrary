using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Syroot.BinaryData;
using System.Numerics;

namespace KclLibrary
{
    /// <summary>
    /// Represents a node in a model triangle octree.
    /// </summary>
    public class PolygonOctree : OctreeNodeBase<PolygonOctree>
    {
        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        public PolygonOctree() : base(0) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PolygonOctree"/> class with the key and data read from the
        /// given <paramref name="reader"/>.
        /// </summary>
        /// <param name="reader">The <see cref="BinaryDataReader"/> to read the node data with.</param>
        /// <param name="parentOffset">The required offset of the start of the parent node.</param>
        internal PolygonOctree(BinaryDataReader reader, long parentOffset, FileVersion version) : base(reader.ReadUInt32())
        {
            int terminator = version >= FileVersion.Version2 ? 0xFFFF : 0x0;

            // Get and seek to the data offset in bytes relative to the parent node's start.
            long offset = parentOffset + Key & ~_flagMask;
            if ((Key >> 31) == 1) //Check for leaf
            {
                // Node is a leaf and key points to triangle list starting 2 bytes later.
                using (reader.TemporarySeek(offset + sizeof(ushort), SeekOrigin.Begin))
                {
                    TriangleIndices = new List<ushort>();
                    ushort index;
                    while ((index = reader.ReadUInt16()) != terminator) {
                        if (version < FileVersion.Version2) //V1 is -1 based indexed
                            TriangleIndices.Add((ushort)(index - 1));
                        else
                            TriangleIndices.Add(index);
                    }
                }
            }
            else
            {
                // Node is a branch and points to 8 child nodes.
                using (reader.TemporarySeek(offset, SeekOrigin.Begin))
                {
                    PolygonOctree[] children = new PolygonOctree[ChildCount];
                    for (int i = 0; i < ChildCount; i++) {
                        children[i] = new PolygonOctree(reader, offset, version);
                    }
                    Children = children;
                }
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="ModelOctreeNode"/> class, initializing children and
        /// subdivisions from the given list of faces sorted into a cube of the given <paramref name="cubeSize"/>.
        /// </summary>
        /// <param name="triangles">The dictionary of <see cref="Triangle"/> instances which have to be sorted into this
        /// octree, with the key being their original index in the model.</param>
        /// <param name="cubePosition">The offset of the cube.</param>
        /// <param name="cubeSize">The size of the cube.</param>
        /// <param name="maxTrianglesInCube">The maximum number of triangles to sort into this node.</param>
        /// <param name="minCubeSize">The minimum size a cube can be subdivided to.</param>
        internal PolygonOctree(Dictionary<ushort, Triangle> triangles, Vector3 cubePosition, float cubeSize,
            int maxTrianglesInCube, int maxCubeSize, int minCubeSize, int cubeBlow, int maxDepth, int depth = 0) : base(0)
        {
            //Adjust the cube sizes based on EFE's method
            Vector3 cubeCenter = cubePosition + new Vector3(cubeSize / 2f, cubeSize / 2f, cubeSize / 2f);
            float newsize = cubeSize + cubeBlow;
            Vector3 newPosition = cubeCenter - new Vector3(newsize / 2f, newsize / 2f, newsize / 2f);

            // Go through all triangles and remember them if they overlap with the region of this cube.
            Dictionary<ushort, Triangle> containedTriangles = new Dictionary<ushort, Triangle>();
            foreach (KeyValuePair<ushort, Triangle> triangle in triangles)
            {
                if (TriangleHelper.TriangleCubeOverlap(triangle.Value, newPosition, newsize)) {
                    containedTriangles.Add(triangle.Key, triangle.Value);
                }
            }

            float halfWidth = cubeSize / 2f;

            bool isTriangleList = cubeSize <= maxCubeSize && containedTriangles.Count <= maxTrianglesInCube ||
                                  cubeSize <= minCubeSize || depth > maxDepth;

            if (containedTriangles.Count > maxTrianglesInCube && halfWidth >= minCubeSize)
            {
                // Too many triangles are in this cube, and it can still be subdivided into smaller cubes.
                float childCubeSize = cubeSize / 2f;
                Children = new PolygonOctree[ChildCount];
                int i = 0;
                for (int z = 0; z < 2; z++) {
                    for (int y = 0; y < 2; y++) {
                        for (int x = 0; x < 2; x++) {
                            Vector3 childCubePosition = cubePosition + childCubeSize * new Vector3(x, y, z);
                            Children[i++] = new PolygonOctree(containedTriangles, childCubePosition, childCubeSize,
                                maxTrianglesInCube, maxCubeSize, minCubeSize, cubeBlow, maxDepth, depth + 1);
                        }
                    }
                }
            }
            else
            {
                // Either the amount of triangles in this cube is okay or it cannot be subdivided any further.
                TriangleIndices = containedTriangles.Keys.ToList();
            }
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets the indices to triangles of the model appearing in this cube.
        /// </summary>
        public List<ushort> TriangleIndices { get; internal set; }
    }

}

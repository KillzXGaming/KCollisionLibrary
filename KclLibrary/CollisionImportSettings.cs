using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace KclLibrary
{
    /// <summary>
    /// Settings used to configure collision generated from triangles.
    /// </summary>
    public class CollisionImportSettings
    {
        /// <summary>
        /// The max cube size of the root octrees.
        /// </summary>
        public int MaxRootSize = 2048;
        /// <summary>
        /// The min cube size of the root octrees.
        /// </summary>
        public int MinRootSize = 128;
        /// <summary>
        /// The max cube size of the all octrees.
        /// </summary>
        public int MaxCubeSize = 0x100000;
        /// <summary>
        /// The min cube size of the all octrees.
        /// </summary>
        public int MinCubeSize = 32;
        /// <summary>
        /// The max depth size of the all octrees.
        /// </summary>
        public int MaxOctreeDepth = 10;
        /// <summary>
        /// The max amount of triangles in an octree.
        /// When the limit is reached, octrees will divide until the min cube size is reached.
        /// </summary>
        public int MaxTrianglesInCube = 10;
        /// <summary>
        /// Determines the max distance of the normals for a prism.
        /// </summary>
        public float PrismThickness = 30;
        /// <summary>
        /// The sphere radius used for an unknown purpose.
        /// </summary>
        public float SphereRadius = 25;
        /// <summary>
        /// The min amount of padding to use for the collison boundings.
        /// </summary>
        public Vector3 PaddingMin = new Vector3(-50, -50, -50);

        /// <summary>
        /// The max amount of padding to use for the collison boundings.
        /// </summary>
        public Vector3 PaddingMax = new Vector3(50, 50, 50);
    }
}

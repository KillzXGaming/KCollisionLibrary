using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace KclLibrary
{
    //Todo add collision detection
    class CollisionHandler
    {
        internal static Vector3 ConvertLocalSpace(Matrix4x4 transform, Vector3 position) {
             Matrix4x4.Invert(transform, out transform);
            return Vector3.Transform(position, transform);
        }

        internal static KCLHit CheckPoint(KCLModel model, Vector3 point, float thicknessScale, float pointDistance)
        {
            float maxDistance = model.PrismThickness * thicknessScale;

            int x = (int)(point.X - model.MinCoordinate.X) | 0;
            int y = (int)(point.Y - model.MinCoordinate.Y) | 0;
            int z = (int)(point.Z - model.MinCoordinate.Z) | 0;

            if ((x & model.CoordinateMask.X) != 0 || (y & model.CoordinateMask.Y) != 0 || (z & model.CoordinateMask.Z) != 0)
                return null;

            float smallestDist = float.MaxValue;
            KCLHit closestHit = null;

            var prismIndices = searchBlock(model, x, y, z);
            for (int i = 0; i < prismIndices.Length; i++) {
                var prism = model.Prisms[prismIndices[i]];
                if (prism.Length <= 0.0f)
                    continue;

                var position = model.Positions[prism.PositionIndex];
                position = point - position; //Local coordinates
                var edgeNormal1 = model.Normals[prism.Normal1Index];
                if (Vector3.Dot(position, edgeNormal1) > 0)
                    continue;

                var edgeNormal2 = model.Normals[prism.Normal2Index];
                if (Vector3.Dot(position, edgeNormal2) > 0)
                    continue;

                var edgeNormal3 = model.Normals[prism.Normal3Index];
                if (Vector3.Dot(position, edgeNormal3) > prism.Length)
                    continue;

                var faceNormal = model.Normals[prism.DirectionIndex];
                float dist = -Vector3.Dot(faceNormal, point);
                // if (dist < 0.0f || dist > maxDistance)
                //     continue;

                if (dist < smallestDist)
                {
                    model.HitPrisms.Add(prism);
                    smallestDist = dist;

                    //Return with a proper hit with all checks passed.
                    closestHit = new KCLHit()
                    {
                        Prism = prism,
                        Distance = dist,
                    };
                }
            }

            return closestHit;
        }

        internal static ushort[] searchBlock(KCLModel model,  int x, int y, int z)
        {
            int blockIdx = 0;
            var shiftR = model.CoordinateShift.X;

            if (model.CoordinateShift.Y == ushort.MaxValue && model.CoordinateShift.Z == ushort.MaxValue)
                blockIdx = 0;
            else
                blockIdx = ((z >> (int)shiftR) << (int)model.CoordinateShift.Z) |
                           ((y >> (int)shiftR) << (int)model.CoordinateShift.Y) | 
                           ((x >> (int)shiftR));

            model.HitOctrees.Clear();
            model.HitOctrees.Add(model.PolygonOctreeRoots[blockIdx]);

            var octree = FindBlock(model, model.PolygonOctreeRoots, x, y, z, blockIdx, shiftR);

            return octree.TriangleIndices.ToArray();
        }

        private static PolygonOctree FindBlock(KCLModel model, PolygonOctree[] children, int x, int y, int z, int blockIdx, uint shiftR)
        {
            model.HitOctrees.Add(children[blockIdx]);

            if (children[blockIdx].Children != null)
            {
                shiftR--;
                var childBlockIdx =  ((x >> (int)shiftR) & 1) |
                                 4 * ((z >> (int)shiftR) & 1) |
                                 2 * ((y >> (int)shiftR) & 1);

                return FindBlock(model, children[blockIdx].Children, x, y, z, childBlockIdx, shiftR);
            }
            else
                return children[blockIdx];
        }
    }


    public class KCLHit
    {
        public float Distance { get; set; }
        public KclPrism Prism { get; set; }
    }
}
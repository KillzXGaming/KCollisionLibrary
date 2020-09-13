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
    public class CollisionHandler
    {
        internal static Vector3 ConvertLocalSpace(Matrix4x4 transform, Vector3 position) {
             Matrix4x4.Invert(transform, out transform);
            return Vector3.Transform(position, transform);
        }

        public static KCLHit FindIntersection(KCLFile kclFile, KCLModel model, Vector3 point, Vector3 eye)
        {
            KCLHit closest = null;

            int x = (int)(point.X - model.MinCoordinate.X);
            int y = (int)(point.Y - model.MinCoordinate.Y);
            int z = (int)(point.Z - model.MinCoordinate.Z);

            if ((x & model.CoordinateMask.X) != 0 || (y & model.CoordinateMask.Y) != 0 || (z & model.CoordinateMask.Z) != 0)
                return null;

            foreach (var prism in model.Prisms)
            {
                var triangle = model.GetTriangle(prism);
                if (triangle.IsRayInTriangle(point, eye, kclFile.Transform)) {
                    closest = new KCLHit() { Prism = prism };
                }
            }
/*
            List<KCLHit> found = new List<KCLHit>();
            var prismIndices = searchBlock(model, x, y, z);
            for (int i = 0; i < prismIndices.Length; i++)
            {
            
            }*/

            return closest;
        }

        internal static KCLHit CheckPoint(KCLModel model, Vector3 point, float thicknessScale, float pointDistance)
        {
            float maxDistance = model.PrismThickness * thicknessScale;

            int x = (int)(point.X - model.MinCoordinate.X);
            int y = (int)(point.Y - model.MinCoordinate.Y);
            int z = (int)(point.Z - model.MinCoordinate.Z);

            if ((x & model.CoordinateMask.X) != 0 || (y & model.CoordinateMask.Y) != 0 || (z & model.CoordinateMask.Z) != 0)
                return null;

            List<KCLHit> found = new List<KCLHit>();
            var prismIndices = searchBlock(model, x, y, z);
            for (int i = 0; i < prismIndices.Length; i++) {
                var prism = model.Prisms[prismIndices[i]];
                if (prism.Length <= 0.0f)
                    continue;

                var triangle = model.GetTriangle(prism);
                if (PointInTriangle(
                    new Vector2(point.X, point.Z), 
                    new Vector2(triangle.Vertices[0].X, triangle.Vertices[0].Z),
                    new Vector2(triangle.Vertices[1].X, triangle.Vertices[1].Z),
                    new Vector2(triangle.Vertices[2].X, triangle.Vertices[2].Z)))
                {
                    found.Add(new KCLHit()
                    {
                        Prism = prism,
                        CenterY = barryCentric(
                           triangle.Vertices[0],
                           triangle.Vertices[1],
                           triangle.Vertices[2], point),
                    });
                }
            }

            if (found.Count == 0)
                return null;

            int closest_index = 0;
            float closest_abs = 9999999.0f;
            for (int i = 0; i < found.Count; i++)
            {
                float abs = Math.Abs(point.Y - found[i].CenterY);
                if (abs < closest_abs)
                {
                    closest_abs = abs;
                    closest_index = i;
                }
            }

            model.HitPrisms.Add(found[closest_index].Prism);

            return found[closest_index];
        }

        public static bool PointInTriangle(Vector2 p, Vector2 p0, Vector2 p1, Vector2 p2)
        {
            var s = p0.Y * p2.X - p0.X * p2.Y + (p2.Y - p0.Y) * p.X + (p0.X - p2.X) * p.Y;
            var t = p0.X * p1.Y - p0.Y * p1.X + (p0.Y - p1.Y) * p.X + (p1.X - p0.X) * p.Y;

            if ((s < 0) != (t < 0))
                return false;

            var A = -p1.Y * p2.X + p0.Y * (p2.X - p1.X) + p0.X * (p1.Y - p2.Y) + p1.X * p2.Y;
            if (A < 0.0)
            {
                s = -s;
                t = -t;
                A = -A;
            }
            return s > 0 && t > 0 && (s + t) <= A;
        }

        internal static float barryCentric(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 pos)
        {
            float det = (p2.Z - p3.Z) * (p1.X - p3.X) + (p3.X - p2.X) * (p1.Z - p3.Z);
            float l1 = ((p2.Z - p3.Z) * (pos.X - p3.X) + (p3.X - p2.X) * (pos.Z - p3.Z)) / det;
            float l2 = ((p3.Z - p1.Z) * (pos.X - p3.X) + (p1.X - p3.X) * (pos.Z - p3.Z)) / det;
            float l3 = 1.0f - l1 - l2;
            return l1 * p1.Y + l2 * p2.Y + l3 * p3.Y;
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
        public float CenterY { get; set; }
        public float Distance { get; set; }
        public KclPrism Prism { get; set; }
    }
}

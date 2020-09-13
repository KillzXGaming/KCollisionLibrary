using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Numerics;

namespace KclLibrary
{
    /// <summary>
    /// Represents a polygon in 3-dimensional space, defined by 3 vertices storing their positions.
    /// </summary>
    public class Triangle
    {
        // ---- FIELDS -------------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets the vertices which store the corner positions of the triangle.
        /// </summary>
        public Vector3[] Vertices;

        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="Triangle"/> class.
        /// </summary>
        public Triangle() {
            Vertices = new Vector3[3];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Triangle"/> class.
        /// </summary>
        public Triangle(Vector3 vertexA, Vector3 vertexB, Vector3 vertexC) {
            Vertices = new Vector3[3] { vertexA, vertexB, vertexC };
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets the face normal of this triangle.
        /// </summary>
        public Vector3 Normal
        {
            get
            {
                return Vector3.Normalize(Vector3.Cross(
                    Vertices[1] - Vertices[0], 
                    Vertices[2] - Vertices[0]));
            }
        }

        /// <summary>
        /// The attribute used by a KCLPrisim for handling material flags.
        /// </summary>
        public ushort Attribute { get; set; }

        public bool IsRayInTriangle(Vector3 ray, Vector3 cameraEye, Matrix4x4 collisionMatrix)
        {
            Vector3 normal = Normal;

            Vector3 vertexA = Vector3.Transform(Vertices[0], collisionMatrix);
            Vector3 vertexB = Vector3.Transform(Vertices[1], collisionMatrix);
            Vector3 vertexC = Vector3.Transform(Vertices[2], collisionMatrix);

            Vector3 triCenter = GetTriangleCenter();

            float numerator = (Vector3.Dot(normal, Vector3.Subtract(cameraEye, triCenter)));
            float denominator = Vector3.Dot(ray, normal);

            float distance = (-(numerator) / denominator);
            Vector3 point = cameraEye + Vector3.Normalize((distance * ray));

            Vector3 vec1 = Vector3.Normalize(vertexA - point);
            Vector3 vec2 = Vector3.Normalize(vertexB - point);
            Vector3 vec3 = Vector3.Normalize(vertexC - point);

            Vector3 n4 = Vector3.Cross(vec3, vec2);
            Vector3 n5 = Vector3.Cross(vec2, vec1);
            Vector3 n6 = Vector3.Cross(vec1, vec3);

            n4 = Vector3.Normalize(n4);
            n5 = Vector3.Normalize(n5);
            n6 = Vector3.Normalize(n6);

            float dist1 = Vector3.Dot(-cameraEye, n4);
            float dist2 = Vector3.Dot(-cameraEye, n5);
            float dist3 = Vector3.Dot(-cameraEye, n6);

            if ((Vector3.Dot(point, n4) + dist1) < 0)
                return false;

            if ((Vector3.Dot(point, n5) + dist2) < 0)
                return false;

            if ((Vector3.Dot(point, n6) + dist3) < 0)
                return false;

            else
                return true;
        }

        /// <summary>
        /// Gets the center of the current triangle and returns the point.
        /// </summary>
        public Vector3 GetTriangleCenter()
        {
            Vector3 triCenter = new Vector3();
            triCenter.X = (Vertices[0].X + Vertices[1].X + Vertices[2].X) / 3;
            triCenter.Y = (Vertices[0].Y + Vertices[1].Y + Vertices[2].Y) / 3;
            triCenter.Z = (Vertices[0].Z + Vertices[1].Z + Vertices[2].Z) / 3;
            return triCenter;
        }
    }
}
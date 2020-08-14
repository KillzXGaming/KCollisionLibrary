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

        public ushort Attribute { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KclLibrary
{
    /// <summary>
    /// Represents 3 Uint32 vector values.
    /// </summary>
    public class Vector3U
    {
        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        public Vector3U(uint x, uint y, uint z) {
            X = x;
            Y = y;
            Z = z;
        }

        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the X value of the 3 vector values.
        /// </summary>
        public uint X { get; set; }

        /// <summary>
        /// Gets or sets the Y value of the 3 vector values.
        /// </summary>
        public uint Y { get; set; }

        /// <summary>
        /// Gets or sets the Z value of the 3 vector values.
        /// </summary>
        public uint Z { get; set; }

        public override string ToString()
        {
            return $"({X} {Y} {Z})";
        }
    }
}

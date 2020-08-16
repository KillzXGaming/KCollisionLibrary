using Syroot.BinaryData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KclLibrary
{
    /// <summary>
    /// Represents a prism as stored in a collision file.
    /// </summary>
    public struct KclPrism
    {
        // ---- FIELDS -------------------------------------------------------------------------------------------------

        /// <summary>
        /// The length of this triangle.
        /// </summary>
        public float Length;

        /// <summary>
        /// The 0-based index of the positional vector in the position array of the model this triangle belongs to.
        /// </summary>
        public ushort PositionIndex;

        /// <summary>
        /// The 0-based index of the direction normal in the normal array of the model this triangle belongs to.
        /// </summary>
        public ushort DirectionIndex;

        /// <summary>
        /// The first 0-based index of the normal in the normal array of the model this triangle belongs to.
        /// </summary>
        public ushort Normal1Index;

        /// <summary>
        /// The second 0-based index of the normal in the normal array of the model this triangle belongs to.
        /// </summary>
        public ushort Normal2Index;

        /// <summary>
        /// The third 0-based index of the normal in the normal array of the model this triangle belongs to.
        /// </summary>
        public ushort Normal3Index;

        /// <summary>
        /// The collision flags determining in-game behavior when colliding with this polygon.
        /// </summary>
        public ushort CollisionFlags;

        /// <summary>
        /// The 0-based index of the triangle in the KCL file this triangle belongs to.
        /// </summary>
        public uint GlobalIndex;

        internal void Read(BinaryDataReader reader, FileVersion version)
        {
            if (version == FileVersion.VersionDS)
                Length = reader.ReadFx32();
            else
                Length = reader.ReadSingle();
            PositionIndex = reader.ReadUInt16();
            DirectionIndex = reader.ReadUInt16();
            Normal1Index = reader.ReadUInt16();
            Normal2Index = reader.ReadUInt16();
            Normal3Index = reader.ReadUInt16();
            CollisionFlags = reader.ReadUInt16();
            if (version >= FileVersion.Version2)
                GlobalIndex = reader.ReadUInt32();
        }

        internal void Write(BinaryDataWriter writer, FileVersion version)
        {
            if (version == FileVersion.VersionDS)
                writer.Write((int)(Length * 4096f));
            else
                writer.Write(Length);
            writer.Write(PositionIndex);
            writer.Write(DirectionIndex);
            writer.Write(Normal1Index);
            writer.Write(Normal2Index);
            writer.Write(Normal3Index);
            writer.Write(CollisionFlags);
            if (version >= FileVersion.Version2)
                writer.Write(GlobalIndex);
        }
    }
}

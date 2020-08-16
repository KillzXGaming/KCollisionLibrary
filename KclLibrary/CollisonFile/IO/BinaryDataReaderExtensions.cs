using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Syroot.BinaryData;
using System.Numerics;

namespace KclLibrary
{
    /// <summary>
    /// Represents extension methods for <see cref="BinaryDataReader"/> instances.
    /// </summary>
    public static class BinaryDataReaderExtensions
    {
        // ---- METHODS (INTERNAL) -------------------------------------------------------------------------------------

        /// <summary>
        /// Reads <see cref="KclFace"/> instances from the current stream and returns them.
        /// </summary>
        /// <param name="self">The extended <see cref="BinaryDataReader"/>.</param>
        /// <param name="count">The number of instances to read.</param>
        /// <returns>The <see cref="KclFace"/> instances.</returns>
        internal static KclPrism[] ReadPrisms(this BinaryDataReader self, int count, FileVersion version)
        {
            KclPrism[] values = new KclPrism[count];
            for (int i = 0; i < count; i++) {
                values[i] = new KclPrism();
                values[i].Read(self, version);
                if (version != FileVersion.Version2) //Manually set the global index for older versions
                    values[i].GlobalIndex = (ushort)i;
            }
            return values;
        }

        /// <summary>
        /// Reads a <see cref="Vector3"/> instance from the current stream and returns it.
        /// </summary>
        /// <param name="self">The extended <see cref="BinaryDataReader"/>.</param>
        /// <returns>The <see cref="Vector3"/> instance.</returns>
        internal static Vector3U ReadVector3U(this BinaryDataReader self) {
            return new Vector3U(self.ReadUInt32(), self.ReadUInt32(), self.ReadUInt32());
        }

        /// <summary>
        /// Reads <see cref="Vector3"/> instances from the current stream and returns them.
        /// </summary>
        /// <param name="self">The extended <see cref="BinaryDataReader"/>.</param>
        /// <param name="count">The number of instances to read.</param>
        /// <returns>The <see cref="Vector3"/> instances.</returns>
        internal static Vector3U[] ReadVector3s(this BinaryDataReader self, int count)
        {
            Vector3U[] values = new Vector3U[count];
            for (int i = 0; i < count; i++) {
                values[i] = ReadVector3U(self);
            }
            return values;
        }

        /// <summary>
        /// Reads a <see cref="Vector3F"/> instance from the current stream and returns it.
        /// </summary>
        /// <param name="self">The extended <see cref="BinaryDataReader"/>.</param>
        /// <returns>The <see cref="Vector3F"/> instance.</returns>
        internal static Vector3 ReadVector3F(this BinaryDataReader self) {
            return new Vector3(self.ReadSingle(), self.ReadSingle(), self.ReadSingle());
        }

        /// <summary>
        /// Reads <see cref="Vector3F"/> instances from the current stream and returns them.
        /// </summary>
        /// <param name="self">The extended <see cref="BinaryDataReader"/>.</param>
        /// <param name="count">The number of instances to read.</param>
        /// <returns>The <see cref="Vector3F"/> instances.</returns>
        internal static Vector3[] ReadVector3Fs(this BinaryDataReader self, int count)
        {
            Vector3[] values = new Vector3[count];
            for (int i = 0; i < count; i++) {
                values[i] = ReadVector3F(self);
            }
            return values;
        }



        //DS Specific readers

        /// <summary>
        /// Reads <see cref="Vector3F"/> instances from the current stream and returns them.
        /// </summary>
        /// <param name="self">The extended <see cref="BinaryDataReader"/>.</param>
        /// <param name="count">The number of instances to read.</param>
        /// <returns>The <see cref="Vector3F"/> instances.</returns>
        internal static Vector3[] ReadVector3Fx16s(this BinaryDataReader self, int count)
        {
            Vector3[] values = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = ReadVector3Fx16(self);
            }
            return values;
        }

        /// <summary>
        /// Reads <see cref="Vector3F"/> instances from the current stream and returns them.
        /// </summary>
        /// <param name="self">The extended <see cref="BinaryDataReader"/>.</param>
        /// <param name="count">The number of instances to read.</param>
        /// <returns>The <see cref="Vector3F"/> instances.</returns>
        internal static Vector3[] ReadVector3Fx32s(this BinaryDataReader self, int count)
        {
            Vector3[] values = new Vector3[count];
            for (int i = 0; i < count; i++)
            {
                values[i] = ReadVector3Fx32(self);
            }
            return values;
        }

        internal static Vector3 ReadVector3Fx32(this BinaryDataReader self) {
            return new Vector3(self.ReadFx32(), self.ReadFx32(), self.ReadFx32());
        }

        internal static Vector3 ReadVector3Fx16(this BinaryDataReader self) {
            return new Vector3(self.ReadFx16(), self.ReadFx16(), self.ReadFx16());
        }

        internal static float ReadFx32(this BinaryDataReader self) {
            return self.ReadInt32() / 4096f;
        }

        internal static float ReadFx16(this BinaryDataReader self) {
            return self.ReadInt16() / 4096f;
        }
    }
}

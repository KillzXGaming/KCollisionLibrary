using Syroot.BinaryData;
using System.Numerics;

namespace KclLibrary
{
    /// <summary>
    /// Represents extension methods for <see cref="BinaryDataWriter"/> instances.
    /// </summary>
    internal static class BinaryDataWriterExtensions
    {
        // ---- METHODS (INTERNAL) -------------------------------------------------------------------------------------

        /// <summary>
        /// Writes <see cref="KclFace"/> instances into the current stream.
        /// </summary>
        /// <param name="self">The extended <see cref="BinaryDataWriter"/>.</param>
        /// <param name="values">The <see cref="KclFace"/> instances.</param>
        internal static void Write(this BinaryDataWriter self, KclPrism[] values, FileVersion version)
        {
            foreach (KclPrism value in values)
                value.Write(self, version);
        }

        /// <summary>
        /// Writes a <see cref="Vector3"/> instance into the current stream.
        /// </summary>
        /// <param name="self">The extended <see cref="BinaryDataWriter"/>.</param>
        /// <param name="value">The <see cref="Vector3"/> instance.</param>
        internal static void Write(this BinaryDataWriter self, Vector3U value)
        {
            self.Write(value.X);
            self.Write(value.Y);
            self.Write(value.Z);
        }

        /// <summary>
        /// Writes <see cref="Vector3"/> instances into the current stream.
        /// </summary>
        /// <param name="self">The extended <see cref="BinaryDataWriter"/>.</param>
        /// <param name="values">The <see cref="Vector3"/> instances.</param>
        internal static void Write(this BinaryDataWriter self, Vector3U[] values)
        {
            foreach (Vector3U value in values) {
                Write(self, value);
            }
        }

        /// <summary>
        /// Writes a <see cref="Vector3F"/> instance into the current stream.
        /// </summary>
        /// <param name="self">The extended <see cref="BinaryDataWriter"/>.</param>
        /// <param name="value">The <see cref="Vector3F"/> instance.</param>
        internal static void Write(this BinaryDataWriter self, Vector3 value)
        {
            self.Write(value.X);
            self.Write(value.Y);
            self.Write(value.Z);
        }

        /// <summary>
        /// Writes <see cref="Vector3F"/> instances into the current stream.
        /// </summary>
        /// <param name="self">The extended <see cref="BinaryDataWriter"/>.</param>
        /// <param name="values">The <see cref="Vector3F"/> instances.</param>
        internal static void Write(this BinaryDataWriter self, Vector3[] values)
        {
            foreach (Vector3 value in values) {
                Write(self, value);
            }
        }

        internal static void WriteVector3Fx16s(this BinaryDataWriter self, Vector3[] values) {
            foreach (Vector3 value in values) {
                WriteVector3Fx16(self, value);
            }
        }

        internal static void WriteVector3Fx32s(this BinaryDataWriter self, Vector3[] values)
        {
            foreach (Vector3 value in values) {
                WriteVector3Fx32(self, value);
            }
        }

        internal static void WriteVector3Fx16(this BinaryDataWriter self, Vector3 value) {
            self.WriteFx16(value.X);
            self.WriteFx16(value.Y);
            self.WriteFx16(value.Z);
        }

        internal static void WriteVector3Fx32(this BinaryDataWriter self, Vector3 value) {
            self.WriteFx32(value.X);
            self.WriteFx32(value.Y);
            self.WriteFx32(value.Z);
        }

        internal static void WriteFx32(this BinaryDataWriter self, float value) {
            self.Write((int)(value * 4096f));
        }

        internal static void WriteFx16(this BinaryDataWriter self, float value) {
            self.Write((short)(value * 4096f));
        }
    }
}

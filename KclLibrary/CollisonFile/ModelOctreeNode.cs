using System;
using System.Collections.Generic;
using System.Linq;
using Syroot.BinaryData;
using System.Threading.Tasks;
using System.Numerics;

namespace KclLibrary
{
    public class ModelOctreeNode : OctreeNodeBase<ModelOctreeNode>
    {
        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelOctreeNode"/> class with an empty key.
        /// </summary>
        internal ModelOctreeNode() : base(0)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CourseOctreeNode"/> class with the key and data read from the
        /// given <paramref name="reader"/>.
        /// </summary>
        /// <param name="reader">The <see cref="BinaryDataReader"/> to read the node data with.</param>
        internal ModelOctreeNode(BinaryDataReader reader) : base(reader.ReadUInt32())
        {
            switch ((Flags)(Key & _flagMask))
            {
                case Flags.Divide:
                    // Node is a branch subdivided into 8 children.
                    Children = new ModelOctreeNode[ChildCount];
                    for (int i = 0; i < ChildCount; i++) {
                        Children[i] = new ModelOctreeNode(reader);
                    }
                    break;
                case Flags.Values:
                    // Node points to a model in the file's model array.
                    ModelIndex = Key & ~_flagMask;
                    break;
            }
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets the index to the model referenced by this node in the model array of the file this node belongs to.
        /// </summary>
        public uint? ModelIndex { get; internal set; }

        // ---- METHODS (INTERNAL) -------------------------------------------------------------------------------------

        internal void Write(BinaryDataWriter writer)
        {
            if (Children == null)
            {
                if (ModelIndex.HasValue)
                {
                    // Node points to a model in the file's model array.
                    Key = (uint)Flags.Values | ModelIndex.Value;
                }
                else
                {
                    // Node is an empty cube.
                    Key = (uint)Flags.NoData;
                }
                writer.Write(Key);
            }
            else
            {
                // Node is a branch subdivided into 8 children.
                Key = 8;
                writer.Write(Key);
            }
        }

        internal void WriteChildren(BinaryDataWriter writer)
        {
            if (Children != null)
            {
                foreach (ModelOctreeNode child in Children) 
                    child.Write(writer);
                foreach (ModelOctreeNode child in Children)
                    child.WriteChildren(writer);
            }
        }
    }
}

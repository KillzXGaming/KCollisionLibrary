using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KclLibrary
{
    /// <summary>
    /// Represents the version of the file binary.
    /// </summary>
    public enum FileVersion
    {
        VersionGC = 0x00000000, //Lacks sphere radius
        VersionWII = 0x01000000, //Has sphere radius. 3DS also uses the same version.
        VersionDS = 0x01020000, //Encodes floats differently  (* 4096f)
        Version2 = 0x02020000, //Additional file header. Multiple model sections. 0 indxed indices.
    }
}
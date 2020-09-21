using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KclLibrary;

namespace KclLibraryGUI
{
    public interface IMaterialPresetBase
    {
        Dictionary<string, ushort> Result { get; }
        MaterialAttributeFileBase GetAttributeFile(List<Triangle> triangles, bool isBigEndian);
        void ReloadDataList();
    }
}

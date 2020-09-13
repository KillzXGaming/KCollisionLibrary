using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace KclLibraryGUI
{
    public class MaterialAttributeFileBase
    {
        public virtual void Save(string fileName)
        {
        }

        public virtual Stream Save() {
            return new MemoryStream();
        }

        public virtual string SetupFileName(string fileName)
        {
            return fileName;
        }
    }
}

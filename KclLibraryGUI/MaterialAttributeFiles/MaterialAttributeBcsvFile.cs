using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using KclLibrary.AttributeHandlers;

namespace KclLibraryGUI
{
    public class MaterialAttributeBcsvFile : MaterialAttributeFileBase
    {
        public BCSV BcsvFile { get; set; }

        public override void Save(string fileName) {
            BcsvFile.Save(fileName);
        }

        public override Stream Save()
        {
            var mem = new MemoryStream();
            BcsvFile.Save(mem);
            return mem;
        }

        public override string SetupFileName(string fileName) {
            return $"{fileName.Replace(".kcl", "")}.pa";
        }
    }
}

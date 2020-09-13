using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using ByamlExt.Byaml;

namespace KclLibraryGUI
{
    public class MaterialAttributeBymlFile : MaterialAttributeFileBase
    {
        public BymlFileData BymlFile { get; set; }

        public override void Save(string fileName) {
            File.WriteAllBytes(fileName, ByamlFile.SaveN(BymlFile));
        }

        public override Stream Save() {
            return new MemoryStream(ByamlFile.SaveN(BymlFile));
        }

        public override string SetupFileName(string fileName) {
            return $"{fileName.Replace(".kcl", "")}Attribute.byml";
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using KclLibrary.AttributeHandlers;
using KclLibrary;

namespace KclLibraryGUI
{
    public partial class SM3DLCollisionPicker : UserControl, IMaterialPresetBase
    {
        bool ItemLoaded = false;

        public Dictionary<string, ushort> Result
        {
            get { return GenerateIDs(); }
        }

        public MaterialAttributeFileBase GetAttributeFile(List<Triangle> triangles, bool isBigEndian)
        {
            var matAttributeFile = new MaterialAttributeBcsvFile();
            matAttributeFile.BcsvFile = GenerateBCSV(triangles);
            return matAttributeFile;
        }

        private MaterialSetForm ParentEditor;

        public string[] Materials => ParentEditor.Materials;
        public string[] Meshes => ParentEditor.Meshes;

        public bool UseObjectMaterials => ParentEditor.UseObjectMaterials;

        public SM3DLCollisionPicker(MaterialSetForm parentForm)
        {
            InitializeComponent();

            ParentEditor = parentForm;

            foreach (string val in FloorCodes)
                floorCodeCB.Items.Add(val);

            foreach (string val in WallCodes)
                wallCodeCB.Items.Add(val);

            foreach (string val in SoundCodes)
                soundCodeCB.Items.Add(val);

            unknownCodeUD.Value = 0;
            chkCameraThrough.Checked = false;
            floorCodeCB.SelectedItem = FloorCodes[0];
            wallCodeCB.SelectedItem = WallCodes[0];
            soundCodeCB.SelectedItem = SoundCodes[0];
        }

        public void ReloadDataList()
        {
            listView1.Items.Clear();

            unknownCodeUD.Value = 0;
            chkCameraThrough.Checked = false;
            floorCodeCB.SelectedItem = FloorCodes[0];
            wallCodeCB.SelectedItem = WallCodes[0];
            soundCodeCB.SelectedItem = SoundCodes[0];

            if (UseObjectMaterials)
            {
                for (int i = 0; i < Materials.Length; i++)
                {
                    var item = new ListViewItem();
                    UpdateListItem(item, new CollisionEntry(Materials[i]));
                    listView1.Items.Add(item);
                }
            }
            else
            {
                for (int i = 0; i < Meshes.Length; i++)
                {
                    var item = new ListViewItem();
                    UpdateListItem(item, new CollisionEntry(Meshes[i]));
                    listView1.Items.Add(item);
                }
            }
        }

        private void UpdateListItem(ListViewItem item, CollisionEntry entry)
        {
            item.SubItems.Clear();

            item.Tag = entry;
            item.Text = entry.Name;
            item.SubItems.Add(entry.UnknownCode.ToString());
            item.SubItems.Add(entry.CameraThrough.ToString());
            item.SubItems.Add(entry.FloorCode);
            item.SubItems.Add(entry.WallCode);
            item.SubItems.Add(entry.SoundCode);
        }

        public class CollisionEntry
        {
            public string Name; //Mesh or material name

            public bool CameraThrough = false;
            public int UnknownCode = 0;
            public string FloorCode = "Normal";
            public string SoundCode = "null";
            public string WallCode = "Normal";

            public CollisionEntry(string name)
            {
                Name = name;
            }
        }

        private List<CollisionEntry> RemoveDuplicateEntries(List<CollisionEntry> entries)
        {
            List<CollisionEntry> col = new List<CollisionEntry>();
            for (int i = 0; i < entries.Count; i++)
            {
                if (!col.Any(x =>
                x.UnknownCode == entries[i].UnknownCode &&
                x.CameraThrough == entries[i].CameraThrough &&
                x.FloorCode == entries[i].FloorCode &&
                x.SoundCode == entries[i].SoundCode &&
                x.WallCode == entries[i].WallCode))
                {
                    col.Add(entries[i]);
                }
            }
            return col;
        }

        private List<CollisionEntry> GetCollisionEntries()
        {
            List<CollisionEntry> entries = new List<CollisionEntry>();
            foreach (ListViewItem item in listView1.Items)
                entries.Add((CollisionEntry)item.Tag);
            return entries;
        }

        public Dictionary<string, ushort> GenerateIDs()
        {
            var ids = new Dictionary<string, ushort>();
            var entries = GetCollisionEntries();
            var col = RemoveDuplicateEntries(entries);
            for (int i = 0; i < entries.Count; i++)
            {
                int index = col.FindIndex(x =>
                x.FloorCode == entries[i].FloorCode &&
                x.CameraThrough == entries[i].CameraThrough &&
                x.UnknownCode == entries[i].UnknownCode &&
                x.SoundCode == entries[i].SoundCode &&
                x.WallCode == entries[i].WallCode
                );

                ids.Add(entries[i].Name, (ushort)index);
            }

            return ids;
        }

        public BCSV GenerateBCSV(List<Triangle> triangles)
        {
            var entries = GetCollisionEntries();
            var col = RemoveDuplicateEntries(entries);

            KclLibrary.DebugLogger.WriteLine("Generating BCSV...");

            var bcsv = new BCSV();
            bcsv.IsBigEndian = false;
            bcsv.Fields.Add(new BCSV.Field("Sound_code", BCSV.FieldType.Int32, 0, 0x7F, 0));
            bcsv.Fields.Add(new BCSV.Field("Floor_code", BCSV.FieldType.Int32, 0, 0x1F80, 7));
            bcsv.Fields.Add(new BCSV.Field(51726534, BCSV.FieldType.Int32, 0, 0x7E000, 13));
            bcsv.Fields.Add(new BCSV.Field("Wall_code", BCSV.FieldType.Int32, 0, 0x780000, 19));
            bcsv.Fields.Add(new BCSV.Field("Camera_through", BCSV.FieldType.Int32, 0, 0x800000, 23));

            foreach (var tri in triangles)
            {
                var entry = col[tri.Attribute];
                var record = new BCSV.Record(new object[5]
                {
                    (uint)CreateEntry(SoundCodes, entry.SoundCode),
                    (uint)CreateEntry(FloorCodes, entry.FloorCode),
                    (uint)entry.UnknownCode,
                    (uint)CreateEntry(WallCodes, entry.WallCode),
                    (uint)(entry.CameraThrough ? 1 : 0),
                });
                tri.Attribute = (ushort)bcsv.Records.Count;
                bcsv.Records.Add(record);
            }

            return bcsv;
        }

        private int CreateEntry(string[] input, string key)
        {
            return Array.IndexOf(input, key);
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                ItemLoaded = false;

                CollisionEntry tag = (CollisionEntry)listView1.SelectedItems[0].Tag;
                unknownCodeUD.Value = tag.UnknownCode;
                chkCameraThrough.Checked = tag.CameraThrough;
                floorCodeCB.SelectedItem = tag.FloorCode;
                soundCodeCB.SelectedItem = tag.SoundCode;
                wallCodeCB.SelectedItem = tag.WallCode;

                ItemLoaded = true;
            }
        }

        private void EditItem(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0 && ItemLoaded)
            {
                foreach (ListViewItem item in listView1.SelectedItems)
                {
                    CollisionEntry tag = (CollisionEntry)item.Tag;

                    tag.CameraThrough = chkCameraThrough.Checked;
                    tag.UnknownCode = (int)unknownCodeUD.Value;
                    tag.FloorCode = floorCodeCB.SelectedItem.ToString();
                    tag.SoundCode = soundCodeCB.SelectedItem.ToString();
                    tag.WallCode = wallCodeCB.SelectedItem.ToString();

                    UpdateListItem(item, tag);
                }
                    
                listView1.Refresh();
            }
        }

        //Codes ported from http://kuribo64.net/board/thread.php?pid=143#143
        public string[] SoundCodes = new string[]
        {
            "null","Ground","Ground2","Metal","Thin Ground","Sand","Metal",
            "Solid Ground","Wood","Snow","Water","Water2","Sand2","Grass","Solid Ground 2",
            "Thin Wood","Snow2","Water Splashes",
        };

        public string[] FloorCodes = new string[]
        {
            "Normal","Damage Normal","Lava Death","Death","Ground1",
            "Ground2","Ground3",
        };

        public string[] WallCodes = new string[]
        {
            "Normal","No Wall Jump","Normal2","Normal3",
            "4","Normal5",
        };
    }
}

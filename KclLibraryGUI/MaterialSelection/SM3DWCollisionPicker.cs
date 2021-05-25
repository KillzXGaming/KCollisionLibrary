using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ByamlExt.Byaml;
using KclLibrary;

namespace KclLibraryGUI
{
    public partial class SM3DWCollisionPicker : UserControl, IMaterialPresetBase
    {
        bool ItemLoaded = false;

        public Dictionary<string, ushort> Result
        {
            get { return GenerateIDs(); }
        }

        public MaterialAttributeFileBase GetAttributeFile(List<Triangle> triangles, bool isBigEndian)
        {
            var matAttributeFile = new MaterialAttributeBymlFile();
            matAttributeFile.BymlFile = GenerateByaml(isBigEndian);
            return matAttributeFile;
        }

        private MaterialSetForm ParentEditor;

        public string[] Materials => ParentEditor.Materials;
        public string[] Meshes => ParentEditor.Meshes;

        public bool UseObjectMaterials => ParentEditor.UseObjectMaterials;

        public SM3DWCollisionPicker(MaterialSetForm parentForm)
        {
            InitializeComponent();

            ParentEditor = parentForm;

            foreach (string val in CameraCodes.Keys)
                cameraCodeCB.Items.Add(val);

            foreach (string val in FloorCodes.Keys)
                floorCodeCB.Items.Add(val);

            foreach (string val in WallCodes.Keys)
                wallCodeCB.Items.Add(val);

            foreach (string val in MaterialCodes.Keys)
                materialCodeCB.Items.Add(val);

            cameraCodeCB.SelectedItem = "NoThrough";
            floorCodeCB.SelectedItem = "Ground";
            wallCodeCB.SelectedItem = "Wall";
            materialCodeCB.SelectedItem = "NoCode";
        }

        public void ReloadDataList()
        {
            listView1.Items.Clear();

            cameraCodeCB.SelectedItem = "NoThrough";
            floorCodeCB.SelectedItem = "Ground";
            wallCodeCB.SelectedItem = "Wall";
            materialCodeCB.SelectedItem = "NoCode";

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
            item.SubItems.Add(entry.CameraCode);
            item.SubItems.Add(entry.FloorCode);
            item.SubItems.Add(entry.MaterialCode);
            item.SubItems.Add(entry.WallCode);
        }

        public class CollisionEntry
        {
            public string Name; //Mesh or material name

            public string CameraCode = "NoThrough";
            public string FloorCode = "Ground";
            public string MaterialCode = "NONE";
            public string WallCode = "Wall";

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
                x.CameraCode == entries[i].CameraCode &&
                x.FloorCode == entries[i].FloorCode &&
                x.MaterialCode == entries[i].MaterialCode &&
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
                x.CameraCode == entries[i].CameraCode &&
                x.MaterialCode == entries[i].MaterialCode &&
                x.WallCode == entries[i].WallCode
                );

                ids.Add(entries[i].Name, (ushort)index);
            }

            return ids;
        }

        public BymlFileData GenerateByaml(bool isBigEndian)
        {
            var entries = GetCollisionEntries();
            var col = RemoveDuplicateEntries(entries);

            List<dynamic> root = new List<dynamic>();

            foreach (var entry in col)
            {
                IDictionary<string, dynamic> colCodes = new Dictionary<string, dynamic>();

                colCodes.Add("CameraCode", CreateEntry(CameraCodes, entry.CameraCode));
                colCodes.Add("FloorCode", CreateEntry(FloorCodes, entry.FloorCode));
                colCodes.Add("MaterialCode", CreateEntry(MaterialCodes, entry.MaterialCode));
                colCodes.Add("WallCode", CreateEntry(WallCodes, entry.WallCode));
                root.Add(colCodes);
            }

            var byml = new BymlFileData();
            byml.byteOrder = isBigEndian ? Syroot.BinaryData.ByteOrder.BigEndian :
                                           Syroot.BinaryData.ByteOrder.LittleEndian;
            byml.Version = (ushort)(isBigEndian ? 1 : 2);

            byml.SupportPaths = false;
            byml.RootNode = root;

            return byml;
        }

        private List<dynamic> CreateEntry(Dictionary<string, int> dictionary, string key)
        {
            List<dynamic> list = new List<dynamic>();
            list.Add(key);
            list.Add(dictionary[key]);
            return list;
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                ItemLoaded = false;

                CollisionEntry tag = (CollisionEntry)listView1.SelectedItems[0].Tag;
                cameraCodeCB.SelectedItem = tag.CameraCode;
                floorCodeCB.SelectedItem = tag.FloorCode;
                materialCodeCB.SelectedItem = tag.MaterialCode;
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

                    tag.CameraCode = cameraCodeCB.SelectedItem.ToString();
                    tag.FloorCode = floorCodeCB.SelectedItem.ToString();
                    tag.MaterialCode = materialCodeCB.SelectedItem.ToString();
                    tag.WallCode = wallCodeCB.SelectedItem.ToString();

                    UpdateListItem(item, tag);
                }
                    
                listView1.Refresh();
            }
        }

        public Dictionary<string, int> CameraCodes = new Dictionary<string, int>()
        {
            { "NoThrough", 7 },
            { "Through", 8 },
        };

        public Dictionary<string, int> FloorCodes = new Dictionary<string, int>()
        {
            { "ClimbSlope", 7 },
            { "DamageFire", 2 },
            { "Ground", 0 },
            { "IgnoreTouch", 8 },
            { "Needle", 1 },
            { "Poison", 3 },
            { "Skate", 6 },
            { "Slide", 4 },
        };

        public Dictionary<string, int> MaterialCodes = new Dictionary<string, int>()
        {
            { "NONE", 0 },
            { "Ashore", 36 },
            { "Carpet", 24 },
            { "ChocoCream", 39 },
            { "Cloth", 34 },
            { "Cloud", 25 },
            { "Cream", 38 },
            { "EchoBlock", 49 },
            { "FallenLeaves", 31 },
            { "Glass", 33 },
            { "Ice", 30 },
            { "InSand", 21 },
            { "InWater", 20 },
            { "Kawara", 44 },
            { "LavaBlue", 43 },
            { "LavaRed", 42 },
            { "Lawn", 11 },
            { "LawnPink", 46 },
            { "Marble", 23 },
            { "Metal", 12 },
            { "MetalHeavy", 15 },
            { "NoCode", 99 },
            { "Puddle", 40 },
            { "Sand", 14 },
            { "Snow", 18 },
            { "Soil", 10 },
            { "SpacePuddle", 51 },
            { "SqueakWood", 45 },
            { "Stone", 13 },
            { "StoneWet", 32 },
            { "Tatami", 48 },
            { "TouchPoint", 41 },
            { "W5Puddle", 50 },
            { "WoodThick", 16 },
            { "WoodThin", 17 },
            { "WoodWet", 35 },
        };

        public Dictionary<string, int> WallCodes = new Dictionary<string, int>()
        {
            { "NoAction", 6 },
            { "Wall", 5 },
        };
    }
}

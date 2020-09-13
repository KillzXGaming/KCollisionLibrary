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
    public partial class OdysseyCollisionPicker : UserControl, IMaterialPresetBase
    {
        bool ItemLoaded = false;

        public Dictionary<string, ushort> Result
        {
            get { return GenerateIDs(); }
        }

        public MaterialAttributeFileBase GetAttributeFile(List<Triangle> triangles)
        {
            var matAttributeFile = new MaterialAttributeBymlFile();
            matAttributeFile.BymlFile = GenerateByaml();
            return matAttributeFile;
        }

        private MaterialSetForm ParentEditor;

        public string[] Materials => ParentEditor.Materials;
        public string[] Meshes => ParentEditor.Meshes;

        public bool UseObjectMaterials => ParentEditor.UseObjectMaterials;

        public OdysseyCollisionPicker(MaterialSetForm parentForm)
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

            foreach (string val in MaterialPrefixCodes.Keys)
                materialPrefixCodeCB.Items.Add(val);

            cameraCodeCB.SelectedItem = "NoThrough";
            floorCodeCB.SelectedItem = "Ground";
            wallCodeCB.SelectedItem = "Wall";
            materialCodeCB.SelectedItem = "NoCode";
            materialPrefixCodeCB.SelectedIndex = 0;
        }

        public void ReloadDataList()
        {
            listView1.Items.Clear();

            cameraCodeCB.SelectedItem = "NoThrough";
            floorCodeCB.SelectedItem = "Ground";
            wallCodeCB.SelectedItem = "Wall";
            materialCodeCB.SelectedItem = "NoCode";
            materialPrefixCodeCB.SelectedIndex = 0;

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
            if (entry.MaterialPrefixCode != "NONE")
                item.SubItems.Add($"{entry.MaterialPrefixCode}_{entry.MaterialCode}");
            else
                item.SubItems.Add($"{entry.MaterialCode}");
            item.SubItems.Add(entry.WallCode);
        }

        public class CollisionEntry
        {
            public string Name; //Mesh or material name

            public string CameraCode = "NoThrough";
            public string FloorCode = "Ground";
            public string MaterialCode = "NONE";
            public string MaterialPrefixCode = "NONE";
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
                x.MaterialPrefixCode == entries[i].MaterialPrefixCode &&
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
                x.MaterialPrefixCode == entries[i].MaterialPrefixCode &&
                x.WallCode == entries[i].WallCode
                );

                ids.Add(entries[i].Name, (ushort)index);
            }

            return ids;
        }

        public BymlFileData GenerateByaml()
        {
            var entries = GetCollisionEntries();
            var col = RemoveDuplicateEntries(entries);

            List<dynamic> root = new List<dynamic>();

            foreach (var entry in col)
            {
                IDictionary<string, dynamic> colCodes = new Dictionary<string, dynamic>();
                colCodes.Add("CameraCode", CameraCodes[entry.CameraCode]);
                colCodes.Add("FloorCode", FloorCodes[entry.FloorCode]);
                if (entry.MaterialPrefixCode != "NONE")
                    colCodes.Add("MaterialCodePrefix", MaterialPrefixCodes[entry.MaterialPrefixCode]);
                colCodes.Add("MaterialCode", MaterialCodes[entry.MaterialCode]);
                colCodes.Add("WallCode", WallCodes[entry.WallCode]);
                root.Add(colCodes);
            }

            var byml = new BymlFileData();
            byml.byteOrder = Syroot.BinaryData.ByteOrder.LittleEndian;
            byml.Version = 3;
            byml.SupportPaths = false;
            byml.RootNode = root;

            return byml;
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
                materialPrefixCodeCB.SelectedItem = tag.MaterialPrefixCode;
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
                    tag.MaterialPrefixCode = materialPrefixCodeCB.SelectedItem.ToString();
                    tag.WallCode = wallCodeCB.SelectedItem.ToString();

                    UpdateListItem(item, tag);
                }

                listView1.Refresh();
            }
        }

        public Dictionary<string, string> CameraCodes = new Dictionary<string, string>()
        {
            { "NONE",            "" },
            { "InvalidThrough",  "CameraInvalidThrough" },
            { "NoThrough",       "CameraNoThrough" },
            { "NoThroughAlways", "CameraNoThroughAlways" },
            { "Through",         "CameraThrough" },
        };


        public Dictionary<string, string> FloorCodes = new Dictionary<string, string>()
        {
            { "NONE", "" },
            { "Bed", "Bed" },
            { "Chair", "Chair" },
            { "ClimbSlope", "ClimbSlope" },
            { "DamageFire", "DamageFire" },
            { "DamageFire2D", "DamageFire2D" },
            { "Fence", "Fence" },
            { "GrabCeil", "GrabCeil" },
            { "Ground", "Ground" },
            { "IgnoreTouch", "IgnoreTouch" },
            { "Jump", "Jump" },
            { "JumpSmall", "JumpSmall" },
            { "Needle", "Needle" },
            { "Poison", "Poison" },
            { "Poison2D", "Poison2D" },
            { "Pole", "Pole" },
            { "Pole10", "Pole10" },
            { "Pole20", "Pole20" },
            { "Pole30Plus", "Pole30Plus" },
            { "Press", "Press" },
            { "Rolling", "Rolling" },
            { "SandSink", "SandSink" },
            { "Skate", "Skate" },
            { "Slide", "Slide" },
            { "Slip", "Slip" },
        };

        public Dictionary<string, string> MaterialCodes = new Dictionary<string, string>()
        {
            { "NONE", "" },
            { "Carpet","Carpet" },
            { "Cloth","Cloth" },
            { "Cloud","Cloud" },
            { "ColNoEffect","ColNoEffect" },
            { "ExStarCube","ExStarCube" },
            { "FlowerForest","FlowerForest" },
            { "FlowerPeach","FlowerPeach" },
            { "Glass","Glass" },
            { "Gravel","Gravel" },
            { "Ice","Ice" },
            { "Kawara","Kawara" },
            { "LavaMarble","LavaMarble" },
            { "LavaPink","LavaPink" },
            { "LavaRed","LavaRed" },
            { "LavaWhite","LavaWhite" },
            { "Lawn","Lawn" },
            { "LawnCap","LawnCap" },
            { "LawnCapTower","LawnCapTower" },
            { "LawnDarkGreen","LawnDarkGreen" },
            { "LawnDeep","LawnDeep" },
            { "LawnDeepForest","LawnDeepForest" },
            { "LawnDeepWaterfall","LawnDeepWaterfall" },
            { "LawnForest","LawnForest" },
            { "LawnWaterfall","LawnWaterfall" },
            { "Marble","Marble" },
            { "MarioCube","MarioCube" },
            { "Metal","Metal" },
            { "MetalHeavy","MetalHeavy" },
            { "MetalLawn","MetalLawn" },
            { "NoCode","NoCode" },
            { "NoCollide","NoCollide" },
            { "PoisonWater","PoisonWater" },
            { "Puddle","Puddle" },
            { "Salt","Salt" },
            { "Sand","Sand" },
            { "SandDesert","SandDesert" },
            { "SandLake","SandLake" },
            { "SandMoon","SandMoon" },
            { "SandSea","SandSea" },
            { "Snow","Snow" },
            { "SnowDeep","SnowDeep" },
            { "Soil","Soil" },
            { "SoilSoft","SoilSoft" },
            { "SqueakWood","SqueakWood" },
            { "Stone","Stone" },
            { "StoneRough","StoneRough" },
            { "Tatami","Tatami" },
            { "WoodThick","WoodThick" },
            { "WoodThin","WoodThin" },
        };

        public Dictionary<string, string> MaterialPrefixCodes = new Dictionary<string, string>()
        {
            { "NONE", "" },
            { "InWater","InWater" },
            { "Shallow","Shallow" },
            { "Wet","Wet" },
        };

        public Dictionary<string, string> WallCodes = new Dictionary<string, string>()
        {
            { "NONE", "" },
            { "NoAction","NoAction" },
            { "NoCeilGrab","NoCeilGrab" },
            { "NoCeilSquat","NoCeilSquat" },
            { "NoClimbPole","NoClimbPole" },
            { "NoClingTongue","NoClingTongue" },
            { "NoTraceStick","NoTraceStick" },
            { "NoWallGrab","NoWallGrab" },
            { "OnlyWallHitDown","OnlyWallHitDown" },
            { "ReflectStick","ReflectStick" },
            { "ReflectStickNoWallGrab","ReflectStickNoWallGrab" },
            { "ThroughStick","ThroughStick" },
            { "Wall","Wall" },
        };
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using KclLibrary;

namespace KclLibraryGUI
{
    public partial class MaterialSetForm : Form
    {
        bool isLoaded = false;

        public Dictionary<string, ushort> Result;
        public ByamlExt.Byaml.BymlFileData AttributeByml;

        public string[] Materials;
        public string[] Meshes;

        public List<CollisionEntry> MatCollisionList = new List<CollisionEntry>();
        public List<CollisionEntry> MeshCollisionList = new List<CollisionEntry>();

        public bool UseObjectMaterials => radioBtnMats.Checked;

        public static CollisionPresetData ActiveGamePreset = null;
        public static string ActiveGamePlatform = null;

        private MaterialGridView DataGridView;
        private OdysseyCollisionPicker OdysseyCollisionPicker;
        private SM3DWCollisionPicker SM3DWCollisionPicker;
        private MaterialCollisionPicker MaterialCollisionPicker;

        public string[] Platforms = new string[]
        {
            "GCN","NDS","N3DS","WII","WII U","SWITCH"
        };

        public MaterialSetForm(string[] mats, string[] meshes)
        {
            Meshes = meshes;
            Materials = mats;
            InitializeComponent();

            if (MaterialWindowSettings.UsePresetEditor)
                chkPresetTypeEditor.Checked = true;

            for (int i = 0; i < Materials.Length; i++)
                MatCollisionList.Add(new CollisionEntry(Materials[i]));
            for (int i = 0; i < Meshes.Length; i++)
                MeshCollisionList.Add(new CollisionEntry(Meshes[i]));

            foreach (var platform in Platforms)
            {
                var item = new ToolStripMenuItem(platform,
                 null, platformPresetToolStripMenuItem_Click);

                platformToolStripMenuItem.DropDownItems.Add(item);

                if (MaterialWindowSettings.Platform == platform) {
                    item.PerformClick();
                }
            }

            foreach (var preset in CollisionPresetData.CollisionPresets)
            {
                var item = new ToolStripMenuItem(preset.GameTitle,
                    null, GamePresetToolStripMenuItem_Click);

                gameSelectToolStripMenuItem.DropDownItems.Add(item);

                if (MaterialWindowSettings.GamePreset == "Default" ||
                    MaterialWindowSettings.GamePreset == preset.GameTitle &&
                    MaterialWindowSettings.Platform == preset.Platform) {
                    item.PerformClick();
                }
            }


            radioBtnMats.Checked = true;
            SetMaterialEditor();
            ReloadDataList();

            isLoaded = true;
        }

        public bool GetEndianness
        {
            get
            {
                switch (ActiveGamePlatform)
                {
                    case "NDS": return false;
                    case "N3DS": return false;
                    case "SWITCH": return false;
                    case "GCN": return true;
                    case "WII": return true;
                    case "WII U": return true;
                    default:
                        return false;
                }
            }
        }

        public FileVersion GetVersion
        {
            get
            {
                switch (ActiveGamePlatform)
                {
                    case "NDS": return FileVersion.VersionDS;
                    case "GCN": return FileVersion.VersionGC;
                    case "WII": return FileVersion.VersionWII;
                    case "N3DS": return FileVersion.VersionWII;
                    default:
                        return FileVersion.Version2;
                }
            }
        }

        private void SetMaterialEditor()
        {
            panel1.Controls.Clear();
            if (ActiveGamePreset != null && chkPresetTypeEditor.Checked && ActiveGamePreset.GameTitle == "Mario Odyssey")
            {
                DataGridView = null;
                MaterialCollisionPicker = null;
                SM3DWCollisionPicker = null;
                OdysseyCollisionPicker = new OdysseyCollisionPicker(this);
                OdysseyCollisionPicker.Dock = DockStyle.Fill;
                panel1.Controls.Add(OdysseyCollisionPicker);
            }
            else if (ActiveGamePreset != null && chkPresetTypeEditor.Checked && ActiveGamePreset.GameTitle == "Mario 3D World")
            {
                DataGridView = null;
                MaterialCollisionPicker = null;
                OdysseyCollisionPicker = null;
                SM3DWCollisionPicker = new SM3DWCollisionPicker(this);
                SM3DWCollisionPicker.Dock = DockStyle.Fill;
                panel1.Controls.Add(SM3DWCollisionPicker);
            }
            else if (ActiveGamePreset != null && ActiveGamePreset.MaterialPresets?.Count > 0 && chkPresetTypeEditor.Checked)
            {
                DataGridView = null;
                OdysseyCollisionPicker = null;
                SM3DWCollisionPicker = null;
                MaterialCollisionPicker = new MaterialCollisionPicker(this);
                MaterialCollisionPicker.Dock = DockStyle.Fill;
                panel1.Controls.Add(MaterialCollisionPicker);
            }
            else
            {
                MaterialCollisionPicker = null;
                OdysseyCollisionPicker = null;
                SM3DWCollisionPicker = null;
                DataGridView = new MaterialGridView(this);
                DataGridView.Dock = DockStyle.Fill;
                panel1.Controls.Add(DataGridView);
            }
        }

        public void ReloadDataList()
        {
            var colList = UseObjectMaterials ? MatCollisionList : MeshCollisionList;

            if (DataGridView != null)
                DataGridView.ReloadDataList(colList);
            else if (MaterialCollisionPicker != null)
                MaterialCollisionPicker.ReloadDataList(colList);
            else if (OdysseyCollisionPicker != null)
                OdysseyCollisionPicker.ReloadDataList();
            else if (SM3DWCollisionPicker != null)
                SM3DWCollisionPicker.ReloadDataList();
        }

        public static MaterialSetForm ShowForm(string[] materials, string[] meshes)
        {
            MaterialSetForm f = new MaterialSetForm(materials, meshes);
            f.ShowDialog();
            return f;
        }

        private void ApplyMaterials()
        {
            if (DataGridView != null)
                Result = DataGridView.Result;
            else if (MaterialCollisionPicker != null)
                Result = MaterialCollisionPicker.Result;
            else if (SM3DWCollisionPicker != null)
            {
                Result = SM3DWCollisionPicker.Result;
                AttributeByml = SM3DWCollisionPicker.GenerateByaml();
            }
            else if (OdysseyCollisionPicker != null)
            {
                Result = OdysseyCollisionPicker.Result;
                AttributeByml = OdysseyCollisionPicker.GenerateByaml();
            }
        }

        private void applyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (ActiveGamePreset == null)
            {
                MessageBox.Show("Make sure to choose a game preset first!");
                return;
            }

            //Apply current setting for next usage
            MaterialWindowSettings.GamePreset = ActiveGamePreset.GameTitle;
            if (ActiveGamePlatform != string.Empty)
                MaterialWindowSettings.Platform = ActiveGamePlatform;
            MaterialWindowSettings.UsePresetEditor = chkPresetTypeEditor.Checked;

            if (DataGridView != null)
            {
                DataGridView.EndEdit();
            }
            ApplyMaterials();
            DialogResult = DialogResult.OK;
            this.Close();
        }

        private void platformPresetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ToolStripMenuItem menu in platformToolStripMenuItem.DropDownItems)
                menu.Checked = false;

            if (sender is ToolStripMenuItem) {
                ((ToolStripMenuItem)sender).Checked = true;

                string name = ((ToolStripMenuItem)sender).Text;
                ActiveGamePlatform = name;
                platformToolStripMenuItem.Text = $"Platform:[{ActiveGamePlatform}]";
            }
        }

        private void GamePresetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ToolStripMenuItem menu in gameSelectToolStripMenuItem.DropDownItems)
                menu.Checked = false;

            if (sender is ToolStripMenuItem) {
                ((ToolStripMenuItem)sender).Checked = true;

                string name = ((ToolStripMenuItem)sender).Text;
                foreach (var preset in CollisionPresetData.CollisionPresets)
                {
                    if (name == preset.GameTitle) {
                        ActiveGamePreset = preset;
                        if (preset.Platform != string.Empty)
                            ActiveGamePlatform = preset.Platform;
                    }
                }

                if (ActiveGamePreset != null)
                    gameSelectToolStripMenuItem.Text = $"Game Select:[{ActiveGamePreset.GameTitle}]";
                if (ActiveGamePlatform != null)
                    platformToolStripMenuItem.Text = $"Platform:[{ActiveGamePlatform}]";

                SetMaterialEditor();
                ReloadDataList();
            }
        }

        private void radioBtnMats_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void radioBtnMeshes_CheckedChanged(object sender, EventArgs e)
        {
        }

        private void radioBtnMats_Click(object sender, EventArgs e)
        {
            radioBtnMeshes.Checked = !radioBtnMats.Checked;
            ReloadDataList();
        }

        private void radioBtnMeshes_Click(object sender, EventArgs e)
        {
            radioBtnMats.Checked = !radioBtnMeshes.Checked;
            ReloadDataList();
        }

        private void chkOdysseyTypeEditor_CheckedChanged(object sender, EventArgs e)
        {
            if (isLoaded)
            {
                SetMaterialEditor();
                ReloadDataList();
            }
        }
    }
}

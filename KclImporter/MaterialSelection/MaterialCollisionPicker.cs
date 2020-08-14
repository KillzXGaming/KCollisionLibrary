using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace KCLExt
{
    public partial class MaterialCollisionPicker : UserControl
    {
        private MaterialSetForm ParentEditor;
        private bool ItemLoaded;

        public string[] Materials => ParentEditor.Materials;
        public string[] Meshes => ParentEditor.Meshes;

        public bool UseObjectMaterials => ParentEditor.UseObjectMaterials;

        public CollisionPresetData CollsionPreset => MaterialSetForm.ActiveGamePreset;

        public Dictionary<string, ushort> Result
        {
            get
            {
                Dictionary<string, ushort> result = new Dictionary<string, ushort>();
                foreach (ListViewItem item in listView1.Items)
                {
                    CollisionEntry tag = (CollisionEntry)item.Tag;
                    result.Add(tag.Name, tag.TypeID);

                    Console.WriteLine($"picker {tag.Name} {tag.TypeID}");
                }
                Console.WriteLine("result picker" + result.Count);
                return result;
            }
        }

        public MaterialCollisionPicker(MaterialSetForm parentForm)
        {
            InitializeComponent();
            ParentEditor = parentForm;
        }

        public void ReloadDataList(List<CollisionEntry> entries)
        {
            listView1.Items.Clear();

            comboBox1.Items.Clear();
            foreach (var item in CollsionPreset.MaterialPresets)
                comboBox1.Items.Add(item.Value);

            for (int i = 0; i < entries.Count; i++)
            {
                var item = new ListViewItem();
                UpdateListItem(item, entries[i]);
                listView1.Items.Add(item);
            }
        }



        private void UpdateListItem(ListViewItem item, CollisionEntry entry)
        {
            item.SubItems.Clear();

            item.Tag = entry;
            item.Text = entry.Name;
            item.SubItems.Add(entry.TypeID.ToString());
            item.SubItems.Add(entry.Type);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0 && ItemLoaded)
            {
                foreach (ListViewItem item in listView1.SelectedItems)
                {
                    CollisionEntry tag = (CollisionEntry)item.Tag;
                    string type = comboBox1.SelectedItem.ToString();
                    tag.TypeID = MaterialSetForm.ActiveGamePreset.GetMaterialID(type);
                    numericUpDown1.Value = tag.TypeID;
                    UpdateListItem(item, tag);
                }

                listView1.Refresh();
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                ItemLoaded = false;
                CollisionEntry tag = (CollisionEntry)listView1.SelectedItems[0].Tag;
                comboBox1.SelectedItem = tag.Type;
                numericUpDown1.Value = tag.TypeID;
                ItemLoaded = true;
            }
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0 && ItemLoaded)
            {
                foreach (ListViewItem item in listView1.SelectedItems)
                {
                    ItemLoaded = false;
                    CollisionEntry tag = (CollisionEntry)item.Tag;
                    tag.TypeID = (ushort)numericUpDown1.Value;
                    comboBox1.SelectedItem = tag.Type;

                    UpdateListItem(item, tag);

                    ItemLoaded = true;
                }
            }
        }
    }
}

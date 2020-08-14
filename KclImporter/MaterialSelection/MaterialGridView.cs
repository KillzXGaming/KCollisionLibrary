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
    public partial class MaterialGridView : UserControl
    {
        private MaterialSetForm ParentEditor;

        public bool UseObjectMaterials => ParentEditor.UseObjectMaterials;

        private DataGridViewComboBoxColumn presetsCB;

        public Dictionary<string, ushort> Result
        {
            get
            {
                var result = new Dictionary<string, ushort>();
                for (int i = 0; i < dataGridView1.Rows.Count; i++)
                {
                    var v = dataGridView1[1, i].Value.ToString();
                    result.Add(dataGridView1[0, i].Value.ToString(), v == "-1" ? ushort.MaxValue : ushort.Parse(v));
                }
                return result;
            }
        }

        public MaterialGridView(MaterialSetForm parentForm)
        {
            InitializeComponent();

            ParentEditor = parentForm;
        }

        public void ReloadDataList(List<CollisionEntry> entries)
        {
            dataGridView1.Rows.Clear();

            for (int i = 0; i < entries.Count; i++)
            {
                dataGridView1.Rows.Add(entries[i].Name, entries[i].TypeID);
            }
        }

        public void EndEdit()
        {
            dataGridView1.EndEdit();
        }


        private Dictionary<string, int> CollisionMk8 = new Dictionary<string, int>()
        {
            { "Road", 0 },
            { "Road (Bumpy)", 2 },
            { "Road (Slippery)", 4 },
            { "Road (Offroad Sand)", 6 },
            { "Road (Slippery Effect Only)", 9 },
            { "Road (Booster)", 10 },
            { "Latiku" ,16 },
            { "Glider", 31 },
            { "Road (Foamy Sound)", 32 },
            { "Road (Offroad, clicking Sound)", 40 },
            { "Unsolid",56 },
            { "Water (Drown reset)", 60 },
            { "Road (Rocky Sound)", 64 },
            { "Wall", 81 },
            { "Road (3DS MP Piano)", 129 },
            { "Road (RoyalR Offroad Grass)", 134 },
            { "Road (3DS MP Xylophone)", 161 },
            { "Road (3DS MP Vibraphone)", 193 },
            { "Road (SNES RR road)", 227 },
            { "Road (MKS Offroad Grass)", 297 },
            { "Road (Water Wall)", 500 },
            { "Road (Stunt)", 4096 },
            { "Road (Booster + Stunt)", 4106 },
            { "Road (Stunt + Glider)", 4108 },
        };
    }
}

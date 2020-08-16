namespace KclLibraryGUI
{
    partial class OdysseyCollisionPicker
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.cameraCodeCB = new System.Windows.Forms.ComboBox();
            this.materialCodeCB = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.floorCodeCB = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.wallCodeCB = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.materialPrefixCodeCB = new System.Windows.Forms.ComboBox();
            this.SuspendLayout();
            // 
            // listView1
            // 
            this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader5,
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4});
            this.listView1.FullRowSelect = true;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(3, 3);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(661, 302);
            this.listView1.TabIndex = 0;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Materials";
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "CameraCode";
            this.columnHeader1.Width = 128;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "FloorCode";
            this.columnHeader2.Width = 138;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "MaterialCode";
            this.columnHeader3.Width = 134;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "WallCode";
            this.columnHeader4.Width = 63;
            // 
            // cameraCodeCB
            // 
            this.cameraCodeCB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.cameraCodeCB.FormattingEnabled = true;
            this.cameraCodeCB.Location = new System.Drawing.Point(60, 314);
            this.cameraCodeCB.Name = "cameraCodeCB";
            this.cameraCodeCB.Size = new System.Drawing.Size(190, 21);
            this.cameraCodeCB.TabIndex = 1;
            this.cameraCodeCB.SelectedIndexChanged += new System.EventHandler(this.EditItem);
            // 
            // materialCodeCB
            // 
            this.materialCodeCB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.materialCodeCB.FormattingEnabled = true;
            this.materialCodeCB.Location = new System.Drawing.Point(347, 314);
            this.materialCodeCB.Name = "materialCodeCB";
            this.materialCodeCB.Size = new System.Drawing.Size(190, 21);
            this.materialCodeCB.TabIndex = 2;
            this.materialCodeCB.SelectedIndexChanged += new System.EventHandler(this.EditItem);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 317);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(46, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Camera:";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(270, 317);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(47, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Material:";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 344);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(33, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Floor:";
            // 
            // floorCodeCB
            // 
            this.floorCodeCB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.floorCodeCB.FormattingEnabled = true;
            this.floorCodeCB.Location = new System.Drawing.Point(60, 341);
            this.floorCodeCB.Name = "floorCodeCB";
            this.floorCodeCB.Size = new System.Drawing.Size(190, 21);
            this.floorCodeCB.TabIndex = 5;
            this.floorCodeCB.SelectedIndexChanged += new System.EventHandler(this.EditItem);
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 371);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(31, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Wall:";
            // 
            // wallCodeCB
            // 
            this.wallCodeCB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.wallCodeCB.FormattingEnabled = true;
            this.wallCodeCB.Location = new System.Drawing.Point(60, 368);
            this.wallCodeCB.Name = "wallCodeCB";
            this.wallCodeCB.Size = new System.Drawing.Size(190, 21);
            this.wallCodeCB.TabIndex = 7;
            this.wallCodeCB.SelectedIndexChanged += new System.EventHandler(this.EditItem);
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(270, 344);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(76, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "Material Prefix:";
            // 
            // materialPrefixCodeCB
            // 
            this.materialPrefixCodeCB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.materialPrefixCodeCB.FormattingEnabled = true;
            this.materialPrefixCodeCB.Location = new System.Drawing.Point(347, 341);
            this.materialPrefixCodeCB.Name = "materialPrefixCodeCB";
            this.materialPrefixCodeCB.Size = new System.Drawing.Size(190, 21);
            this.materialPrefixCodeCB.TabIndex = 9;
            this.materialPrefixCodeCB.SelectedIndexChanged += new System.EventHandler(this.EditItem);
            // 
            // OdysseyCollisionPicker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label5);
            this.Controls.Add(this.materialPrefixCodeCB);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.wallCodeCB);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.floorCodeCB);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.materialCodeCB);
            this.Controls.Add(this.cameraCodeCB);
            this.Controls.Add(this.listView1);
            this.Name = "OdysseyCollisionPicker";
            this.Size = new System.Drawing.Size(667, 397);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ComboBox cameraCodeCB;
        private System.Windows.Forms.ComboBox materialCodeCB;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox floorCodeCB;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox wallCodeCB;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox materialPrefixCodeCB;
        private System.Windows.Forms.ColumnHeader columnHeader5;
    }
}

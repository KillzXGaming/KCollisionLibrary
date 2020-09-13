namespace KclLibraryGUI
{
    partial class SM3DLCollisionPicker
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
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.soundCodeCB = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.floorCodeCB = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.wallCodeCB = new System.Windows.Forms.ComboBox();
            this.chkCameraThrough = new System.Windows.Forms.CheckBox();
            this.unknownCodeUD = new System.Windows.Forms.NumericUpDown();
            this.label1 = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.unknownCodeUD)).BeginInit();
            this.SuspendLayout();
            // 
            // listView1
            // 
            this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader6,
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
            // columnHeader6
            // 
            this.columnHeader6.Text = "Material";
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "0x31548C6";
            this.columnHeader5.Width = 74;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Camera Through";
            this.columnHeader1.Width = 128;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Floor Code";
            this.columnHeader2.Width = 138;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Wall Code";
            this.columnHeader3.Width = 134;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Sound Code";
            this.columnHeader4.Width = 114;
            // 
            // soundCodeCB
            // 
            this.soundCodeCB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.soundCodeCB.FormattingEnabled = true;
            this.soundCodeCB.Location = new System.Drawing.Point(347, 314);
            this.soundCodeCB.Name = "soundCodeCB";
            this.soundCodeCB.Size = new System.Drawing.Size(190, 21);
            this.soundCodeCB.TabIndex = 2;
            this.soundCodeCB.SelectedIndexChanged += new System.EventHandler(this.EditItem);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(290, 317);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Sound:";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(3, 317);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(33, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "Floor:";
            // 
            // floorCodeCB
            // 
            this.floorCodeCB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.floorCodeCB.FormattingEnabled = true;
            this.floorCodeCB.Location = new System.Drawing.Point(88, 314);
            this.floorCodeCB.Name = "floorCodeCB";
            this.floorCodeCB.Size = new System.Drawing.Size(181, 21);
            this.floorCodeCB.TabIndex = 5;
            this.floorCodeCB.SelectedIndexChanged += new System.EventHandler(this.EditItem);
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(290, 344);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(31, 13);
            this.label4.TabIndex = 8;
            this.label4.Text = "Wall:";
            // 
            // wallCodeCB
            // 
            this.wallCodeCB.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.wallCodeCB.FormattingEnabled = true;
            this.wallCodeCB.Location = new System.Drawing.Point(347, 341);
            this.wallCodeCB.Name = "wallCodeCB";
            this.wallCodeCB.Size = new System.Drawing.Size(190, 21);
            this.wallCodeCB.TabIndex = 7;
            this.wallCodeCB.SelectedIndexChanged += new System.EventHandler(this.EditItem);
            // 
            // chkCameraThrough
            // 
            this.chkCameraThrough.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.chkCameraThrough.AutoSize = true;
            this.chkCameraThrough.Location = new System.Drawing.Point(177, 343);
            this.chkCameraThrough.Name = "chkCameraThrough";
            this.chkCameraThrough.Size = new System.Drawing.Size(105, 17);
            this.chkCameraThrough.TabIndex = 9;
            this.chkCameraThrough.Text = "Camera Through";
            this.chkCameraThrough.UseVisualStyleBackColor = true;
            this.chkCameraThrough.CheckedChanged += new System.EventHandler(this.EditItem);
            // 
            // unknownCodeUD
            // 
            this.unknownCodeUD.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.unknownCodeUD.Location = new System.Drawing.Point(88, 342);
            this.unknownCodeUD.Maximum = new decimal(new int[] {
            255,
            0,
            0,
            0});
            this.unknownCodeUD.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            -2147483648});
            this.unknownCodeUD.Name = "unknownCodeUD";
            this.unknownCodeUD.Size = new System.Drawing.Size(83, 20);
            this.unknownCodeUD.TabIndex = 10;
            this.unknownCodeUD.ValueChanged += new System.EventHandler(this.EditItem);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 344);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(81, 13);
            this.label1.TabIndex = 11;
            this.label1.Text = "Unknown Code";
            // 
            // SM3DLCollisionPicker
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.label1);
            this.Controls.Add(this.unknownCodeUD);
            this.Controls.Add(this.chkCameraThrough);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.wallCodeCB);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.floorCodeCB);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.soundCodeCB);
            this.Controls.Add(this.listView1);
            this.Name = "SM3DLCollisionPicker";
            this.Size = new System.Drawing.Size(667, 397);
            ((System.ComponentModel.ISupportInitialize)(this.unknownCodeUD)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ComboBox soundCodeCB;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox floorCodeCB;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox wallCodeCB;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.CheckBox chkCameraThrough;
        private System.Windows.Forms.NumericUpDown unknownCodeUD;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ColumnHeader columnHeader6;
    }
}

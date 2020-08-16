namespace KclLibraryGUI
{
    partial class MaterialSetForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.applyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.gameSelectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.radioBtnMats = new System.Windows.Forms.RadioButton();
            this.radioBtnMeshes = new System.Windows.Forms.RadioButton();
            this.chkPresetTypeEditor = new System.Windows.Forms.CheckBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.platformToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.applyToolStripMenuItem,
            this.gameSelectToolStripMenuItem,
            this.platformToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(559, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // applyToolStripMenuItem
            // 
            this.applyToolStripMenuItem.Name = "applyToolStripMenuItem";
            this.applyToolStripMenuItem.Size = new System.Drawing.Size(50, 20);
            this.applyToolStripMenuItem.Text = "Apply";
            this.applyToolStripMenuItem.Click += new System.EventHandler(this.applyToolStripMenuItem_Click);
            // 
            // gameSelectToolStripMenuItem
            // 
            this.gameSelectToolStripMenuItem.Name = "gameSelectToolStripMenuItem";
            this.gameSelectToolStripMenuItem.Size = new System.Drawing.Size(84, 20);
            this.gameSelectToolStripMenuItem.Text = "Game Select";
            // 
            // radioBtnMats
            // 
            this.radioBtnMats.AutoSize = true;
            this.radioBtnMats.Location = new System.Drawing.Point(12, 30);
            this.radioBtnMats.Name = "radioBtnMats";
            this.radioBtnMats.Size = new System.Drawing.Size(120, 17);
            this.radioBtnMats.TabIndex = 2;
            this.radioBtnMats.TabStop = true;
            this.radioBtnMats.Text = "Material by materials";
            this.radioBtnMats.UseVisualStyleBackColor = true;
            this.radioBtnMats.CheckedChanged += new System.EventHandler(this.radioBtnMats_CheckedChanged);
            this.radioBtnMats.Click += new System.EventHandler(this.radioBtnMats_Click);
            // 
            // radioBtnMeshes
            // 
            this.radioBtnMeshes.AutoSize = true;
            this.radioBtnMeshes.Location = new System.Drawing.Point(138, 30);
            this.radioBtnMeshes.Name = "radioBtnMeshes";
            this.radioBtnMeshes.Size = new System.Drawing.Size(115, 17);
            this.radioBtnMeshes.TabIndex = 3;
            this.radioBtnMeshes.TabStop = true;
            this.radioBtnMeshes.Text = "Material by meshes";
            this.radioBtnMeshes.UseVisualStyleBackColor = true;
            this.radioBtnMeshes.CheckedChanged += new System.EventHandler(this.radioBtnMeshes_CheckedChanged);
            this.radioBtnMeshes.Click += new System.EventHandler(this.radioBtnMeshes_Click);
            // 
            // chkPresetTypeEditor
            // 
            this.chkPresetTypeEditor.AutoSize = true;
            this.chkPresetTypeEditor.Location = new System.Drawing.Point(404, 5);
            this.chkPresetTypeEditor.Name = "chkPresetTypeEditor";
            this.chkPresetTypeEditor.Size = new System.Drawing.Size(86, 17);
            this.chkPresetTypeEditor.TabIndex = 4;
            this.chkPresetTypeEditor.Text = "Preset Editor";
            this.chkPresetTypeEditor.UseVisualStyleBackColor = true;
            this.chkPresetTypeEditor.CheckedChanged += new System.EventHandler(this.chkOdysseyTypeEditor_CheckedChanged);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.Location = new System.Drawing.Point(0, 53);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(559, 326);
            this.panel1.TabIndex = 5;
            // 
            // platformToolStripMenuItem
            // 
            this.platformToolStripMenuItem.Name = "platformToolStripMenuItem";
            this.platformToolStripMenuItem.Size = new System.Drawing.Size(68, 20);
            this.platformToolStripMenuItem.Text = "Platform:";
            // 
            // MaterialSetForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(559, 381);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.chkPresetTypeEditor);
            this.Controls.Add(this.radioBtnMeshes);
            this.Controls.Add(this.radioBtnMats);
            this.Controls.Add(this.menuStrip1);
            this.Name = "MaterialSetForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Material Selection";
            this.TopMost = true;
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem applyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem gameSelectToolStripMenuItem;
        private System.Windows.Forms.RadioButton radioBtnMats;
        private System.Windows.Forms.RadioButton radioBtnMeshes;
        private System.Windows.Forms.CheckBox chkPresetTypeEditor;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ToolStripMenuItem platformToolStripMenuItem;
    }
}
namespace igCauldron2
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this._menuStrip = new System.Windows.Forms.MenuStrip();
            this._tmsiFile = new System.Windows.Forms.ToolStripMenuItem();
            this._tmsiOpenFile = new System.Windows.Forms.ToolStripMenuItem();
            this._treeObjects = new System.Windows.Forms.TreeView();
            this._panelInspector = new System.Windows.Forms.Panel();
            this._menuStrip.SuspendLayout();
            this.SuspendLayout();
            //
            // menuStrip
            //
            this._menuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._tmsiFile});
            this._menuStrip.Location = new System.Drawing.Point(0, 0);
            this._menuStrip.Name = "menuStrip";
            this._menuStrip.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this._menuStrip.Size = new System.Drawing.Size(788, 24);
            this._menuStrip.TabIndex = 17;
            this._menuStrip.Text = "menuStrip";
            //
            // tmsiFile
            //
			this._tmsiFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this._tmsiOpenFile});
			this._tmsiFile.Name = "tmsiOpen";
			this._tmsiFile.Size = new System.Drawing.Size(37, 20);
			this._tmsiFile.Text = "File";
			// 
			// tmsiOpenFile
			// 
			this._tmsiOpenFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {});
			this._tmsiOpenFile.Name = "tmsiOpenFile";
			this._tmsiOpenFile.Size = new System.Drawing.Size(180, 22);
			this._tmsiOpenFile.Text = "Open File";
            this._tmsiOpenFile.Click += new System.EventHandler(this.OnOpenFileClick);
			// 
			// treeObjects
			// 
			this._treeObjects.Location = new System.Drawing.Point(12, 40);
			this._treeObjects.Name = "treObjects";
			this._treeObjects.Size = new System.Drawing.Size(515, 480);
			this._treeObjects.TabIndex = 1;
            // 
            // _panelInspector
            // 
            this._panelInspector.Dock = System.Windows.Forms.DockStyle.None;
            this._panelInspector.Location = new System.Drawing.Point(12, 400);
            this._panelInspector.Name = "_panelInspector";
            this._panelInspector.Size = new System.Drawing.Size(400, 400);
            this._panelInspector.Text = "_panelInspector";
			this._panelInspector.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            //
            // Form1
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this._treeObjects);
            this.Controls.Add(this._menuStrip);
            this.Controls.Add(this._panelInspector);
            this.MainMenuStrip = this._menuStrip;
            this.Text = "Form1";
            this.Resize += new System.EventHandler(this.OnResize);
            this._menuStrip.ResumeLayout(false);
            this._menuStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.MenuStrip _menuStrip;
        private System.Windows.Forms.ToolStripMenuItem _tmsiFile;
        private System.Windows.Forms.ToolStripMenuItem _tmsiOpenFile;
        private System.Windows.Forms.TreeView _treeObjects;
        private System.Windows.Forms.Panel _panelInspector;
    }
}


namespace RedBlackTreeGui
{
    partial class KeysLayout
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
            this.vScrollBar1 = new System.Windows.Forms.VScrollBar();
            this.treeViewKeys = new System.Windows.Forms.TreeView();
            this.SuspendLayout();
            // 
            // vScrollBar1
            // 
            this.vScrollBar1.Location = new System.Drawing.Point(1066, -2);
            this.vScrollBar1.Name = "vScrollBar1";
            this.vScrollBar1.Size = new System.Drawing.Size(26, 619);
            this.vScrollBar1.TabIndex = 0;
            // 
            // treeViewKeys
            // 
            this.treeViewKeys.Location = new System.Drawing.Point(12, 12);
            this.treeViewKeys.Name = "treeViewKeys";
            this.treeViewKeys.Size = new System.Drawing.Size(1041, 615);
            this.treeViewKeys.TabIndex = 1;
            // 
            // KeysLayout
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1097, 639);
            this.Controls.Add(this.treeViewKeys);
            this.Controls.Add(this.vScrollBar1);
            this.Name = "KeysLayout";
            this.Text = "KeysLayout";
            this.ResumeLayout(false);

        }

        #endregion

        private VScrollBar vScrollBar1;
        private TreeView treeViewKeys;
    }
}
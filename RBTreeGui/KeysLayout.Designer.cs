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
            this.treeViewKeys = new System.Windows.Forms.TreeView();
            this.SuspendLayout();
            // 
            // treeViewKeys
            // 
            this.treeViewKeys.Font = new System.Drawing.Font("Segoe UI", 10.2F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.treeViewKeys.Indent = 40;
            this.treeViewKeys.ItemHeight = 20;
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
            this.Name = "KeysLayout";
            this.Text = "KeysLayout";
            this.ResumeLayout(false);

        }

        #endregion
        private TreeView treeViewKeys;
    }
}
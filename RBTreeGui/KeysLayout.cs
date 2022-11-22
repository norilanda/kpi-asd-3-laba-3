using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace RedBlackTreeGui
{
    public partial class KeysLayout : Form
    {
        public KeysLayout(Dictionary<int, (int color, int? leftKey, int? rightKey)> keys, int? rootKey)
        {
            InitializeComponent();
            DisplayKeys(keys, rootKey);
        }
        private void DisplayKeys(Dictionary<int, (int color, int? leftKey, int? rightKey)> keys, int? rootKey)
        {
            //TreeNode node, child1, child2;
            //node = treeViewKeys.Nodes.Add("Master node");
            //child1 = node.Nodes.Add("Child node");
            //child1.BackColor = Color.Red;
            //child2 = node.Nodes.Add("Child node 2");

            //node = child1;
            //child1 = node.Nodes.Add("mychild");
            //child2 = node.Nodes.Add("mychild");
            TreeNode root;
            if (rootKey != null)
            {
                root = treeViewKeys.Nodes.Add(Convert.ToString(rootKey));
                DisplayKeysRecurion(keys, root, rootKey);
            }
            else
                root = treeViewKeys.Nodes.Add("null");



            treeViewKeys.ExpandAll();
        }
        private void DisplayKeysRecurion(Dictionary<int, (int color, int? leftKey, int? rightKey)> keys, TreeNode parent, int? key)
        {
            TreeNode leftChild, rightChild;
            if (key != null && keys[(int)key].color == 0)
                parent.BackColor= Color.Red;
            else
            {
                parent.BackColor = Color.Black;
                parent.ForeColor = Color.White;
            }
            if (key != null)
            {
                int? leftKey = keys[(int)key].leftKey;
                int? rightKey = keys[(int)key].rightKey;
                leftChild = parent.Nodes.Add(Convert.ToString(leftKey));
                DisplayKeysRecurion(keys, leftChild, leftKey);
                rightChild = parent.Nodes.Add(Convert.ToString(rightKey));
                DisplayKeysRecurion(keys, rightChild, rightKey);
            }
            else
            {
                TreeNode nullNode = parent.Nodes.Add("null");
                nullNode.BackColor = Color.Black;
                nullNode.ForeColor = Color.White;
            }
        }
    }
}

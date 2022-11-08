namespace RBTreeGui
{
    using RedBlackTreeAlgo;
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            RedBlackTree rbt =  new RedBlackTree();
            int[] arr = { 3, 2, 1 }; //, 6, 4, 0
            for (int i=0; i<arr.Length; i++)
                rbt.Insert(arr[i], arr[i]);
        }
    }
}
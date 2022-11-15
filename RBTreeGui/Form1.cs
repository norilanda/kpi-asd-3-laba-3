namespace RBTreeGui
{
    using RedBlackTreeAlgo;
    using RedBlackTreeAlgo.DatabaseManager;


    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            string fileName = "file.txt";
            byte[] b = Parser.CreateMetadataForDB("id int, lake char(15)");
            int failer = 0;
            DBManager.CreateDatabase(fileName, b);
            DBManager dBManager = new DBManager(fileName);
            
            //dBManager.Insert(3, b);
            //dBManager.Insert(4, b);
            //dBManager.Insert(5, b);
            //dBManager.Insert(6, b);
            //dBManager.Insert(1, b);
            //dBManager.Insert(2, b);
            //dBManager.Insert(9, b);
            for(int i=0;i<10000;i++)
            {
                if (!dBManager.Insert(i, b))
                    failer++;
            }

            if (failer == 0)
            {
                int e = 0;
            }
            else
            {
                int e = failer;
            }
            byte[] data = dBManager.Search(1098);

            /*
            
            RedBlackTree rbt = new RedBlackTree();
            int[] arr = { 3, 4, 5, 1, 2, 6, 7, 0 }; //, 6, 4, 0
            for (int i = 0; i < arr.Length; i++)
            {
                bool success = rbt.Insert(arr[i], arr[i]);
                if (!success)
                    MessageBox.Show("Smth went wrong!", "Error");
            }
            rbt.Delete(6);
            Node? n = rbt.Search(4);
            if (n != null)
                MessageBox.Show("Node is found", "Node searching");
            else
                MessageBox.Show("Node is NOT found :( ", "Node searching"); */
        }
    }
}
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
            int dataSize;
            byte[] metadata = Parser.CreateMetadataForDB("id int, lake char(15)", out dataSize);
            int failer = 0;
            //DBManager.CreateDatabase(fileName, metadata, dataSize);
            DBManager dBManager = new DBManager(fileName);

            byte[] data = new byte[dataSize];

            //dBManager.Insert(3, data);

            //dBManager.Insert(4, data);

            //dBManager.Insert(5, data);
            //dBManager.Insert(6, data);
            //dBManager.Insert(1, data);
            //dBManager.Insert(2, data);
            //dBManager.Insert(43, data);
            //for(int i=0;i<10000;i++)
            //{
            //    if (!dBManager.Insert(i, data))
            //        failer++;
            //}

            //if (failer == 0)
            //{
            //    int e = 0;
            //}
            //else
            //{
            //    int e = failer;
            //}
            byte[] searchingData = dBManager.Search(78);

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
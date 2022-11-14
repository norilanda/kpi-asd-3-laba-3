namespace RBTreeGui
{
    using RedBlackTreeAlgo;
    using RedBlackTreeAlgo.DatabaseManager;
    using RedBlackTreeAlgo.FileStructure;

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            string fileName = "file.txt";
            BinaryWriter binaryWriter = new BinaryWriter(File.Open(fileName, FileMode.Create));
            Record record = new Record(98, 1, 65536);
            byte[] recordS = record.RecordSerialization();
            binaryWriter.Write(recordS);
            binaryWriter.Close();

            BinaryReader br = new BinaryReader(File.Open(fileName, FileMode.Open));
            byte[] bytesFromFile = br.ReadBytes(recordS.Length);
            Record record2 = new Record(bytesFromFile);

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
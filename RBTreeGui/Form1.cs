namespace RBTreeGui
{
    using RedBlackTreeAlgo;
    using RedBlackTreeAlgo.DatabaseManager;


    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string DBname = textBoxDBName.Text;
            string input = textBoxInput.Text;

            DBname = "file.txt";
            int dataSize;
            byte[] metadata = Parser.CreateMetadataForDB("id int, lake char(15)", out dataSize);
            int failer = 0;
            //DBManager.CreateDatabase(fileName, metadata, dataSize);
            DBManager dBManager = new DBManager(DBname);

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
        }
    }
}
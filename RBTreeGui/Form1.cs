namespace RBTreeGui
{
    using RedBlackTreeAlgo;
    using RedBlackTreeAlgo.DatabaseManager;


    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            int temp;
            byte[] md = Parser.CreateMetadataForDB("user_id int,name char(10),income double", out temp); //should write without space
            byte[] d = Parser.DataToByte(md, "5,AnnMarrie,400.25");
            string dat = Parser.BytesToData(md, d);            
        }

        private void btnInsert_Click(object sender, EventArgs e)
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
            
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string DBname = textBoxDBName.Text;
            string input = textBoxInput.Text;
            int keyToSearch = Convert.ToInt32(input);
            DBname = "file.txt";
            DBManager dBManager = new DBManager(DBname);
            byte[]? searchingData = dBManager.Search(keyToSearch);
            if(searchingData != null)
            {
                textBoxErrors.Text = "Success; 1 row returned";
            }
            else
            {
                textBoxErrors.Text = "Success; 0 row returned";
            }
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            string DBname = textBoxDBName.Text;
            string input = textBoxInput.Text;

            DBname = "file.txt";
            int dataSize;
            byte[] metadata = Parser.CreateMetadataForDB("id int,lake char(15)", out dataSize);//should write without space
            DBManager.CreateDatabase(DBname, metadata, dataSize);
        }
    }
}
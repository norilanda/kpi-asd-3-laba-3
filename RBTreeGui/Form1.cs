namespace RBTreeGui
{
    using RedBlackTreeAlgo;
    using RedBlackTreeAlgo.DatabaseManager;


    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            
            //byte[] md = Parser.CreateMetadataForDB("user_id int,name char(10),income double", out temp); //should write without space
            //byte[] d = Parser.DataToByte(md, "5,AnnMarrie,400.25");
            //string dat = Parser.BytesToData(md, d);            
        }

        private void btnInsert_Click(object sender, EventArgs e)
        {
            string DBname = textBoxDBName.Text;
            string input = textBoxInput.Text;

            if (input.Length == 0 )
                textBoxErrors.Text = "You haven't entered the data!";
            else
            {
                DBname = "file.txt";
                DBManager dBManager = new DBManager(DBname);
                bool flag = dBManager.InsertData(input);
                //dBManager.InsertData(input);

                if (flag)
                    textBoxErrors.Text = "Success; 1 row inserted";
                else
                    textBoxErrors.Text = "Failed";
            }           
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string DBname = textBoxDBName.Text;
            string input = textBoxInput.Text;
            if (input.Length == 0)
                textBoxErrors.Text = "You haven't entered the key!";
            else
            {
                int keyToSearch = Convert.ToInt32(input);
                DBname = "file.txt";
                DBManager dBManager = new DBManager(DBname);
                string? searchingData = dBManager.SearchData(keyToSearch);
                if (searchingData != null)
                {
                    textBoxErrors.Text = "Success; 1 row returned";
                    textBoxResults.Text = searchingData;
                }
                else
                {
                    textBoxErrors.Text = "Success; 0 row returned";
                    textBoxResults.Text = "";
                }
            }             
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            string DBname = textBoxDBName.Text;
            string input = textBoxInput.Text;

            DBname = "file.txt";
            input = "id int,lake char(15)";
            if (DBname.Length == 0)
                textBoxErrors.Text = "You haven't entered the DB name!";
            else if(input.Length == 0)
                textBoxErrors.Text = "You haven't entered the DB description!";
            else
            {                
                DBManager.CreateDatabase(DBname, input);
                textBoxErrors.Text = "Success; DB "+ DBname + "has been created.";
            }           
        }
    }
}
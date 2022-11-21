namespace RBTreeGui
{
    using RedBlackTreeAlgo;
    using RedBlackTreeAlgo.DatabaseManager;
    using RedBlackTreeAlgo.Exceptions;
    using RedBlackTreeGui;
    using System.Linq.Expressions;
    using System.Xml.Linq;

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

                     
        }

        private void btnInsert_Click(object sender, EventArgs e)
        {
            string DBname = textBoxDBName.Text;
            string input = textBoxInput.Text;

            if (CheckIfDBExists(DBname))
            {
                if (input.Length == 0)
                    textBoxErrors.Text = "You haven't entered the data!";
                else
                {
                    DBManager dBManager = new DBManager(DBname);
                    try
                    {
                        dBManager.InsertData(input);
                        textBoxErrors.Text = "Success; 1 row inserted";
                    }
                    catch (RecordAlreadyExists ex)
                    {
                        textBoxErrors.Text = "Failed; " + ex.Message;
                    }
                    catch (WrongDataFormat ex)
                    {
                        textBoxErrors.Text = "Failed; " + ex.Message;
                    }
                }
            }                   
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string DBname = textBoxDBName.Text;
            string input = textBoxInput.Text;
            if (CheckIfDBExists(DBname)) 
            {
                if (input.Length == 0)
                    textBoxErrors.Text = "You haven't entered the key!";
                else if (DBname.Length == 0)
                    textBoxErrors.Text = "You haven't specified DB name!";

                else
                {
                    try
                    {
                        int keyToSearch = Convert.ToInt32(input);                                       
                        DBManager dBManager = new DBManager(DBname);
                        int comparisonNumber;
                        string? searchingData = dBManager.SearchData(keyToSearch, out comparisonNumber);
                        if (searchingData != null)
                        {
                            textBoxErrors.Text = "Success; 1 row returned";
                            textBoxResults.Text = searchingData;
                            textBoxStatistic.Text = Convert.ToString(comparisonNumber);
                        }
                        else
                        {
                            textBoxErrors.Text = "Success; 0 row returned";
                            textBoxResults.Text = "";
                        }
                    }
                    catch (FormatException ex)
                    {
                        textBoxErrors.Text = "Key should be an integer!";
                    }
                }
            }                    
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            string DBname = textBoxDBName.Text;
            string input = textBoxInput.Text;

            input = "id int,lake char(15)";
            if (DBname.Length == 0)
                textBoxErrors.Text = "You haven't entered the DB name!";
            else if(input.Length == 0)
                textBoxErrors.Text = "You haven't entered the DB description!";
            else
            {                
                DBManager.CreateDatabase(DBname, input);
                textBoxErrors.Text = "Success; DB "+ DBname + " has been created.";
            }           
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            string DBname = textBoxDBName.Text;
            string input = textBoxInput.Text;

            if (CheckIfDBExists(DBname))
            {
                if (input.Length == 0)
                    textBoxErrors.Text = "You haven't entered the key!";
                else
                {
                    DBManager dBManager = new DBManager(DBname);
                    int keyToDelete = Convert.ToInt32(input);
                    bool flag = dBManager.Delete(keyToDelete);
                    if (flag)
                        textBoxErrors.Text = "Success; 1 row deleted";
                    else
                        textBoxErrors.Text = "Failed";
                    textBoxResults.Text = "";
                }
            }            
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            string DBname = textBoxDBName.Text;
            string input = textBoxInput.Text;

            if (CheckIfDBExists(DBname))
            {
                if (input.Length == 0)
                    textBoxErrors.Text = "You haven't entered the data!";
                else
                {
                    DBManager dBManager = new DBManager(DBname);
                    bool flag = dBManager.UpdateData(input);
                    if (flag)
                        textBoxErrors.Text = "Success; 1 row updated";
                    else
                        textBoxErrors.Text = "Failed";
                }
            }                
        }
        private bool CheckIfDBExists(string DBname)
        {
            if (DBname.Length == 0)
            {
                textBoxErrors.Text = "You haven't specified DB name!";
                return false;
            }                
            else if (!File.Exists(DBname))
            {
                textBoxErrors.Text = "There is no such a DB";
                return false;
            }
            return true;
        }

        private void btnShowStructure_Click(object sender, EventArgs e)
        {
            KeysLayout formLayout = new KeysLayout();
            formLayout.Show();
        }
    }
}
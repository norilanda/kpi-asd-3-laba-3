using RedBlackTreeAlgo.DatabaseManager;
using RedBlackTreeAlgo.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RedBlackTreeAlgo
{
    public class Test
    {
        //Test.createDB10_000Records("DB10000records");
        public static int createDB10_000Records(string DBname)
        {
            const int N = 10000;
            
            Random random = new Random();
            int[] keys = Enumerable.Range(0, N).OrderBy(c => random.Next()).ToArray();

            string input = "id int,name char(15)";
            DBManager.CreateDatabase(DBname, input);
            DBManager dBManager = new DBManager(DBname);
            int errors = 0;
            for (int i = 0; i<N; i++)
            {
                try
                {
                    dBManager.InsertData(keys[i] + ",name" + keys[i]);
                }
                catch (RecordAlreadyExists ex)
                {
                    errors++;
                }
                catch (WrongDataFormat ex)
                {
                    errors++;
                }
            }
            return errors;
        }
        public static void generateIntDB(string DBname)
        {
            //string DBname = "f1";
            //string input = "id int";
            ////DBManager.CreateDatabase(DBname, input);
            //textBoxDBName.Text = DBname;
            //Test.generateIntDB(DBname);

            int[] keys = { 3, 5, 2, 4898, 908, 87, 47, 453, 22, 30, 13, 41, 1009, 765, 54, 432, 7600, 6, 44, 1, 4, 7, 8, 9,10, 16, 15, 14, 11, 55, 102, 103, 111, 209, 208 };
            string input = "id int";
            DBManager.CreateDatabase(DBname, input);
            DBManager dBManager = new DBManager(DBname);
            for (int i=0;i<keys.Length;i++)
            {
                try
                {
                    dBManager.InsertData(Convert.ToString(keys[i]));
                }
                catch (RecordAlreadyExists ex)
                {

                }
            }
            //for (int i = keys.Length-1; i >=0;i--)
            //{
            //    dBManager.Delete(keys[i]);
            //}
            for (int i = keys.Length - 1; i >= keys.Length/2; i--)
            {
                dBManager.Delete(keys[i]);
            }
        }
    }
}

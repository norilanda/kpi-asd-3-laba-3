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
        //int errors = Test.createDB10_000Records("DB10000records");
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
    }
}

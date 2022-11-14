using RedBlackTreeAlgo.FileStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedBlackTreeAlgo.FileStructure;
using System.Reflection;

namespace RedBlackTreeAlgo.DatabaseManager
{
    public class DBManager
    {
        private static int spacePerPage = 4 * 1024;    //space for each page in bytes
        public static bool CreateDatabase(string name)
        {
            //check name ? maybe later
            //store metadata somewhere???            
            Page indexPage = new Page(0, PageType.index, spacePerPage - Page.pageHeaderSizeBytes());
            byte[] IndexPageBytes = new byte[spacePerPage];
            byte[] IndexPageHeader = indexPage.PageSerialization();
            IndexPageHeader.CopyTo(IndexPageBytes, 0);

            Page dataPage = new Page(1, PageType.data, spacePerPage - Page.pageHeaderSizeBytes());
            byte[] DataPageBytes = new byte[spacePerPage];
            byte[] dataPageHeader = indexPage.PageSerialization();

            int currIndexPage = 0;
            int currDataPage = 1;
            BinaryWriter binaryWriter = new BinaryWriter(File.Open(name, FileMode.Create));
            binaryWriter.Write(currIndexPage);
            binaryWriter.Write(currDataPage);
            binaryWriter.Write(IndexPageBytes);
            binaryWriter.Write(DataPageBytes);
            return true;
                        
            //BinaryWriter bw = new BinaryWriter(File.Open(fileName, FileMode.Create));
            //Record record = new Record(98, 1, 65536);
            //byte[] recordS = record.RecordSerialization();
            //bw.Write(recordS);
            //bw.Close();

            //BinaryReader br = new BinaryReader(File.Open(fileName, FileMode.Open));
            //byte[] bytesFromFile = br.ReadBytes(recordS.Length);
            //Record record2 = new Record(bytesFromFile);
        }
    }
}

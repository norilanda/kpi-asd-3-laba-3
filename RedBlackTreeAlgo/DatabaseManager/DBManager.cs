using RedBlackTreeAlgo.FileStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedBlackTreeAlgo.FileStructure;
using System.Reflection;
using System.IO;

namespace RedBlackTreeAlgo.DatabaseManager
{
    public class DBManager
    {
        private static int spacePerPage = 4 * 1024;    //space for each page in bytes
        private static int offsetFromStart = sizeof(int) * 2;   //indicates the pages start

        private string? currDB;
        BinaryWriter? binaryWriter;
        public DBManager(string? name=null)
        {
            currDB = name;
            binaryWriter = null;
        }
        public bool CreateDatabase(string name, byte[] metadata)
        {
            //check name ? maybe later
            //store metadata in separate file
            binaryWriter = new BinaryWriter(File.Open(name+"Meta", FileMode.Create));
            binaryWriter.Write(metadata);
            binaryWriter.Close();
            //create file to store data
            Page indexPage = new Page(0, PageType.index, spacePerPage - Page.pageHeaderSizeBytes());
            byte[] IndexPageBytes = new byte[spacePerPage];
            byte[] IndexPageHeader = indexPage.PageSerialization();
            IndexPageHeader.CopyTo(IndexPageBytes, 0);

            Page dataPage = new Page(1, PageType.data, spacePerPage - Page.pageHeaderSizeBytes());
            byte[] DataPageBytes = new byte[spacePerPage];
            byte[] dataPageHeader = indexPage.PageSerialization();

            int currIndexPage = 0;
            int currDataPage = 1;
            binaryWriter = new BinaryWriter(File.Open(name, FileMode.Create));
            binaryWriter.Write(currIndexPage);
            binaryWriter.Write(currDataPage);
            binaryWriter.Write(IndexPageBytes);
            binaryWriter.Write(DataPageBytes);
            binaryWriter.Flush();
            return true;        
        }
        public bool Insert(int key, byte[] data)
        {

            return true;
        }
    }
}

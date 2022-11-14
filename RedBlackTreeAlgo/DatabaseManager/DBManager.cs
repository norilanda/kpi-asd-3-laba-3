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
        private static int spacePerPage = Page.pageSizeTotal;    //space for each page in bytes
        private static int offsetFromStart = sizeof(int) * 2;   //indicates the pages start
        private static int pageHeaderSize = Page.pageHeaderSize;
        private static int recordSize = Record.RecordSize();

        private string? currDB;
        BinaryWriter? binaryWriter;
        BinaryReader? binaryReader;
        byte[] buffer;
        byte[] buffer2;
        public DBManager(string? name=null)
        {
            currDB = name;
            binaryWriter = null;
            binaryReader = null;
        }
        public string CurrDB
        {
            get { return currDB; }
            set { currDB = value; }            
        }
        public bool CreateDatabase(string name, byte[] metadata)
        {
            //check name ? maybe later
            //store metadata in separate file
            binaryWriter = new BinaryWriter(File.Open(name+"Meta", FileMode.Create));
            binaryWriter.Write(metadata);
            binaryWriter.Close();
            //create file to store data
            Page indexPage = new Page(0, PageType.index, spacePerPage - pageHeaderSize);
            byte[] IndexPageBytes = new byte[spacePerPage];
            byte[] IndexPageHeader = indexPage.PageSerialization();
            IndexPageHeader.CopyTo(IndexPageBytes, 0);

            Page dataPage = new Page(1, PageType.data, spacePerPage - Page.pageHeaderSize);
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
        private byte[] ReadPageWithNumber(int number)
        {
            binaryReader.BaseStream.Position = offsetFromStart + spacePerPage*number;
            return binaryReader.ReadBytes(spacePerPage);
        }
        //private byte[] ReadRecord(int page, int offset)
        //{
        //    binaryReader.BaseStream.Position = offsetFromStart + spacePerPage * page + offset;
        //    return binaryReader.ReadBytes(recordSize);
        //}
        public bool Insert(int key, byte[] data)
        {
            binaryReader = new BinaryReader(File.Open(currDB, FileMode.Open));            
            

            buffer = ReadPageWithNumber(0);         
            Page currPage = new Page(buffer);
            Record? y = null;
            
            Record? x = currPage.getRecord(pageHeaderSize);
            while(x.Datapage!= 0 && x.DataOffset!= 0 )
            {
                y = x;
                if (key < x.Key)
                {
                    if (x.LeftPage == currPage.Number)
                        x = currPage.getRecord(x.LeftOffset);
                    else
                    {
                        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    }
                }
                else
                {
                    if (x.RightPage == currPage.Number)
                        x = currPage.getRecord(x.RightOffset);
                    else
                    {
                        //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
                    }
                }
            }
            //Record record = new Record(key, ?, ?, currPage, ?);

            //Node? y = null;
            //Node? x = root;
            //while (x != null)
            //{
            //    y = x;
            //    if (key < x.Key)
            //        x = x.Left;
            //    else
            //        x = x.Right;
            //}

            //Node node = new Node(key, value);
            //node.P = y;
            //if (y == null)
            //    root = node;
            //else if (key < y.Key)
            //    y.Left = node;
            //else y.Right = node;
            return InsertFixup(0, 0);

        }
        private bool InsertFixup(int NodePage, int NodeOffset)
        {

            return true;
        }
    }
}

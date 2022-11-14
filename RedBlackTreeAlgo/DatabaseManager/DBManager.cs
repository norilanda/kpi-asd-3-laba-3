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
        int currIndexPageNumb;
        int currDataPageNumb;
        bool needToWriteNewCurrPageNum;
        public DBManager(string? name=null)
        {
            binaryWriter = null;
            binaryReader = null;
            needToWriteNewCurrPageNum = false;
            Use(name);
        }
        public string CurrDB
        {
            get { return currDB; }
            set { currDB = value; }            
        }
        public void Use(string? dbName)
        {
            if (dbName != null)
            {
                currDB = dbName;
                binaryReader = new BinaryReader(File.Open(currDB, FileMode.Open));
                currIndexPageNumb = binaryReader.ReadInt32();
                currDataPageNumb = binaryReader.ReadInt32();
                binaryReader.Close();
            }
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
            binaryWriter.Write(currIndexPage);  //storing current index page number
            binaryWriter.Write(currDataPage);   //storing current data page number
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
        private int calcNumberOfNewPage()
        {
            long fileSize = new System.IO.FileInfo(CurrDB).Length;
            return (int)(fileSize / spacePerPage)+1;
        }
        private void WritePage(Page page) { }
        
        public bool Insert(int key, byte[] data)
        {
            binaryReader = new BinaryReader(File.Open(currDB, FileMode.Open));            
            Page currPage = new Page(ReadPageWithNumber(0));
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
            Page currDataPage = new Page(ReadPageWithNumber(currDataPageNumb));
            if(!currDataPage.isEnoughSpace(data.Length))
            {             
                int newPageNum = calcNumberOfNewPage();
                currDataPage = new Page(newPageNum, PageType.data, spacePerPage - Page.pageHeaderSize);
                currDataPageNumb = newPageNum;//need to be written
                needToWriteNewCurrPageNum = true;
            }
            data.CopyTo(currDataPage.buff, currDataPage.Position);
            currDataPage.Position += data.Length;   //move pointer to next position
            currDataPage.FreeSpace -= data.Length;
            currDataPage.IsDirty = true;
            Page currIndexPage = currPage;
            if(currIndexPageNumb != currPage.Number )
            {
                currIndexPage = new Page(ReadPageWithNumber(currDataPageNumb));
                if (!currIndexPage.isEnoughSpace(recordSize))
                {
                    int newPageNum = calcNumberOfNewPage();
                    currIndexPage = new Page(newPageNum, PageType.index, spacePerPage - Page.pageHeaderSize);
                    currIndexPageNumb = newPageNum;
                    needToWriteNewCurrPageNum = true;
                }                    
            }
            Record record = new Record(key, currDataPage.Number, currDataPage.Position-data.Length, currIndexPage.Number, currIndexPage.Position);
            if ( y != null )
            {
                record.ParentPage = y.recordPage;
                record.ParentOffset = y.recordOffset;
                if(key < y.Key)
                {
                    y.LeftPage = record.recordPage;
                    y.LeftOffset = record.recordOffset;//!!!!!!!!!Write later to buff
                }
                else
                {
                    y.RightPage= record.recordPage;
                    y.RightOffset = record.recordOffset;
                }
            }
            else
            {//root = node
                record.ParentPage = 0;
                record.ParentOffset = 0;
            }
            /*
            Node? y = null;
            Node? x = root;
            while (x != null)
            {
                y = x;
                if (key < x.Key)
                    x = x.Left;
                else
                    x = x.Right;
            }
            Node node = new Node(key, value);
            node.P = y;
            if (y == null)
                root = node;
            else if (key < y.Key)
                y.Left = node;
            else y.Right = node;
             */
            return InsertFixup(record);
        }
        private Record
        private bool InsertFixup(Record record)
        {
            while(record.ParentOffset!=0 && record.P)
            return true;
            /*
             while (node.P != null && node.P.Color == NodeColor.RED)
                {
                    if (node.P == node.G.Left)   //if parent is a left child
                    {
                        Node y = node.G.Right;  //uncle
                        if (y != null && y.Color == NodeColor.RED)  //case 1 (uncle is RED). Solution: recolor
                        {
                            node.P.Color = NodeColor.BLACK;
                            y.Color = NodeColor.BLACK;
                            node.G.Color = NodeColor.RED;
                            node = node.G;
                        }
                        else 
                        {
                            if (node == node.P.Right)   //case 2 (uncle is black, triangle). Solution: transform case 2 into case 3 (rotate)
                            {                                
                                node = node.P;
                                LeftRotate(node);
                            }
                            node.P.Color = NodeColor.BLACK;    //case 3 (uncle is black, line). Solution: recolor and rotate
                            node.G.Color = NodeColor.RED;
                            RightRotate(node.G);
                        }
                    }
                    else //if parent is a right child
                    {
                        Node y = node.G.Left;   //uncle
                        if (y != null && y.Color == NodeColor.RED)  //case 1 (uncle is RED). Solution: recolor
                        {
                            node.P.Color = NodeColor.BLACK;
                            y.Color = NodeColor.BLACK;
                            node.G.Color = NodeColor.RED;
                            node = node.G;
                        }
                        else 
                        {
                            if (node == node.P.Left)   //case 2 (uncle is black, triangle). Solution: transform case 2 into case 3 (rotate)
                            {
                                node = node.P;
                                RightRotate(node);
                            }
                            node.P.Color = NodeColor.BLACK;    //case 3 (uncle is black, line). Solution: recolor and rotate
                            node.G.Color = NodeColor.RED;
                            LeftRotate(node.G);
                        }                        
                    }
                }
                root.Color = NodeColor.BLACK;   //case 0 (node is root)
             */
        }
    }
}

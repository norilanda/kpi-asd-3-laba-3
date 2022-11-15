using RedBlackTreeAlgo.FileStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedBlackTreeAlgo.FileStructure;
using System.Reflection;
using System.IO;
using System.Xml.Linq;

namespace RedBlackTreeAlgo.DatabaseManager
{
    public class DBManager
    {
        private static int spacePerPage = Page.pageSizeTotal;    //space for each page in bytes
        private static int offsetFromStart = sizeof(int) * 2;   //indicates the pages start
        private static int pageHeaderSize = Page.pageHeaderSize;
        private static int recordSize = Record.RecordSize();

        private string? currDB;
        private BinaryWriter? binaryWriter;
        private BinaryReader? binaryReader;
        
        private BufferManager buffManager;
        public DBManager(string name)
        {
            binaryWriter = null;
            binaryReader = null;
            currDB = name;
            //Use(name);
            buffManager = new BufferManager(currDB);
        }
        //public void Use(string? dbName)
        //{
        //    if (dbName != null)
        //    {
        //        currDB = dbName;
        //        binaryReader = new BinaryReader(File.Open(currDB, FileMode.Open));
        //        binaryReader.Close();
        //    }
        //}
        public static bool CreateDatabase(string name, byte[] metadata)
        {
            //check name ? maybe later
            //store metadata in separate file
            BinaryWriter bw = new BinaryWriter(File.Open(name+"Meta", FileMode.Create));
            bw.Write(metadata);
            bw.Close();
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
            bw = new BinaryWriter(File.Open(name, FileMode.Create));
            bw.Write(currIndexPage);  //storing current index page number
            bw.Write(currDataPage);   //storing current data page number
            bw.Write(IndexPageBytes);
            bw.Write(DataPageBytes);
            bw.Close();
            return true;        
        }
        public bool Insert(int key, byte[] data)
        {
            Page currPage = buffManager.getPageWithNumber(0);
            Record? y = null;
            Record? x = currPage.getRecord(pageHeaderSize);
            while(x.Datapage!= 0 && x.DataOffset!= 0 )//while x != null
            {
                y = x;
                if (key < x.Key)
                    x = buffManager.getRecordFromPage(x.LeftPage, x.LeftOffset);                   
                else
                    x = buffManager.getRecordFromPage(x.RightPage, x.RightOffset);                   
            }
            Page currDataPage = buffManager.getCurrDataPage();
            if(!currDataPage.isEnoughSpace(data.Length))
                currDataPage = buffManager.CreateNewPage(PageType.data);
            currDataPage.AddData(data);

            Page currIndexPage = buffManager.getCurrIndexPage();
            if (!currIndexPage.isEnoughSpace(recordSize))
                currIndexPage = buffManager.CreateNewPage(PageType.index);                  
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
        private bool InsertFixup(Record record)
        {
           while(record.ParentOffset!=0 && buffManager.getParent(record).Color == Color.RED)
            {
                if (Record.AreEqual(buffManager.getParent(record), buffManager.getLeft(buffManager.getGrandparent(record)) ))
                {

                }
            }
            return true;
            /*
             while (node.P != null && node.P.Color == NodeColor.RED)
                {
        -            if (node.P == node.G.Left)   //if parent is a left child
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

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
        private static int offsetFromStart = sizeof(int) * 4;   //indicates the pages start
        private static int pageHeaderSize = Page.pageHeaderSize;
        private static int recordSize = Record.RecordSize();

        private string? currDB;       
        
        private BufferManager buffManager;
        public DBManager(string name)
        {     
            currDB = name;
            buffManager = new BufferManager(name);
        }
       
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
            byte[] dataPageHeader = dataPage.PageSerialization();
            dataPageHeader.CopyTo(DataPageBytes, 0);

            int currIndexPage = 0;
            int currDataPage = 1;
            bw = new BinaryWriter(File.Open(name, FileMode.Create));
            bw.Write(currIndexPage);  //storing current index page number
            bw.Write(currDataPage);   //storing current data page number
            bw.Write(new byte[sizeof(int)]);  //storing root page 
            bw.Write(BitConverter.GetBytes(pageHeaderSize)); //storing root offset
            bw.Write(IndexPageBytes);
            bw.Write(DataPageBytes);
            bw.Close();
            return true;        
        }
        public bool Insert(int key, byte[] data)
        {
            Record? y = null;
            Record? x = buffManager.getRoot();
            while(!x.isNull())//while x != null
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
            Record record = new Record(key, currDataPage.Number, currDataPage.Position-data.Length, currIndexPage.Number, currIndexPage.Position);  //creating record with reference to written data
            currIndexPage.AddRecord(record); //adding record to page records
            if ( y != null )
            {
                buffManager.setParent(record, y);   //set record parent to y
                if(key < y.Key)
                    buffManager.setLeft(y, record);
                else
                    buffManager.setRight(y, record);
            }
            else
                buffManager.setRoot(record);//root = record
            
            bool flag =  InsertFixup(record);
            buffManager.CleanPagesAndWriteRoot();
            return flag;
        }
        private bool InsertFixup(Record record)
        {
           while(record.ParentOffset!=0 && buffManager.getParent(record).Color == Color.RED)
            {
                if (Record.AreEqual(buffManager.getParent(record), buffManager.getLeft(buffManager.getGrandparent(record)) ))   //if parent is a left child
                {
                    Record y = buffManager.getRight(buffManager.getGrandparent(record));  //uncle
                    if (!y.isNull() && y.Color == Color.RED)   //case 1 (uncle is RED). Solution: recolor
                    {
                        buffManager.setColor(buffManager.getParent(record), Color.BLACK);   //set parent color to black
                        buffManager.setColor(y, Color.BLACK);   //set uncle color to black
                        buffManager.setColor(buffManager.getGrandparent(record), Color.RED);    //set grandparent color to red
                        record = buffManager.getGrandparent(record);    //record = record.Grandparent
                    }
                    else
                    {
                        //case 2 (uncle is black, triangle). Solution: transform case 2 into case 3 (rotate)
                        if (Record.AreEqual(record, buffManager.getRight(buffManager.getParent(record)) ))
                        {
                            record = buffManager.getParent(record);
                            buffManager.LeftRotate(record);
                        }
                        //case 3 (uncle is black, line). Solution: recolor and rotate
                        buffManager.setColor(buffManager.getParent(record), Color.BLACK);
                        buffManager.setColor(buffManager.getGrandparent(record), Color.RED);
                        buffManager.RightRotate(buffManager.getGrandparent(record));//RightRotate(node.G)
                    }
                }
                else //if parent is a right child
                {
                    Record y = buffManager.getLeft(buffManager.getGrandparent(record));  //uncle
                    if (!y.isNull() && y.Color == Color.RED)   //case 1 (uncle is RED). Solution: recolor
                    {
                        buffManager.setColor(buffManager.getParent(record), Color.BLACK);   //set parent color to black
                        buffManager.setColor(y, Color.BLACK);   //set uncle color to black
                        buffManager.setColor(buffManager.getGrandparent(record), Color.RED);    //set grandparent color to red
                        record = buffManager.getGrandparent(record);    //record = record.Grandparent
                    }
                    else
                    {
                        //case 2 (uncle is black, triangle). Solution: transform case 2 into case 3 (rotate)
                        if (Record.AreEqual(record, buffManager.getLeft(buffManager.getParent(record))))
                        {
                            record = buffManager.getParent(record);
                            buffManager.RightRotate(record);
                        }
                        //case 3 (uncle is black, line). Solution: recolor and rotate
                        buffManager.setColor(buffManager.getParent(record), Color.BLACK);
                        buffManager.setColor(buffManager.getGrandparent(record), Color.RED);
                        buffManager.RightRotate(buffManager.getGrandparent(record));//RightRotate(node.G)
                    }
                }
            }
            buffManager.setColor(buffManager.getRoot(), Color.BLACK);   //case 0 (node is root)
            return true;            
        }
    }
}

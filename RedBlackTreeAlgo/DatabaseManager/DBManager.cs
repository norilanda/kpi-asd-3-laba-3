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
using System.Text.RegularExpressions;

namespace RedBlackTreeAlgo.DatabaseManager
{
    public class DBManager
    {
        private string? currDB;       
        
        private BufferManager buffManager;
        public DBManager(string name)
        {     
            currDB = name;
            buffManager = new BufferManager(name);
        }
       
        public static bool CreateDatabase(string name, byte[] metadata, int spaceDataInNodes)
        {
            //check name ? maybe later
            //store metadata in separate file
            BinaryWriter bw = new BinaryWriter(File.Open(name+"Meta", FileMode.Create));
            bw.Write(metadata);
            bw.Close();
            //create file to store data
            Page firstPage = new Page(0, Page.spacePerPage - Page.pageHeaderSize);
            byte[] firstPageBytes = new byte[Page.spacePerPage];
            byte[] firstPageHeader = firstPage.PageSerialization();
            firstPageHeader.CopyTo(firstPageBytes, 0);          

            int currIndexPage = 0;
            int dataSpace = spaceDataInNodes;
            bw = new BinaryWriter(File.Open(name, FileMode.Create));
            bw.Write(currIndexPage);  //storing current page number
            bw.Write(spaceDataInNodes);   //storing space for data
            bw.Write(new byte[sizeof(int)]);  //storing initial root page 
            bw.Write(BitConverter.GetBytes(Page.pageHeaderSize)); //storing initial root offset
            bw.Write(firstPageBytes); //write first page to file
            bw.Close();
            return true;        
        }
        public bool Insert(int key, byte[] data)
        {
            Record? y = null;
            Record? x = buffManager.getRoot();
            while(x!=null)//while x != null //&& !x.isNull()
            {
                y = x;
                if (key < x.Key)
                    x = buffManager.getRecordFromPage(x.LeftPage, x.LeftOffset);
                else if (key > x.Key)
                    x = buffManager.getRecordFromPage(x.RightPage, x.RightOffset);
                else
                    return false;
            }

            Page currPage = buffManager.getCurrPage();
            if (!currPage.isEnoughSpace())
                currPage = buffManager.CreateNewPage();                  
            Record record = new Record(key, data, currPage.Number, currPage.Position);  //creating record with reference to written data
            currPage.AddRecord(record); //adding record to page records
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
                    if (y != null && y.Color == Color.RED)   //case 1 (uncle is RED). Solution: recolor //&& !y.isNull() 
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
                    if (y!=null && y.Color == Color.RED)   //case 1 (uncle is RED). Solution: recolor //&& !y.isNull() 
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
                        buffManager.LeftRotate(buffManager.getGrandparent(record));//RightRotate(node.G)
                    }
                }
            }
            buffManager.setColor(buffManager.getRoot(), Color.BLACK);   //case 0 (node is root)
            return true;            
        }
        public byte[]? Search(int key)
        {
            byte[] data = new byte[BufferManager.dataSpace];
            Record? x = buffManager.getRoot();
            while (x != null  && x.Key != key) //&& !x.isNull()
            {
                if (key < x.Key)
                    x = buffManager.getRecordFromPage(x.LeftPage, x.LeftOffset);
                else
                    x = buffManager.getRecordFromPage(x.RightPage, x.RightOffset);
            }
            if (x == null)
                return null;//!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
            return x.Data;
            /*
            Node? x = root;
            while (x != null && x.Key != key)
            {
                if (key < x.Key)
                    x = x.Left;
                else
                    x = x.Right;
            }
            return x;
            */
        }
    }
}

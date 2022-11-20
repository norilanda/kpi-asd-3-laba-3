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
            byte[] md = ReadMetadata(name + "Meta");
            buffManager = new BufferManager(name, md);
        }
       
        public static bool CreateDatabase(string name, string description)
        {
            int dataSize;
            byte[] metadata = Parser.CreateMetadataForDB(description, out dataSize);//should write without space
            return CreateDatabase(name, metadata, dataSize);
        }
        private static bool CreateDatabase(string name, byte[] metadata, int spaceDataInNodes)
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
        public static byte[] ReadMetadata(string metadataFileName)
        {
            byte[] md;
            int fileSize = (int)new System.IO.FileInfo(metadataFileName).Length;
            using (var stream = File.Open(metadataFileName, FileMode.Open))
            {
                using (var binaryReader = new BinaryReader(stream))
                {
                    binaryReader.BaseStream.Position = 0;
                    md = binaryReader.ReadBytes(fileSize);
                }
            }
            return md;
        }
        public bool InsertData(string data)
        {
            int key = Convert.ToInt32(data.Split(',')[0]);
            byte[] dataBytes = buffManager.GetDataBytesFromString(data);
            if (dataBytes.Length == Record.dataSpace)
            {
                return Insert(key, dataBytes);
            }
            return false;
        }
        private bool Insert(int key, byte[] data)
        {
            Record? y = null;
            Record? x = buffManager.getRoot();
            while(x!=null)//while x != null
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
                    if (y != null && y.Color == Color.RED)   //case 1 (uncle is RED). Solution: recolor 
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
                    if (y!=null && y.Color == Color.RED)   //case 1 (uncle is RED). Solution: recolor 
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
        public string? SearchData(int key)
        {
            string? result = null;
            byte[]? dataBytes = null;
            Record? record = Search(key);
            if (record != null)
                dataBytes = record.Data;
            if (dataBytes != null)
            {
                result = buffManager.GetDataStringFromBytes(dataBytes);
            }
            return result;
        }
        private Record? Search(int key)
        {
            Record? x = buffManager.getRoot();
            while (x != null  && x.Key != key)
            {
                if (key < x.Key)
                    x = buffManager.getRecordFromPage(x.LeftPage, x.LeftOffset);
                else
                    x = buffManager.getRecordFromPage(x.RightPage, x.RightOffset);
            }
            return x;
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
        public bool Delete(int key)
        {
            Record? record = Search(key);
            if (record == null) return false; //if there is no such an element
            Record? x, y = record;
            Color yOriginalColor = y.Color;
            if (buffManager.getLeft(record) == null)
            {
                x = buffManager.getRight(record);
                buffManager.Transplant(record, buffManager.getRight(record));
            }
            else if (buffManager.getRight(record) == null)
            {
                x = buffManager.getLeft(record);
                buffManager.Transplant(record, buffManager.getLeft(record));
            }
            else
            {
                y = buffManager.Minimum(buffManager.getRight(record));
                yOriginalColor = y.Color;
                x = buffManager.getRight(y);
                if (x != null && buffManager.getParent(y) == record)
                    buffManager.setParent(x, y);
                else
                {
                    buffManager.Transplant(y, buffManager.getRight(y));
                    buffManager.setRight(y, buffManager.getRight(record)); //insert successor instead of node
                    if (buffManager.getRight(y) != null)
                        buffManager.setParent(buffManager.getRight(y), y);
                }
                buffManager.Transplant(record, y);
                buffManager.setLeft(y, buffManager.getLeft(record));
                buffManager.setParent(buffManager.getLeft(y), y);
                buffManager.setColor(y, record.Color);
                if (yOriginalColor == Color.BLACK)
                    return Delete_Fixup(x);
            }
            return true;
            /*
            Node? node = Search(key);
            if (node == null) return;
            Node? x, y = node;
            NodeColor yOriginalColor = y.Color;
            if (node.Left == null)
            {
                x = node.Right;
                Transplant(node, node.Right);
            }
            else if (node.Right == null)
            {
                x = node.Left;
                Transplant(node, node.Left);
            }*/
            /*
            else
            {
                y = Minimum(node.Right);
                yOriginalColor = y.Color;
                x = y.Right;
                if (x != null && y.P == node)
                    x.P = y;
                else
                {
                    Transplant(y, y.Right);
                    y.Right = node.Right;   //insert successor instead of node
                    if (y.Right!= null)
                        y.Right.P = y;//null
                }
                Transplant(node, y);
                y.Left = node.Left;
                y.Left.P = y;
                y.Color = node.Color;
            }
            if (yOriginalColor == NodeColor.BLACK)
                Delete_Fixup(x);
             */

        }
        private bool Delete_Fixup(Record x)
        {
            Record w; //sibling
            while(x != null && !Record.AreEqual(x, buffManager.getRoot()) && x.Color == Color.BLACK)
            {
                if (Record.AreEqual(x, buffManager.getLeft(buffManager.getParent(x))))//if left child
                {
                    w = buffManager.getRight(buffManager.getParent(x));
                    if (w.Color == Color.RED)
                    {
                        buffManager.setColor(w, Color.BLACK);
                        buffManager.setColor(buffManager.getParent(x), Color.RED);
                        buffManager.LeftRotate(buffManager.getParent(x));
                        w = buffManager.getRight(buffManager.getParent(x));
                    }
                    if (buffManager.getLeft(w).Color == Color.BLACK && buffManager.getRight(w).Color == Color.BLACK)
                    {
                        buffManager.setColor(w, Color.RED);
                        x = buffManager.getParent(x);
                    }
                    else
                    {
                        if (buffManager.getRight(w).Color == Color.BLACK)
                        {
                            buffManager.setColor(buffManager.getLeft(w), Color.BLACK);
                            buffManager.setColor(w, Color.RED);
                            buffManager.RightRotate(w);
                            w = buffManager.getRight(buffManager.getParent(w));
                        }
                        buffManager.setColor(w, buffManager.getParent(x).Color);
                        buffManager.setColor(buffManager.getParent(x), Color.BLACK);
                        buffManager.setColor(buffManager.getRight(w), Color.BLACK);
                        buffManager.LeftRotate(buffManager.getParent(x));
                        x = buffManager.getRoot();
                    }
                }
                else //if right child
                {

                }
            }
            return true;
            /*
             Node w; //sibling
            while (x!=null && x != root && x.Color == NodeColor.BLACK)//
            {
                if (x == x.P.Left)  //if left child
                {
                    w = x.P.Right;
                    if (w.Color == NodeColor.RED)
                    {
                        w.Color = NodeColor.BLACK;
                        x.P.Color = NodeColor.RED;
                        LeftRotate(x.P);
                        w = x.P.Right;
                    }
                    if (w.Left.Color == NodeColor.BLACK && w.Right.Color == NodeColor.BLACK)
                    {
                        w.Color = NodeColor.RED;
                        x = x.P;
                    } */
            /*
                    else
                    {
                        if (w.Right.Color == NodeColor.BLACK)
                        {
                            w.Left.Color = NodeColor.BLACK;
                            w.Color = NodeColor.RED;
                            RightRotate(w);
                            w = x.P.Right;
                        }
                        w.Color = x.P.Color;
                        x.P.Color = NodeColor.BLACK;
                        w.Right.Color = NodeColor.BLACK;
                        LeftRotate(x.P);
                        x = root;
                    }
                } */ 
            /*
                else //if right child
                {
                    w = x.P.Left;
                    if (w.Color == NodeColor.RED)
                    {
                        w.Color = NodeColor.BLACK;
                        x.P.Color = NodeColor.RED;
                        RightRotate(x.P);
                        w = x.P.Left;
                    }
                    if (w.Right.Color == NodeColor.BLACK && w.Left.Color == NodeColor.BLACK)
                    {
                        w.Color = NodeColor.RED;
                        x = x.P;
                    }
                    else
                    {
                        if (w.Left.Color == NodeColor.BLACK)
                        {
                            w.Right.Color = NodeColor.BLACK;
                            w.Color = NodeColor.RED;
                            LeftRotate(w);
                            w = x.P.Left;
                        }
                        w.Color = x.P.Color;
                        x.P.Color = NodeColor.BLACK;
                        w.Left.Color = NodeColor.BLACK;
                        RightRotate(x.P);
                        x = root;
                    }
                }
            }
             */
        }
    }
}

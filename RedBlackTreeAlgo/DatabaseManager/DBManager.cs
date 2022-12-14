using RedBlackTreeAlgo.FileStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RedBlackTreeAlgo.Exceptions;
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
            byte[] metadata = Parser.CreateMetadataForDB(description, out dataSize);
            return CreateDatabase(name, metadata, dataSize);
        }
        private static bool CreateDatabase(string name, byte[] metadata, int spaceDataInNodes)
        {            
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
        public void InsertData(string data)
        {
            int key = Convert.ToInt32(data.Split(',')[0]);
            byte[] dataBytes = buffManager.GetDataBytesFromString(data);
            if (dataBytes.Length == Record.dataSpace)
                Insert(key, dataBytes);
            else
                throw new WrongDataFormat("Data has wrong format");
        }
        private void Insert(int key, byte[] data)
        {
            Record y = new Record(null);// y = nill
            Record x = buffManager.getRoot();
            while(!x.IsNill())
            {
                y = x;
                if (key < x.Key)
                    x = buffManager.getLeft(x);
                else if (key > x.Key)
                    x = buffManager.getRight(x);
                else
                    throw new RecordAlreadyExists("Record with key "+ key + " already Exsists");
            }

            Page currPage = buffManager.getCurrPage();
            if (!currPage.isEnoughSpace())
                currPage = buffManager.CreateNewPage();                  
            Record record = new Record(key, data, currPage.Number, currPage.Position);  //creating record with reference to written data
            currPage.AddRecord(record); //adding record to page records
            if (!y.IsNill())
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
        }
        private bool InsertFixup(Record record)
        {
           while( buffManager.getParent(record).Color == Color.RED)
            {
                if (buffManager.getParent(record) == buffManager.getLeft(buffManager.getGrandparent(record)) )   //if parent is a left child
                {
                    Record y = buffManager.getRight(buffManager.getGrandparent(record));  //uncle
                    if (y.Color == Color.RED)   //case 1 (uncle is RED). Solution: recolor 
                    {
                        buffManager.setColor(buffManager.getParent(record), Color.BLACK);   //set parent color to black
                        buffManager.setColor(y, Color.BLACK);   //set uncle color to black
                        buffManager.setColor(buffManager.getGrandparent(record), Color.RED);    //set grandparent color to red
                        record = buffManager.getGrandparent(record);    //record = record.Grandparent
                    }
                    else
                    {
                        //case 2 (uncle is black, triangle). Solution: transform case 2 into case 3 (rotate)
                        if (record == buffManager.getRight(buffManager.getParent(record) ))
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
                    if (y.Color == Color.RED)   //case 1 (uncle is RED). Solution: recolor 
                    {
                        buffManager.setColor(buffManager.getParent(record), Color.BLACK);   //set parent color to black
                        buffManager.setColor(y, Color.BLACK);   //set uncle color to black
                        buffManager.setColor(buffManager.getGrandparent(record), Color.RED);    //set grandparent color to red
                        record = buffManager.getGrandparent(record);    //record = record.Grandparent
                    }
                    else
                    {
                        //case 2 (uncle is black, triangle). Solution: transform case 2 into case 3 (rotate)
                        if (record == buffManager.getLeft(buffManager.getParent(record)) ) //Record.AreEqual(record, buffManager.getLeft(buffManager.getParent(record)))//
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
        public string? SearchData(int key, out int comparisonNumber)
        {
            comparisonNumber = 0;
            string? result = null;
            byte[]? dataBytes = null;
            Record record = Search(key, out comparisonNumber);
            if (!record.IsNill())
                dataBytes = record.Data;
            if (dataBytes != null)
            {
                result = buffManager.GetDataStringFromBytes(dataBytes);
            }
            return result;
        }
        private Record Search(int key, out int comparisonNumber)
        {
            comparisonNumber = 0;
            Record x = buffManager.getRoot();
            while (!x.IsNill()  && x.Key != key)
            {                
                if (key < x.Key)
                    x = buffManager.getLeft(x);
                else
                    x = buffManager.getRight(x);
                comparisonNumber++;
            }
            comparisonNumber++;
            return x;           
        }
        public bool Delete(int key)
        {
            bool flag = true; int dummy;
            Record record = Search(key, out dummy);
            if (record.IsNill()) return false; //if there is no such an element
            Record x, y = record;
            Color yOriginalColor = y.Color;
            if (buffManager.getLeft(record).IsNill())// right child or no children
            {
                x = buffManager.getRight(record);
                buffManager.Transplant(record, buffManager.getRight(record));
            }
            else if (buffManager.getRight(record).IsNill())// left child
            {
                x = buffManager.getLeft(record);
                buffManager.Transplant(record, buffManager.getLeft(record));
            }
            else//has both children
            {
                y = buffManager.Minimum(buffManager.getRight(record));
                yOriginalColor = y.Color;
                x = buffManager.getRight(y);
                if (buffManager.getParent(y) == record)// if y is a direct child for record
                    buffManager.setParent(x, y);
                else
                {
                    buffManager.Transplant(y, buffManager.getRight(y));
                    buffManager.setRight(y, buffManager.getRight(record)); //insert successor instead of node
                    buffManager.setParent(buffManager.getRight(y), y);
                }
                buffManager.Transplant(record, y);
                buffManager.setLeft(y, buffManager.getLeft(record));
                buffManager.setParent(buffManager.getLeft(y), y);
                buffManager.setColor(y, record.Color);                
            }
            if (yOriginalColor == Color.BLACK)
                flag = Delete_Fixup(x);
            record.DeleteRecordData();
            buffManager.getPageWithNumber(record.recordPage).IsDirty = true;
            buffManager.CleanPagesAndWriteRoot();
            return flag;           
        }
        private bool Delete_Fixup(Record x)
        {
            Record w; //sibling
            while(x != buffManager.getRoot() && x.Color == Color.BLACK )
            {
                if (x == buffManager.getLeft(buffManager.getParent(x)) )//if left child
                {
                    w = buffManager.getRight(buffManager.getParent(x));
                    //case 1
                    if (w.Color == Color.RED)
                    {
                        buffManager.setColor(w, Color.BLACK);
                        buffManager.setColor(buffManager.getParent(x), Color.RED);
                        buffManager.LeftRotate(buffManager.getParent(x));
                        w = buffManager.getRight(buffManager.getParent(x));
                    }
                    //case 2
                    if (!w.IsNill() && buffManager.getLeft(w).Color == Color.BLACK && buffManager.getRight(w).Color == Color.BLACK)
                    {
                        buffManager.setColor(w, Color.RED);
                        x = buffManager.getParent(x);
                    }
                    else
                    {
                        if (!w.IsNill() && buffManager.getRight(w).Color == Color.BLACK)
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
                    w = buffManager.getLeft(buffManager.getParent(x));
                    if (w.Color == Color.RED)
                    {
                        buffManager.setColor(w, Color.BLACK);
                        buffManager.setColor(buffManager.getParent(x), Color.RED);
                        buffManager.RightRotate(buffManager.getParent(x));
                        w = buffManager.getLeft(buffManager.getParent(x));
                    }
                    if (!w.IsNill() && buffManager.getRight(w).Color == Color.BLACK && buffManager.getLeft(w).Color == Color.BLACK)
                    {
                        buffManager.setColor(w, Color.RED);
                        x = buffManager.getParent(x);
                    }
                    else
                    {
                        if (!w.IsNill() && buffManager.getLeft(w).Color == Color.BLACK)
                        {
                            buffManager.setColor(buffManager.getRight(w), Color.BLACK);
                            buffManager.setColor(w, Color.RED);
                            buffManager.LeftRotate(w);
                            w = buffManager.getLeft(buffManager.getParent(w));
                        }
                        buffManager.setColor(w, buffManager.getParent(x).Color);
                        buffManager.setColor(buffManager.getParent(x), Color.BLACK);
                        buffManager.setColor(buffManager.getLeft(w), Color.BLACK);
                        buffManager.RightRotate(buffManager.getParent(x));
                        x = buffManager.getRoot();
                    }
                }
            }            
            buffManager.setColor(x, Color.BLACK);
            return true;            
        }
        public bool UpdateData(string data)
        {
            int key = Convert.ToInt32(data.Split(',')[0]);
            byte[] dataBytes = buffManager.GetDataBytesFromString(data);
            if (dataBytes.Length == Record.dataSpace)
            {
                return Update(key, dataBytes);
            }
            return false;
        }
        private bool Update(int key, byte[] data)
        {
            int dummy;
            Record record = Search(key, out dummy);
            if (record.IsNill()) return false; //if there is no such an element
            record.Data = data;
            buffManager.getPageWithNumber(record.recordPage).IsDirty = true;
            buffManager.CleanPagesAndWriteRoot();
            return true;
        }
        public Dictionary<int, (int color, int? leftKey, int? rightKey)> GetNodesToDisplay(ref int? rootKey)
        {
            Dictionary<int, (int color, int? leftKey, int? rightKey)> keys = new Dictionary<int, (int color, int? leftKey, int? rightKey)>();
            Record? rootRecord = buffManager.getRoot();
            if (rootRecord != null)
                rootKey = rootRecord.Key;
            GetNodesRecursion(rootRecord, keys, 0);

            return keys;
        }
        private void GetNodesRecursion(Record? record, Dictionary<int, (int color, int? leftKey, int? rightKey)> keys, int currHeight)
        {
            int maxNodesNumberToDisplay = 50;
            int height = (int)Math.Log2(maxNodesNumberToDisplay + 1);//
            if (record != null && !record.IsNill())
            {
                int? leftKeyValue = null;
                int? rightKeyValue = null;
                Record? leftChild = buffManager.getLeft(record);
                Record? rightChild = buffManager.getRight(record);
                if (leftChild != null)
                    leftKeyValue = leftChild.Key;
                if (rightChild != null)
                    rightKeyValue = rightChild.Key;
                keys.Add((int)record.Key, ((int)record.Color, leftKeyValue, rightKeyValue));
                if (currHeight <= height)
                {
                    GetNodesRecursion(leftChild, keys, currHeight+1);
                    GetNodesRecursion(rightChild, keys, currHeight + 1);
                }                
            }
        }
    }
}

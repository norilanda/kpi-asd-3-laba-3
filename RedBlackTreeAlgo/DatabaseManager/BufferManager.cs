using RedBlackTreeAlgo.FileStructure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RedBlackTreeAlgo.DatabaseManager
{
    public class BufferManager
    {
        private Dictionary<int, Page> bufferPool;   //page number, page
        private string currDB;
        private List<(int typeSize, char t, string cName)> colmns;


        private static int offsetFromStart = sizeof(int) * 4;   //indicates the pages start
        private static int pageHeaderSize = Page.pageHeaderSize;
        //
        private int currPageNumb;
        // dataSpace;//
        private int rootPage;
        private int rootOffset;

        private bool needToWriteCurrPage; //start of file
        private bool needToWriteRoot;
        private Record _root;

        public BufferManager(string dbName, byte[] metadata)
        {
            currDB = dbName;            

            bufferPool = new Dictionary<int, Page>();
            using (var stream = File.Open(currDB, FileMode.Open))
            {
                using(var binaryReader = new BinaryReader(stream))
                {
                    currPageNumb = binaryReader.ReadInt32();//current page
                    Record.dataSpace = binaryReader.ReadInt32();//space of data in each node
                    Record.RecordSize = sizeof(int) * 9 + Record.dataSpace;
                    rootPage = binaryReader.ReadInt32();
                    rootOffset = binaryReader.ReadInt32();
                }
            }
            colmns = Parser.MetadataToData(metadata);//get columns sizes, types and names
            _root = getRecordFromPage(rootPage, rootOffset);
            needToWriteRoot = false;
        }
        //data
        public byte[] GetDataBytesFromString(string dataString)// returns data, converted from string to bytes
        {
            return Parser.DataToByte(colmns, dataString, Record.dataSpace);
        }
        public string GetDataStringFromBytes(byte[] bytes)// returns data, converted from bytes to string
        {
            return Parser.BytesToData(colmns, bytes);
        }
   
        // pages
        //------------------------------------------------------------------------------
        public Page getCurrPage()
        {
            return getPageWithNumber(currPageNumb);
        }
        public Page getPageWithNumber(int pageNumber)
        {
            if (!bufferPool.ContainsKey(pageNumber))
                bufferPool.Add(pageNumber, new Page(ReadPageWithNumber(pageNumber)));
            return bufferPool[pageNumber];
        }
        public Record getRecordFromPage(int pageNumber, int recordOffset)   //returns the record in page with pageNumber and recordOffset bytes from start
        {
            Page currPage = getPageWithNumber(pageNumber);
            return currPage.getRecord(recordOffset);
        }
        public byte[] ReadPageWithNumber(int number)
        {
            byte[] bytes;
            using (var stream = File.Open(currDB, FileMode.Open))
            {
                using (var binaryReader = new BinaryReader(stream))
                {
                    binaryReader.BaseStream.Position = offsetFromStart + Page.spacePerPage * number;
                    bytes = binaryReader.ReadBytes(Page.spacePerPage);
                }
            }
            return bytes;
        }
        public Page CreateNewPage()
        {
            int newPageNum = calcNumberOfNewPage();
            Page currPage = new Page(newPageNum, Page.spacePerPage - Page.pageHeaderSize);
            WriteNewPage(currPage); //write it to file
            bufferPool.Add(currPage.Number, currPage); //add to dictionary
            currPageNumb = newPageNum;
            needToWriteCurrPage = true;
            
            return currPage;
        }
        public int calcNumberOfNewPage()
        {
            long fileSize = new System.IO.FileInfo(currDB).Length - offsetFromStart;
            return (int)(fileSize / Page.spacePerPage) + 1;
        }
        public void WriteNewPage(Page page)
        {
            using (var stream = File.Open(currDB, FileMode.Append))
            {
                using (var binaryWriter = new BinaryWriter(stream))
                {
                    binaryWriter.Write(page.PageSerialization());
                }
            }            
        }
        private void WritePage(BinaryWriter bw, Page page)
        {
            byte[] bytes = page.getFullPageBytes();
            bw.BaseStream.Position = offsetFromStart + page.Number * Page.spacePerPage;
            bw.Write(bytes);
        }
        public void CleanPagesAndWriteRoot() 
        {
            using (var stream = File.Open(currDB, FileMode.Open))
            {
                using (var binaryWriter = new BinaryWriter(stream))
                {
                    foreach (KeyValuePair<int, Page> p in bufferPool)
                    {
                        if (p.Value.IsDirty)
                        {
                            WritePage(binaryWriter, p.Value);
                            p.Value.IsDirty = false;
                        }
                    }
                    if (needToWriteRoot)//write root
                    {
                        binaryWriter.BaseStream.Position = sizeof(int) * 2;
                        binaryWriter.Write(BitConverter.GetBytes(rootPage));
                        binaryWriter.Write(BitConverter.GetBytes(rootOffset));
                        needToWriteRoot = false;
                    }
                    if (needToWriteCurrPage)
                    {
                        binaryWriter.BaseStream.Position = 0;//set position to the start (index page number)
                        binaryWriter.Write(BitConverter.GetBytes(currPageNumb));                        
                        needToWriteCurrPage = false;
                    }
                }
            }            
        }

        //----------------------------------------------------------------------------------
        //records manipulations
        public Record getRoot()
        {
            return _root;
        }
        public void setRoot(Record? record)
        {
            if(record != null && !record.IsNill())
            {
                rootPage = record.recordPage;
                rootOffset = record.recordOffset;
                _root = record;
            }
            else
            {
                rootPage = 0;
                rootOffset = 0;
                if (record != null)
                    _root = record;
                else
                    _root = new Record(null);
            }            
            needToWriteRoot = true;
        }

        public void setRecordOnPage(int pageNum, Record record)
        {
            bufferPool[pageNum].setRecord(pageHeaderSize, record);
        }
        //parent
        public Record getParent(Record record)
        {
            if (record == null)
                return new Record(null);
            if (record.IsNill())
                return record.P;
            return getRecordFromPage(record.ParentPage, record.ParentOffset);
        }
        public void setParent(Record record, Record parent)
        {          
            if (!parent.IsNill())
            {
                record.ParentPage = parent.recordPage;
                record.ParentOffset = parent.recordOffset;
                if (record.IsNill())
                    record.P = parent;
            }
            else
            {
                record.ParentPage = 0;
                record.ParentOffset = 0;
                record.P = parent;
            }            
            bufferPool[record.recordPage].IsDirty = true;
        }
        public Record? getGrandparent(Record record)
        {            
            return getParent(getParent(record));
        }

        //left
        public Record? getLeft(Record record)
        {
            if (record.IsNill())
                return null;
            if (record.LeftOffset == 0)
                return record.leftNill;
            return getRecordFromPage(record.LeftPage, record.LeftOffset);
        }
        public void setLeft(Record record, Record? successor)
        {
            if (successor != null && !successor.IsNill())
            {
                record.LeftPage = successor.recordPage;
                record.LeftOffset = successor.recordOffset;
            }
            else
            {
                record.LeftPage = 0;
                record.LeftOffset = 0;
                if (successor != null)
                    record.leftNill = successor;
                else
                    record.leftNill = new Record(record);
            }            
            bufferPool[record.recordPage].IsDirty = true;
        }
        //right
        public Record? getRight(Record record)
        {
            if (record.IsNill())
                return null;
            if (record.RightOffset == 0)
                return record.rightNill;
            return getRecordFromPage(record.RightPage, record.RightOffset);
        }
        public void setRight(Record? record, Record successor)
        {            
            if (successor != null && !successor.IsNill())
            {
                record.RightPage = successor.recordPage;
                record.RightOffset = successor.recordOffset;
            }
            else
            {
                record.RightPage = 0;
                record.RightOffset = 0;
                if (successor != null)
                    record.rightNill = successor;
                else
                    record.rightNill = new Record(record);
            }           
            bufferPool[record.recordPage].IsDirty = true;
        }
        //color
        public void setColor(Record? record, Color color)
        {
            if (record != null && record.Color != color)
            {
                record.Color = color;
                bufferPool[record.recordPage].IsDirty = true;
            }
        }
       
        public void LeftRotate(Record x)
        {
            Record y = getRight(x);
            setRight(x, getLeft(y)); // x.Right = y.Left
            if (getLeft(y)!=null)              
                setParent(getLeft(y), x);   //y.Left.P = x
            setParent(y, getParent(x)); //y.P = x.P
                                        
            if (x.ParentOffset == 0) //if x.P == null
                setRoot(y); //set root to y
            else if (x == getLeft(getParent(x)) ) //x == x.P.Left//(x is left child)
                setLeft(getParent(x), y);//x.P.Left = y
            else //(x is right child)
                setRight(getParent(x), y);//x.P.Right = y
            setLeft(y, x); //y.Left = x
            setParent(x, y);
        }
        public void RightRotate(Record? x)
        {
            Record y = getLeft(x);
            setLeft(x, getRight(y)); // x.Left = y.Right
            if (getRight(y)!=null)
                setParent(getRight(y), x);   //y.Right.P = x
            setParent(y, getParent(x)); //y.P = x.P
                                        
            if (x.ParentOffset == 0) //if x.P == null
                setRoot(y); //set root to y
            else if (x == getRight(getParent(x)) ) //x == x.P.Right
                setRight(getParent(x), y);//x.P.Right = y
            else
                setLeft(getParent(x), y);//x.P.Left = y
            setRight(y, x);//y.Right = x
            setParent(x, y);
        }
        public void Transplant(Record u, Record v)
        {
            if (getParent(u).IsNill())
                setRoot(v);//root = v;
            else if(u == getLeft(getParent(u)) )
                setLeft(getParent(u), v);//u.P.Left = v;
            else
                setRight(getParent(u), v);//u.P.Right = v;
                        
            setParent(v, getParent(u));//v.P = u.P;            
        }
        public Record Minimum(Record record)
        {
            while (!getLeft(record).IsNill()) //getLeft(record) != null
            { 
                record = getLeft(record);
            }
            return record;

        }

    }
}

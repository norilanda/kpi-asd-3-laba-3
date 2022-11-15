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
 
        private static int spacePerPage = Page.pageSizeTotal;
        private static int offsetFromStart = sizeof(int) * 4;
        private static int pageHeaderSize = Page.pageHeaderSize;
        private int currIndexPageNumb;
        private int currDataPageNumb;
        private int rootPage;
        private int rootOffset;

        private bool needToWriteCurrPage; //start of file
        private bool needToWriteRoot;

        public BufferManager(string dbName)
        {
            currDB = dbName;
            bufferPool = new Dictionary<int, Page>();
            //StartRead();
            using (var stream = File.Open(currDB, FileMode.Open))
            {
                using(var binaryReader = new BinaryReader(stream))
                {
                    currIndexPageNumb = binaryReader.ReadInt32();
                    currDataPageNumb = binaryReader.ReadInt32();
                    rootPage = binaryReader.ReadInt32();
                    rootOffset = binaryReader.ReadInt32();
                }
            }            
            needToWriteRoot = false;
        }
             
        public Page getCurrDataPage()
        {
            return getPageWithNumber(currDataPageNumb);
        }
        public Page getCurrIndexPage()
        {
            return getPageWithNumber(currIndexPageNumb);
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
                    binaryReader.BaseStream.Position = offsetFromStart + spacePerPage * number;
                    bytes = binaryReader.ReadBytes(spacePerPage);
                }
            }
            return bytes;
        }
        public Page CreateNewPage(PageType type)
        {
            int newPageNum = calcNumberOfNewPage();
            Page currPage = new Page(newPageNum, type, spacePerPage - Page.pageHeaderSize);
            WriteNewPage(currPage); //write it to file
            bufferPool.Add(currPage.Number, currPage); //add to dictionary
            if (type == PageType.index)
                currIndexPageNumb = newPageNum;
            else
                currDataPageNumb = newPageNum;
            needToWriteCurrPage = true;
            
            return currPage;
        }
        public int calcNumberOfNewPage()
        {
            long fileSize = new System.IO.FileInfo(currDB).Length;
            return (int)(fileSize / spacePerPage) + 1;
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
            bw.BaseStream.Position = offsetFromStart + page.Number * spacePerPage;
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
                        binaryWriter.Write(BitConverter.GetBytes(currIndexPageNumb));
                        binaryWriter.BaseStream.Position = 4;//set position to the 2-nd int (data page number)
                        binaryWriter.Write(BitConverter.GetBytes(currDataPageNumb));
                        needToWriteCurrPage = false;
                    }
                }
            }            
        }

        //----------------------------------------------------------------------------------
        //records manipulations
        public Record getRoot()
        {
            return getRecordFromPage(rootPage, rootOffset);
        }
        public void setRoot(Record record)
        {
            if(record != null)
            {
                rootPage = record.recordPage;
                rootOffset = record.recordOffset;
            }
            else
            {
                rootPage = 0;
                rootOffset = 0;
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
            return getRecordFromPage(record.ParentPage, record.ParentOffset);
        }
        public void setParent(Record record, Record parent)
        {
            if (parent != null)
            {
                record.ParentPage = parent.recordPage;
                record.ParentOffset = parent.recordOffset;
            }
            else
            {
                record.ParentPage = 0;
                record.ParentOffset = 0;
            }            
            bufferPool[record.recordPage].IsDirty = true;
        }
        public Record getGrandparent(Record record) => getParent(getParent(record));

        //left
        public Record getLeft(Record record)
        {
            return getRecordFromPage(record.LeftPage, record.LeftOffset);
        }
        public void setLeft(Record record, Record successor)
        {
            if (successor!= null)
            {
                record.LeftPage = successor.recordPage;
                record.LeftOffset = successor.recordOffset;
            }
            else
            {
                record.LeftPage = 0;
                record.LeftOffset = 0;
            }            
            bufferPool[record.recordPage].IsDirty = true;
        }
        //right
        public Record getRight(Record record)
        {
            return getRecordFromPage(record.RightPage, record.RightOffset);
        }
        public void setRight(Record record, Record successor)
        {

            if (successor != null)
            {
                record.RightPage = successor.recordPage;
                record.RightOffset = successor.recordOffset;
            }
            else
            {
                record.RightPage = 0;
                record.RightOffset = 0;
            }           
            bufferPool[record.recordPage].IsDirty = true;
        }
        public void setColor(Record record, Color color)
        {
            record.Color = color;
            bufferPool[record.recordPage].IsDirty = true;
        }
        public void LeftRotate(Record x)
        {
            Record y = getRight(x);
            setRight(x, getLeft(y)); // x.Right = y.Left
            if (getLeft(y)!=null && !getLeft(y).isNull())                
                setParent(getLeft(y), x);   //y.Left.P = x
            setParent(y, getParent(x)); //y.P = x.P
                                        //
            if (x.ParentOffset == 0) //if x.P == null
                setRoot(y); //set root to y
            else if (Record.AreEqual(x, getLeft(getParent(x)))) //x == x.P.Left
                setLeft(getParent(x), y);//x.P.Left = y
            else
                setRight(getParent(x), y);//x.P.Right = y
            setLeft(y, x); //y.Left = x
            setParent(x, y);
        }
        public void RightRotate(Record x)
        {
            Record y = getLeft(x);
            setLeft(x, getRight(y)); // x.Left = y.Right
            if (getRight(y)!=null && !getRight(y).isNull())
                setParent(getRight(y), x);   //y.Right.P = x
            setParent(y, getParent(x)); //y.P = x.P
                                        //
            if (x.ParentOffset == 0) //if x.P == null
                setRoot(y); //set root to y
            else if (Record.AreEqual(x, getRight(getParent(x)))) //x == x.P.Right
                setRight(getParent(x), y);//x.P.Right = y
            else
                setLeft(getParent(x), y);//x.P.Left = y
            setRight(y, x);//y.Right = x
            setParent(x, y);
        }
        
    }
}

using RedBlackTreeAlgo.FileStructure;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace RedBlackTreeAlgo.DatabaseManager
{
    public class BufferManager
    {
        private Dictionary<int, Page> bufferPool;   //page number, page
        private string? currDB;
        private BinaryWriter? binaryWriter;
        private BinaryReader? binaryReader;
        private static int spacePerPage = Page.pageSizeTotal;
        private static int offsetFromStart = sizeof(int) * 4;
        private static int pageHeaderSize = Page.pageHeaderSize;
        private int currIndexPageNumb;
        private int currDataPageNumb;
        private int rootPage;
        private int rootOffset;
        private bool needToWriteRoot;

        public BufferManager(string? dbName)
        {
            binaryWriter = null;
            binaryReader = null;
            currDB = dbName;
            bufferPool = new Dictionary<int, Page>();
            StartRead();
            currIndexPageNumb = binaryReader.ReadInt32();
            currDataPageNumb = binaryReader.ReadInt32();
            rootPage = binaryReader.ReadInt32();
            rootOffset = binaryReader.ReadInt32();
            needToWriteRoot = false;
        }
        private void StartRead()
        {
            if (binaryWriter != null)
                binaryWriter.Close();
            if (binaryReader != null) binaryReader.Close();
            binaryReader = new BinaryReader(File.Open(currDB, FileMode.Open));
        }
        private void StartWrite(FileMode mode = FileMode.Open)
        {
            if (binaryReader != null) binaryReader.Close();
            if (binaryWriter != null) binaryWriter.Close();
            binaryWriter = new BinaryWriter(File.Open(currDB, mode));
        }
        private void StopWrite()
        {
            if (binaryWriter != null)
                binaryWriter.Close();
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
            binaryReader.BaseStream.Position = offsetFromStart + spacePerPage * number;
            return binaryReader.ReadBytes(spacePerPage);
        }
        public Page CreateNewPage(PageType type)
        {
            int newPageNum = calcNumberOfNewPage();
            Page currPage = new Page(newPageNum, type, spacePerPage - Page.pageHeaderSize);
            WriteNewPage(currPage); //write it to file
            bufferPool.Add(currPage.Number, currPage); //add to dictionary
            StopWrite();
            StartWrite();
            if (type == PageType.index)
                binaryWriter.BaseStream.Position = 0;//set position to the start (index page number)
            else
                binaryWriter.BaseStream.Position = 4;//set position to the 2-nd int (data page number)
            binaryWriter.Write(BitConverter.GetBytes(newPageNum));
            return currPage;
        }
        public int calcNumberOfNewPage()
        {
            long fileSize = new System.IO.FileInfo(currDB).Length;
            return (int)(fileSize / spacePerPage) + 1;
        }
        public void WriteNewPage(Page page)
        {
            StartWrite(FileMode.Append);
            binaryWriter.Write(page.PageSerialization());
        }
        //public void WritePage(Page page) { }

        //records manipulations
        public Record getRoot()
        {
            return getRecordFromPage(rootPage, rootOffset);
        }
        public void setRoot(Record record)
        {
            rootPage = record.recordPage;
            rootOffset = record.recordOffset;
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
            record.ParentPage = parent.recordPage;
            record.ParentOffset = parent.recordOffset;
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
            record.LeftPage = successor.recordPage;
            record.LeftOffset = successor.recordOffset;
            bufferPool[record.recordPage].IsDirty = true;
        }
        //right
        public Record getRight(Record record)
        {
            return getRecordFromPage(record.RightPage, record.RightOffset);
        }
        public void setRight(Record record, Record successor)
        {
            record.RightPage = successor.recordPage;
            record.RightOffset = successor.recordOffset;
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
            if (!getLeft(y).isNull())                
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
            if (!getRight(y).isNull())
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

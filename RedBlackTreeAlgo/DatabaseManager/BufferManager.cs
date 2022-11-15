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
        private static int offsetFromStart = sizeof(int) * 2;
        private static int pageHeaderSize = Page.pageHeaderSize;
        private int currIndexPageNumb;
        private int currDataPageNumb;

        public BufferManager(string? dbName)
        {
            binaryWriter = null;
            binaryReader = null;
            currDB = dbName;
            bufferPool= new Dictionary<int, Page>();
            StartRead();
            currIndexPageNumb = binaryReader.ReadInt32();
            currDataPageNumb = binaryReader.ReadInt32();
        }
        private void StartRead()
        {
            if(binaryWriter != null)
                binaryWriter.Close();
            binaryReader = new BinaryReader(File.Open(currDB, FileMode.Open));
        }
        private void StartWrite(FileMode mode = FileMode.Open)
        {
            if(binaryReader != null) binaryReader.Close();
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
        public Record getParent(Record record)
        {
            return getRecordFromPage(record.ParentPage, record.ParentOffset);
        }
        public Record getGrandparent(Record record)
        {
            return getParent(getParent(record));
        }
        public Record getLeft(Record record)
        {
            return getRecordFromPage(record.LeftPage, record.LeftOffset);
        }
        public Record getRight(Record record)
        {
            return getRecordFromPage(record.RightPage, record.RightOffset);
        }
    }
}

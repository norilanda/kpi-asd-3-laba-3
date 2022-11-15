using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace RedBlackTreeAlgo.FileStructure
{
    public enum PageType
    {
        index,
        data
    }
    public class Page//internal?
    {
        /* Class Page represents the block in file.
         */
        public static int pageSizeTotal = 4 * 1024;    //page size in bytes
        public static int recordSize = Record.RecordSize();
        public static int pageHeaderSize = sizeof(int) * 3 + sizeof(PageType);   //size in file
        //header
        private int _number;
        private PageType _type;  //page type, shows which information is stored in this part
        private int _freeSpace;  //how many bytes are avaliable in this page 
        private int _position;   //position to add records
        //helping vars
        private bool _isDirty;   //has been currently written or not // IS NOT added to file

        public byte[] buff;
        public Dictionary<int, Record> records; //offset from page start, record
        //getters/setters
        public int Number => _number;
        public int Position
        {
            get { return _position; }
            set { _position = value; }
        }
        public int FreeSpace
        {
            get { return _freeSpace; }
            set { _freeSpace = value; }
        }
        public bool IsDirty{ 
            get { return _isDirty; }
            set { _isDirty = value; }
        }

        public Page(int number, PageType type, int freeSpace)
        {
            this._number = number;
            this._type = type;
            this._freeSpace = freeSpace;
            this._position = pageHeaderSize;
            this._isDirty = false;
            this.buff = new byte[pageSizeTotal];
        }
        public Page(byte[] buff)
        {
            this.buff = buff;
            byte[] header = new byte[pageHeaderSize];
            Array.Copy(buff, 0, header, 0, pageHeaderSize);
            PageDeserialization(header);
            setRecords();
        }
        public void setRecords()
        {
            records = new Dictionary<int, Record>();
            int currPos = pageHeaderSize;
            while (currPos < _position)
            {
                records.Add(currPos, getRecord(currPos));
                currPos += Record.RecordSize();
            }
        }
        public Record getRecord(int offset)
        {
            byte[] record = new byte[recordSize];
            Array.Copy(buff, offset, record, 0, recordSize);
            return new Record(record, _number, offset);
        }
        public void setRecord(int offset, Record record)
        {
            records[offset] = record;
            _isDirty= true;
        }
        public void AddData(byte[] data)
        {
            data.CopyTo(buff, _position);
            _position += data.Length;   //move pointer to next position
            _freeSpace -= data.Length;
            _isDirty = true;
        }
        public bool isEnoughSpace(int recordSize)
        {
            return _freeSpace >= recordSize;
        }
        public byte[] PageSerialization()
        {
            byte[] pageBytes = new byte[pageHeaderSize];
            int pos = 0;
            byte[] bytes = BitConverter.GetBytes(_number);
            bytes.CopyTo(pageBytes, pos);

            bytes = BitConverter.GetBytes((int)_type);
            bytes.CopyTo(pageBytes, pos += bytes.Length);

            bytes = BitConverter.GetBytes(_freeSpace);
            bytes.CopyTo(pageBytes, pos += bytes.Length);

            bytes = BitConverter.GetBytes(_position);
            bytes.CopyTo(pageBytes, pos += bytes.Length);
            return pageBytes;
        }
        public void PageDeserialization(byte[] bytes)
        {
            int pos = 0;
            this._number = BitConverter.ToInt32(bytes, pos);
            this._type = (PageType)BitConverter.ToInt32(bytes, pos += sizeof(int));
            this._freeSpace = BitConverter.ToInt32(bytes, pos += sizeof(int));
            this._position = BitConverter.ToInt32(bytes, pos += sizeof(int));
        }
    }
}

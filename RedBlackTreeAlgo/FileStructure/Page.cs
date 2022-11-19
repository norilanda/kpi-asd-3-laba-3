using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;

namespace RedBlackTreeAlgo.FileStructure
{   
    public class Page//internal?
    {
        /* Class Page represents the block in file.
         */
        public static int spacePerPage = 4 * 1024;    //page size in bytes
        public static int pageHeaderSize = sizeof(int) * 3 ;   //size in file
        //header
        private int _number;
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

        public Page(int number, int freeSpace)
        {
            this._number = number;
            this._freeSpace = freeSpace;
            this._position = pageHeaderSize;
            this._isDirty = false;
            this.buff = new byte[spacePerPage];
            setRecords();
        }
        public Page(byte[] buff)
        {
            this.buff = buff;
            byte[] header = new byte[pageHeaderSize];
            Array.Copy(buff, 0, header, 0, pageHeaderSize);
            PageHeaderDeserialization(header);
            setRecords();
        }
        public void setRecords()
        {
            records = new Dictionary<int, Record>();
            int currPos = pageHeaderSize;
            while (currPos < _position)
            {
                records.Add(currPos, BackupRecordFromFile(currPos));
                currPos += Record.RecordSize;
            }
        }
        public Record BackupRecordFromFile(int offset)
        {
            byte[] record = new byte[Record.RecordSize];
            Array.Copy(buff, offset, record, 0, Record.RecordSize);
            return new Record(record, _number, offset);
        }
        public Record getRecord(int offset)
        {
            try
            {
                return records[offset];
            }
            catch (System.Collections.Generic.KeyNotFoundException e)
            {
                return null;
            }        
        }
        public void setRecord(int offset, Record record)
        {
            records[offset] = record;
            _isDirty= true;
        }
        public void AddRecord(Record record)
        {
            records.Add(record.recordOffset, record);
            _freeSpace -= Record.RecordSize;
            _position += Record.RecordSize;
            _isDirty = true;
        }
        //public void AddData(byte[] data)//?????????????????
        //{
        //    data.CopyTo(buff, _position);
        //    _position += data.Length;   //move pointer to next position
        //    _freeSpace -= data.Length;
        //    _isDirty = true;
        //}
        public bool isEnoughSpace()
        {
            return _freeSpace >= Record.RecordSize;
        }
        public byte[] PageSerialization()
        {
            byte[] pageBytes = new byte[pageHeaderSize];
            int pos = 0;
            byte[] bytes = BitConverter.GetBytes(_number);
            bytes.CopyTo(pageBytes, pos);

            bytes = BitConverter.GetBytes(_freeSpace);
            bytes.CopyTo(pageBytes, pos += bytes.Length);

            bytes = BitConverter.GetBytes(_position);
            bytes.CopyTo(pageBytes, pos += bytes.Length);
            return pageBytes;
        }
        public void PageHeaderDeserialization(byte[] bytes)
        {
            int pos = 0;
            this._number = BitConverter.ToInt32(bytes, pos);
            this._freeSpace = BitConverter.ToInt32(bytes, pos += sizeof(int));
            this._position = BitConverter.ToInt32(bytes, pos += sizeof(int));
        }
        public byte[] getFullPageBytes()
        {
            byte[] header = PageSerialization();
            Array.Copy(header, buff, header.Length);
            byte[] record;
            foreach (KeyValuePair<int, Record> r in records)
            {
                record = r.Value.RecordSerialization();
                Array.Copy(record,0, buff, r.Key, Record.RecordSize);
            }
            return buff;
        }
    }
}

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
        //getters/setters
        public int Number => _number;
       

        public Page(int number, PageType type, int freeSpace)
        {
            this._number = number;
            this._type = type;
            this._freeSpace = freeSpace;
            this._position = pageHeaderSize;
            this._isDirty = false;
            this.buff = new byte[pageHeaderSize];
        }
        public Page(byte[] buff)
        {
            this.buff = buff;
            byte[] header = new byte[pageHeaderSize];
            Array.Copy(buff, 0, header, 0, pageHeaderSize);
            PageDeserialization(header);
        }
        public Record getRecord(int offset)
        {
            byte[] record = new byte[recordSize];
            Array.Copy(buff, offset, record, 0, recordSize);
            return new Record(record, _number, offset);
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

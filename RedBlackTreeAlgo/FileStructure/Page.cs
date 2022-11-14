using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedBlackTreeAlgo.FileStructure
{
    public enum Type
    {
        index,
        data
    }
    public class Page//internal?
    {
        /* Class Page represents the block in file.
         */
        public static int pageSize;    //page size in bytes
        private static int pageSizeBytes() { return sizeof(int) * 3 + sizeof(Type); }   //size in file
        //header
        private int _number;
        private Type _type;  //page type, shows which information is stored in this part
        private int _freeSpace;  //how many bytes are avaliable in this page 
        private int _position;   //position to add records
        //helping vars
        private bool _isDirty;   //has been currently written or not // IS NOT added to file

        //getters/setters


        public Page(int number, Type type, int freeSpace)
        {
            this._number = number;
            this._type = type;
            this._freeSpace = freeSpace;
            this._position = pageSizeBytes();
            this._isDirty = false;
        }
        public Page(byte[] bytes)
        {
            PageDeserialization(bytes);
        }
        public byte[] PageSerialization()
        {
            byte[] pageBytes = new byte[pageSizeBytes()];
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
            this._type = (Type)BitConverter.ToInt32(bytes, pos += sizeof(int));
            this._freeSpace = BitConverter.ToInt32(bytes, pos += sizeof(int));
            this._position = BitConverter.ToInt32(bytes, pos += sizeof(int));
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedBlackTreeAlgo.DatabaseManager
{
    public enum Color
    {
        RED,
        BLACK
    }
    public class Record
    {
        /*Represents the record in the page. For index type page contains key, index for data of the key (page-row), index for a left successor (page-row) and index for a right successor (page-row)
         */
        /*offset - number of bytes from the page start*/

        private int _key;
        //data
        private int _dataPage;
        private int _dataOffset;    //+color
        private Color _color;

        //left successor
        private int _leftPage;
        private int _leftOffset;
        //right successor
        private int _rightPage;
        private int _rightOffset;
        //parent
        private int _parentPage;
        private int _parentOffset;

        public Record(int key, int page, int offset)
        {
            _key = key;
            _dataPage = page;
            _dataOffset = offset;
            _color = Color.RED;
            _leftPage = _leftOffset = 0;
            _rightPage = _rightOffset = 0;
            _parentPage = _parentOffset = 0;
        }
        public void RecordSerialization()
        {
            const int LAST_BIT_POSITION = 7;
            byte[] recordBytes = new byte[sizeof(int)*9];
            int pos = 0;
            byte[] bytes = BitConverter.GetBytes(_key);
            recordBytes.CopyTo(bytes, pos);
            pos += bytes.Length;

            bytes = BitConverter.GetBytes(_dataPage);
            recordBytes.CopyTo(bytes, pos);//!!!!!!!!!!lambda?
            pos += bytes.Length;
            bytes = BitConverter.GetBytes(_dataOffset);
            if(!BitConverter.IsLittleEndian)//if big endian (important to color storage)
                Array.Reverse(bytes);

            if (_color == Color.RED)    //storing the color as the less significant bit
                bytes[bytes.Length - 1] = (byte)(bytes[bytes.Length - 1] &~ (1 << LAST_BIT_POSITION));
            else
                bytes[bytes.Length - 1] = (byte)(bytes[bytes.Length - 1]|(1 << LAST_BIT_POSITION));
            recordBytes.CopyTo(bytes, pos);
            pos += bytes.Length;

            bytes = BitConverter.GetBytes(_leftPage);
            recordBytes.CopyTo(bytes, pos);
            pos += bytes.Length;
            bytes = BitConverter.GetBytes(_leftOffset);
            recordBytes.CopyTo(bytes, pos);
            pos += bytes.Length;

            bytes = BitConverter.GetBytes(_rightPage);
            recordBytes.CopyTo(bytes, pos);
            pos += bytes.Length;
            bytes = BitConverter.GetBytes(_rightOffset);
            recordBytes.CopyTo(bytes, pos);
            pos += bytes.Length;

            bytes = BitConverter.GetBytes(_parentPage);
            recordBytes.CopyTo(bytes, pos);
            pos += bytes.Length;
            bytes = BitConverter.GetBytes(_parentOffset);
            recordBytes.CopyTo(bytes, pos);
            pos += bytes.Length;
        }
        public void RecordDeserialization()
        {

        }
    }
}

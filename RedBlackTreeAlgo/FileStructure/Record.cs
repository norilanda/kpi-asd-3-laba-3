using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedBlackTreeAlgo.FileStructure
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

        //helping vars
        public int recordPage;
        public int recordOffset;

        //getters/setters
        public int Key
        {
            get { return _key; }
        }
        public int Datapage
        {
            get { return _dataPage; }
            set { _dataPage = value; }
        }
        public int DataOffset
        {
            get { return _dataOffset; }
            set { _dataOffset = value; }
        }
        public Color Color
        {
            get { return _color; }
            set { _color = value; }
        }
        public int LeftPage
        {
            get { return _leftPage; }
            set { _leftPage = value; }
        }
        public int LeftOffset
        {
            get { return _leftOffset; }
            set { _leftOffset = value; }
        }
        public int RightPage
        {
            get { return _rightPage; }
            set { _rightPage = value; }
        }
        public int RightOffset
        {
            get { return _rightOffset; }
            set { _rightOffset = value; }
        }
        public int ParentPage
        {
            get { return _parentPage; }
            set { _parentPage = value; }
        }
        public int ParentOffset
        {
            get { return _parentPage; }
            set { _parentPage = value; }
        }

        public static int RecordSize() { return sizeof(int) * 9; }
        public Record(int key, int page, int offset, int recordPage, int recordOffset)
        {
            _key = key;
            _dataPage = page;
            _dataOffset = offset;
            _color = Color.RED;
            _leftPage = _leftOffset = 0;
            _rightPage = _rightOffset = 0;
            _parentPage = _parentOffset = 0;
            this.recordPage = recordPage;
            this.recordOffset = recordOffset;
        }
        public Record(byte[] bytes, int recordPage, int recordOffset) {
            this.recordPage = recordPage;
            this.recordOffset = recordOffset;
            RecordDeserialization(bytes); }
        public byte[] RecordSerialization()
        {
            const int LAST_BIT_POSITION = 7;
            //Func<byte[], byte[], int, void> CopyField = (bytes, recordBytes, pos) => { } //maybe later
            byte[] recordBytes = new byte[RecordSize()];
            int pos = 0;
            byte[] bytes = BitConverter.GetBytes(_key);
            bytes.CopyTo(recordBytes, pos);
            pos += bytes.Length;

            bytes = BitConverter.GetBytes(_dataPage);
            bytes.CopyTo(recordBytes, pos); //!!!!!!!!!!lambda?
            pos += bytes.Length;
            bytes = BitConverter.GetBytes(_dataOffset);
            if(!BitConverter.IsLittleEndian)//if big endian (important to color storage)
                Array.Reverse(bytes);

            if (_color == Color.RED)    //storing the color as the less significant bit
                bytes[^1] = (byte)(bytes[^1] &~ (1 << LAST_BIT_POSITION));
            else
                bytes[^1] = (byte)(bytes[^1]|(1 << LAST_BIT_POSITION));
            bytes.CopyTo(recordBytes, pos);
            pos += bytes.Length;

            bytes = BitConverter.GetBytes(_leftPage);
            bytes.CopyTo(recordBytes, pos);
            pos += bytes.Length;
            bytes = BitConverter.GetBytes(_leftOffset);
            bytes.CopyTo(recordBytes, pos);
            pos += bytes.Length;

            bytes = BitConverter.GetBytes(_rightPage);
            bytes.CopyTo(recordBytes, pos);
            pos += bytes.Length;
            bytes = BitConverter.GetBytes(_rightOffset);
            bytes.CopyTo(recordBytes, pos);
            pos += bytes.Length;

            bytes = BitConverter.GetBytes(_parentPage);
            bytes.CopyTo(recordBytes, pos);
            pos += bytes.Length;
            bytes = BitConverter.GetBytes(_parentOffset);
            bytes.CopyTo(recordBytes, pos);
            return recordBytes;
        }
        public void RecordDeserialization(byte[] bytes)
        {
            const int LAST_BIT_POSITION = 7;
            int pos = 0;
            this._key = BitConverter.ToInt32(bytes, pos);
            this._dataPage = BitConverter.ToInt32(bytes, pos += sizeof(int));
            byte[] offsetAndColor = new byte[sizeof(int)];
            Array.Copy(bytes, pos += sizeof(int), offsetAndColor, 0, sizeof(int));
            if ((offsetAndColor[^1] & (1 << LAST_BIT_POSITION)) == 0)
                this._color = Color.RED;
            else
                this._color = Color.BLACK;
            offsetAndColor[^1] = (byte)(bytes[^1] & ~(1 << LAST_BIT_POSITION));

            this._dataOffset = BitConverter.ToInt32(offsetAndColor, 0); ;
            this._leftPage = BitConverter.ToInt32(bytes, pos += sizeof(int));
            this._leftOffset = BitConverter.ToInt32(bytes, pos += sizeof(int));
            this._rightPage = BitConverter.ToInt32(bytes, pos += sizeof(int));
            this._rightOffset = BitConverter.ToInt32(bytes, pos += sizeof(int));
            this._parentPage = BitConverter.ToInt32(bytes, pos += sizeof(int));
            this._parentOffset = BitConverter.ToInt32(bytes, pos += sizeof(int));
        }
    }
}

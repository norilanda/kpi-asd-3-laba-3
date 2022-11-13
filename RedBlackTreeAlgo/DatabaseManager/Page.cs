using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedBlackTreeAlgo.DatabaseManager
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
        //header
        private int _number;
        private Type _type;  //page type, shows which information is stored in this part
        private int _freeSpace;  //how many bytes are avaliable in this page 
        private int _position;   //position to add records
        private bool _isDirty;   //has been currently written or not

        //getters/setters


        public Page(int number, Type type, int freeSpace)
        {
            this._number = number;
            this._type = type;
            this._freeSpace = freeSpace;
            this._position = sizeof(int)*3+sizeof(Type)+ sizeof(bool);
            this._isDirty = false;
        }
        public Page()
        {
            PageDeserialization();
        }
        public void PageSerialization()
        {

        }
        public void PageDeserialization()
        {

        }
    }
}

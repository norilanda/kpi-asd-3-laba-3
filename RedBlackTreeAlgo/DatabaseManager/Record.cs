using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedBlackTreeAlgo.DatabaseManager
{
    public class Record
    {
        /*Represents the record in the page. For index type page contains key, index for data of the key (page-row), index for a left successor (page-row) and index for a right successor (page-row)
         */
        int key;    //
        int dataPage;
        int dataOffset; //number of bytes from the page start
        int leftPage;
        int leftOffset;
        int rightPage;
        int rightOffset;
    }
}

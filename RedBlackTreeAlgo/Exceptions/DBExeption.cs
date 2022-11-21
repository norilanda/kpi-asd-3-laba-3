using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedBlackTreeAlgo.Exceptions
{
    public class RecordAlreadyExists : Exception
    {
        public RecordAlreadyExists(string message) : base(message) { }
    }
    public class WrongDataFormat : Exception
    {
        public WrongDataFormat(string message) : base(message) { }
    }
}

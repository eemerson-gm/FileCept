using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCept.Classes
{
    public class ByteIndex
    {
        public int Index;
        public string Extension;

        public ByteIndex(int index, string extension)
        {
            Index = index;
            Extension = extension;
        }
    }
}

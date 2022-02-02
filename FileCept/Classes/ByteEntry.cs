using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCept.Classes
{
    public class ByteEntry
    {
        public byte[] Start { get; set; }
        public byte[] End { get; set; }
        public string Type { get; set; }
        public string Extension { get; set; }

        public ByteEntry(string start, string end, string type, string extension)
        {
            Start = StringToByteArray(start);
            End = StringToByteArray(end);
            Type = type;
            Extension = extension;
        }

        public static byte[] StringToByteArray(string str)
        {
            string[] tokens = str.Split(" ");
            byte[] bytes = new byte[tokens.Length];
            for (int a = 0; a < tokens.Length; a++)
            {
                bytes[a] = Convert.ToByte(tokens[a], 16);
            }
            return bytes;
        }

        public string StartToString()
        {
            return BitConverter.ToString(Start).Replace("-", " ");
        }
        
        public string EndToString()
        {
            return BitConverter.ToString(End).Replace("-", " ");
        }

    }
}

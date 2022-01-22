using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCept.Classes
{
    public class ByteMapper
    {
        private string dirSource;
        private string dirDestination;
        public List<string> byteFiles = new List<string>();
        public List<byte[]> byteStarts = new List<byte[]>();
        public List<byte[]> byteEnds = new List<byte[]>();
        public List<string> byteExtensions = new List<string>();

        public ByteMapper(string source, string destination)
        {
            dirSource = source;
            dirDestination = destination;
            string[] fileEntries = Directory.GetFiles(dirSource, "*", SearchOption.AllDirectories);
            foreach(string entry in fileEntries)
            {
                byteFiles.Add(entry);
            }
        }

        public void Start(BackgroundWorker worker)
        {
            worker.DoWork += DoWork;
            worker.RunWorkerAsync();
        }

        public void DoWork(object sender, DoWorkEventArgs e)
        {
            if (!Directory.Exists(dirDestination))
            {
                Directory.CreateDirectory(dirDestination);
            }
            foreach(string file in byteFiles)
            {
                using(FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    int indexStart = -1;
                    int indexEnd = -1;
                    do
                    {
                        indexStart = FindNextPattern(fileStream, byteStarts, true);
                        indexEnd = FindNextPattern(fileStream, byteEnds, false);
                        if (indexStart != -1 && indexEnd != -1)
                        {
                            byte[] bytes = new byte[indexEnd - indexStart];
                            fileStream.Seek(indexStart, SeekOrigin.Begin);
                            fileStream.Read(bytes, 0, bytes.Length);
                            fileStream.Seek(indexEnd, SeekOrigin.Begin);
                            File.WriteAllBytes(dirDestination + Guid.NewGuid().ToString("N") + ".png", bytes);
                        }
                    }
                    while (indexStart != -1 && indexEnd != -1);
                }
            }
        }

        public void AddPattern(string start, string end)
        {
            string[] tokenStarts = start.Split(" ");
            string[] tokenEnds = end.Split(" ");
            byte[] tempStarts = new byte[tokenStarts.Length];
            byte[] tempEnds = new byte[tokenEnds.Length];

            for(int a = 0; a < tokenStarts.Length; a++)
            {
                tempStarts[a] = Convert.ToByte(tokenStarts[a], 16);
            }
            for(int a = 0; a < tokenEnds.Length; a++)
            {
                tempEnds[a] = Convert.ToByte(tokenEnds[a], 16);
            }

            byteStarts.Add(tempStarts);
            byteEnds.Add(tempEnds);
        }

        private int FindNextPattern(FileStream fileStream, List<byte[]> listStarts, bool offset)
        {
            int byteValue = -1;
            do
            {
                byteValue = fileStream.ReadByte();
                for(int a = 0; a < listStarts.Count; a++)
                {
                    for(int b = 0; b < listStarts[a].Length; b++)
                    {
                        if(byteValue == listStarts[a][b])
                        {
                            if(b == listStarts[a].Length - 1)
                            {
                                return (int)fileStream.Position - ((offset == true) ? byteStarts[a].Length : 0);
                            }
                            else
                            {
                                byteValue = fileStream.ReadByte();
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            while ((byteValue != -1) && (listStarts.Count > 0));
            return -1;
        }

    }
}

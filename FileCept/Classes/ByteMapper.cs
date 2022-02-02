using System.ComponentModel;

namespace FileCept.Classes
{
    public class ByteMapper
    {
        private string DirectorySource;
        private string DirectoryDestination;
        public List<string> Files = new List<string>();
        public List<ByteEntry> Entries = new List<ByteEntry>();

        public ByteMapper(string source, string destination)
        {
            DirectorySource = source;
            DirectoryDestination = destination;
            string[] fileEntries = Directory.GetFiles(DirectorySource, "*", SearchOption.AllDirectories);
            foreach(string entry in fileEntries)
            {
                Files.Add(entry);
            }
        }

        public void Start(BackgroundWorker worker)
        {
            worker.DoWork += DoWork;
            worker.WorkerReportsProgress = true;
            worker.WorkerSupportsCancellation = true;
            worker.RunWorkerAsync();
        }

        public void DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = (BackgroundWorker)sender;
            int fileNumber = 0;
            foreach (string file in Files)
            {
                int fileCounter = 0;
                FileInfo fileInfo = new FileInfo(file);
                using(FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    int indexStart = -1;
                    int indexEnd = -1;
                    do
                    {
                        if (worker.CancellationPending)
                        {
                            return;
                        }
                        indexStart = FindFirstPattern(fileStream);
                        indexEnd = FindLastPattern(fileStream);
                        if (indexStart != -1 && indexEnd != -1)
                        {
                            byte[] bytes = new byte[indexEnd - indexStart];
                            fileStream.Seek(indexStart, SeekOrigin.Begin);
                            fileStream.Read(bytes, 0, bytes.Length);
                            fileStream.Seek(indexEnd, SeekOrigin.Begin);
                            File.WriteAllBytes(DirectoryDestination + fileInfo.Name.Replace(".", "_") + "_" + fileCounter.ToString() + ".png", bytes);
                            fileCounter++;
                        }
                    }
                    while (indexStart != -1 && indexEnd != -1);
                }
                fileNumber++;
                worker.ReportProgress((int)(((double)fileNumber/(double)Files.Count) * 100));
                Thread.Sleep(1);
            }
        }

        private int FindFirstPattern(FileStream fileStream)
        {
            int byteValue = -1;
            do
            {
                byteValue = fileStream.ReadByte();
                for(int a = 0; a < Entries.Count; a++)
                {
                    for(int b = 0; b < Entries[a].Start.Length; b++)
                    {
                        if(byteValue == Entries[a].Start[b])
                        {
                            if(b == Entries[a].Start.Length - 1)
                            {
                                return (int)fileStream.Position - Entries[a].Start.Length;
                            }
                            else
                            {
                                byteValue = fileStream.ReadByte();
                            }
                        }
                        else
                        {
                            fileStream.Seek(-b, SeekOrigin.Current);
                            break;
                        }
                    }
                }
            }
            while ((byteValue != -1) && (Entries.Count > 0));
            return -1;
        }

        private int FindLastPattern(FileStream fileStream)
        {
            int byteValue = -1;
            do
            {
                byteValue = fileStream.ReadByte();
                for (int a = 0; a < Entries.Count; a++)
                {
                    for (int b = 0; b < Entries[a].End.Length; b++)
                    {
                        if (byteValue == Entries[a].End[b])
                        {
                            if (b == Entries[a].End.Length - 1)
                            {
                                return (int)fileStream.Position;
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
            while ((byteValue != -1) && (Entries.Count > 0));
            return -1;
        }

    }
}

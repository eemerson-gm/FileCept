using System.ComponentModel;

namespace FileCept.Classes
{
    public class ByteMapper
    {

        private ulong ByteIndex;
        private ulong ByteSize;
        public List<string> Files;
        public List<BytePattern> Patterns;

        private string SourceDirectory;
        private string DestinationDirectory;
        private BackgroundWorker ByteWorker;
        private volatile bool IsWorking;

        public ByteMapper(string source, string destination)
        {

            SourceDirectory = source;
            DestinationDirectory = destination;
            Files = new List<string>();
            Patterns = new List<BytePattern>();

            string[] fileEntries = Directory.GetFiles(SourceDirectory, "*", SearchOption.AllDirectories);

            ByteIndex = 0;
            ByteSize = 0;
            IsWorking = false;

            foreach(string entry in fileEntries)
            {
                FileInfo fileInfo = new FileInfo(entry);
                ByteSize += (ulong)fileInfo.Length;
                Files.Add(entry);
            }

        }

        public void Start(BackgroundWorker worker)
        {
            IsWorking = true;
            ByteWorker = worker;
            ByteWorker.DoWork += DoWork;
            ByteWorker.WorkerReportsProgress = true;
            ByteWorker.WorkerSupportsCancellation = true;
            ByteWorker.RunWorkerAsync();

            Thread reportThread = new Thread(DoReport);
            reportThread.Start();
        }

        private void DoReport()
        {
            while (IsWorking)
            {
                int percent = (int)(((double)ByteIndex / (double)ByteSize) * 100);
                ByteWorker.ReportProgress(percent);
                Thread.Sleep(1000);
            }
        }

        private void DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = (BackgroundWorker)sender;
            foreach (string file in Files)
            {
                int fileCounter = 0;
                FileInfo fileInfo = new FileInfo(file);
                using(FileStream fileStream = new FileStream(file, FileMode.Open, FileAccess.Read))
                {
                    ByteIndex? indexStart = null;
                    int indexEnd = -1;
                    do
                    {
                        if (worker.CancellationPending)
                        {
                            return;
                        }
                        indexStart = FindFirstPattern(fileStream, worker);
                        indexEnd = FindLastPattern(fileStream, worker);
                        if (indexStart != null && indexEnd != -1)
                        {
                            byte[] bytes = new byte[indexEnd - indexStart.Index];
                            fileStream.Seek(indexStart.Index, SeekOrigin.Begin);
                            fileStream.Read(bytes, 0, bytes.Length);
                            fileStream.Seek(indexEnd, SeekOrigin.Begin);
                            File.WriteAllBytes(DestinationDirectory + fileInfo.Name.Replace(".", "_") + "_" + fileCounter.ToString() + indexStart.Extension, bytes);
                            fileCounter++;
                        }
                    }
                    while (indexStart != null && indexEnd != -1);
                }
            }
            IsWorking = false;
            ByteWorker.ReportProgress(100);
        }

        private ByteIndex? FindFirstPattern(FileStream fileStream, BackgroundWorker worker)
        {
            int byteValue = -1;
            do
            {
                byteValue = fileStream.ReadByte();
                ByteIndex += 1;
                for (int a = 0; a < Patterns.Count; a++)
                {
                    for(int b = 0; b < Patterns[a].Start.Length; b++)
                    {
                        if(byteValue == Patterns[a].Start[b])
                        {
                            if(b == Patterns[a].Start.Length - 1)
                            {
                                var byteIndex = new ByteIndex((int)fileStream.Position - Patterns[a].Start.Length, Patterns[a].Extension);
                                return byteIndex;
                            }
                            else
                            {
                                byteValue = fileStream.ReadByte();
                                ByteIndex += 1;
                            }
                        }
                        else
                        {
                            fileStream.Seek(-b, SeekOrigin.Current);
                            ByteIndex -= (ulong)b;
                            break;
                        }
                    }
                }
            }
            while ((byteValue != -1) && (Patterns.Count > 0));
            return null;
        }

        private int FindLastPattern(FileStream fileStream, BackgroundWorker worker)
        {
            int byteValue = -1;
            do
            {
                byteValue = fileStream.ReadByte();
                ByteIndex += 1;
                for (int a = 0; a < Patterns.Count; a++)
                {
                    for (int b = 0; b < Patterns[a].End.Length; b++)
                    {
                        if (byteValue == Patterns[a].End[b])
                        {
                            if (b == Patterns[a].End.Length - 1)
                            {
                                return (int)fileStream.Position;
                            }
                            else
                            {
                                byteValue = fileStream.ReadByte();
                                ByteIndex += 1;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            while ((byteValue != -1) && (Patterns.Count > 0));
            return -1;
        }

    }
}

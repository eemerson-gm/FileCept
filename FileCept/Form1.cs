using FileCept.Classes;
using System.ComponentModel;

namespace FileCept
{
    public partial class Form1 : Form
    {
        public string destDirectory = Environment.CurrentDirectory + "/results/";
        public Form1()
        {
            InitializeComponent();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(destDirectory);
                foreach(FileInfo file in directoryInfo.GetFiles())
                {
                    file.Delete();
                }
                var byteMapper = new ByteMapper(folderBrowserDialog.SelectedPath, destDirectory);
                byteMapper.AddPattern("89 50 4E 47 0D 0A 1A 0A", "49 45 4E 44 AE 42 60 82");
                var worker = new BackgroundWorker();
                byteMapper.Start(worker);
            }
        }
    }
}
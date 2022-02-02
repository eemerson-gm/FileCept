using FileCept.Classes;
using System.ComponentModel;
using System.Diagnostics;

namespace FileCept
{
    public partial class Form1 : Form
    {
        public string formDirectory;
        public ByteMapper byteMapper;
        public List<ByteEntry> Entries;
        public BackgroundWorker? backgroundWorker;
        public Form1()
        {
            InitializeComponent();
            formDirectory = Environment.CurrentDirectory + "\\output\\";
            Entries = new List<ByteEntry>();
            byteMapper = null;
            backgroundWorker = null;
            listViewEntires.View = View.Details;
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            progressBar.Value = 0;
            labelStatus.Text = "Running...";
            labelStatus.ForeColor = Color.Orange;
            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
            {
                if (!Directory.Exists(formDirectory))
                {
                    Directory.CreateDirectory(formDirectory);
                }
                DirectoryInfo directoryInfo = new DirectoryInfo(formDirectory);
                foreach(FileInfo file in directoryInfo.GetFiles())
                {
                    file.Delete();
                }
                byteMapper = new ByteMapper(folderBrowserDialog.SelectedPath, formDirectory);
                byteMapper.Entries = Entries.ToList();
                backgroundWorker = new BackgroundWorker();
                backgroundWorker.ProgressChanged += updateProgress;
                backgroundWorker.RunWorkerCompleted += completedProgress;
                byteMapper.Start(backgroundWorker);
            }
        }

        private void resultsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", @formDirectory);
        }

        private void updateProgress(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = e.ProgressPercentage;
        }

        private void completedProgress(object sender, RunWorkerCompletedEventArgs e)
        {
            labelStatus.Text = "Complete!";
            labelStatus.ForeColor = Color.Green;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            if(backgroundWorker != null)
            {
                backgroundWorker.CancelAsync();
                backgroundWorker = null;
            }
        }

        private void AddItem(ByteEntry entry)
        {
            Entries.Add(entry);
            listViewEntires.Items.Add(new ListViewItem(new string[] { entry.Type, entry.Extension, entry.StartToString(), entry.EndToString() }));
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            AddItem(new ByteEntry(textBoxStart.Text, textBoxEnd.Text, textBoxName.Text, textBoxExtension.Text));
        }
    }
}
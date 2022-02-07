using FileCept.Classes;
using System.ComponentModel;
using System.Diagnostics;

namespace FileCept
{
    public partial class Form1 : Form
    {

        public string SettingsPath;
        public string ResultsDirectory;
        public List<BytePattern> PatternList;
        public ByteMapper? ByteMapperCopy;
        public BackgroundWorker? BackgroundWorkerCopy;
        public ByteSettings SettingsFile;


        public Form1()
        {
            InitializeComponent();
            ResultsDirectory = Environment.CurrentDirectory + "\\results\\";
            SettingsPath = Environment.CurrentDirectory + "\\Settings.txt";
            PatternList = new List<BytePattern>();
            SettingsFile = new ByteSettings(SettingsPath);
            ByteMapperCopy = null;
            BackgroundWorkerCopy = null;
            listViewEntires.View = View.Details;

            if (File.Exists(SettingsPath))
            {
                foreach (BytePattern pattern in SettingsFile.ReadSettings())
                {
                    AddItem(pattern);
                }
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            progressBar.Value = 0;
            labelStatus.Text = "Running...";
            labelStatus.ForeColor = Color.Orange;

            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    if (!Directory.Exists(ResultsDirectory))
                    {
                        Directory.CreateDirectory(ResultsDirectory);
                    }
                    DirectoryInfo directoryInfo = new DirectoryInfo(ResultsDirectory);
                    foreach (FileInfo file in directoryInfo.GetFiles())
                    {
                        file.Delete();
                    }
                    ByteMapperCopy = new ByteMapper(folderBrowserDialog.SelectedPath, ResultsDirectory);
                    ByteMapperCopy.Patterns = PatternList.ToList();
                    BackgroundWorkerCopy = new BackgroundWorker();
                    BackgroundWorkerCopy.ProgressChanged += updateProgress;
                    BackgroundWorkerCopy.RunWorkerCompleted += completedProgress;
                    ByteMapperCopy.Start(BackgroundWorkerCopy);
                }
            }

        }

        private void resultsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("explorer.exe", @ResultsDirectory);
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
            if(BackgroundWorkerCopy != null)
            {
                BackgroundWorkerCopy.CancelAsync();
                BackgroundWorkerCopy = null;
            }
        }

        private void AddItem(BytePattern entry)
        {
            PatternList.Add(entry);
            listViewEntires.Items.Add(new ListViewItem(new string[] { entry.Type, entry.Extension, entry.StartToString(), entry.EndToString() }));
        }

        private void buttonAdd_Click(object sender, EventArgs e)
        {
            var entry = new BytePattern(textBoxStart.Text, textBoxEnd.Text, textBoxName.Text, textBoxExtension.Text);
            AddItem(entry);
            SettingsFile.AddSetting(entry);
        }
    }
}
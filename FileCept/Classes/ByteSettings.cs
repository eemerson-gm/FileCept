using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileCept.Classes
{
    public class ByteSettings
    {
        private string SettingsPath;

        public ByteSettings(string path)
        {
            SettingsPath = path;
        }

        public void AddSetting(BytePattern pattern)
        {
            using (StreamWriter streamWriter = File.AppendText(SettingsPath))
            {
                streamWriter.WriteLine(pattern.Type + "," + pattern.Extension + "," + pattern.StartToString() + "," + pattern.EndToString());
            }
        }

        public BytePattern[] ReadSettings()
        {
            string[] lines = File.ReadAllLines(SettingsPath);
            BytePattern[] patterns = new BytePattern[lines.Length];

            int index = 0;
            foreach(string pattern in lines)
            {
                string[] info = pattern.Split(",");
                patterns[index] = new BytePattern(info[2], info[3], info[0], info[1]);
                index++;
            }

            return patterns;
        }

    }
}

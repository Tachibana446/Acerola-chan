using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SinobigamiBot
{
    public class Yomiage
    {
        public int VoiceIndex = 0;
        public int Speed = 100;
        public int Volume = 80;

        public string SaveFilePath = "./yomiage.wav";

        private Setting Setting { get; set; }

        private Process process = null;

        public Yomiage(Setting setting)
        {
            Setting = setting;
        }

        public async Task Speak(string text, bool isSave = false)
        {
            await Task.Run(() =>
           {
               var fileInfo = new System.IO.FileInfo(Setting.YomiagePath);
               if (!fileInfo.Exists) return;
               var command = $" /T:{VoiceIndex} /S:{Speed} /V:{Volume}";
               if (isSave) command += $" /R:{SaveFilePath}";
               command += $" /W:{text}";
               if (process != null) process.WaitForExit(); // これがないと複数呼ばれた時同時に喋りだしてやばい
               process = Process.Start(fileInfo.FullName, command);
           });
        }
    }
}

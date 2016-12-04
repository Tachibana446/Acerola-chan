using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SinobigamiBot
{
    public class IniFile
    {
        private static bool Debug = true;

        private static string DefaultFilePath { get; } = "./setting.ini";
        private FileInfo File { get; set; } = new FileInfo(DefaultFilePath);
        private List<string> FileData = new List<string>();

        public IniFile(string _filePath = null)
        {
            if (_filePath != null)
                File = new FileInfo(_filePath);

            // ファイルがなければ通常時は例外　デバッグ時は生成して開く
            if (!File.Exists)
            {
                if (Debug)
                {
                    var sw = new System.IO.StreamWriter(File.FullName);
                    sw.WriteLine("");
                    sw.Close();
                    System.Diagnostics.Process.Start(File.FullName);
                }
                else
                {
                    throw new Exception("指定されたパスにiniファイルがありません");
                }
            }
            else
            {
                FileData = System.IO.File.ReadLines(File.FullName).ToList();
            }
        }

        /// <summary>
        /// 値の取得　
        /// </summary>
        /// <param name="section">セクション名のプロパティ</param>
        /// <param name="key">キー名のプロパティ</param>
        /// <returns>見つからなければnull</returns>
        public string GetValue(string section, string key)
        {
            bool inTargetSection = false;
            foreach (var line in FileData)
            {
                // Comment
                if (line.Trim().Count() == 0 || line.Trim().First() == ';')
                    continue;   
                // Section
                if (line.Trim().First() == '[')
                {
                    if (line.Trim() == $"[{section}]")
                        inTargetSection = true;
                    else
                        inTargetSection = false;
                }
                // Key=Value
                else if (inTargetSection && line.Split('=').First().Trim() == key)
                {
                    return line.Split('=')[1].Trim();
                }
            }
            return null;
        }

        public void OpenIniFile()
        {
            System.Diagnostics.Process.Start(File.FullName);
        }

        /*-------------------------------------------------------------------------------------
                  セクション・キー名
         *-------------------------------------------------------------------------------------*/

        public object TestSection, TestValue;

        /* ------------------------------------------------------------------------------------
        *         private で使うもろもろ
        * -------------------------------------------------------------------------------------*/

    }
}

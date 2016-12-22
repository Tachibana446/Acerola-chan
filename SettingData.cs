using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SinobigamiBot
{
    public class SettingData
    {
        public bool PlayDiceSE = true;
        /// <summary>
        /// プロットの表示時に自動的にプロットをリセットするか
        /// </summary>
        public bool ResetPlotOnShow = true;

        public Dictionary<string, string> Data = new Dictionary<string, string>();

        public static string filePath = "./data/setting.txt";

        public string Token { get; private set; }
        public string ClientId { get; private set; }

        /// <summary>
        /// 体力の初期値
        /// </summary>
        public int DefaultHP
        {
            get
            {
                if (!Data.Keys.Contains(nameof(DefaultHP))) return 6;
                return int.Parse(Data[nameof(DefaultHP)]);
            }
            set { Data[nameof(DefaultHP)] = value.ToString(); }
        }

        public string YomiagePath
        {
            get { return (string)GetData("yomiage", "./data/yomiage/softalkw.exe"); }
            set { SetData("yomiage", value); }
        }
        /// <summary>
        /// メッセージが来た時クライアントで読み上げるかどうか
        /// </summary>
        public bool IsYomiageMessage
        {
            get { return (bool)GetData(nameof(IsYomiageMessage), false); }
            set { SetData(nameof(IsYomiageMessage), value); }
        }
        /// <summary>
        /// メッセージを読み上げないユーザーのリスト
        /// </summary>
        public List<string> YomiageIgnoreUsers
        {
            get { return GetData(nameof(YomiageIgnoreUsers), "").ToString().Split(',').ToList(); }
            set { SetData(nameof(IsYomiageMessage), string.Join(",", value)); }
        }

        public SettingData()
        {
            Load();
        }

        public void Save()
        {
            var directory = System.IO.Path.GetDirectoryName(filePath);
            if (!System.IO.Directory.Exists(directory))
                System.IO.Directory.CreateDirectory(directory);

            var sw = new System.IO.StreamWriter(filePath);
            foreach (var key in Data.Keys)
            {
                sw.WriteLine($"{key}={Data[key]}");
            }
            sw.Close();
        }

        public void Load()
        {
            if (!System.IO.File.Exists(filePath)) return;
            foreach (var line in System.IO.File.ReadLines(filePath))
            {
                var sp = line.Split('=').Select(s => s.Trim()).ToArray();
                if (sp.Length != 2) continue;
                if (Data.Keys.Contains(sp[0]))
                    Data[sp[0]] = sp[1];
                else
                    Data.Add(sp[0], sp[1]);
            }

            // bool
            if (Data.Keys.Contains(nameof(PlayDiceSE))) PlayDiceSE = bool.Parse(Data[nameof(PlayDiceSE)]);
            if (Data.Keys.Contains(nameof(ResetPlotOnShow))) ResetPlotOnShow = bool.Parse(Data[nameof(ResetPlotOnShow)]);
            // string 
            if (Data.Keys.Contains(nameof(Token))) Token = Data[nameof(Token)];
            if (Data.Keys.Contains(nameof(ClientId))) ClientId = Data[nameof(ClientId)];

        }

        /// <summary>
        /// Dataからデータを取得
        /// 無かった時のデフォルト値と、その時デフォルト値をファイルに保存するか
        /// </summary>
        /// <param name="key"></param>
        /// <param name="_default"></param>
        /// <param name="writeOnDefault">キーが無かった時デフォルト値をファイルに保存するか</param>
        /// <returns></returns>
        private object GetData(string key, object _default, bool writeOnDefault = true)
        {
            if (Data.Keys.Contains(key))
            {
                return UserInfo.ParseStatus(Data[key]);
            }
            else
            {
                if (writeOnDefault) SetData(key, _default);
                return _default;
            }
        }

        private void SetData(string key, object value)
        {
            if (Data.Keys.Contains(key))
                Data[key] = value.ToString();
            else
                Data.Add(key, value.ToString());
            Save();
        }
    }
}

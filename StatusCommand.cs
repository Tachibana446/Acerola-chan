using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SinobigamiBot
{
    /// <summary>
    /// ステータス操作に関するコマンド
    /// </summary>
    public class StatusCommand
    {
        private const string HelpFilePath = "./data/usage-status.txt";

        private ServerData Server { get; set; }
        private UserInfo user;

        public StatusCommand(ServerData server)
        {
            Server = server;
        }

        /// <summary>
        /// #ステータス arg のargの部分を渡すことで適切な処理を行う。
        /// 文末に顔文字を付けて返す
        /// </summary>
        /// <param name="argText"></param>
        /// <returns></returns>
        public string ExcuteWithKaomoji(string argText)
        {
            string result = Excute(argText);
            Server.SavePlayersInfo();
            bool success = false;
            if (Regex.IsMatch(result, @"設定したよ|削除したよ"))
                success = true;
            if (success)
            {
                return result + gradKaomoji.Split('\n').ToList().Sample();
            }
            else
            {
                return result + sadKaomoji.Split('\n').ToList().Sample();
            }
        }

        /// <summary>
        /// #ステータス arg　のargの部分を渡すことで適切な処理を行う
        /// </summary>
        /// <param name="argText"></param>
        /// <returns></returns>
        private string Excute(string argText)
        {
            var args = argText.ToNarrow().Trim().Split(' ').ToList();
            args.RemoveAll(s => s == "");

            if (args.Count == 0) return "引数を指定してね";

            // 第一引数がヘルプ
            if (args[0] == "help" || args[0] == "usage")
            {
                return Help();
            }
            if (args.Count >= 2)
            {
                try
                {
                    user = Server.GetMatchPlayer(args[0]);
                }
                catch (Exception e)
                {
                    return e.Message;
                }
                argText = argText.Substring(args[0].Length).Trim();
                if (Regex.IsMatch(argText, @".+(\+=|-=).+"))
                    return AddStatus(argText);
                if (Regex.IsMatch(argText, @".+=.+"))
                    return SetStatus(argText);
                if (Regex.IsMatch(argText, @".+\s+削除"))
                    return RemoveStatus(argText);
                if (Regex.IsMatch(argText, @"(.+)(\+\+|--)"))
                    return IncrementStatus(argText);
            }
            return $"{argText}に一致する操作はないよ。helpを呼んでね";
        }

        /// <summary>
        /// += / -=
        /// </summary>
        /// <param name="argText"></param>
        /// <returns></returns>
        private string AddStatus(string argText)
        {
            var match = Regex.Match(argText, @"(.+)(\+=|-=)(.+)");
            var key = match.Groups[1].Value.Trim();
            var opperation = match.Groups[2].Value.Trim();
            var valStr = match.Groups[3].Value.Trim();
            try
            {
                GetStatus(key);
            }
            catch (Exception e)
            {
                return e.Message;
            }
            int val = 0;
            try
            {
                val = int.Parse(valStr);
            }
            catch (Exception)
            {
                return $"整数を与えてね！{valStr}は整数じゃないよ！";
            }
            if (!user.Status[key].IsInt())
            {
                return $"{key}は整数じゃないから{opperation}は使えないよ";
            }
            if (opperation == "-=") val *= -1;
            user.Status[key] = (int)user.Status[key] + val;
            return $"{key}を{user.Status[key]}に設定したよ。";
        }

        /// <summary>
        /// key 削除
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        private string RemoveStatus(string v)
        {
            var sp = v.Split(' ').Select(s => s.Trim()).ToArray();
            try
            {
                GetStatus(sp[0]);
            }
            catch (Exception e)
            {
                return e.Message;
            }
            user.Status.Remove(sp[0]);
            return $"{sp[0]}を削除したよ";
        }

        /// <summary>
        /// key=valueで設定
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        private string SetStatus(string v)
        {
            var sp = v.Split('=').Select(s => s.Trim()).ToArray();
            try
            {
                GetStatus(sp[0]);
            }
            catch (Exception e)
            {
                user.Status.Add(sp[0], UserInfo.ParseStatus(sp[1]));
            }
            user.Status[sp[0]] = UserInfo.ParseStatus(sp[1]);
            return $"{sp[0]}を{user.Status[sp[0]]}に設定したよ";
        }

        /// <summary>
        /// ステータスをインクリメント
        /// </summary>
        /// <param name="key"></param>
        /// <param name="operation">++/--</param>
        /// <returns></returns>
        private string IncrementStatus(string argText)
        {
            var match = Regex.Match(argText, @"(.+)(\+\+|--)");
            var key = match.Groups[1].Value.Trim();
            var operation = match.Groups[2].Value;

            try
            {
                GetStatus(key);
            }
            catch (Exception e)
            {
                return e.Message;
            }

            if (!user.Status[key].IsInt())
                return $"{user.Status[key].GetType().ToString()}型に{operation}は使えないよ";
            int add = operation == "++" ? 1 : -1;
            user.Status[key] = int.Parse(user.Status[key].ToString()) + add;
            return $"{key}を{user.Status[key]}に設定したよ";
        }

        /// <summary>
        /// Status[key]を取得。キーが存在しない場合は例外
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        private object GetStatus(string key)
        {
            if (!user.Status.Keys.Contains(key))
                throw new Exception(key + "というステータスはないよ");
            return user.Status[key];
        }

        /// <summary>
        /// ヘルプファイルを読み込み、返す
        /// </summary>
        /// <returns></returns>
        private string Help()
        {
            if (System.IO.File.Exists(HelpFilePath))
            {
                var lines = System.IO.File.ReadLines(HelpFilePath);
                return string.Join("\n", lines);
            }
            return $"エラー：ヘルプファイル:{HelpFilePath}が見つかりませんでした";

        }

        private static string sadKaomoji = "(｡>д<｡)\nヾ(｡>﹏<｡)ﾉ\n(ﾟﾉ´Д`ﾟ)ﾉﾟ\n( ´・ω・｀)";
        private static string gradKaomoji = "٩(๑•̀ω•́๑)۶\n╭(๑•̀ㅂ•́)و \n(｡☌ᴗ☌｡)";
    }
}

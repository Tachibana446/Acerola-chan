using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SinobigamiBot
{
    static class Extensions
    {
        /// <summary>
        /// ランダムに一つ取り出す
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="self"></param>
        /// <returns></returns>
        public static T Sample<T>(this List<T> self)
        {
            var rand = new Random();
            return self[rand.Next(self.Count)];
        }

        /// <summary>
        /// 先頭のアイテムをポップ（返して削除）
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static T Pop<T>(this List<T> list)
        {
            var res = list[0];
            list.RemoveAt(0);
            return res;
        }


        /// <summary>
        /// 文字を中央揃え
        /// </summary>
        /// <param name="input"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static string ToCenter(this string input, int size, string pad = "　")
        {
            if (input.Length >= size) return input;
            int diff = size - input.Length;
            int right = diff / 2;
            int left = diff - right;
            string result = "";
            for (int i = 0; i < left; i++) result += pad;
            result += input;
            for (int i = 0; i < right; i++) result += pad;
            return result;
        }

        static string wide = "　１２３４５６７８９０－ａｂｃｄｅｆｇｈｉｊｋｌｍｎｏｐｑｒｓｔｕｖｗｘｙｚＡＢＣＤＥＦＧＨＩＪＫＬＭＮＯＰＱＲＳＴＵＶＷＸＹＺ＝＋！”＃＄％＆’（）";
        static string narrow = " 1234567890-abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ=+!\"#$%&'()";


        /// <summary>
        /// 全角英数字を半角に
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static string ToNarrow(this string input)
        {

            string output = "";
            foreach (var c in input)
            {
                int index = wide.IndexOf(c);
                if (index >= 0)
                    output += narrow[index];
                else
                    output += c;
            }
            return output;
        }

        public static string ToWide(this string input)
        {
            string output = "";
            foreach (var c in input)
            {
                int index = narrow.IndexOf(c);
                if (index >= 0)
                    output += wide[index];
                else
                    output += c;
            }
            return output;
        }

        public static bool IsInt(this object obj)
        {
            return obj.GetType() == typeof(int);
        }

        public static bool IsString(this object obj)
        {
            return obj.GetType() == typeof(string);
        }

        public static bool IsBool(this object obj)
        {
            return obj.GetType() == typeof(bool);
        }
    }
}

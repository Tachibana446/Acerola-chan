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
    }
}

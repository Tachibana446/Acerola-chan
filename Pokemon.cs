using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using System.Text.RegularExpressions;

namespace SinobigamiBot
{
    class Pokemon
    {
        public async Task Do(MessageEventArgs e)
        {
            await PokeList(e);
        }

        /// <summary>
        /// パターンに一致するとtrueを返す
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task<bool> PokeList(MessageEventArgs e)
        {
            if (e.Message.IsAuthor || !e.Message.IsMentioningMe() || !System.IO.File.Exists("data/pokemon/list.txt"))
                return false;
            var list = System.IO.File.ReadLines("data/pokemon/list.txt");
            int i = 0;
            foreach (var poke in list)
            {
                i++;
                if (Regex.IsMatch(e.Message.Text, poke))
                {
                    await e.Channel.SendMessage(e.User.Mention + $" http://yakkun.com/sm/zukan/n{i}");
                    return true;
                }
            }
            return false;
        }

    }
}

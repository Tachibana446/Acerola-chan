using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SinobigamiBot
{
    /// <summary>
    /// 一般的な会話
    /// </summary>
    class GeneralTalk
    {
        public async Task Do(MessageEventArgs e)
        {
            await YouAreWelcome(e);
        }

        private async Task YouAreWelcome(MessageEventArgs e)
        {
            if (e.Message.IsAuthor || !e.Message.IsMentioningMe()) return;
            if (Regex.IsMatch(e.Message.Text.ToNarrow(), @"ありがとう|(T|t)hank you|サンクス|さんきゅー|サンキュー"))
            {
                await e.Channel.SendMessage(e.User.Mention + " どういたしまして！");
            }
        }
    }
}

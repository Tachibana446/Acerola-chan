using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace SinobigamiBot
{
    class UserInfo
    {
        public UserInfo(User user)
        {
            User = user;
        }

        public User User { get; private set; }
        /// <summary>
        /// ユーザーに対して抱いている感情
        /// </summary>
        public Tuple<User, Emotion> Emotions { get; private set; }
        // TODO: 持っている秘密など
    }

    class Emotion
    {
        public string Name { get; private set; }
        public EmotionType Type { get; private set; }
    }

    enum EmotionType
    {
        plus, minus, none
    }
}

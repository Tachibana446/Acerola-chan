using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SinobigamiBot
{
    enum Operation
    {
        /// <summary>
        /// 感情の選択を出して入力待機状態
        /// </summary>
        EmotionChoice,
        /// <summary>
        /// 感情を取得した場合それをなかったことに
        /// </summary>
        GetEmotion,
        /// <summary>
        /// プロットを決めた場合なかったことに
        /// </summary>
        SetPlot,
        /// <summary>
        /// プロットのリセットをなかったことに
        /// </summary>
        ResetPlot,

        None
    }

}

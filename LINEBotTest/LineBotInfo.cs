using System;
using System.Collections.Generic;
using System.Text;

namespace ihoseco
{
    public class LineBotInfo
    {
        public LineBotInfo()
        {
            registdate = Utility.GetJapanseseNow();
        }
        /// <summary>
        /// LINEの1対1トーク、トークルーム、グループのID
        /// </summary>
        public string? lineid { get; set; }
        /// <summary>
        /// 1対1トーク:user トークルーム:room グループ:group
        /// </summary>
        public string? linetype { get; set; }
        /// <summary>
        /// このセンサーが登録されたローカル日時
        /// </summary>
        public DateTime registdate { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace ihoseco
{
    public class LineBotEx : DBCommon
    {
        public LineBotEx() : base(ContainerType.linebot, "0000000")
        {
        }
        /// <summary>
        /// LineBotコンテナーのsensoridにはlineidが格納されている。
        /// </summary>
        /// <param name="containertype"></param>
        /// <param name="lineid"></param>
        public LineBotEx(string lineid) : base(ContainerType.linebot, lineid)
        {
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
        /// 揺れ検出時の通知タイプ　0:リアルタイムにLINE Notifyに揺れを通知（デフォルト）1:6時、22時にまとめて通知
        /// </summary>
        public int notifytype { get; set; }
        /// <summary>
        /// 見回りタイプ　0:見回りしてくださいを通知しない（デフォルト） 1:72時間(3日)以上見回りしないと通知する。
        /// </summary>
        public int patroltype { get; set; }
        /// <summary>
        /// 最後に見回りを実施した日時
        /// </summary>
        public DateTime lastpatroldatetime { get; set; }
        /// <summary>
        /// このLINEに登録されているセンサーIDのリスト
        /// </summary>
        public List<RegistSensorInfo>? registsensorlist { get; set; }
        /// <summary>
        /// このLINEに登録されているLINE Notify通知先リスト
        /// </summary>
        public List<LineNotifyBotInfo>? linenotifylist { get; set; }
    }
    public class RegistSensorInfo
    {
        public string sensorid { get; set; }
        public string nickname { get; set; }
        public DateTime registdatetime { get; set; }
    }
    public class PatrolSensorInfo
    {
        public string sensorid { get; set; }
        public string? nickname { get; set; }
        public DateTime lastaccelerometerdatetime { get; set; }
        public PatrolSensorInfo (string sensorid, string? sensorname, DateTime lastaccelerometer)
        {
            this.sensorid = sensorid;
            this.nickname = sensorname;
            this.lastaccelerometerdatetime = lastaccelerometer;
        }
    }
    public class PatrolSensorInfoData
    {
        public List<PatrolSensorInfo> accelsensorlist { get; set; }
        public int countaccelerometer { get; set; }
        public PatrolSensorInfoData()
        {
            accelsensorlist = new List<PatrolSensorInfo>();
            countaccelerometer = 0;
        }
    }
    public class LineNotifyBotInfo
    {
        public string AccessToken { get; set; }
        public string Message { get; set; }
        public string TargetType { get; set; }
        public string Target { get; set; }
    }
}

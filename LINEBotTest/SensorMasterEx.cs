using System;
using System.Collections.Generic;
using System.Text;

namespace ihoseco
{
    /// <summary>
    /// Sigfox センサーマスタークラス
    /// </summary>
    public class SensorMasterEx : DBCommon
    {
        public enum SensorType
        {
            prod = 0,
            develop = 1,
            poc = 2
        }
        public enum SensorStatus
        {
            heartbeat1 = 0x00,                  //0x00
            heartbeat2 = 0x01,                  //0x01
            startofarmingbybutton = 0x10,       //0x10
            armedbybutton = 0x11,               //0x11
            automaticallyarmed = 0x12,          //0x12
            disarmedafterevent = 0x13,          //0x13
            disarmedbylongpress = 0x14,         //0x14
            downlinkconfirmationwithmore = 0x15,//0x15
            downlinkconfirmationwithnomore = 0x16,//0x16
            armedbyforcedarm = 0x17,            //0x17
            disarmedbyforceddisarm = 0x18,      //0x18
            startofmovement = 0x20,             //0x20
            endofmovement = 0x21,               //0x21
            tracingtag = 0x22,                  //0x22
            dropalert = 0x23,                   //0x23
            rotationalert = 0x24,               //0x24
            shortpress = 0x30,                  //0x30
            doubleclick = 0x31,                 //0x31
            longpress = 0x32,                   //0x32
            oddshortpress = 0x33,               //0x33
            evenshortpress = 0x34,              //0x34
            temperaturebelowthresholdA = 0x50,  //0x50
            temperatureabovethresholdA = 0x51,  //0x51
            temperaturebelowthresholdB = 0x52,  //0x52
            temperatureabovethresholdB = 0x53,  //0x53
            temperaturebelowthresholdC = 0x54,  //0x54
            temperatureabovethresholdC = 0x55,  //0x55
            temperaturedeltaalert = 0x56,       //0x56
            lightchangeon = 0x60,               //0x60
            lightchangeoff = 0x61,              //0x61
            reedchangeon = 0x62,                //0x62
            reedchangeoff = 0x63,               //0x63
            Extralongpress = 0x81,               //0x81 Extra long press (6 seconds)
        }

        public SensorMasterEx() : base(ContainerType.sensormaster, "0000000")
        {
            Initialize();
        }
        public SensorMasterEx(string sensorid) : base(ContainerType.sensormaster, sensorid)
        {
            Initialize();
        }
        public void Initialize()
        {
            isIoT = false;
            isAccelNotifyOff = false;
            isExpire = false;
            startIoTTransferDatetime = new DateTime(2020, 1, 1, 0, 0, 0);
            state = 0;
            lastvolt = 0;
            lastaccelerometerdatetime = new DateTime(2020, 1, 1, 0, 0, 0);
            lastheartbeatdatetime = new DateTime(2020, 1, 1, 0, 0, 0);
            lastpatroldatetime = new DateTime(2020, 1, 1, 0, 0, 0);
            lastheartbeatpushdatetime = new DateTime(2020, 1, 1, 0, 0, 0);
            expiredate = new DateTime(2020, 1, 1, 0, 0, 0);
            lastpatrolpushdatetime = new DateTime(2020, 1, 1, 0, 0, 0);
            deliverydatetime = new DateTime(2020, 1, 1, 0, 0, 0);
            customer = "----";
            pictureurl = null;
            longitude = null;
            latitude = null;
            address = null;
            sigfoxlog = new List<SimplePackPlus>();
            linebotinfolist = new List<LineBotInfo>();
        }
        public static SensorMasterEx.SensorStatus GetSensorStatus(string rawdata)
        {
            string cmd = rawdata.Substring(2, 2);  //ex. 0f220の20がほしい
            int value = Convert.ToInt32(cmd, 16);

            return (SensorMasterEx.SensorStatus)value;
        }
        public static string GetSensorModeString(string rawdata)
        {
            return rawdata.Substring(0, 2);
        }
        public static string GetSensorStatusString(string rawdata)
        {
            return rawdata.Substring(2, 2);
        }
        //ハートビートの時だけ呼び出す
        public static int GetSensorVoltage(string rawdata)
        {
            int value = -1;
            if (rawdata.Length >= 8)
            {
                string cmd = rawdata.Substring(6, 2);  //ex. 0200c02850の28がほしい(2.8V)
                value = Convert.ToInt32(cmd, 10);
            }
            return value;
        }
        //ハートビートの時だけ呼び出す
        public static int GetSensorTemperature(string rawdata)
        {
            int value = 0;
            if (rawdata.Length >= 10)
            {
                string cmd = rawdata.Substring(8, 2);//ex. 0200c02850の50がほしい(0度)
                value = Convert.ToInt32(cmd, 16);
            }
            return value;
        }
        //揺れ検出の時だけ呼び出す
        public static int GetSensorXAxis(string rawdata)
        {
            int value = 0;
            if (rawdata.Length >= 12)
            {
                string cmd = rawdata.Substring(6, 2);  //ex. 022020e3dfc3のe3がほしい
                value = Convert.ToInt32(cmd, 16);
            }

            return value;
        }
        //揺れ検出の時だけ呼び出す
        public static int GetSensorYAxis(string rawdata)
        {
            int value = 0;
            if (rawdata.Length >= 12)
            {
                string cmd = rawdata.Substring(8, 2);  //ex. 022020e3dfc3のdfがほしい
                value = Convert.ToInt32(cmd, 16);
            }
            return value;
        }
        //揺れ検出の時だけ呼び出す
        public static int GetSensorZAxis(string rawdata)
        {
            int value = 0;
            if (rawdata.Length >= 12)
            {
                string cmd = rawdata.Substring(10, 2);  //ex. 022020e3dfc3のc3がほしい
                value = Convert.ToInt32(cmd, 16);
            }
            return value;
        }
        /// <summary>
        /// PACコード(Ver1では未使用)
        /// </summary>
        public string? pac { get; set; }
        /// <summary>
        /// ケーブルネTV業界IoTダッシュボードへのペイロード転送フラグ
        /// </summary>
        public bool isIoT { get; set; }
        /// <summary>
        /// 有効期限切れフラグ false:有効センサー true:期限切れ
        /// </summary>
        public bool isExpire { get; set; }
        /// <summary>
        /// 揺れ検出通知Offフラグ false:揺れを通知する true:揺れを通知しない
        /// </summary>
        public bool isAccelNotifyOff { get; set; }
        /// <summary>
        /// ケーブルネTV業界IoTダッシュボードへのペイロード転送開始日時
        /// </summary>
        public DateTime startIoTTransferDatetime { get; set; }
        /// <summary>
        /// 最終加速度センサー反応データ受信日時
        /// </summary>
        public DateTime lastaccelerometerdatetime { get; set; }
        /// <summary>
        /// 最終ハートビート受信日時
        /// </summary>
        public DateTime lastheartbeatdatetime { get; set; }
        /// <summary>
        /// 最後にハートビート未受信メッセージを通知した日時
        /// </summary>
        public DateTime lastheartbeatpushdatetime { get; set; }
        /// <summary>
        /// このセンサーのパトロールを最後に実行した日時
        /// </summary>
        public DateTime lastpatroldatetime { get; set; }
        /// <summary>
        /// 最後にパトロール未実行を通知した日時
        /// </summary>
        public DateTime lastpatrolpushdatetime { get; set; }
        /// <summary>
        /// このセンサーの有効期限
        /// </summary>
        public DateTime expiredate { get; set; }
        /// <summary>
        /// このセンサーの出荷日
        /// </summary>
        public DateTime deliverydatetime { get; set; }
        /// <summary>
        /// 最後に受信したハートビートメッセージに付与されていたVoltデータの10倍値（3.1Vの場合は31となる）
        /// </summary>
        public int lastvolt { get; set; }
        /// <summary>
        /// センサーのタイプ
        /// </summary>
        public int sensortype { get; set; }
        /// <summary>
        /// sigofoxログ
        /// </summary>
        public List<SimplePackPlus> sigfoxlog { get; set; }
        /// <summary>
        /// このセンサーが登録されているLINEユーザー、トーク、グループのリスト
        /// </summary>
        public List<LineBotInfo> linebotinfolist { get; set; }
        /// <summary>
        /// センサー名称（初期値はセンサーID）
        /// </summary>
        public string? sensorname { get; set; }
        /// <summary>
        /// 写真やイラストの登録用URL(空文字の時は未登録)
        /// </summary>
        public string? pictureurl { get; set; }
        /// <summary>
        /// Blobに格納するファイル名
        /// </summary>
        public string? picturefilename { get; set; }
        /// <summary>
        /// Blobに格納するサムネールファイル名
        /// </summary>
        public string? picturethumbnailfilename { get; set; }
        /// <summary>
        /// 緯度
        /// </summary>
        public string? longitude { get; set; }
        /// <summary>
        /// 経度
        /// </summary>
        public string? latitude { get; set; }
        /// <summary>
        /// 住所
        /// </summary>
        public string? address { get; set; }
        /// <summary>
        /// ダウンリンク用ステータス
        /// </summary>
        public int state { get; set; }
        /// <summary>
        /// このセンサーのダウンリンクデータ1
        /// </summary>
        public List<string> downlink { get; set; }
        /// <summary>
        /// このセンサーの出荷先（エンドユーザー、わからない場合は販売代理店を入れておく）
        /// </summary>
        public string? customer { get; set; }
        /// <summary>
        /// 販売代理店　どのセンサーをどこに販売したかを管理する。
        /// </summary>
        public string? distributor { get; set; }
        /// <summary>
        /// 予備1 (string)
        /// </summary>
        public string? reservestr1 { get; set; }
        /// <summary>
        /// 予備2 (string)
        /// </summary>
        public string? reservestr2 { get; set; }
        /// <summary>
        /// 予備3 (string)
        /// </summary>
        public string? reservestr3 { get; set; }
        /// <summary>
        /// 予備1 (int)
        /// </summary>
        public int? reserveint1 { get; set; }
        /// <summary>
        /// 予備2 (int)
        /// </summary>
        public int? reserveint2 { get; set; }
        /// <summary>
        /// 予備3 (int)
        /// </summary>
        public int? reserveint3 { get; set; }
    }
    public class SensorLog
    {
        /// <summary>
        /// 主キー(センサーID)
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// センサーID
        /// </summary>
        public string sensorid { get; set; }
        /// <summary>
        /// sigofoxログ
        /// </summary>
        public List<SimplePackPlus> sigfoxlog { get; set; }
        /// <summary>
        /// レコード更新日時
        /// </summary>
        public DateTime updateddatetime { get; set; }
        /// <summary>
        /// レコード登録日時
        /// </summary>
        public DateTime createddatetime { get; set; }

    }
}
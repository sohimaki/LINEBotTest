using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using TimeZoneConverter;

namespace ihoseco
{
    public class Utility
    {
        public const int NearAccelHour = 24;
        public const int MaxRegistSensorCount = 100;
        public enum AddSensorStatus
        {
            success = 0,
            sensormasternotfound = 1,
            sensoronly1talk = 2,
            sensoronly1group = 3,
            sensoronly1 = 4,
            sensorduplicate = 5,
            sensorcountover = 6,
            linebotnotexist = -1,
            internalexception = -2,

        }

        public static string messageON()
        {
            return "センサーを登録しました。";
        }
        public static string messageONERROR()
        {
            return "センサーの登録でエラーが発生しました。";
        }
        public static string messageAlreadyON()
        {
            return "このセンサーは登録済みです。";
        }
        public static string messageMaxON()
        {
            return "センサーは10個しか登録できません。";
        }
        public static string messageERROR()
        {
            return "センサーの登録でエラーが発生しました。";
        }
        public static string messageNOTON()
        {
            return "センサーが登録されていません。";
        }
        public static string messageReAction()
        {
            return "登録されているセンサーに反応があったようです。\n見回りを実行しセンサーを確認してください。";
        }
        public static string messageDelete()
        {
            return "解除するセンサーを指定してください。";
        }
        public static string messageSensorDeleted()
        {
            return "センサーを解除しました。";
        }
        public static string messageDeleteError()
        {
            return "センサーの解除に失敗しました。";
        }
        public static string messageNOTNotifyGroup()
        {
            return "グループでは通知設定は出来ません。\n1:1のトークで通知設定してください。";
        }
        public static string GetMessageWithAddSensorMessage(AddSensorStatus status)
        {
            string msg = "センサーを登録しました。";

            switch (status)
            {
                case AddSensorStatus.success:
                    msg =  "センサーを登録しました。";
                    break;
                case AddSensorStatus.sensormasternotfound:
                    msg = "アイホスエコのセンサーではありません。";
                    break;
                case AddSensorStatus.sensoronly1talk:
                    msg = "このセンサーはトークに登録済みです。";
                    break;
                case AddSensorStatus.sensoronly1group:
                    msg = "このセンサーはグループに登録済みです。該当センサーをOFFしてから再度登録してください。";
                    break;
                case AddSensorStatus.sensoronly1:
                    msg = "このセンサーは他のトークまたはグループに登録済みです。登録済みセンサーをOFFしてから再度登録してください。";
                    break;
                case AddSensorStatus.sensorduplicate:
                    msg = "このセンサーは登録済みです。";
                    break;
                case AddSensorStatus.sensorcountover:
                    msg = string.Format("トークまたはグループに、センサーは{0}個しか登録できません。", Utility.MaxRegistSensorCount.ToString());
                    break;
                case AddSensorStatus.linebotnotexist:
                case AddSensorStatus.internalexception:
                    msg = "アイホスエコでエラーが発生しました";
                    break;
            }
            return msg;
        }
        public static DateTime GetJapanseseNow()
        {
            //TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
            //DateTime utc = DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
            //// 現在日時UTC
            //DateTime dtnow = TimeZoneInfo.ConvertTimeFromUtc(utc, tzi);
            return GetJapanseseTime(DateTime.UtcNow);
        }
        public static DateTime GetJapanseseTime(DateTime utctime)
        {
            var jstZoneInfo = TZConvert.GetTimeZoneInfo("Tokyo Standard Time");
            DateTime dateTime = DateTime.SpecifyKind(utctime, DateTimeKind.Unspecified);
            dateTime = TimeZoneInfo.ConvertTimeFromUtc(dateTime, jstZoneInfo);

            //TimeZoneInfo tzi = TimeZoneInfo.FindSystemTimeZoneById("Tokyo Standard Time");
            //DateTime utc = DateTime.SpecifyKind(utctime, DateTimeKind.Unspecified);
            //// 現在日時UTC
            //DateTime dtnow = TimeZoneInfo.ConvertTimeFromUtc(utc, tzi);

            return dateTime;
        }
        /// <summary>
        /// 半角数字を全角数字に変換する。
        /// </summary>
        /// <param name="s">変換する文字列</param>
        /// <returns>変換後の文字列</returns>
        public static string HanToZenNum(string s)
        {
            return Regex.Replace(s, "[0-9]", p => ((char)(p.Value[0] - '0' + '０')).ToString());
        }

        /// <summary>
        /// 全角数字を半角数字に変換する。
        /// </summary>
        /// <param name="s">変換する文字列</param>
        /// <returns>変換後の文字列</returns>
        public static string ZenToHanNum(string s)
        {
            return Regex.Replace(s, "[０-９]", p => ((char)(p.Value[0] - '０' + '0')).ToString());
        }

        /// <summary>
        /// 半角アルファベットを全角アルファベットに変換する。
        /// </summary>
        /// <param name="s">変換する文字列</param>
        /// <returns>変換後の文字列</returns>
        public static string HanToZenAlpha(string s)
        {
            var str = Regex.Replace(s, "[a-z]", p => ((char)(p.Value[0] - 'a' + 'ａ')).ToString());

            return Regex.Replace(str, "[A-Z]", p => ((char)(p.Value[0] - 'A' + 'Ａ')).ToString());
        }

        /// <summary>
        /// 全角アルファベットを半角アルファベットに変換する。
        /// </summary>
        /// <param name="s">変換する文字列</param>
        /// <returns>変換後の文字列</returns>
        public static string ZenToHanAlpha(string s)
        {
            var str = Regex.Replace(s, "[ａ-ｚ]", p => ((char)(p.Value[0] - 'ａ' + 'a')).ToString());

            return Regex.Replace(str, "[Ａ-Ｚ]", p => ((char)(p.Value[0] - 'Ａ' + 'A')).ToString());
        }
        /// <summary>
        /// Windows形式の日付時刻をUnix形式に変換する。
        /// </summary>
        /// <param name="dt">Windows形式の日付時刻</param>
        /// <returns>Unix形式のlong型日付時刻</returns>
        public static long ToUnixTime(DateTime dt)
        {
            var dto = new DateTimeOffset(dt.Ticks, new TimeSpan(+09, 00, 00));
            return dto.ToUnixTimeSeconds();
        }
        /// <summary>
        /// Unix形式の日付時刻をWindows形式に変換する。
        /// </summary>
        /// <param name="dt">Unix形式のlong型日付時刻</param>
        /// <returns>Windows形式のDateTime型日付時刻</returns>
        public static DateTime FromUnixTime(long unixTime)
        {
            return DateTimeOffset.FromUnixTimeSeconds(unixTime).LocalDateTime;
        }
        public static bool IsDifferenceGreaterThanHours(DateTime currentDateTime, DateTime lastDateTime, int hour)
        {
            if(currentDateTime < lastDateTime)
            {
                return false;
            }
            TimeSpan difference = currentDateTime - lastDateTime;
            return difference.TotalHours < hour;
        }
        public static string GetEnvironmentVariable(string key, string defaultvalue=null)
        {
            string? env_value = Environment.GetEnvironmentVariable(key);
            if (string.IsNullOrWhiteSpace(env_value))
            {
                return defaultvalue;
            }
            return env_value;
        }
        public static int GetEnvironmentVariableInt(string key, int defaultvalue)
        {
            string? env_value = Environment.GetEnvironmentVariable(key);
            if (string.IsNullOrWhiteSpace(env_value))
            {
                return defaultvalue;
            }
            int w;
            if (Int32.TryParse(env_value, out w) == true)
            {
                return w;
            }
            return defaultvalue;
        }
    }
}

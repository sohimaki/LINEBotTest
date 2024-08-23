using System;
using System.Collections.Generic;
using System.Text;

namespace ihoseco
{
    public class DBCommon
    {
        public enum ContainerType
        {
            sensormaster = 0,
            linebot = 1
        }
        public enum EnableState
        {
            None = 0,
            Enabled = 1,
            Disabled = 2
        }
        public DBCommon(ContainerType containertype, string primaryid)
        {
            id = primaryid;

            IsEnable = true;
            sensorid = primaryid;     //ex. 12C7AE6
            type = (int)containertype;
            //Blazorでは、TimeZoneInfoが使えないので、TimeZoneConverterを使用する。
            createddatetime = Utility.GetJapanseseNow();
            updateddatetime = Utility.GetJapanseseNow();
        }
        public string GetQueryID()
        {
            string query = "SELECT * FROM c WHERE c.id = " + AddSingleQuotation(id);
            return query;
        }
        public string GetEnabledQueryID()
        {
            string query = "SELECT * FROM c WHERE c.id = " + AddSingleQuotation(id) + " AND c.IsEnable = true";
            return query;
        }
        public string GetDisabledQueryID()
        {
            string query = "SELECT * FROM c WHERE c.id = " + AddSingleQuotation(id) + " AND c.IsEnable = false";
            return query;
        }
        public string GetEnabledQuery()
        {
            string query = "SELECT * FROM c WHERE c.IsEnable = true AND c.type = " + type.ToString();
            return query;
        }
        public string GetDisabledQuery()
        {
            string query = "SELECT * FROM c WHERE c.IsEnable = false AND c.type = " + type.ToString();
            return query;
        }
        public static string AddSingleQuotation(string name)
        {
            return "'" + name + "'";
        }
        /// <summary>
        /// 主キー(センサーマスターはセンサーID,LINEボットマスターはLINEID)
        /// </summary>
        public string id { get; set; }
        /// <summary>
        /// センサーID
        /// </summary>
        public string sensorid { get; set; }
        /// <summary>
        /// レコードタイプ
        /// </summary>
        public int type { get; set; }
        /// <summary>
        /// レコード有効、無効種別 true:有効 false:無効
        /// </summary>
        public bool IsEnable { get; set; }
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

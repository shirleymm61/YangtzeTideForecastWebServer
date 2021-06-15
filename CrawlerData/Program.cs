using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using NPOI.SS.Formula.Functions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Match = System.Text.RegularExpressions.Match;
using log4net;
using Npgsql;
using System.Data.SqlClient;

namespace CrawlerData
{
    class Program
    {

        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            log4net.ILog log = log4net.LogManager.GetLogger("log");

            string filename = @"D:\WCHWork\Changjiang\process\e水情信息.csv";
            try
            {
                string url = "http://www.cjh.com.cn/sqindex.html";

                //var req= System.Diagnostics.Process.Start(url);
                while (true)
                {
                    try
                    {
                        HttpWebRequest request = (HttpWebRequest)WebRequest.CreateHttp(url);
                        // req.Method = "Post";
                        request.Timeout = 2000;
                        request.KeepAlive = true;
                        request.AllowAutoRedirect = false;
                        using (WebResponse response = (HttpWebResponse)request.GetResponse())
                        {
                            using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                            {
                                log.InfoFormat("读取html源代码", reader);
                                string message = reader.ReadToEnd();
                                string[] strArray = message.Split(new string[] { "var sssq = ", "var sqHtml =" }, StringSplitOptions.RemoveEmptyEntries);
                                var initialjson = JsonConvert.SerializeObject(strArray[1].Trim());
                                var arrayJson = strArray[1].Trim().Substring(0, strArray[1].Trim().Length - 1);
                                var jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(arrayJson);
                                Newtonsoft.Json.Linq.JArray jArray = jsonObj as JArray;

                                DataTable dt = new DataTable();
                                dt.Columns.Add("站名");
                                dt.Columns.Add("时间");
                                dt.Columns.Add("水位");
                                dt.Columns.Add("流量");
                                for (int i = 0; i < jArray.Count; i++)
                                {
                                    DataRow dr = dt.NewRow();
                                    dr["站名"] = (jArray[i]["stnm"] as Newtonsoft.Json.Linq.JValue).Value;
                                    //dr["时间"] = (jArray[i]["tm"] as Newtonsoft.Json.Linq.JValue).Value;
                                    var time = (jArray[i]["tm"] as Newtonsoft.Json.Linq.JValue).Value;
                                    dr["时间"] = ToTimeStrDateByMilliSeconds(Convert.ToInt64(time));
                                    dr["水位"] = (jArray[i]["z"] as Newtonsoft.Json.Linq.JValue).Value;
                                    var traffic = (jArray[i]["q"] as Newtonsoft.Json.Linq.JValue).Value;
                                    if (Convert.ToDouble(traffic) <= 0)
                                    {
                                        dr["流量"] = (jArray[i]["oq"] as Newtonsoft.Json.Linq.JValue).Value;
                                    }
                                    else
                                    {
                                        dr["流量"] = (jArray[i]["q"] as Newtonsoft.Json.Linq.JValue).Value;
                                    }
                                    dt.Rows.Add(dr);

                                }

                                long time3 = Convert.ToInt64((jArray[5]["tm"] as Newtonsoft.Json.Linq.JValue).Value);

                                List<string> list = new List<string>();
                                bool isSave = false;
                                using (StreamReader sr = new StreamReader(filename, Encoding.UTF8))
                                {
                                    while (!sr.EndOfStream) //判断是否读完文件
                                    {
                                        string str = sr.ReadLine();
                                        list.Add(str);
                                    }
                                    var la = list.Last();
                                    var ti = la.Split(',');
                                    var time1 = Convert.ToDateTime(ti[1].Trim());
                                    //var time2 = Convert.ToDateTime((jArray[5]["tm"] as Newtonsoft.Json.Linq.JValue).Value);
                                    var time2 = Convert.ToDateTime(ToTimeStrDateByMilliSeconds(time3));
                                    if (time1 == time2)
                                    {
                                        log.InfoFormat("csv时间和读到最新的时间做比较", time1, time2);
                                        break;
                                    }
                                    else
                                    {
                                        log.InfoFormat("时间不相等", time1, time2);
                                        isSave = true;

                                    }
                                }
                                if (isSave)
                                {
                                    SaveCSV(dt, filename);
                                }

                            }

                        }
                    }
                    catch (Exception e)
                    {
                        log.Error(e);
                        Console.WriteLine(e.Message);
                        System.Threading.Thread.Sleep(2000);
                    }

                }
            }
            catch (Exception ex)
            {
                log.Error(ex);
                Console.WriteLine(ex.Message);
            }
            Console.ReadLine();
        }


        /// <summary>
        /// unix时间戳 
        ///转成时间
        /// </summary>
        /// <param name="unix">json 字符串时间 1621220400000</param>
        /// <returns></returns>
        public static string ToTimeStrDateByMilliSeconds(long? unix)
        {
            if (unix == null)
            {
                return "";
            }
            var dto = DateTimeOffset.FromUnixTimeMilliseconds(unix.Value);
            return dto.ToLocalTime().DateTime.ToString("yyyy-MM-dd HH:mm:ss");
        }


        /// <summary>
        /// 将DataTable中数据写入到CSV文件中
        /// </summary>
        /// <param name="dt">提供保存数据的DataTable</param>
        /// <param name="fileName">CSV的文件路径</param>
        public static void SaveCSV(DataTable dt, string fileName)
        {
            try
            {
                //FileStream fs = new FileStream(fileName, System.IO.FileMode.Create, System.IO.FileAccess.Write);
                using (StreamWriter sw = new StreamWriter(fileName, true, System.Text.Encoding.UTF8))
                {
                    string data = "";

                    //写出列名称
                    //for (int i = 0; i < dt.Columns.Count; i++)
                    //{
                    //    data += dt.Columns[i].ColumnName.ToString();
                    //    if (i < dt.Columns.Count - 1)
                    //    {
                    //        data += ",";
                    //    }
                    //}
                    //sw.WriteLine(data);

                    //写出各行数据
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        data = "";
                        for (int j = 0; j < dt.Columns.Count; j++)
                        {
                            data += dt.Rows[i][j].ToString();
                            if (j < dt.Columns.Count - 1)
                            {
                                data += ",";
                            }
                        }
                        sw.WriteLine(data);
                    }
                    // sw.Close();
                }
            }
            catch (Exception EX)
            {
                Console.WriteLine(EX.Message);
            }
        }

        /// <summary>
        /// 将csv数据写进数据库
        /// </summary>
        public static void AddDatabase()
        {
            //连接数据库
            string connectionString = "Server=127.0.0.1;Port=5432;User Id=postgres;Password='9111=hot';Database=CJK_Aliyun";
            NpgsqlConnection SqlConn = new NpgsqlConnection(connectionString);
            string filename = @"D:\WCHWork\Changjiang\process\e水情信息.csv";
            List<string> list = new List<string>();
            try
            {
                using (StreamReader sr = new StreamReader(filename, Encoding.UTF8))
                {
                    DataTable dt = new DataTable();
                    dt.Columns.Add("站名");
                    dt.Columns.Add("时间");
                    dt.Columns.Add("水位");
                    dt.Columns.Add("流量");
                    while (!sr.EndOfStream) //判断是否读完文件
                    {
                        DataRow dr = dt.NewRow();
                        string str = sr.ReadLine();
                        list.Add(str);
                        foreach (var item in list)
                        {
                            foreach (DataColumn dtColumn in dt.Columns)
                            {
                                int i = dt.Columns.IndexOf(dtColumn);
                                if (dr.GetType().IsPrimitive)
                                {
                                    dr[i] = item.GetType().GetProperty(dtColumn.ColumnName).GetValue(item);
                                }
                                else
                                {
                                    dr[i] = JsonConvert.SerializeObject(item.GetType().GetProperty(dtColumn.ColumnName).GetValue(item));
                                }
                            }
                            dt.Rows.Add(dr);
                        }
                    }
                    //cmd.CommandText = "insert into Class1  (StationName,Time,Z,P) values ('{0}','{1}','{2}','{3}')";
                    SqlBulkCopyInsert(connectionString, "CJKForecastTide",dt);
                }

            }
            catch (Exception)
            {

                throw;
            }

        }
        /// <summary> 
        /// 注意：DataTable中的列需要与数据库表中的列完全一致。
        /// 已自测可用。
        /// </summary> 
        /// <param name="conStr">数据库连接串</param>
        /// <param name="strTableName">数据库中对应的表名</param> 
        /// <param name="dtData">数据集</param> 
        public static void SqlBulkCopyInsert(string conStr, string strTableName, DataTable dtData)
        {
            try
            {
                using (SqlBulkCopy sqlRevdBulkCopy = new SqlBulkCopy(conStr))//引用SqlBulkCopy 
                {
                    sqlRevdBulkCopy.DestinationTableName = strTableName;//数据库中对应的表名 
                    sqlRevdBulkCopy.NotifyAfter = dtData.Rows.Count;//有几行数据 
                    sqlRevdBulkCopy.WriteToServer(dtData);//数据导入数据库 
                    sqlRevdBulkCopy.Close();//关闭连接 
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
    }

    /// <summary>
    /// 类
    /// </summary>
    public class Class1
    {
        public int Id { get; set; }
        public string StationName { get; set; }
        public DateTime Time { get; set; }
        public decimal Z { get; set; }
        public int P { get; set; }
    }
}

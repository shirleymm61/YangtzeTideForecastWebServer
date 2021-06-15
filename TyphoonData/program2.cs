using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace TyphoonData
{
    class program2
    {
        static void Main(string[] args)
        {
            var tyPhoonName = GetTyphoonName();
            for (int i = 0; i < tyPhoonName.Count; i++)
            {
                GetTyphoonImg(tyPhoonName[i]);
            }
            Console.ReadLine();
        }
        /// <summary>
        /// 获取台风名称
        /// </summary>
        public static List<string> GetTyphoonName()
        {
            List<string> typhoonNameList = new List<string>();
            string url = "http://typhoon.zjwater.gov.cn/Api/TyphoonList/2020?callback=jQuery18303447907970491517_1622171094530&_=1622171104685";
            HttpWebRequest request = WebRequest.CreateHttp(url);
            using (WebResponse response = request.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    string message = reader.ReadToEnd();
                    string[] strArray = message.Split(new string[] { "1622171094530(", ");" }, StringSplitOptions.RemoveEmptyEntries);
                    string initialjson = JsonConvert.SerializeObject(strArray[1]);
                    var jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(initialjson);
                    //string tempJsonParse = jsonObj.ToString().Substring(1, jsonObj.ToString().Length - 1);
                    string tempJsonParse = jsonObj.ToString();
                    JArray jObj = Newtonsoft.Json.JsonConvert.DeserializeObject<JArray>(tempJsonParse);
                    DataTable dt = new DataTable();
                    dt.Columns.Add("endtime");
                    dt.Columns.Add("enname");
                    dt.Columns.Add("isactive");
                    dt.Columns.Add("name");
                    dt.Columns.Add("starttime");
                    dt.Columns.Add("tfid");
                    for (int i = 0; i < jObj.Count; i++)
                    {
                        DataRow dr = dt.NewRow();
                        dr["endtime"] = (jObj[i]["endtime"] as Newtonsoft.Json.Linq.JValue).Value;
                        dr["enname"] = (jObj[i]["enname"] as Newtonsoft.Json.Linq.JValue).Value;
                        dr["isactive"] = (jObj[i]["isactive"] as Newtonsoft.Json.Linq.JValue).Value;
                        dr["name"] = (jObj[i]["name"] as Newtonsoft.Json.Linq.JValue).Value;
                        dr["starttime"] = (jObj[i]["starttime"] as Newtonsoft.Json.Linq.JValue).Value;
                        dr["tfid"] = (jObj[i]["tfid"] as Newtonsoft.Json.Linq.JValue).Value;
                        typhoonNameList.Add((jObj[i]["tfid"] as Newtonsoft.Json.Linq.JValue).Value.ToString());
                        dt.Rows.Add(dr);
                    }
                    return typhoonNameList;
                }
            }
        }

        /// <summary>
        /// 获取台风数据
        /// </summary>
        public static void GetTyphoonImg(string typhoonId)
        {
            string connectionString = "Server=139.196.41.243;Port=5432;User Id=postgres;Password=9111=hot;Database=CJKForecastTide";
            string url = $@"http://typhoon.zjwater.gov.cn/Api/TyphoonInfo/{typhoonId}?callback=jQuery18309048401199719922_1622082469982&_=1622086766123";
            HttpWebRequest request = (HttpWebRequest)WebRequest.CreateHttp(url);
            using (WebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    NpgsqlConnection SqlConn = new NpgsqlConnection(connectionString);
                    SqlConn.Open();
                    string message = reader.ReadToEnd();
                    string[] strArray = message.Split(new string[] { "1622082469982(", ");" }, StringSplitOptions.RemoveEmptyEntries);
                    string initialjson = JsonConvert.SerializeObject(strArray[1].Trim());
                    var jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(initialjson);
                    string tempJsonParse = jsonObj.ToString().Substring(1, jsonObj.ToString().Length - 2);
                    JObject jObj = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(tempJsonParse);
                    var points = jObj["points"] as JArray;
                    var tfid = jObj["tfid"].ToString();
                    Dictionary<int, string> pointsId = new Dictionary<int, string>();
                    for (int i = 0; i < points.Count; i++)
                    {
                        var uuid = Guid.NewGuid().ToString();
                        string lat = (points[i]["lat"] as Newtonsoft.Json.Linq.JValue).Value.ToString();
                        string lng = (points[i]["lng"] as Newtonsoft.Json.Linq.JValue).Value.ToString();
                        string movedirection = (points[i]["movedirection"] as Newtonsoft.Json.Linq.JValue).Value.ToString();
                        string movespeed = (points[i]["movespeed"] as Newtonsoft.Json.Linq.JValue).Value.ToString();
                        string power = (points[i]["power"] as Newtonsoft.Json.Linq.JValue).Value.ToString();
                        string pressure = (points[i]["pressure"] as Newtonsoft.Json.Linq.JValue).Value.ToString();
                        string radius10 = (points[i]["radius10"] as Newtonsoft.Json.Linq.JValue).Value.ToString();
                        string radius12 = (points[i]["radius12"] as Newtonsoft.Json.Linq.JValue).Value.ToString();
                        string radius7 = (points[i]["radius7"] as Newtonsoft.Json.Linq.JValue).Value.ToString();
                        string speed = (points[i]["speed"] as Newtonsoft.Json.Linq.JValue).Value.ToString();
                        string strong = (points[i]["strong"] as Newtonsoft.Json.Linq.JValue).Value.ToString();
                        string time = (points[i]["time"] as Newtonsoft.Json.Linq.JValue).Value.ToString();
                        string id = uuid;
                        string sqlpath = $@"INSERT INTO typhoon_path (id,lat,lng,movedirection,movespeed,power,pressure,radius10,radius12,radius7,speed,strong,time,tfid)VALUES
('{id}',{lat}, {lng}, '{movedirection}', '{movespeed}', '{power}', '{pressure}', '{radius10}', '{radius12}', '{radius7}', '{speed}','{strong}', '{time}', '{tfid}')";
                        NpgsqlCommand cmd = new NpgsqlCommand(sqlpath, SqlConn);
                         int pointscmd = cmd.ExecuteNonQuery();
                        if (pointscmd > 0)
                        {
                            Console.WriteLine("添加成功");
                        }
                        else
                        {
                            Console.WriteLine("添加失败");
                        }
                        var forecast = jObj["points"][i]["forecast"] as JArray;
                        for (int iforecast = 0; iforecast < forecast.Count; iforecast++)
                        {
                            var forecastPoints = forecast[iforecast]["forecastpoints"] as JArray;
                            var tm = (forecast[iforecast]["tm"] as JValue).ToString();
                            for (int iforecastPoint = 0; iforecastPoint < forecastPoints.Count; iforecastPoint++)
                            {
                                string lat_forecastPoint = (forecastPoints[iforecastPoint]["lat"] as Newtonsoft.Json.Linq.JValue).Value.ToString();
                                string lng_forecastPoint = (forecastPoints[iforecastPoint]["lng"] as Newtonsoft.Json.Linq.JValue).Value.ToString();
                                string power_forecastPoint = (forecastPoints[iforecastPoint]["power"] as Newtonsoft.Json.Linq.JValue).Value.ToString();
                                string pressure_forecastPoint = (forecastPoints[iforecastPoint]["pressure"] as Newtonsoft.Json.Linq.JValue).Value.ToString();
                                string speed_forecastPoint = (forecastPoints[iforecastPoint]["speed"] as Newtonsoft.Json.Linq.JValue).Value.ToString();
                                string strong_forecastPoint = (forecastPoints[iforecastPoint]["strong"] as Newtonsoft.Json.Linq.JValue).Value.ToString();
                                string time_forecastPoint = (forecastPoints[iforecastPoint]["time"] as Newtonsoft.Json.Linq.JValue).Value.ToString();
                                string psth = $@"INSERT INTO typhoon_forecast ( lat, lng, power, pressure, speed, strong, time, tm, tfid, pathid)
 VALUES('{lat_forecastPoint}', '{lng_forecastPoint}', '{power_forecastPoint}', '{pressure_forecastPoint}','{speed_forecastPoint}', '{strong_forecastPoint}', '{time_forecastPoint}', '{tm}', '{tfid}', '{id}')";
                                NpgsqlCommand cmdP = new NpgsqlCommand(psth, SqlConn);
                                int cmdreturn = cmdP.ExecuteNonQuery();
                                if (cmdreturn > 0)
                                {
                                    Console.WriteLine("添加成功");
                                }
                                else
                                {
                                    Console.WriteLine("添加失败");
                                }
                            }
                        }
                    }
                        var dttable = ToDataTable(strArray[1].ToString());
                        foreach (DataRow item in dttable.Rows)
                        {
                            string sql = $@"INSERT INTO typhoon_info (centerlat, centerlng, endtime, enname, name, starttime, warnlevel, tfid) VALUES
                      ('{item["centerlat"]}','{item["centerlng"]}', '{item["endtime"]}','{item["enname"]}','{item["name"]}', '{item["starttime"]}','{item["warnlevel"]}', '{tfid}')";
                            NpgsqlCommand cmd = new NpgsqlCommand(sql, SqlConn);
                            int cmdtable = cmd.ExecuteNonQuery();
                            if (cmdtable > 0)
                            {
                                Console.WriteLine("添加成功");
                            }
                            else
                            {
                                Console.WriteLine("添加失败");
                            }
                        }
                    SqlConn.Close();
                    SqlConn.Dispose();
                }
            }
        }

        /// <summary>
        /// 将json字符串转换位datatable
        /// </summary>
        public static DataTable ToDataTable(string json)
        {
            DataTable dataTable = new DataTable();
            //实例化        
            DataTable result;
            try
            {
                JavaScriptSerializer javaScriptSerializer = new JavaScriptSerializer();
                javaScriptSerializer.MaxJsonLength = Int32.MaxValue;
                //取得最大数值            
                ArrayList arrayList = javaScriptSerializer.Deserialize<ArrayList>(json);
                if (arrayList.Count > 0)
                {
                    foreach (Dictionary<string, object> dictionary in arrayList)
                    {
                        if (dictionary.Keys.Count<string>() == 0)
                        {
                            result = dataTable;
                            return result;
                        }
                        if (dataTable.Columns.Count == 0)
                        {

                            foreach (string current in dictionary.Keys)
                            {
                                //dataTable.Columns.Add(current, dictionary[current].GetType());
                                dataTable.Columns.Add(current);
                            }
                        }
                        DataRow dataRow = dataTable.NewRow();
                        foreach (string current in dictionary.Keys)
                        {
                            if (!dataTable.Columns.Contains(current))
                            {
                                dataTable.Columns.Add(current);
                            }
                            dataRow[current] = dictionary[current];
                        }
                        dataTable.Rows.Add(dataRow);
                        //循环添加行到DataTable中               
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                // MessageBox.Show(ex.ToString());
            }
            result = dataTable;
            return result;
        }
    }
}

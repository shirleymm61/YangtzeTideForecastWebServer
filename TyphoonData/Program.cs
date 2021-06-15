using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace TyphoonData
{
    class Program
    {
        static void Main(string[] args)
        {
            string connectionString = "Server=139.196.41.243;Port=5432;User Id=postgres;Password=9111=hot;Database=CJKForecastTide";
            string url = "http://typhoon.zjwater.gov.cn/Api/TyphoonInfo/202003?callback=jQuery18309048401199719922_1622082469982&_=1622086766123";
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
                    //var centerLat = (jObj["centerlat"] as JValue).Value.ToString();
                    var points = jObj["points"] as JArray;
                    var forecast = jObj["points"][0]["forecast"] as JArray;
                    //var tm = jObj["points"][0]["tm"] as JArray;
                    var tm = (forecast[0]["tm"] as JValue).Value.ToString();
                    var uuid = Guid.NewGuid().ToString();
                    var tfid = jObj["tfid"].ToString();

                    DataTable dt = new DataTable();
                    dt.Columns.Add("jl");
                    dt.Columns.Add("lat");
                    dt.Columns.Add("lng");
                    dt.Columns.Add("movedirection");
                    dt.Columns.Add("movespeed");
                    dt.Columns.Add("power");
                    dt.Columns.Add("pressure");
                    dt.Columns.Add("radius10");
                    dt.Columns.Add("radius12");
                    dt.Columns.Add("radius7");
                    dt.Columns.Add("speed");
                    dt.Columns.Add("strong");
                    dt.Columns.Add("time");
                    for (int i = 0; i < points.Count; i++)
                    {
                        DataRow dr = dt.NewRow();
                        dr["jl"] = (points[i]["jl"] as Newtonsoft.Json.Linq.JValue).Value;
                        dr["lat"] = (points[i]["lat"] as Newtonsoft.Json.Linq.JValue).Value;
                        dr["lng"] = (points[i]["lng"] as Newtonsoft.Json.Linq.JValue).Value;
                        dr["movedirection"] = (points[i]["movedirection"] as Newtonsoft.Json.Linq.JValue).Value;
                        dr["movespeed"] = (points[i]["movespeed"] as Newtonsoft.Json.Linq.JValue).Value;
                        dr["power"] = (points[i]["power"] as Newtonsoft.Json.Linq.JValue).Value;
                        dr["pressure"] = (points[i]["pressure"] as Newtonsoft.Json.Linq.JValue).Value;
                        dr["radius10"] = (points[i]["radius10"] as Newtonsoft.Json.Linq.JValue).Value;
                        dr["radius12"] = (points[i]["radius12"] as Newtonsoft.Json.Linq.JValue).Value;
                        dr["radius7"] = (points[i]["radius7"] as Newtonsoft.Json.Linq.JValue).Value;
                        dr["speed"] = (points[i]["speed"] as Newtonsoft.Json.Linq.JValue).Value;
                        dr["strong"] = (points[i]["strong"] as Newtonsoft.Json.Linq.JValue).Value;
                        dr["time"] = (points[i]["time"] as Newtonsoft.Json.Linq.JValue).Value;
                        dt.Rows.Add(dr);
                    }

                    foreach (DataRow item in dt.Rows)
                    {
                        string sqlpath = $@"INSERT INTO typhoon_path (id,lat,lng,movedirection,movespeed,power,pressure,radius10,radius12,radius7,speed,strong,time,tfid)VALUES
('{uuid}',{item["lat"]}, {item["lng"]}, '{item["movedirection"]}', '{item["movespeed"]}', '{item["power"]}', '{item["pressure"]}', '{item["radius10"]}', '{item["radius12"]}', '{item["radius7"]}', '{item["speed"]}','{item["strong"]}', '{item["time"]}', '{tfid}')";
                        NpgsqlCommand cmd = new NpgsqlCommand(sqlpath, SqlConn);
                        int i = cmd.ExecuteNonQuery();
                        if (i > 0)
                        {
                            Console.WriteLine("添加成功");
                        }
                        else
                        {
                            Console.WriteLine("添加失败");
                        }
                    }

                    dt = new DataTable();
                    dt.Columns.Add("lat");
                    dt.Columns.Add("lng");
                    dt.Columns.Add("power");
                    dt.Columns.Add("pressure");
                    dt.Columns.Add("speed");
                    dt.Columns.Add("strong");
                    dt.Columns.Add("time");
                    dt.Columns.Add("tm");
                    for (int i = 0; i < forecast.Count; i++)
                    {
                        DataRow dr = dt.NewRow();
                        dr["lat"] = (points[i]["lat"] as Newtonsoft.Json.Linq.JValue).Value;
                        dr["lng"] = (points[i]["lng"] as Newtonsoft.Json.Linq.JValue).Value;
                        dr["power"] = (points[i]["power"] as Newtonsoft.Json.Linq.JValue).Value;
                        dr["pressure"] = (points[i]["pressure"] as Newtonsoft.Json.Linq.JValue).Value;
                        dr["speed"] = (points[i]["speed"] as Newtonsoft.Json.Linq.JValue).Value;
                        dr["power"] = (points[i]["power"] as Newtonsoft.Json.Linq.JValue).Value;
                        dr["pressure"] = (points[i]["pressure"] as Newtonsoft.Json.Linq.JValue).Value;
                        dr["strong"] = (points[i]["strong"] as Newtonsoft.Json.Linq.JValue).Value;
                        dr["time"] = (points[i]["time"] as Newtonsoft.Json.Linq.JValue).Value;
                        //dr["tm"] = (points[i]["tm"] as Newtonsoft.Json.Linq.JValue).Value;
                        dt.Rows.Add(dr);
                    }
                    foreach (DataRow item in dt.Rows)
                    {
                        string psth = $@"INSERT INTO typhoon_forecast ( lat, lng, power, pressure, speed, strong, time, tm, tfid, pathid)
 VALUES('{item["lat"]}', '{item["lng"]}', '{item["power"]}', '{item["pressure"]}','{item["speed"]}', '{item["strong"]}', '{item["time"]}', '{tm}', '{tfid}', '{uuid}')";
                        NpgsqlCommand cmd = new NpgsqlCommand(psth, SqlConn);
                        int i = cmd.ExecuteNonQuery();
                        if (i > 0)
                        {
                            Console.WriteLine("添加成功");
                        }
                        else
                        {
                            Console.WriteLine("添加失败");
                        }

                    }

                    DataSet dat = new DataSet();
                    DataTable table = new DataTable();
                    string urltfid = "select tfid from typhoon_info";
                    NpgsqlDataAdapter dattype = new NpgsqlDataAdapter(urltfid, SqlConn);
                    dattype.Fill(dat);
                    table = dat.Tables[0];
                    var id = table.Rows[0]["tfid"].ToString();
                    if (id==tfid)
                    {
                        
                    }
                    else
                    {
                        var dttable = ToDataTable(strArray[1].ToString());

                        foreach (DataRow item in dttable.Rows)
                        {
                            //var uuid = Guid.NewGuid().ToString();
                            string sql = $@"INSERT INTO typhoon_info (centerlat, centerlng, endtime, enname, name, starttime, warnlevel, tfid) VALUES
                      ('{item["centerlat"]}','{item["centerlng"]}', '{item["endtime"]}','{item["enname"]}','{item["name"]}', '{item["starttime"]}','{item["warnlevel"]}', '{item["tfid"]}')";
                            NpgsqlCommand cmd = new NpgsqlCommand(sql, SqlConn);
                            int i = cmd.ExecuteNonQuery();
                            if (i > 0)
                            {
                                Console.WriteLine("添加成功");
                            }
                            else
                            {
                                Console.WriteLine("添加失败");
                            }
                            // var dataType = dttable.Columns["points"];
                        }
                    }
                    SqlConn.Close();
                    SqlConn.Dispose();
                }
            }
            Console.ReadLine();
        }

        /// <summary>
        /// 将json字符串转换位datatable
        /// </summary>
        /// <param name="json"></param>
        /// <returns></returns>
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

        public static void GetTyphoonName()
        {
            string url = "http://typhoon.zjwater.gov.cn/Api/TyphoonList/2020?callback=jQuery18303447907970491517_1622171094530&_=1622171104685";
            HttpWebRequest request = WebRequest.CreateHttp(url);
            using (WebResponse response = request.GetResponse())
            {
                using (StreamReader reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    string message = reader.ReadToEnd();
                    string[] strArray = message.Split(new string[] { "1622171094530((", ");" }, StringSplitOptions.RemoveEmptyEntries);
                    string initialjson = JsonConvert.SerializeObject(strArray[1].Trim());
                    var jsonObj = Newtonsoft.Json.JsonConvert.DeserializeObject(initialjson);
                    string tempJsonParse = jsonObj.ToString().Substring(1, jsonObj.ToString().Length - 2);
                    JObject jObj = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(tempJsonParse);
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
                        dt.Rows.Add(dr);
                    }
                }
            }
        }
    }
}

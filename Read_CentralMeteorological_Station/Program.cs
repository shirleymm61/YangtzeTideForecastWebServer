using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Newtonsoft.Json;

namespace Read_CentralMeteorological_Station
{
    class Program
    {
        static void Main(string[] args)
        {
            log4net.Config.XmlConfigurator.Configure();
            log4net.ILog log = log4net.LogManager.GetLogger("log");
            try
            {
                string url = "";
                string url1 = "";
                string fileName = "";
                string fileName1 = "";
                string urlwind = "";
                string filename2 = "";

                //string path = @"D:\WCHWork\CentralMeteorological\wind";
                string weatherPath = ConfigurationManager.AppSettings.Get("weatherPath").ToString();

                //string filepath = @"D:\Source\ReadFiles\time.json";
                string windPath = ConfigurationManager.AppSettings.Get("windPath").ToString();

                string filepath = ConfigurationManager.AppSettings.Get("filepath").ToString();

                //图片保存
                string imgFile = ConfigurationManager.AppSettings.Get("FileRoot").ToString();
                List<string> items = new List<string>() { "L92", "L85", "L70", "L50" };

                //获取天气图
                for (int i = 3; i >= 0; i--)
                {
                    DateTime dtime = DateTime.Now.AddDays((-1) * i);
                    string dateStr1 = dtime.ToString("yyyy/MM/dd");
                    string dateStr2 = dtime.ToString("yyyyMMdd");
                    foreach (string item in items)
                    {
                        url = $"http://image.nmc.cn/product/{dateStr1}/WESA/SEVP_NMC_WESA_SFER_EGH_ACWP_{item}_P9_{dateStr2}000000000.jpg";
                        url1 = $"http://image.nmc.cn/product/{dateStr1}/WESA/SEVP_NMC_WESA_SFER_EGH_ACWP_{item}_P9_{dateStr2}120000000.jpg";

                        //fileName = $@"D:\CJKProject\WCHWork\CentralMeteorological\am\Weather_map_{item}_{dateStr2}" + "08" + ".png";

                        //fileName = sdf + "\\am\\Weather_map_{ item}_{ dateStr2} " + "08" + ".png";
                        fileName = $@"{imgFile}\weather\Weather_map_{item}_{dateStr2}08.png";
                        fileName1 = $@"{imgFile}\weather\Weather_map_{item}_{dateStr2}20.png";
                        if (File.Exists(fileName) || File.Exists(fileName1))
                        {
                            Console.WriteLine("文件存在");
                            break;
                        }
                        else
                        {
                            DownImg(url, fileName);
                            DownImg(url1, fileName1);
                        }
                    }
                }

                //获取风速图
                for (int i = 0; i < 23; i++)
                {
                    DateTime timeUrl = DateTime.Now.AddHours(((-1) * i) - 8);
                    DateTime timeReal = DateTime.Now.AddHours(((-1) * i));
                    string timestr1 = timeUrl.ToString("yyyy/MM/dd");
                    string timestr2 = timeUrl.ToString("yyyyMMddhh");
                    urlwind = $"http://image.nmc.cn/product/{timestr1}/STFC/medium/SEVP_NMC_STFC_SFER_EDA_ACHN_L88_PB_{timestr2}0000000.jpg?v=1621478778669";
                    filename2 = $@"{imgFile}\wind\wind_speed_{timeReal.ToString("yyyyMMddHH")}.png";
                    if (File.Exists(filename2))
                    {
                        break;
                    }
                    else
                    {
                        DownImg(urlwind, filename2);
                    }
                }

                MaxTimeJsonString(weatherPath, windPath, filepath);
            }
            catch (Exception ex)
            {
                log.Info(ex);
                Console.WriteLine(ex.Message);
                throw;
            }


            Console.ReadLine();
        }
        /// <summary>
        /// 获取图片
        /// </summary>
        /// <param name="imageUrl"></param>
        /// <param name="ImagePath"></param>
        public static void DownImg(string imageUrl, string ImagePath)
        {
            try
            {
                HttpWebRequest request = HttpWebRequest.Create(imageUrl) as HttpWebRequest;
                HttpWebResponse response = null;
                response = request.GetResponse() as HttpWebResponse;
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    return;
                }
                Stream reader = response.GetResponseStream();
                FileStream writer = new FileStream(ImagePath, FileMode.OpenOrCreate, FileAccess.Write);
                byte[] buff = new byte[512];
                int c = 0; //实际读取的字节数
                while ((c = reader.Read(buff, 0, buff.Length)) > 0)
                {
                    writer.Write(buff, 0, c);
                }
                writer.Close();
                writer.Dispose();
                reader.Close();
                reader.Dispose();
                response.Close();
            }
            catch (Exception exe)
            {
                Console.WriteLine(exe.Message);
            }


        }

        /// <summary>
        /// 获取最大时间
        /// </summary>
        public static void MaxTimeJsonString(string weatherpath,string windpath,string filepath)
        {
            var files = Directory.GetFiles(weatherpath, "*.png");
            var filewind = Directory.GetFiles(windpath, "*.png");
            List<DateTime> dtList = new List<DateTime>();
            List<DateTime> windlist = new List<DateTime>();
            foreach (var file in files)
            {
                string[] tim = file.Split(new string[] { "map_", ".png" }, StringSplitOptions.RemoveEmptyEntries);
                tim[1] = tim[1].Substring(4);
                DateTime dt = DateTime.ParseExact(tim[1], "yyyyMMddHH", System.Globalization.CultureInfo.InvariantCulture);
                dtList.Add(dt);
            }
            foreach (var filew in filewind)
            {
                string[] windtime = filew.Split(new string[] { "speed_", ".png" }, StringSplitOptions.RemoveEmptyEntries);
                DateTime dtw = DateTime.ParseExact(windtime[1], "yyyyMMddHH", System.Globalization.CultureInfo.InvariantCulture);
                windlist.Add(dtw);
            }
            DateTime dtMax = dtList.Max();
            DateTime windmax = windlist.Max();
            string sJsonString = Newtonsoft.Json.JsonConvert.SerializeObject(dtMax.ToString("yyyy-MM-dd HH:mm:ss"));
            string wJsonString = Newtonsoft.Json.JsonConvert.SerializeObject(windmax.ToString("yyyy-MM-dd"));
            TimeMaxInfo cla = new TimeMaxInfo();
            cla.windtime = sJsonString;
            cla.weatherTime = wJsonString;

            File.WriteAllText(filepath, JsonConvert.SerializeObject(cla));

            Console.ReadLine();
        }

    }
}

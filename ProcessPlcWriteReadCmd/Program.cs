using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ConsoleApp2
{
    class Program
    {
        private static string CmdPath = @"C:\Windows\System32\cmd.exe";
        static void Main(string[] args)
        {
            for (int i = 0; i < 308; i++)
            {
                Console.WriteLine(ProcessPlc(i));
                string cmd = $@"MzPlotCompApp.exe d:\WCHWork\Changjiang\MzPlot{i}.plc -printtofile d:\WCHWork\Changjiang\{i}.png -screen -width 2000 -height 2000";
                string output = "";
                string msg = RunCmd(cmd, output);
                Console.WriteLine(msg);
            }



            Console.ReadKey();
        }

        /// <summary>
        /// 调用cmd
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        public static string RunCmd(string cmd, string output)
        {
            cmd = cmd.Trim().TrimEnd('&') + "&exit";
            using (Process p = new Process())
            {
                //cmd工作目录
                p.StartInfo.WorkingDirectory = @"c:\Program Files (x86)\DHI\2014\bin\x64";
                p.StartInfo.FileName = CmdPath;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardInput = true;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.RedirectStandardError = true;
                p.StartInfo.CreateNoWindow = true;
                p.Start();//启动程序

                //向cmd窗口写入命令
                p.StandardInput.WriteLine(cmd);
                p.StandardInput.AutoFlush = true;

                //获取cmd窗口的输出信息
                output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
                p.Close();
            }
            return output;

        }

        /// <summary>
        /// 处理plc文件
        /// </summary>
        /// <param name="timestepNo">传入的值</param>
        /// <returns></returns>
        public static string ProcessPlc(int timestepNo)
        {
            try
            {
                //读取plc文件
                string filename = @"D:\WCHWork\Changjiang\MzPlot.plc";
                List<string> list = new List<string>();
                using (StreamReader sr = new StreamReader(filename))
                {
                    while (!sr.EndOfStream) //判断是否读完文件
                    {

                        string str = sr.ReadLine();
                        list.Add(str);
                        if (str.Contains("[PLOT_PROPERTIES]"))
                        {
                            while (!sr.EndOfStream)
                            {
                                string strPLOT_PROPERTIES = sr.ReadLine();
                                var strarr = strPLOT_PROPERTIES.Split('=');
                                if (strarr[0].Trim().Equals("TimeStep"))
                                {
                                    string sNewLine = $"TimeStep = {timestepNo}";
                                    list.Add(sNewLine);
                                }
                                else
                                {
                                    list.Add(strPLOT_PROPERTIES);
                                }
                            }
                            break;
                        }

                    }
                }

                //写进plc文件
                //FileStream fs = new FileStream("Mzp.plc", FileMode.CreateNew);
                string newfilename = $@"D:\WCHWork\Changjiang\MzPlot{timestepNo}.plc";
                using (StreamWriter sw = new StreamWriter(newfilename))
                {
                    for (int i = 0; i < list.Count; i++)
                    {
                        sw.WriteLine(list[i]);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            return string.Empty;
        }

    }
}

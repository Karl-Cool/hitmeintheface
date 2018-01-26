using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using Newtonsoft.Json;
using System.Net;
using System.IO;
using System.Diagnostics;

namespace ServerCheckTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Check started: " + DateTime.Now.ToString());
            doSomeIntervalChecking();
            Console.ReadLine();
        }

        private static async Task doSomeIntervalChecking()
        {
            decimal iteration = 1;
            while (true)
            {
                checkRemote(iteration);
                await Task.Delay(60000);
                iteration++;
            }
        }

        //Remote access
        private static async Task<string> checkRemote(decimal iteration)
        {
            try
            {
                ConnectionOptions options = new ConnectionOptions();
                options.Username = "QuiZzYy";
                options.Password = "karl123";
                options.Impersonation = ImpersonationLevel.Impersonate;
                options.Authentication = AuthenticationLevel.Default;
                options.EnablePrivileges = true;
                Console.WriteLine("Status iteration [" + Convert.ToString(iteration) + "] started at: " + DateTime.Now.ToString());
                //skapar queryn (i det här fallet till själva operativsystemet)
                var machine = @"\\10.1.10.184";
                var path = @"\root\cimv2";
                var osQuery = new SelectQuery("Win32_OperatingSystem");
                //Skapar scopet (alltså vilket system som vi vill ansluta till, 10.1.10.140 är ett lokalt ip)
                var mgmtScope = new ManagementScope(machine + path, options);

                //Ansluter till WMI:n
                mgmtScope.Connect();

                //skapar en 'sökare' för queryn
                var mgmtSrchr = new ManagementObjectSearcher(mgmtScope, osQuery);
                string osStatus = "empty";
                string osNumberOfProcesses = "empty";
                string osCap = "empty";

                foreach (var os in mgmtSrchr.Get())
                {
                    osCap = os.GetPropertyValue("Caption").ToString();
                    osStatus = os.GetPropertyValue("Status").ToString();
                    osNumberOfProcesses = os.GetPropertyValue("NumberOfProcesses").ToString();

                    if (!string.IsNullOrEmpty(osCap))
                    {
                        Console.WriteLine("Operating System caption: " + osCap);
                    }
                    if (!string.IsNullOrEmpty(osStatus))
                    {
                        Console.WriteLine("Status: " + osStatus);
                    }
                    if (!string.IsNullOrEmpty(osNumberOfProcesses))
                    {
                        Console.WriteLine("Number of processes running: " + osNumberOfProcesses);
                    }
                }
                var serverStatus = new JsonStatusObject();
                serverStatus.nop = osNumberOfProcesses;
                serverStatus.status = osStatus;
                serverStatus.caption = osCap;
                string json = JsonConvert.SerializeObject(serverStatus);
                Console.WriteLine();
                Console.WriteLine("Uploading Json:");
                Console.WriteLine(json);
                Console.WriteLine("_______________________________________________________");
                Console.WriteLine();
                Console.WriteLine();
                PostJson(json);
                return null;
                //url till vår json
                //"https://api.myjson.com/bins/n6d95"
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                return null;
            }

        }

        private static async Task<string> checkLocal(decimal iteration)
        {
            //skapar queryn (i det här fallet till själva operativsystemet)
            var osQuery = new SelectQuery("Win32_OperatingSystem");
            //Skapar scopet (alltså vilket system som vi vill ansluta till)
            var machine = @"\\10.1.10.184";
            var path = @"\root\cimv2";
            var mgmtScope = new ManagementScope(machine + path);

            //Ansluter till WMI:n
            mgmtScope.Connect();
            Console.WriteLine("Status iteration [" + Convert.ToString(iteration) + "] started at: " + DateTime.Now.ToString() + " for machine: " + machine);

            //skapar en 'sökare' för queryn
            var mgmtSrchr = new ManagementObjectSearcher(mgmtScope, osQuery);
            string osStatus = "empty";
            string osNumberOfProcesses = "empty";
            string osCap = "empty";
            string datetimenow = DateTime.Now.ToString();
            foreach (var os in mgmtSrchr.Get())
            {
                osCap = os.GetPropertyValue("Caption").ToString();
                osStatus = os.GetPropertyValue("Status").ToString();
                osNumberOfProcesses = os.GetPropertyValue("NumberOfProcesses").ToString();

                if (!string.IsNullOrEmpty(osCap))
                {
                    Console.WriteLine("Operating System caption: " + osCap);
                }
                if (!string.IsNullOrEmpty(osStatus))
                {
                    Console.WriteLine("Status: " + osStatus);
                }
                if (!string.IsNullOrEmpty(osNumberOfProcesses))
                {
                    Console.WriteLine("Number of processes running: " + osNumberOfProcesses);
                }
            }
            var serverStatus = new JsonStatusObject();
            serverStatus.nop = osNumberOfProcesses;
            serverStatus.status = osStatus;
            serverStatus.caption = osCap;
            serverStatus.datetime = datetimenow;
            string json = JsonConvert.SerializeObject(serverStatus);
            Console.WriteLine();
            Console.WriteLine("Uploading Json:");
            Console.WriteLine(json);
            Console.WriteLine("_______________________________________________________");
            Console.WriteLine();
            Console.WriteLine();
            PostJson(json);
            return null;
            //url till vår json
            //"https://api.myjson.com/bins/n6d95"
        }
        private static void PostJson(string json)
        {
            var httpWebRequest = (HttpWebRequest)WebRequest.Create("https://api.myjson.com/bins/n6d95");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "PUT";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                streamWriter.Write(json);
                streamWriter.Flush();
                streamWriter.Close();
            }

            var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                var result = streamReader.ReadToEnd();
            }
        }
        public class JsonStatusObject
        {
            public string status { get; set; }
            public string nop { get; set; }
            public string caption { get; set; }
            public string datetime { get; set; }
        }
    }
}

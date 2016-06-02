using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace loader_lib
{
    public static class Update
    {
        public static string version { get; private set; }
        public static int build { get; private set; }
        public static bool isforcibly { get; private set; }
        public static string info { get; private set; }
        public static List<string> updatefiles { get; private set; }
        public static string updatescript { get; private set; }

        public static void GetUpdateInfo(Uri address)
        {
            try
            {
                Task<Stream> t = ReadStreamAsync(address);

                t.Wait(15000);

                if (t.IsCompleted && t.Result != null)
                {
                    XDocument xml = XDocument.Load(t.Result);

                    version = xml.Element("UpgradeBuild").Attribute("version").Value;
                    build = Convert.ToInt32(xml.Element("UpgradeBuild").Attribute("build").Value);
                    isforcibly = Convert.ToBoolean(xml.Element("UpgradeBuild").Attribute("isforcibly").Value);
                    info = xml.Element("UpgradeBuild").Attribute("info").Value;
                    updatefiles = (from files in xml.Element("UpgradeFiles").Elements()
                                   select files.Element("File").Value).ToList();
                    updatescript = xml.Element("UpgradeScript").Value;
                }
                else
                {
                    throw new Exception("检查更新失败了，可能是网络不通或无法连接更新服务器");
                }
            }
            catch (Exception)
            {
                throw new Exception("检查更新失败了，可能是网络不通或无法连接更新服务器");
            }
        }

        [DllImport("wininet.dll")]
        private static extern bool InternetGetConnectedState(out int connectionDescription, int reservedValue);

        private static bool isConnected
        {
            get
            {
                int i;
                return InternetGetConnectedState(out i, 0);
            }
        }

        private static async Task<Stream> ReadStreamAsync(Uri address)
        {
            try
            {
                if (isConnected)
                {
                    using (HttpClient http = new HttpClient())
                    {
                        http.Timeout = new TimeSpan(0, 0, 10);

                        var response = await http.GetAsync(address);
                        response.EnsureSuccessStatusCode();

                        var content = await response.Content.ReadAsStreamAsync();

                        return content;
                    }
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}

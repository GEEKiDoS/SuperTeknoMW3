using System;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;

namespace loader_lib
{
    public static class CheckUpdate
    {
        public static string version { get; set; }
        public static string info { get; set; }
        public static bool isforcibly { get; set; }

        public static void DoCheckUpdate(Uri address)
        {
            try
            {
                Task<string> t = GetUpgradeInfo(address);

                t.Wait(10000);

                if (t.IsCompleted)
                {
                    string result = t.Result;

                    var ts = result.Split(new char[] { '-' }, 3);
                    version = ts[0];
                    isforcibly = Convert.ToBoolean(ts[1]);
                    info = ts[2].Replace("/", Environment.NewLine);
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

        private static async Task<string> GetUpgradeInfo(Uri address)
        {
            try
            {
                IPHostEntry hostip = await Dns.GetHostEntryAsync(address.DnsSafeHost);
                if (hostip != null)
                {
                    using (HttpClient http = new HttpClient())
                    {
                        var response = await http.GetAsync(address);
                        response.EnsureSuccessStatusCode();

                        string content = await response.Content.ReadAsStringAsync();

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
                throw new Exception("检查更新失败了，可能是网络不通或无法连接更新服务器");
            }
        }
    }
}

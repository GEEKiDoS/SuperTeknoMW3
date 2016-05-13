using System;
using System.Threading.Tasks;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Http;

namespace loader_lib
{
    public static class CheckUpdate
    {
        public static string version = "";
        public static string info = "";
        public static bool isforcibly = false;

        public static async Task DoCheckUpdate(Uri address)
        {
            try
            {
                string result = null;

                Task task = new Task(async ()=> 
                {
                    result = await GetUpgradeInfo(address);
                });

                task.Start();
                
                for (int i = 0; i <= 4; i++)
                {
                    await Task.Delay(1000);
                    if (i < 4)
                    {
                        if (task.IsCompleted)
                        {
                            string temp = result;

                            var ts = temp.Split(new char[] { '-' }, 3);
                            version = ts[0];
                            isforcibly = Convert.ToBoolean(ts[1]);
                            info = ts[2].Replace("/", Environment.NewLine);

                            break;
                        }
                        else if (i == 4)
                        {
                            throw new Exception("检查更新失败了呢！原因是连接升级服务器超时，请检查你的网络连接和防火墙！");
                        }
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static async Task<string> GetUpgradeInfo(Uri address)
        {
            try
            {
                using (HttpClient http = new HttpClient())
                {
                    var response = await http.GetAsync(address);
                    response.EnsureSuccessStatusCode();

                    string content = await response.Content.ReadAsStringAsync();

                    return content;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}

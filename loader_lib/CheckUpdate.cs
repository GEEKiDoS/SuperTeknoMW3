using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Reflection;

namespace loader_lib
{
    public static class CheckUpdate
    {
        public static string version = "";

        public static async Task<bool> CheckVersion(string currectversion)
        {
            try
            {
                Task task = new Task(async () =>
                {
                    version = await GetVersion();
                });

                task.Start();

                await Task.Delay(2000);

                if (task.IsCompleted && version != "")
                {
                    if (version != currectversion)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                await Task.Delay(2000);

                if (task.IsCompleted && version != "")
                {
                    if (version != currectversion)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                await Task.Delay(2000);

                if (task.IsCompleted && version != "")
                {
                    if (version != currectversion)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                await Task.Delay(2000);

                if (task.IsCompleted && version != "")
                {
                    if (version != currectversion)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }

                await Task.Delay(2000);

                if (task.IsCompleted && version != "")
                {
                    if (version != currectversion)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static async Task<string> GetVersion()
        {
            try
            {
                using (HttpClient http = new HttpClient())
                {
                    http.BaseAddress = new Uri("http://121.42.186.178:80/version.html");

                    var request = new HttpRequestMessage();
                    request.Headers.Add("GACG-Client", "SuperTeknoMW3");
                    request.Method = HttpMethod.Get;

                    var response = await http.SendAsync(request);
                    response.EnsureSuccessStatusCode();

                    string content = await response.Content.ReadAsStringAsync();

                    http.Dispose();

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

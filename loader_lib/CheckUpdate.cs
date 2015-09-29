using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;

namespace loader_lib
{
    public static class CheckUpdate
    {
        public static async Task<string> GetVersion()
        {
            try
            {
                using (HttpClient http = new HttpClient())
                {
                    http.BaseAddress = new Uri("http://bbs.3dmgame.com/thread-4461802-1-1.html");

                    var request = new HttpRequestMessage();
                    request.Headers.Add("User-Agent", "(Windows NT 6.3; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/43.0.2351.3 Safari/537.36");
                    request.Method = HttpMethod.Get;

                    var response = await http.SendAsync(request);
                    response.EnsureSuccessStatusCode();

                    string content = await response.Content.ReadAsStringAsync();

                    return content.Substring(content.IndexOf("%VERSION:"), content.Split(new string[] { "%VERSION:" }, 2, StringSplitOptions.RemoveEmptyEntries)[1].Split(new char[] { '%' }, 2)[0].Length);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}

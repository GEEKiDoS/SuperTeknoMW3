using System;
using System.ComponentModel;
using System.Net;
using System.Threading.Tasks;

namespace loader_lib
{
    public static class FileDownload
    {
        public static event DownloadProgressChangedEventHandler ProgressChanged;
        public static event AsyncCompletedEventHandler DownloadComplete;

        public static async Task DownloadFile(Uri BaseUri, string Path)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    client.DownloadProgressChanged += new DownloadProgressChangedEventHandler((object sender, DownloadProgressChangedEventArgs e) => ProgressChanged(sender, e));
                    client.DownloadFileCompleted += new AsyncCompletedEventHandler((object sender, AsyncCompletedEventArgs e) => DownloadComplete(sender, e));

                    await client.DownloadFileTaskAsync(BaseUri.ToString() + Path, "cache\\" + Path);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}

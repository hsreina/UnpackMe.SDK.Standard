using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace UnpackMe.SDK.Core.Extensions
{
    static class HttpContentExtensions
    {
        public static Task ReadAsFileAsync(this HttpContent content, string filename)
        {
            string pathname = Path.GetFullPath(filename);

            FileStream fileStream = null;

            try
            {
                fileStream = new FileStream(pathname, FileMode.Create, FileAccess.Write, FileShare.None);
                return
                    content.CopyToAsync(fileStream).ContinueWith((copyTask) => {
                        fileStream.Dispose();
                    });
            }
            catch
            {
                if (fileStream != null)
                {
                    fileStream.Dispose();
                }
                throw;
            }
        }
    }
}

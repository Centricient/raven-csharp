
#if !(net40) && !(net35)

using Newtonsoft.Json;
using SharpRaven.Utilities;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace SharpRaven.Data
{
    public partial class Requester
    {
        /// <summary>
        /// Executes the <c>async</c> HTTP request to Sentry.
        /// </summary>
        /// <returns>
        /// The <see cref="JsonPacket.EventID" /> of the successfully captured JSON packet, or <c>null</c> if it fails.
        /// </returns>
        public async Task<string> RequestAsync()
        {
            using (var s = await this.webRequest.GetRequestStreamAsync())
            {
                if (this.Client.Compression)
                    await GzipUtil.WriteAsync(this.data.Scrubbed, s);
                else
                {
                    using (var sw = new StreamWriter(s))
                    {
                        await sw.WriteAsync(this.data.Scrubbed);
                    }
                }
            }

            using (var wr = (HttpWebResponse)await this.webRequest.GetResponseAsync())
            {
                using (var responseStream = wr.GetResponseStream())
                {
                    if (responseStream == null)
                        return null;

                    using (var sr = new StreamReader(responseStream))
                    {
                        var content = await sr.ReadToEndAsync();
                        var response = JsonConvert.DeserializeObject<dynamic>(content);
                        return response.id;
                    }
                }
            }
        }
    }
}

#endif
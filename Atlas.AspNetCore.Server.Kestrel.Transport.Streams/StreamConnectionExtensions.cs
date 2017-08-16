using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Atlas.AspNetCore.Server.Kestrel.Transport.Streams
{
    public static class StreamConnectionExtensions
    {
        public static async Task<string> Get(this StreamConnection connection, string path)
        {
            string request = $@"GET {path} HTTP/1.1
Connection: Keep-Alive
Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8
Accept-Language: en-US,en;q=0.8
Host: localhost
User-Agent: Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.36
MS-ASPNETCORE-TOKEN: {StreamTransportFactory.PairingToken}
X-Forwarded-For: [::1]:51669
X-Forwarded-Proto: http

";
            var requestBytes = System.Text.UTF8Encoding.UTF8.GetBytes(request);
            await connection.RequestStream.WriteAsync(requestBytes, 0, requestBytes.Length);

            connection.RequestStream.Complete();

            await connection.StartAsync();

            string response = System.Text.UTF8Encoding.UTF8.GetString(connection.ResponseStream.ToArray());
            int indexOfFirstDoubleLineBreak = response.IndexOf("\r\n\r\n");
            string headers = response.Substring(0, indexOfFirstDoubleLineBreak);

            string body = string.Empty;

            // If the response is chunked, then we need to decode it
            if (headers.Contains("Transfer-Encoding: chunked"))
            {
                var bodyBuilder = new StringBuilder();
                int currentIndex = indexOfFirstDoubleLineBreak + 4;

                while(currentIndex < response.Length)
                {
                    int nextLineBreak = response.IndexOf("\r\n", currentIndex);
                    string line = response.Substring(currentIndex, nextLineBreak - currentIndex);

                    int chunkLength = Convert.ToInt32(line, 16);

                    bodyBuilder.Append(response.Substring(nextLineBreak + 2, chunkLength));

                    currentIndex = nextLineBreak + 2 + chunkLength + 2;
                }

                body = bodyBuilder.ToString();
            }
            else
            {
                body = response.Substring(indexOfFirstDoubleLineBreak + 4);
            }

            return body;
        }
    }
}

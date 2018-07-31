using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValidationTest.Extension
{
        public static class HttpRequestExtensions
        {
            /// <summary>
            /// Retrieve the raw body as a string from the Request.Body stream
            /// </summary>
            /// <param name="request">Request instance to apply to</param>
            /// <param name="encoding">Optional - Encoding, defaults to UTF8</param>
            /// <returns></returns>
            public static async Task<string> GetRawBodyStringAsync(this HttpRequest request, Encoding encoding = null)
            {
                if (encoding == null)
                    encoding = Encoding.UTF8;

            var requestBodyStream = new MemoryStream();
            var originalRequestBody = request.Body;

            await request.Body.CopyToAsync(requestBodyStream);
            requestBodyStream.Seek(0, SeekOrigin.Begin);

            var requestBodyText = new StreamReader(requestBodyStream).ReadToEnd();
            return requestBodyText;

            using (StreamReader reader = new StreamReader(request.Body, encoding, true, 2048, false))
                return await reader.ReadToEndAsync();
            }
        }
}

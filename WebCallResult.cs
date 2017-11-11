using System;
using System.Net;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.Net.Http;

namespace Utils.BasicWebCall
{
    /// <summary>
    /// Результат HTTP запроса
    /// </summary>
    public class WebCallResult
    {
        /// <summary>
        /// URL запрашиваемого адреса
        /// </summary>
        public Uri RequestUri { get; private set; }
        /// <summary>
        /// Cookies запроса
        /// </summary>
        public Cookies Cookies { get; private set; }
        /// <summary>
        /// URL адрес, с которого получен запрос
        /// </summary>
        public Uri ResponseUri { get; private set; }
        /// <summary>
        /// Код статуса ответа
        /// </summary>
        public HttpStatusCode StatusCode { get; private set; }
        /// <summary>
        /// Поток данных ответа
        /// </summary>
        public MemoryStream ResponseStream { get; private set; }

        public WebCallResult(Uri requestUri, Cookies cookies)
        {
            RequestUri = requestUri;
            Cookies = cookies;
        }

        /// <summary>
        /// Вносит в <see cref="Cookies"/> указанные куки
        /// </summary>
        /// <param name="cookies">Cookies</param>
        public void SaveCookies(CookieCollection cookies)
        {
            // TODO: что-то не то
            Cookies.AddFrom(ResponseUri, cookies);
        }
        public async Task SaveResponseAsync(HttpResponseMessage response, Stream stream)
        {
            ResponseUri = response.RequestMessage.RequestUri;
            StatusCode = response.StatusCode;
            if (stream != null)
            {
                ResponseStream = new MemoryStream((int)stream.Length);
                await stream.CopyToAsync(ResponseStream);
            }
            else
                ResponseStream = new MemoryStream();
        }
        public async Task<string> GetResponseAsync(Encoding encoding)
        {
            using (MemoryStream ms = new MemoryStream((int)ResponseStream.Length))
            {
                lock (ResponseStream)
                {
                    long pos = ResponseStream.Position;
                    ResponseStream.Position = 0;
                    ResponseStream.CopyTo(ms);
                    ResponseStream.Position = pos;
                }
                ms.Position = 0;
                using (var reader = new StreamReader(ms, encoding))
                {
                    return await reader.ReadToEndAsync();
                }
            }
        }
        /// <summary>
        /// Get response as string in UTF8 Encoding
        /// </summary>
        /// <returns></returns>
        public Task<string> GetResponseAsync()
        {
            return GetResponseAsync(Encoding.UTF8);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Utils.BasicWebCall
{
    public class WebCall : IDisposable
    {
        private static TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(30);

        private readonly HttpClient _client;
        private readonly HttpContent _content;
        private readonly WebCallResult _result;

        public TimeSpan Timeout { get; private set; }

        private WebCall(Uri uri, Cookies cookies, IEnumerable<KeyValuePair<string, string>> parameters = null, HttpContent content = null, IEnumerable<KeyValuePair<string, string>> headers = null, TimeSpan? timeout = null, bool allowAutoRedirect = true)
        {
            HttpClientHandler handler = new HttpClientHandler
            {
                CookieContainer = cookies.Container,
                UseCookies = true,
                AllowAutoRedirect = allowAutoRedirect
            };
            _client = new HttpClient(handler)
            {
                BaseAddress = uri,
                DefaultRequestHeaders =
                {
                    Accept = { MediaTypeWithQualityHeaderValue.Parse("*/*") }
                },
                Timeout = timeout ?? DefaultTimeout
            };

            if (content != null)
                _content = content;
            else if (parameters != null)
                _content = new FormUrlEncodedContent(parameters);

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    _client.DefaultRequestHeaders.TryAddWithoutValidation(header.Key, header.Value);
                }
            }

            _result = new WebCallResult(uri, cookies);
        }
        
        private async Task<WebCallResult> MakeRequestAsync(HttpResponseMessage response, Uri uri)
        {
            if (response.IsSuccessStatusCode)
            {
                using (var stream = await response.Content.ReadAsStreamAsync())
                {
                    if (stream == null)
                    {
                        throw new Exception("Response is null.");
                    }
                    
                    var cookies = _result.Cookies.Container;

                    _result.SaveCookies(cookies.GetCookies(uri));

                    await _result.SaveResponseAsync(response, stream);

                    return response.StatusCode == HttpStatusCode.Redirect
                            ? await RedirectToAsync(response.Headers.Location)
                            : _result;
                }
            }
            await _result.SaveResponseAsync(response, null);
            return _result;
        }

        private async Task<WebCallResult> RedirectToAsync(Uri uri)
        {
            using (var call = new WebCall(uri, _result.Cookies, null, null, null, Timeout))
            {
                call._client.DefaultRequestHeaders.Add("ContentType", "text/html");

                var response = call._client.GetAsync(uri);

                return await call.MakeRequestAsync(await response, uri);
            }
        }

        /// <summary>
        /// Выполняет GET запрос по указанному адресу
        /// </summary>
        /// <param name="uri">URL</param>
        /// <param name="cookies">Cookie отправляемые с запросом</param>
        /// <param name="headers">Заголовки, используемые в запросе</param>
        /// <returns></returns>
        public async static Task<WebCallResult> MakeCallAsync(Uri uri, Cookies cookies, IEnumerable<KeyValuePair<string, string>> headers = null, TimeSpan? timeout = null)
        {
            using (var call = new WebCall(uri, cookies ?? new Cookies(), null, null, headers, timeout))
            {
                var response = call._client.GetAsync(uri);
                return await call.MakeRequestAsync(await response, uri);
            }
        }
        /// <summary>
        /// Выполняет POST запрос по указанному адресу отправляя параметры в теле запроса
        /// </summary>
        /// <param name="uri">URL</param>
        /// <param name="cookies">Cookie отправляемые с запросом</param>
        /// <param name="parameters">Параметры POST запроса</param>
        /// <param name="headers">Заголовки, используемые в запросе</param>
        /// <returns></returns>
        public async static Task<WebCallResult> PostCallAsync(Uri uri, Cookies cookies, IEnumerable<KeyValuePair<string, string>> parameters, IEnumerable<KeyValuePair<string, string>> headers = null, TimeSpan? timeout = null)
        {
            using (var call = new WebCall(uri, cookies ?? new Cookies(), parameters, null, headers, timeout))
            {
                var response = call._client.PostAsync(uri, call._content);
                return await call.MakeRequestAsync(await response, uri);
            }
        }
        /// <summary>
        /// Выполняет POST запрос по указанному адресу с использованием заданного тела
        /// </summary>
        /// <param name="uri">URL</param>
        /// <param name="cookies">Cookie отправляемые с запросом</param>
        /// <param name="content">Тело запроса</param>
        /// <param name="headers">Заголовки, используемые в запросе</param>
        /// <returns></returns>
        public async static Task<WebCallResult> PostCallAsync(Uri uri, Cookies cookies, HttpContent content, IEnumerable<KeyValuePair<string, string>> headers = null, TimeSpan? timeout = null)
        {
            using (var call = new WebCall(uri, cookies ?? new Cookies(), null, content, headers, timeout))
            {
                var response = call._client.PostAsync(uri, call._content);
                return await call.MakeRequestAsync(await response, uri);
            }
        }

        public void Dispose()
        {
            _client?.Dispose();
            _content?.Dispose();
        }
    }
}

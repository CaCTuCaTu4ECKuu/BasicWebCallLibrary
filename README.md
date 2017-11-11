# BasicWebCallLibrary

Чтобы передавать куки от запроса к запросу нужно создать Cookies и передавать его при каждом вызове

Примеры использования:
```
Uri url = new Uri("http://example.com/api/method");
KeyValuePair<string, string>[] headers = new KeyValuePair<string, string>[]
{
  new KeyValuePair<string, string>("Accept", "text/plain"),
  new KeyValuePair<string, string>("Accept-Encoding", "br")
};
Dictionary<string, string> p = new Dictionary<string, string>
{
    { "query", "xxx" },
    { "filter", "birth" },
    { "count", "10" },
    { "offset", "0" }
};

WebCallResult resp = await WebCall.PostCallAsync(url, null, p, headers);
string responseString = await resp.GetResponseAsync();

var content = new StringContent("{"name":"John","age":30,"cars":[ "Ford", "BMW", "Fiat" ]}", Encoding.UTF8, "application/json");

resp = await WebCall.PostCallAsync(url, null, content, headers);
responseString = await resp.GetResponseAsync();
```

using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;

namespace OmniCore.Services
{
    public class XDripWebServiceClient
    {
        public async void Test()
        {
            using (var hc = new HttpClient())
            {
                var result = await hc.GetAsync(new Uri("http://127.0.0.1:17580/sgv.json"));
                string resultContent = await result.Content.ReadAsStringAsync();
                var document = JsonDocument.Parse(resultContent);
                foreach (var element in document.RootElement.EnumerateArray())
                {
                    var udate = element.GetProperty("date").GetInt64();
                    var date = DateTimeOffset.FromUnixTimeMilliseconds(udate);
                    var direction = element.GetProperty("direction").GetString();
                    var type = element.GetProperty("type").GetString();
                    Debug.WriteLine($"date: {date} dir: {direction} type: {type}");
                }
            }
        }
    }
}
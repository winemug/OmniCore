using System;
using System.Diagnostics;
using System.Net.Http;

namespace OmniCore.Services.Data
{
    public class XDripWebServiceClient
    {
        public async void Test()
        {
            using (var hc = new HttpClient())
            {
                var result = await hc.GetAsync(new Uri("http://127.0.0.1:17580/sgv.json"));
                string resultContent = await result.Content.ReadAsStringAsync();
                Debug.WriteLine(resultContent);
            }
        }
    }
}
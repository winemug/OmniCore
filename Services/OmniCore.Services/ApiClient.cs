using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OmniCore.Services.Entities;
using OmniCore.Services.Interfaces;
using Xamarin.Forms;

namespace OmniCore.Services
{
    public class ApiClient
    {
        private bool _authorizedWithAccount = false;
        private bool _authorizedWithClientToken = false;
        private Timer _jwtRefreshTimer = null;
        
        private HttpClient _httpClient = new HttpClient()
        {
            BaseAddress = new Uri("")
        };

        public async Task AuthorizeAccount(string email, string password)
        {
            await Unauthorize();
            
            var j = JObject.FromObject(new
            {
                email = email,
                password = password
            }).ToString();

            var content = new StringContent(j, Encoding.Default, "application/json");
            var result = await _httpClient.PostAsync(new Uri("/auth/login"), content);
            var resultContent = await result.Content.ReadAsStringAsync();
            var o = JObject.Parse(resultContent);
            var access_token = (string)o["access_token"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", access_token);

            var refreshInterval = (long)o["token_refresh_interval"];
            _authorizedWithAccount = true;
            _authorizedWithClientToken = false;
            StartJwtRefreshTimer(TimeSpan.FromSeconds(refreshInterval));
        }

        public async Task AuthorizeClient(ClientConfiguration cc)
        {
            await Unauthorize();
            var j = JObject.FromObject(new
            {
                client_id = cc.ClientId.Value.ToString("N"),
                token = cc.Token
            }).ToString();

            var content = new StringContent(j, Encoding.Default, "application/json");
            var result = await _httpClient.PostAsync(new Uri("/client/auth"), content);
            var resultContent = await result.Content.ReadAsStringAsync();
            var o = JObject.Parse(resultContent);
            var access_token = (string)o["access_token"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", access_token);
            
            var refreshInterval = (long)o["token_refresh_interval"];
            _authorizedWithClientToken = true;
            _authorizedWithAccount = false;
            StartJwtRefreshTimer(TimeSpan.FromSeconds(refreshInterval));
        }

        private void StartJwtRefreshTimer(TimeSpan refreshInterval)
        {
            if (_jwtRefreshTimer == null)
            {
                _jwtRefreshTimer = new Timer(async state =>
                {
                    if (_authorizedWithAccount || _authorizedWithClientToken)
                    {
                        await RefreshToken();
                    }
                }, null, refreshInterval, refreshInterval);
            }
            else
            {
                _jwtRefreshTimer.Change(refreshInterval, refreshInterval);
            }
        }

        public async Task Unauthorize()
        {
            if (_jwtRefreshTimer != null)
            {
                _jwtRefreshTimer.Dispose();
                _jwtRefreshTimer = null;
            }
            _authorizedWithAccount = false;
            _authorizedWithClientToken = false;
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
        
        public async Task<ClientConfiguration> RegisterClient(ClientConfiguration cc)
        {
            var j = JObject.FromObject(new
            {
                name = cc.Name,
                platform = cc.Platform,
                hw_version = cc.HardwareVersion,
                sw_version = cc.SoftwareVersion
            }).ToString();

            var content = new StringContent(j, Encoding.Default, "application/json");;
            var result = await _httpClient.PostAsync(new Uri("/client/register"), content);
            var resultContent = await result.Content.ReadAsStringAsync();
            var o = JObject.Parse(resultContent);

            cc.AccountId = Guid.Parse((string)o["account_id"]);
            cc.ClientId = Guid.Parse((string)o["client_id"]);
            cc.Token = (string)o["token"];
            return cc;
        }
        
        public async Task<EndpointResponse> GetClientEndpoint(ClientConfiguration cc)
        {
            if (!_authorizedWithClientToken)
                throw new ApplicationException("Not authorized with client token");
            
            var j = JObject.FromObject(new
            {
                sw_version = cc.SoftwareVersion
            }).ToString();
            
            var content = new StringContent(j, Encoding.Default, "application/json");;
            var result = await _httpClient.PostAsync(new Uri("/client/endpoint"), content);
            var resultContent = await result.Content.ReadAsStringAsync();
            var o = JObject.Parse(resultContent);

            return new EndpointResponse()
            {
                Success = (bool)o["success"],
                Message = (string)o["message"],
                Dsn = (string)o["dsn"],
                Queue = (string)o["queue"],
                Exchange = (string)o["exchange"],
                UserId = (string)o["user_id"]
            };
        }

        public async Task<ProfileEntry> GetDefaultProfile()
        {
            if (!_authorizedWithClientToken && !_authorizedWithAccount)
                throw new ApplicationException("Not authorized");
            
            var result = await _httpClient.GetAsync(new Uri("/profile/get"));
            var resultContent = await result.Content.ReadAsStringAsync();
            var o = JObject.Parse(resultContent);
            return new ProfileEntry()
            {
                Id = Guid.Parse((string)o["id"]),
                Name = (string)o["name"],
                IsDefault = (bool)o["is_default"]
            };
        }

        // public async Task<ProfileEntry> CreateProfile(string name)
        // {
        //     if (!_authorizedWithClientToken && !_authorizedWithAccount)
        //         throw new ApplicationException("Not authorized");
        //     
        //     var j = JObject.FromObject(new
        //     {
        //         name = name
        //     }).ToString();
        //     
        //     var content = new StringContent(j, Encoding.Default, "application/json");;
        //     var result = await _httpClient.PostAsync(new Uri("/profile/create"), content);
        //     var resultContent = await result.Content.ReadAsStringAsync();
        //     var o = JObject.Parse(resultContent);
        //     return new ProfileEntry()
        //     {
        //         Id = Guid.Parse((string)o["profile_id"]),
        //         Name = name
        //     };
        // }
        
        private async Task RefreshToken()
        {
            var result = await _httpClient.GetAsync(new Uri("/auth/refresh"));
            var resultContent = await result.Content.ReadAsStringAsync();
            var o = JObject.Parse(resultContent);
            var token = (string)o["access_token"];
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }
    }
}

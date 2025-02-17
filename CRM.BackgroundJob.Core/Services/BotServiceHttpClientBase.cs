namespace CRM.BackgroundJob.Core.Services
{
    using CRM.BackgroundJob.Core.Models.BotServices;
    using Fanex.Caching;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Net.Http.Json;
    using System.Text;
    using System.Threading.Tasks;

    public class BotServiceHttpClientBase
    {
        protected readonly BotServiceSettings botSettings;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly ICacheService cacheService;
        private const string JwtTokenCacheKey = "CRM_BotService_Authenticate_Token_Cache_Key";

        public BotServiceHttpClientBase(BotServiceSettings botSettings, IHttpClientFactory httpClientFactory, ICacheService cacheService)
        {
            this.botSettings = botSettings;
            this.httpClientFactory = httpClientFactory;
            this.cacheService = cacheService;
        }

        protected async Task<T> PostAsync<T>(string url, object data)
        {
            var httpClient = httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await GetTokenAsync());

            using (var response = await httpClient.PostAsJsonAsync($"{botSettings.ServiceUrl}{url}", data))
            {
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    await cacheService.RemoveAsync(JwtTokenCacheKey);
                    return await PostAsync<T>(url, data);
                }

                response.EnsureSuccessStatusCode();
                var responseData = await response.Content.ReadAsStringAsync();
                var results = JsonConvert.DeserializeObject<T>(responseData);
                return results;
            }
        }

        private async Task<string> GetTokenAsync()
        {
            string token = await cacheService.GetAsync<string>(JwtTokenCacheKey);
            if (token == null || string.IsNullOrWhiteSpace(token))
            {
                var authData = new
                {
                    botSettings.AppId,
                    Token = Convert.ToBase64String(Encoding.UTF8.GetBytes(botSettings.Token))
                };
                var httpClient = httpClientFactory.CreateClient();
                using (var response = await httpClient.PostAsJsonAsync($"{botSettings.ServiceUrl}{botSettings.AuthenticatePath}", authData))
                {
                    response.EnsureSuccessStatusCode();
                    var responseString = await response.Content.ReadAsStringAsync();
                    var data = JObject.Parse(responseString);
                    token = data["accessToken"].Value<string>();
                    DateTime expired = (data["expriedDate"].Value<DateTime>());
                    await cacheService.SetAsync(JwtTokenCacheKey, token, new CacheItemOptions
                    {
                        AbsoluteExpiration = expired
                    });
                }
            }
            return token;
        }
    }
}
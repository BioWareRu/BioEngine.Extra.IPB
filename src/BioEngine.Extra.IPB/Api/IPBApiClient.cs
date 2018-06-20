using System.Net.Http;
using System.Threading.Tasks;
using BioEngine.Extra.IPB.Models;
using JetBrains.Annotations;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace BioEngine.Extra.IPB.Api
{
    [UsedImplicitly]
    public class IPBApiClientFactory
    {
        private readonly IOptions<IPBApiConfig> _options;

        public IPBApiClientFactory(IOptions<IPBApiConfig> options)
        {
            _options = options;
        }

        public IPBApiClient GetClient(string token)
        {
            return new IPBApiClient(_options.Value, token);
        }
    }

    public class IPBApiClient
    {
        private readonly IPBApiConfig _apiConfig;
        private readonly HttpClient _client;

        public IPBApiClient(IPBApiConfig apiConfig, string token)
        {
            _apiConfig = apiConfig;
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        }

        public async Task<User> GetUser()
        {
            return await Get<User>("core/me");
        }

        private async Task<T> Get<T>(string url)
        {
            var response = await _client.GetAsync($"{_apiConfig.ApiUrl}/{url}");
            var json = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<T>(json);
            }

            throw new IPBApiException(response.StatusCode, JsonConvert.DeserializeObject<IPBApiError>(json));
        }
    }
}
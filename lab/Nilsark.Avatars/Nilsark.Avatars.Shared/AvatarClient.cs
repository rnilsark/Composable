using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Nilsark.Avatars.Shared
{
    public class AvatarClient : IDisposable
    {
        private readonly HttpClient _client;

        public AvatarClient(string baseUrl)
        {
            _client = new HttpClient { BaseAddress = new Uri(baseUrl + "/") };
            _client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            
        }

        public async Task<string> GetAvatarAsync(string id)
        {
            var response = await _client.GetAsync($"api/avatar/{id}");
            
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            return response.Content.ReadAsStringAsync().Result;
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}

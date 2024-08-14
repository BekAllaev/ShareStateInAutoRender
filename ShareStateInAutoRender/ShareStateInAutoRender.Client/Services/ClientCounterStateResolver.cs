
using System.Net.Http.Json;
using System.Text.Json;
using System.Text;

namespace ShareStateInAutoRender.Client.Services
{
    public class ClientCounterStateResolver : ICounterStateResolver
    {
        private readonly HttpClient _httpClient;

        public ClientCounterStateResolver(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public Task<int> GetCounter()
        {
            return _httpClient.GetFromJsonAsync<int>("api/counterState");
        }

        public Task SetCounter(int counter)
        {
            using StringContent jsonContent = new(
                JsonSerializer.Serialize(new
                {
                    counterValue = counter
                }),
                Encoding.UTF8,
                "application/json");

            return _httpClient.PostAsync("api/counterState", jsonContent);
        }
    }
}

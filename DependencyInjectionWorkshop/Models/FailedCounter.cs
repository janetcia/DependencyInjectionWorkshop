using System;
using System.Net.Http;

namespace DependencyInjectionWorkshop.Models
{
    public interface IFailedCounter
    {
        int GetFailedCount(string accountId);
        void AddFailedCount(string accountId);
        void ResetFailedCount(string accountId);
        bool GetAccountIsLocked(string accountId);
    }

    public class FailedCounter : IFailedCounter
    {
        private HttpClient _httpClient;

        public FailedCounter(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public FailedCounter()
        {
            _httpClient = new HttpClient {BaseAddress = new Uri("http://joey.com/")};
        }

        public int GetFailedCount(string accountId)
        {
            var failedCountResponse =
                _httpClient
                    .PostAsJsonAsync("api/failedCounter/GetFailedCount", accountId).Result;
            failedCountResponse.EnsureSuccessStatusCode();
            var failedCount = failedCountResponse.Content.ReadAsAsync<int>().Result;
            return failedCount;
        }

        public void AddFailedCount(string accountId)
        {
            var addFailedCountResponse = _httpClient
                .PostAsJsonAsync("api/failedCounter/Add", accountId).Result;
            addFailedCountResponse.EnsureSuccessStatusCode();
        }

        public void ResetFailedCount(string accountId)
        {
            var resetResponse = _httpClient
                .PostAsJsonAsync("api/failedCounter/Reset", accountId).Result;
            resetResponse.EnsureSuccessStatusCode();
        }

        public bool GetAccountIsLocked(string accountId)
        {
            var isLockedResponse = _httpClient
                .PostAsJsonAsync("api/failedCounter/IsLocked", accountId).Result;
            isLockedResponse.EnsureSuccessStatusCode();
            var isLocked = isLockedResponse.Content.ReadAsAsync<bool>().Result;
            return isLocked;
        }
    }
}
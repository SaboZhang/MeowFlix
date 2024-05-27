using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Http;
using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace MeowFlix.Tools
{
    public class HttpHelper
    {
        /// <summary>
        /// http工具类
        /// </summary>

        // 将HttpClient实例化为静态字段，以避免资源泄漏并重用连接
        private static readonly HttpClient HttpClient;

        static HttpHelper()
        {
            var retryPipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
                .AddRetry(new HttpRetryStrategyOptions
                {
                    BackoffType = DelayBackoffType.Exponential,
                    MaxRetryAttempts = 3
                })
                .Build();

            var socketHandler = new SocketsHttpHandler
            {
                PooledConnectionLifetime = TimeSpan.FromMinutes(15)
            };
#pragma warning disable EXTEXP0001 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。
            var resilienceHandler = new ResilienceHandler(retryPipeline)
            {
                InnerHandler = socketHandler
            };
#pragma warning restore EXTEXP0001 // 类型仅用于评估，在将来的更新中可能会被更改或删除。取消此诊断以继续。

            HttpClient = new HttpClient(resilienceHandler);
        }
        
        // 提供一个公共静态方法来获取HttpClient实例
        public static HttpClient GetHttpClient()
        {
            // 可以在这里添加更多的逻辑，例如检查httpClient是否已经关闭或出现其他问题
            // 并且在需要时重新初始化
            return HttpClient;
        }
        
        // 异步请求，返回json串
        public static async Task<string> GetJsonAsync(string url)
        {
            var response = await HttpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadAsStringAsync();
            return json;
        }
        
        // 异步Post请求，返回json串
        public static async Task<string> PostJsonAsync<T>(string url, T data)
        {
            var jsonPayload = JsonSerializer.Serialize(data); 
            var response = await HttpClient.PostAsync(url, new StringContent(jsonPayload, Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync();
            return result;
        }
        
    }
}

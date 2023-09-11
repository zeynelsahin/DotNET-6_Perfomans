using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using CarvedRock.Core;

namespace CarvedRock.Domain
{
    public class ApiCaller : IApiCaller
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ApiCaller(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
        }


        public async Task<List<LocalClaim>?> CallExternalApiAsync()
        {
                if (_httpContextAccessor.HttpContext == null)
                {
                    throw new Exception("Can't get access token.");
                }


                var token = await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");

               _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                var response = await _httpClient.GetAsync("test");
                response.EnsureSuccessStatusCode();
                var claims = await response.Content.ReadFromJsonAsync<List<LocalClaim>>();
                return claims;
        }
    }

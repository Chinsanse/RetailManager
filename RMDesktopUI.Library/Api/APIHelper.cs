﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using RMDesktopUI.Library.Api;
using RMDesktopUI.Library.Models;


namespace RMDesktopUI.Library.Api
{
    public class APIHelper : IAPIHelper
    {
        private HttpClient _apiCLient;
        private readonly ILoggedInUserModel _loggedInUser;
        private readonly IConfiguration _config;

        public APIHelper(ILoggedInUserModel loggedInUser, IConfiguration config)
        {
            _loggedInUser = loggedInUser;
            _config = config;
            InitializeClient();
        }

        public HttpClient ApiClient
        {
            get
            {
                return _apiCLient;
            }
        }

        private void InitializeClient()
        {
            string api = _config.GetValue<string>("api");

            _apiCLient = new HttpClient();
            _apiCLient.BaseAddress = new Uri(api);
            _apiCLient.DefaultRequestHeaders.Accept.Clear();
            _apiCLient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<AuthenticatedUser> Authenticate(string username, string password)
        {
            var data = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type","password"),
                new KeyValuePair<string, string>("username",username),
                new KeyValuePair<string, string>("password",password),
            });

            using (HttpResponseMessage response = await _apiCLient.PostAsync("/Token", data))
            {
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsAsync<AuthenticatedUser>();
                    return result;
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
        }

        public void LogOffUser()
        {
            _apiCLient.DefaultRequestHeaders.Clear();
        }

        public async Task GetLoggedInUserInfo(string token)
        {
            _apiCLient.DefaultRequestHeaders.Clear();
            _apiCLient.DefaultRequestHeaders.Accept.Clear();
            _apiCLient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _apiCLient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

            using (HttpResponseMessage response = await _apiCLient.GetAsync("/api/User"))
            {
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsAsync<LoggedInUserModel>();
                    _loggedInUser.CreatedDate = result.CreatedDate;
                    _loggedInUser.EmailAddress = result.EmailAddress;
                    _loggedInUser.FirstName = result.FirstName; 
                    _loggedInUser.LastName = result.LastName;  
                    _loggedInUser.Id = result.Id;
                    _loggedInUser.Token = token;
                }
                else
                {
                    throw new Exception(response.ReasonPhrase);
                }
            }
        }
    }
}

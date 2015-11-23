﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.OptionsModel;
using MultiTenantSurveyApp.Configuration;
using MultiTenantSurveyApp.DAL.DataModels;
using MultiTenantSurveyApp.DAL.DTOs;
using MultiTenantSurveyApp.Models;
using MultiTenantSurveyApp.Security;
using Newtonsoft.Json;

namespace MultiTenantSurveyApp.Services
{
    // This is the client for MultiTenantSurveyApp.WebAPI SurveyController
    // Note: If we used Swagger for the API definition, we could generate the client.
    // (see Azure API Apps) 
    // Note the MVC6 version of Swashbuckler is called "Ahoy" and is still in beta: https://github.com/domaindrivendev/Ahoy

    public class SurveyService : ISurveyService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAccessTokenService _accessTokenService;
        private readonly HttpClient _httpClient;
        private readonly AppSettingsOptions _appSettings;

        public SurveyService(HttpClientService factory, IHttpContextAccessor contextAccessor, IAccessTokenService accessTokenService, IOptions<ConfigurationOptions> options)
        {
            _httpContextAccessor = contextAccessor;
            _appSettings = options?.Value?.AppSettings;
            _httpClient = factory.GetHttpClient();
            _accessTokenService = accessTokenService;
        }

        public async Task<ApiResult<SurveyDTO>> GetSurveyAsync(int id)
        {
            var path = $"/surveys/{id}";
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, path);
            await AddBearerTokenToAuthenticationHeader(requestMessage).ConfigureAwait(false);
            var response = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);
            return await ApiResult<SurveyDTO>.FromResponseAsync(response).ConfigureAwait(false);
        }

        public async Task<ApiResult<UserSurveysDTO>> GetSurveysForUserAsync(int userId)
        {
            var path = $"/users/{userId}/surveys";
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, path);
            await AddBearerTokenToAuthenticationHeader(requestMessage).ConfigureAwait(false);
            var response = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);
            return await ApiResult<UserSurveysDTO>.FromResponseAsync(response).ConfigureAwait(false);
        }

        public async Task<ApiResult<TenantSurveysDTO>> GetSurveysForTenantAsync(string tenantId)
        {
            var path = $"/tenants/{tenantId}/surveys";
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, path);
            await AddBearerTokenToAuthenticationHeader(requestMessage).ConfigureAwait(false);
            var response = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);
            return await ApiResult<TenantSurveysDTO>.FromResponseAsync(response).ConfigureAwait(false);
        }
        public async Task<ApiResult<IEnumerable<SurveyDTO>>> GetPublishedSurveysAsync()
        {
            var path = "/surveys/published";
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, path);
            var response = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);
            return await ApiResult<IEnumerable<SurveyDTO>>.FromResponseAsync(response).ConfigureAwait(false);
        }

        public async Task<ApiResult<SurveyDTO>> CreateSurveyAsync(SurveyDTO survey)
        {
            var path = "/surveys";
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, path) {Content = CreateJsonContent(survey)};
            await AddBearerTokenToAuthenticationHeader(requestMessage);
            var response = await _httpClient.SendAsync(requestMessage);
            return await ApiResult<SurveyDTO>.FromResponseAsync(response);
        }

        public async Task<ApiResult<SurveyDTO>> UpdateSurveyAsync(SurveyDTO survey)
        {
            var path = $"/surveys/{survey.Id}";
            var requestMessage = new HttpRequestMessage(HttpMethod.Put, path) {Content = CreateJsonContent(survey)};
            await AddBearerTokenToAuthenticationHeader(requestMessage).ConfigureAwait(false);
            var response = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);
            return await ApiResult<SurveyDTO>.FromResponseAsync(response).ConfigureAwait(false);
        }

        public async Task<ApiResult<SurveyDTO>> DeleteSurveyAsync(int id)
        {
            var path = $"/surveys/{id}";
            var requestMessage = new HttpRequestMessage(HttpMethod.Delete, path);
            await AddBearerTokenToAuthenticationHeader(requestMessage).ConfigureAwait(false);
            var response = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);
            return await ApiResult<SurveyDTO>.FromResponseAsync(response).ConfigureAwait(false);
        }
        public async Task<ApiResult<SurveyDTO>> PublishSurveyAsync(int id)
        {
            var path = $"/surveys/{id}/publish";
            var requestMessage = new HttpRequestMessage(HttpMethod.Put, path);
            await AddBearerTokenToAuthenticationHeader(requestMessage).ConfigureAwait(false);
            var response = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);
            return await ApiResult<SurveyDTO>.FromResponseAsync(response).ConfigureAwait(false);
        }
        public async Task<ApiResult<SurveyDTO>> UnPublishSurveyAsync(int id)
        {
            var path = $"/surveys/{id}/unpublish";
            var requestMessage = new HttpRequestMessage(HttpMethod.Put, path);
            await AddBearerTokenToAuthenticationHeader(requestMessage).ConfigureAwait(false);
            var response = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);
            return await ApiResult<SurveyDTO>.FromResponseAsync(response).ConfigureAwait(false);
        }

        public async Task<ApiResult<ContributorsDTO>> GetSurveyContributorsAsync(int id)
        {
            var path = $"/surveys/{id}/contributors";
            var requestMessage = new HttpRequestMessage(HttpMethod.Get, path);
            await AddBearerTokenToAuthenticationHeader(requestMessage);
            var response = await _httpClient.SendAsync(requestMessage);
            return await ApiResult<ContributorsDTO>.FromResponseAsync(response);
        }

        public async Task<ApiResult> ProcessPendingContributorRequestsAsync()
        {
            var path = $"/surveys/processpendingcontributorrequests";
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, path);
            await AddBearerTokenToAuthenticationHeader(requestMessage).ConfigureAwait(false);
            var response = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);
            return new ApiResult { Response = response };
        }

        public async Task<ApiResult> AddContributorRequestAsync(ContributorRequest contributorRequest)
        {
            var path = $"/surveys/{contributorRequest.SurveyId}/contributorrequests";
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, path) { Content = CreateJsonContent(contributorRequest) };
            await AddBearerTokenToAuthenticationHeader(requestMessage).ConfigureAwait(false);
            var response = await _httpClient.SendAsync(requestMessage).ConfigureAwait(false);
            return new ApiResult { Response = response };
        }

        private static HttpContent CreateJsonContent<T>(T item)
        {
            var json = JsonConvert.SerializeObject(item);
            return new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        }

        private async Task AddBearerTokenToAuthenticationHeader(HttpRequestMessage requestMessage)
        {
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await _accessTokenService.GetTokenForWebApiAsync(_httpContextAccessor.HttpContext.User).ConfigureAwait(false));
        }
    }
}

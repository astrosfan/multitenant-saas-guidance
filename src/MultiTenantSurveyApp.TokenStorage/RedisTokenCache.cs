﻿// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using StackExchange.Redis;

namespace MultiTenantSurveyApp.TokenStorage
{
    // sample implementation of a token cache which persists tokens specific to a user to redis to be used in multi tenanted scenarios
    // the key is the users object unique object identifier   
    // [masimms-roshar] Since we expect this to act as a copy-paste sample, please add an optional verbose log for 
    // all access to ease future debugging
    //[manikris] done
    public class RedisTokenCache : TokenCache
    {
        private IConnectionMultiplexer _connection;
        private IDatabase _cache;
        private TokenCacheKey _key;
        private ILogger _logger;

        // [masimms-roshar] Wrong verb - this creates, not retrieves
        //[manikris] done
        public async static Task<RedisTokenCache> CreateCacheAsync(IConnectionMultiplexer connection, TokenCacheKey key, ILogger logger)
        {
            var redisTokenCache = new RedisTokenCache();
            await redisTokenCache.InitializeAsync(connection, key, logger).ConfigureAwait(false);
            return redisTokenCache;
        }
        private RedisTokenCache() : base() { }

        private async Task InitializeAsync(IConnectionMultiplexer connection, TokenCacheKey key, ILogger logger)
        {
            _connection = connection;
            _key = key;
            _logger = logger;
            _cache = _connection.GetDatabase();
           // BeforeAccess = BeforeAccessNotification;
            AfterAccess = AfterAccessNotification;

            await LoadFromStoreAsync().ConfigureAwait(false);
        }

        private async Task LoadFromStoreAsync()
        {
            try
            {
                byte[] cachedData = await _cache.StringGetAsync(_key.ToString()).ConfigureAwait(false);
                this.Deserialize((cachedData == null) ? null : cachedData);
                _logger.TokensRetrievedFromStore(this._key.ToString());
            }
            catch (Exception exp)
            {
                _logger.ReadFromCacheFailed(exp);
                throw;
            }
        }

        // Triggered right before ADAL needs to access the cache.
        // Reload the cache from the persistent store in case it changed since the last access.
        //TODO- verify this. the assumption right now is that we dont need to impement this in a web app
        //This ie because will be reading from redis for every request and writing back if the state changes so that the next request gets the latest
        //public async void BeforeAccessNotification(TokenCacheNotificationArgs args)
        //{
        //    await LoadFromStoreAsync();
        //}

        // Triggered right after ADAL accessed the cache.
        public async void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            if (this.HasStateChanged)
            {
                try
                {
                    await _cache.StringSetAsync(this._key.ToString(), this.Serialize()).ConfigureAwait(false);
                    _logger.TokensWrittenToStore(args.ClientId, args.UniqueId, args.Resource);
                    this.HasStateChanged = false;
                }
                catch (Exception exp)
                {
                    _logger.WriteToRedisCacheFailed(exp);
                    throw;
                }
            }
        }

        // clean the cache of all tokens associated with the user.
        public override void Clear()
        {
            base.Clear();
            _cache.KeyDelete(_key.ToString());
            _logger.TokenCacheCleared(_key.ToString());
        }
    }
}


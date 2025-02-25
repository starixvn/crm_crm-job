namespace CRM.UnitTests.Services
{
    using CRM.Enum;
    using CRM.Models.BotServices;
    using CRM.Services;
    using CRM.UnitTests.Helpers;
    using Fanex.Caching;
    using Newtonsoft.Json;
    using NSubstitute;
    using NSubstitute.ReturnsExtensions;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading.Tasks;
    using Xunit;

    public class ChatBotSendMessageTests
    {
        private BotEndpointSettings botServiceSettings;
        private IHttpClientFactory httpClientFactory;
        private ICacheService cacheService;

        [Fact]
        public async Task SendMultipleMessageAsync_WhenChangeConfig_ThrowsMissConfigException()
        {
            IEnumerable<NotificationMessage> param = FakeListNotificationMessage();
            var service = GetService();
            cacheService.GetAsync<string>(Arg.Any<string>()).ReturnsNull();
            FakeHttpClientResponse();

            await Assert.ThrowsAsync<ArgumentException>(() => service.SendMultiplePlaintextMessageAsync((BotChannel)3, param));
        }

        [Fact]
        public async Task SendMultipleMessageAsync_WhenNoCache_CallHttpClient2Times_AndCallCacheServiceSetAsync()
        {
            IEnumerable<NotificationMessage> param = FakeListNotificationMessage();
            var service = GetService();
            cacheService.GetAsync<string>(Arg.Any<string>()).ReturnsNull();
            FakeHttpClientResponse();

            var actual = await service.SendMultiplePlaintextMessageAsync(BotChannel.Telegram, param);

            httpClientFactory.Received(2).CreateClient();
            await cacheService.Received(1).SetAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CacheItemOptions>());
            Assert.NotNull(actual);
            Assert.Equal(4, actual.Count());
        }

        [Fact]
        public async Task SendMultipleMessageAsync_WhenHaveCache_CallHttpClient1Times_AndNoCallCacheServiceSetAsync()
        {
            IEnumerable<NotificationMessage> param = FakeListNotificationMessage();
            var service = GetService();
            cacheService.GetAsync<string>(Arg.Any<string>()).Returns("token");
            FakeHttpClientResponse();

            var actual = await service.SendMultiplePlaintextMessageAsync(BotChannel.Telegram, param);

            httpClientFactory.Received(1).CreateClient();
            await cacheService.DidNotReceive().SetAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CacheItemOptions>());
            Assert.NotNull(actual);
            Assert.Equal(4, actual.Count());
        }

        [Fact]
        public async Task SendMultipleMessageAsync_WhenStatusCodeNotEqualOK_ThrowsException()
        {
            IEnumerable<NotificationMessage> param = FakeListNotificationMessage();
            var service = GetService();
            cacheService.GetAsync<string>(Arg.Any<string>()).Returns("token");
            FakeHttpClientResponse(HttpStatusCode.Forbidden);

            await Assert.ThrowsAsync<HttpRequestException>(() => service.SendMultiplePlaintextMessageAsync(BotChannel.Letstalk, param));
        }

        [Fact]
        public async Task SendSingleMessageAsync_WhenNoCache_CallHttpClient2Times_AndCallCacheServiceSetAsync()
        {
            NotificationMessage param = FakeNotificationMessage();
            var service = GetService();
            cacheService.GetAsync<string>(Arg.Any<string>()).ReturnsNull();
            FakeHttpClientResponse();

            var actual = await service.SendSinglePlainTexMessageAsync(BotChannel.Letstalk, param);

            httpClientFactory.Received(2).CreateClient();
            await cacheService.Received(1).SetAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CacheItemOptions>());
            Assert.NotNull(actual);
            Assert.Equal(100, actual.NotificationId);
            Assert.Equal(100, actual.ConversationGroupId);
            Assert.True(actual.IsSuccessful);
            Assert.False(actual.IsIgnoreRetry);
            Assert.False(actual.IsBlockedBot);
        }

        [Fact]
        public async Task SendSingleMessageAsync_WhenHaveCache_CallHttpClient1Times_AndNoCallCacheServiceSetAsync()
        {
            NotificationMessage param = FakeNotificationMessage();
            var service = GetService();
            cacheService.GetAsync<string>(Arg.Any<string>()).Returns("token");
            FakeHttpClientResponse();

            var actual = await service.SendSinglePlainTexMessageAsync(param.ChannelId, param);

            httpClientFactory.Received(1).CreateClient();
            await cacheService.DidNotReceive().SetAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CacheItemOptions>());
            Assert.NotNull(actual);
            Assert.Equal(100, actual.NotificationId);
            Assert.Equal(100, actual.ConversationGroupId);
            Assert.True(actual.IsSuccessful);
            Assert.False(actual.IsIgnoreRetry);
            Assert.False(actual.IsBlockedBot);
        }

        [Fact]
        public async Task SendSingleMessageAsync_WhenStatusCodeNotEqualOK_ThrowsException()
        {
            NotificationMessage param = FakeNotificationMessage();

            var service = GetService();
            cacheService.GetAsync<string>(Arg.Any<string>()).Returns("token");
            FakeHttpClientResponse(HttpStatusCode.Forbidden);

            await Assert.ThrowsAsync<HttpRequestException>(() => service.SendSinglePlainTexMessageAsync(BotChannel.Letstalk, param));
        }

        private static NotificationMessage FakeNotificationMessage() => new()
        {
            NotificationId = 1,
            ConversationGroupId = 1,
            BotId = 1,
            ChannelId = BotChannel.Letstalk
        };

        private static IEnumerable<NotificationMessage> FakeListNotificationMessage()
        {
            return new List<NotificationMessage>
            {
                new() {NotificationId = 1, ConversationGroupId = 1, BotId = 1, ChannelId = BotChannel.Letstalk},
                new() {NotificationId = 2, ConversationGroupId = 2, BotId = 1, ChannelId = BotChannel.Letstalk},
                new() {NotificationId = 3, ConversationGroupId = 3, BotId = 2, ChannelId = BotChannel.Telegram},
                new() {NotificationId = 4, ConversationGroupId = 4, BotId = 2, ChannelId = BotChannel.Telegram}
            };
        }

        #region Helpers

        private void FakeHttpClientResponse(HttpStatusCode status = HttpStatusCode.OK)
        {
            IEnumerable<SendResult> fakeResult = FakeListSendResult();
            SendResult signeResult = new SendResult { NotificationId = 100, ConversationGroupId = 100, ErrorCode = 0, IsBlockedBot = false, IsIgnoreRetry = false };
            var fakeTokenResult = new
            {
                accessToken = "token",
                expriedDate = new DateTime(2100, 1, 1)
            };
            var httpClientFake = new Dictionary<string, HttpResponseMessage>
            {
                { botServiceSettings.ServiceUrl + "api/authenticate", new HttpResponseMessage(status) { Content = new StringContent(JsonConvert.SerializeObject(fakeTokenResult))} },
                { botServiceSettings.ServiceUrl + "api/multiple-notify", new HttpResponseMessage(status) { Content = new StringContent(JsonConvert.SerializeObject(fakeResult))} },
                { botServiceSettings.ServiceUrl + "api/single-notify", new HttpResponseMessage(status) { Content = new StringContent(JsonConvert.SerializeObject(signeResult))} },
            };
            httpClientFactory.CreateClient().Returns(new HttpClient(new MockHttpMessageHandler(httpClientFake)));
        }

        private static IEnumerable<SendResult> FakeListSendResult()
        {
            return new List<SendResult>
            {
                new() {NotificationId = 1, ConversationGroupId = 1, ErrorCode = 0, IsBlockedBot = false, IsIgnoreRetry = false},
                new() {NotificationId = 2, ConversationGroupId = 2, ErrorCode = 0, IsBlockedBot = true, IsIgnoreRetry = true},
                new() {NotificationId = 3, ConversationGroupId = 3, ErrorCode = 1, IsBlockedBot = false, IsIgnoreRetry = false},
                new() {NotificationId = 4, ConversationGroupId = 4, ErrorCode = 1, IsBlockedBot = true, IsIgnoreRetry = true}
            };
        }

        private ChatBotSendMessageService GetService()
        {
            botServiceSettings = new BotEndpointSettings
            {
                ServiceUrl = "http://bot.com/",
                AppId = "Id",
                Token = "1234aa",
                AuthenticatePath = "api/authenticate",
                ApiConfigs =
                [
                    new ApiConfig
                    {
                        Channel = BotChannel.Letstalk, SendMultipleMessagePath = "api/multiple-notify",
                        SendSingleMessagePath = "api/single-notify"
                    },
                    new ApiConfig
                    {
                        Channel = BotChannel.Telegram, SendMultipleMessagePath = "api/multiple-notify",
                        SendSingleMessagePath = "api/single-notify"
                    }
                ],
            };
            httpClientFactory = Substitute.For<IHttpClientFactory>();
            cacheService = Substitute.For<ICacheService>();
            return new ChatBotSendMessageService(botServiceSettings, httpClientFactory, cacheService);
        }

        #endregion Helpers
    }
}
namespace CRM.UnitTests.Services.AutoFeedback
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoFixture;
    using CRM.Criteria.ProblemsAccount;
    using CRM.Criteria.TurnoverControl;
    using CRM.Criteria.WinLossLimit;
    using CRM.Models.Common;
    using CRM.Models.ProblemsAccount;
    using CRM.Models.TurnoverControl;
    using CRM.Models.WinLossLimit;
    using CRM.Services.AutoFeedback;
    using CRM.Services.Common;
    using NSubstitute;
    using Xunit;

    public class FeedbackTransactionServiceTests
    {
        private IBaseService baseService;

        [Fact]
        public async Task EditFeedbackWinLossInfoAsync_WhenCalled_ReturnExpectedCommandResult()
        {
            // Arrange
            var fixture = new Fixture();
            var expected = fixture.Create<CRM.Models.Common.CommandResult>();
            var criteria = new EditFeedbackWinLossInfoCommand();
            var service = GetFeedbackTransactionService();
            baseService.ExecuteNonQueryAsync(Arg.Do<EditFeedbackWinLossInfoCommand>(x => criteria = x)).Returns(expected);

            // Act
            var actual = await service.EditFeedbackWinLossInfoAsync(12345, "Completed", "");

            // Assert
            Assert.Equal(expected, actual);
            Assert.Equal(12345, criteria.LogID);
            Assert.Equal("Completed", criteria.Status);
            Assert.Equal("", criteria.Description);
        }

        #region ImportWinLimitFeedbackInfoAsync

        [Fact]
        public async Task ImportWinLimitFeedbackInfoAsync_CustomerLimitNull_ReturnValueTupleNull()
        {
            // Arrange

            var winLimitReturn = new ValueTuple<IEnumerable<WinLimitFeedBackInfo>, IEnumerable<BettypeInfo>, IEnumerable<SportInfo>>(new List<WinLimitFeedBackInfo>(), new List<BettypeInfo>(), new List<SportInfo>());
            var criteria = new ImportWinLossFeedbackInfoCriteria();
            var service = GetFeedbackTransactionService();
            baseService.FetchMultipleAsync<WinLimitFeedBackInfo, BettypeInfo, SportInfo>(Arg.Do<ImportWinLossFeedbackInfoCriteria>(x => criteria = x)).Returns(winLimitReturn);

            // Act
            var actual = await service.ImportWinLimitFeedbackInfoAsync(new List<CustomerWinLimit>(), new List<GroupHeadline>());

            // Assert

            Assert.Empty(actual.Item1);
            Assert.Empty(actual.Item2);
            Assert.Equal("<ROOT></ROOT>", criteria.xml);
        }

        [Fact]
        public async Task ImportWinLimitFeedbackInfoAsync_MaxWinLimitSettingNull_ReturnExpectedXML()
        {
            // Arrange
            var cusWinLimit = new List<CustomerWinLimit>
            {
                new()
                {
                    LogID = 1,
                    MaxWinLimitSetting = "",
                    ProductID = 1,
                    ProductGroup = "1",
                    Level = "1",
                    ReachedTime = new DateTime(2020, 11, 13, 5, 10, 20, DateTimeKind.Utc),
                    BettypeIDList = "1",
                    CustId = 1,
                    TotalWinAmt = 1000,
                    TotalWinAmt_RM = 1000
                }
            };
            var headlines = new List<GroupHeadline>
            {
                new()
                {
                    CustId = 1,
                    HeadlineId = 1,
                    HeadlineName = "1"
                }
            };

            var winLimitReturn = new ValueTuple<IEnumerable<WinLimitFeedBackInfo>, IEnumerable<BettypeInfo>, IEnumerable<SportInfo>>(new List<WinLimitFeedBackInfo>(), new List<BettypeInfo>(), new List<SportInfo>());
            var service = GetFeedbackTransactionService();
            baseService.FetchMultipleAsync<WinLimitFeedBackInfo, BettypeInfo, SportInfo>(Arg.Do<ImportWinLossFeedbackInfoCriteria>(x => _ = x)).Returns(winLimitReturn);

            // Act
            var actual = await service.ImportWinLimitFeedbackInfoAsync(cusWinLimit, headlines);

            // Assert

            Assert.Equal(winLimitReturn, actual);
        }

        [Fact]
        public async Task ImportWinLimitFeedbackInfoAsync_MaxWinLimitSettingNotNull_ReturnExpectedXML()
        {
            // Arrange
            var cusWinLimit = new List<CustomerWinLimit>
            {
                new()
                {
                    LogID = 1,
                    MaxWinLimitSetting = "<MaxWinLimitSetting Level1='1000' Level2='2000' Level3='5000' Level101='0.00' Level102='0.00' />",
                    ProductID = 1,
                    ProductGroup = "1",
                    Level = "1",
                    LevelID = 3,
                    ReachedTime = new DateTime(2020, 11, 13, 5, 10, 20, DateTimeKind.Utc),
                    BettypeIDList = "1",
                    CustId = 1,
                    TotalWinAmt = 1000,
                    TotalWinAmt_RM = 1000
                }
            };
            var headlines = new List<GroupHeadline>
            {
                new()
                {
                    CustId = 1,
                    HeadlineId = 1,
                    HeadlineName = "1"
                }
            };

            var winLimitReturn = new ValueTuple<IEnumerable<WinLimitFeedBackInfo>, IEnumerable<BettypeInfo>, IEnumerable<SportInfo>>(new List<WinLimitFeedBackInfo>(), new List<BettypeInfo>(), new List<SportInfo>());
            var service = GetFeedbackTransactionService();
            baseService.FetchMultipleAsync<WinLimitFeedBackInfo, BettypeInfo, SportInfo>(Arg.Do<ImportWinLossFeedbackInfoCriteria>(x => _ = x)).Returns(winLimitReturn);

            // Act
            var actual = await service.ImportWinLimitFeedbackInfoAsync(cusWinLimit, headlines);

            // Assert

            Assert.Equal(winLimitReturn, actual);
        }

        #endregion ImportWinLimitFeedbackInfoAsync

        [Theory]
        [InlineData(0, 0)]
        [InlineData(1, 100)]
        [InlineData(2, 200)]
        [InlineData(3, 300)]
        [InlineData(101, 400)]
        [InlineData(102, 500)]
        public void GetLimitSetting_WhenCalled_ReturnExpectedAmount(int levelID, int expect)
        {
            // Arrange
            var setting = new MaxWinLimitSetting
            {
                Level1 = 100,
                Level2 = 200,
                Level3 = 300,
                Level101 = 400,
                Level102 = 500
            };
            var service = GetFeedbackTransactionService();

            // Act
            var actual = service.Public_GetLimitSetting(setting, levelID);

            // Assert
            Assert.Equal(expect, actual);
        }

        [Fact]
        public async Task ImportTOControlFeedbackInfoAsync_CustOutStandingHasData_ReturnExpectedXML()
        {
            // Arrange
            var custNotifications = new List<TONotification>
            {
                new()
                {
                   LogID = 1,
                   CustID = 1,
                   MatchID = 1,
                   ProfileID = 1,
                   ProfileName = "1",
                   MaxTurnOver = 1,
                   HouseTurnover = 1,
                   TotalTurnover = 1,
                   Bettype = "1",
                   ReachLevel = 1
                }
            };
            var headlines = new List<GroupHeadline>
            {
                new()
                {
                    CustId = 1,
                    HeadlineId = 1,
                    HeadlineName = "1"
                }
            };
            var custOutStandings = new List<CustOutStanding>
            {
                new()
                {
                    CustID = 1,
                    MatchID = 1,
                    Outstanding = 1
                }
            };

            const string expected = "<ROOT><r logid='1' custid='1' matchid='1' profileID='1' hl='1' hlname='1' related='1' profileName='1' maxTO='1' " +
                                        "houseTO='1' totalTO='1' outstanding='1' bettype='1' level='1'/></ROOT>";
            var criteria = new InsertTOControlAutoFeedbackInfoCommand();
            var service = GetFeedbackTransactionService();
            baseService.GetDataAsync<TOControlFeedbackInfo>(Arg.Do<InsertTOControlAutoFeedbackInfoCommand>(x => criteria = x)).Returns(new List<TOControlFeedbackInfo>());

            // Act
            await service.ImportTOControlFeedbackInfoAsync(headlines, custOutStandings, custNotifications);

            // Assert

            Assert.Equal(expected, criteria.xml);
        }

        [Fact]
        public async Task ImportTOControlFeedbackInfoAsync_CustOutStandingNoData_ReturnExpectedXML()
        {
            // Arrange
            var custNotifications = new List<TONotification>
            {
                new()
                {
                   LogID = 1,
                   CustID = 1,
                   MatchID = 1,
                   ProfileID = 1,
                   ProfileName = "1",
                   MaxTurnOver = 1,
                   HouseTurnover = 1,
                   TotalTurnover = 1,
                   Bettype = "1",
                   ReachLevel = 1
                }
            };
            var headlines = new List<GroupHeadline>
            {
                new()
                {
                    CustId = 1,
                    HeadlineId = 1,
                    HeadlineName = "1"
                }
            };
            var custOutStandings = new List<CustOutStanding>
            {
                new()
                {
                    CustID = 0,
                    MatchID = 1,
                    Outstanding = 1
                }
            };

            const string expected = "<ROOT><r logid='1' custid='1' matchid='1' profileID='1' hl='1' hlname='1' related='1' profileName='1' maxTO='1' " +
                                        "houseTO='1' totalTO='1' outstanding='0' bettype='1' level='1'/></ROOT>";
            var criteria = new InsertTOControlAutoFeedbackInfoCommand();
            var service = GetFeedbackTransactionService();
            baseService.GetDataAsync<TOControlFeedbackInfo>(Arg.Do<InsertTOControlAutoFeedbackInfoCommand>(x => criteria = x)).Returns(new List<TOControlFeedbackInfo>());

            // Act
            await service.ImportTOControlFeedbackInfoAsync(headlines, custOutStandings, custNotifications);

            // Assert

            Assert.Equal(expected, criteria.xml);
        }

        [Fact]
        public async Task ImportProblemsAccountFeedbackInfoAsync_CustCategoryHasData_ReturnExpectedXML()
        {
            // Arrange
            var custCategoryInfo = new List<CustCategoryInfo>
            {
                new()
                {
                    LogID = 1,
                    CustID = 1,
                    CategoryID = 1
                }
            };
            var headlines = new List<GroupHeadline>
            {
                new()
                {
                    CustId = 1,
                    HeadlineId = 1,
                    HeadlineName = "1"
                }
            };

            const string expected = "<ROOT><r logid='1' custid='1' hl='1' hlname='1' related='1' catid='1'/></ROOT>";
            var criteria = new InsertProblemsAccountAutoFeedbackInfoCommand();
            var service = GetFeedbackTransactionService();
            baseService.GetDataAsync<ProblemsAccountAutoFeedbackInfo>(Arg.Do<InsertProblemsAccountAutoFeedbackInfoCommand>(x => criteria = x)).Returns(new List<ProblemsAccountAutoFeedbackInfo>());

            // Act
            await service.ImportProblemsAccountFeedbackInfoAsync(headlines, custCategoryInfo, "LIVE");

            // Assert

            Assert.Equal(expected, criteria.xml);
        }

        [Fact]
        public async Task ImportProblemsAccountFeedbackInfoAsync_CustCategoryNotEqualHeadline_ReturnExpectedXML()
        {
            // Arrange
            var custCategoryInfo = new List<CustCategoryInfo>
            {
                new()
                {
                    LogID = 1,
                    CustID = 2,
                    CategoryID = 1
                }
            };
            var headlines = new List<GroupHeadline>
            {
                new()
                {
                    CustId = 1,
                    HeadlineId = 1,
                    HeadlineName = "1"
                }
            };

            const string expected = "<ROOT><r logid='1' custid='2' hl='0' hlname='' related='2' catid='1'/></ROOT>";
            var criteria = new InsertProblemsAccountAutoFeedbackInfoCommand();
            var service = GetFeedbackTransactionService();
            baseService.GetDataAsync<ProblemsAccountAutoFeedbackInfo>(Arg.Do<InsertProblemsAccountAutoFeedbackInfoCommand>(x => criteria = x)).Returns(new List<ProblemsAccountAutoFeedbackInfo>());

            // Act
            await service.ImportProblemsAccountFeedbackInfoAsync(headlines, custCategoryInfo, "LIVE");

            // Assert

            Assert.Equal(expected, criteria.xml);
        }

        [Fact]
        public async Task EditTOControlFeedbackInfoAsync_WhenCalled_ReturnExpectedCommandResult()
        {
            // Arrange
            var fixture = new Fixture();
            var expected = fixture.Create<CRM.Models.Common.CommandResult>();
            var criteria = new EditTOControlAutoFeedbackInfoUpdCommand();
            var service = GetFeedbackTransactionService();
            baseService.ExecuteNonQueryAsync(Arg.Do<EditTOControlAutoFeedbackInfoUpdCommand>(x => criteria = x)).Returns(expected);

            // Act
            var actual = await service.EditTOControlFeedbackInfoAsync("12345", "Completed", "");

            // Assert
            Assert.Equal(expected, actual);
            Assert.Equal("12345", criteria.LogID);
            Assert.Equal("Completed", criteria.Status);
            Assert.Equal("", criteria.Description);
        }

        [Fact]
        public async Task EditProblemsAccountFeedbackInfoAsync_WhenCalled_ReturnExpectedCommandResult()
        {
            // Arrange
            var fixture = new Fixture();
            var expected = fixture.Create<CRM.Models.Common.CommandResult>();
            var criteria = new EditProblemsAccountAutoFeedbackInfoCommand();
            var service = GetFeedbackTransactionService();
            baseService.ExecuteNonQueryAsync(Arg.Do<EditProblemsAccountAutoFeedbackInfoCommand>(x => criteria = x)).Returns(expected);

            // Act
            var actual = await service.EditProblemsAccountFeedbackInfoAsync("12345", "Completed", "");

            // Assert
            Assert.Equal(expected, actual);
            Assert.Equal("12345", criteria.LogID);
            Assert.Equal("Completed", criteria.Status);
            Assert.Equal("", criteria.Description);
        }

        #region Helpers

        public class FeedbackTransactionServiceTestable(IBaseService baseService)
            : FeedbackTransactionService(baseService)
        {
            public double Public_GetLimitSetting(MaxWinLimitSetting setting, int levelID)
            {
                return base.GetLimitSetting(setting, levelID);
            }
        }

        private FeedbackTransactionServiceTestable GetFeedbackTransactionService()
        {
            baseService = Substitute.For<IBaseService>();
            return Substitute.For<FeedbackTransactionServiceTestable>(baseService);
        }

        #endregion Helpers
    }
}
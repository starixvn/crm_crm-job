namespace CRM.BackgroundJob.UnitTests.Criteria.VoidingSettledTickets
{
    using System;
    using CRM.BackgroundJob.Criteria.VoidingSettledTickets;
    using Xunit;

    public class GetAutoVoidingMessageCriteriaTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new GetAutoVoidingMessageCriteria
            {
                BatchSize = 10,
                Environment = "LIVE",
                LastDateUAT = new DateTime()
            };
            var expected = "Acc_CRM_AutoVoidingMessageSel";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_WhenCalled_ReturnTrue()
        {
            // Arrange
            var criteria = new GetAutoVoidingMessageCriteria();

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}

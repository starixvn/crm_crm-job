namespace CRM.BackgroundJob.UnitTests.Criteria.VoidingSettledTickets
{
    using System.Collections.Generic;
    using CRM.BackgroundJob.Criteria.VoidingSettledTickets;
    using Xunit;

    public class GetAutoVoidingIssueTypeCriteriaTests
    {
        [Fact]
        public void GetSettingKey_WhenCalled_ReturnExpectedSettingKey()
        {
            // Arrange
            var criteria = new GetAutoVoidingIssueTypeCriteria();
            var expected = "Acc_CRM_AutoVoidingIssueType_Sel";

            // Act
            var actual = criteria.GetSettingKey();

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void IsValid_WhenCalled_ReturnTrue()
        {
            // Arrange
            var criteria = new GetAutoVoidingIssueTypeCriteria
            {
                VoidingMessages = new List<int> { 1, 2, 3 }
            };

            // Act
            var actual = criteria.IsValid();

            // Assert
            Assert.True(actual);
        }
    }
}

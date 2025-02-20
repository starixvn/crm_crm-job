namespace CRM.BackgroundJob.Services;

using System.Collections.Generic;
using System.Threading.Tasks;
using CRM.BackgroundJob.Criteria.PushResultAmendment;
using CRM.BackgroundJob.Models.MessageNotificationCenter;
using CRM.Services;

public interface IPushNotificationResultAmendmentService
{
    Task<IEnumerable<MncMessageTarget>> GetPrepareNotificationAsync(string environment);

    Task GetMessageResultAmendmentAsync(GetMessageResultAmendmentCommand command);

    Task<(bool isTest, byte approveMode)> EditPushNotificationResultAmendmentAsync(int notificationID, int messageID, string environment, string updType);

    Task<AdminMessageSite> EditPushMessageResultAmendmentAsync(int messageID, string updType, bool isTest, byte approveMode);

    Task InsertCompleteResultAmendmentAsync(string environment);

    Task InsertAdminMessageSiteAsync(AdminMessageSite adminMessage);
}

public class PushNotificationResultAmendmentService(IBaseService baseService) : IPushNotificationResultAmendmentService
{
    public async Task<IEnumerable<MncMessageTarget>> GetPrepareNotificationAsync(string environment)
    {
        var criteria = new GetPrepareNotiResultAmendmentCriteria
        {
            Environment = environment
        };
        return await baseService.GetDataAsync<MncMessageTarget>(criteria);
    }

    public async Task GetMessageResultAmendmentAsync(GetMessageResultAmendmentCommand command)
    {
        await baseService.ExecuteNonQueryAsync(command);
    }

    public async Task<(bool isTest, byte approveMode)> EditPushNotificationResultAmendmentAsync(int notificationID, int messageID, string environment, string updType)
    {
        var command = new EditNotiResultAmendmentCommand
        {
            NotificationID = notificationID,
            MessageID = messageID,
            Environment = environment,
            UpdType = updType
        };
        await baseService.ExecuteNonQueryAsync(command);
        return (command.IsTest, command.ApproveMode);
    }

    public async Task<AdminMessageSite> EditPushMessageResultAmendmentAsync(int messageID, string updType, bool isTest, byte approveMode)
    {
        var criteria = new EditMessageResultAmendmentCriteria
        {
            MessageID = messageID,
            UpdType = updType,
            IsTest = isTest,
            NotiApproveMode = approveMode
        };
        return await baseService.GetDataEntityAsync<AdminMessageSite>(criteria);
    }

    public async Task InsertCompleteResultAmendmentAsync(string environment)
    {
        var command = new InsertCompleteResultAmendmentCommand
        {
            Environment = environment
        };
        await baseService.ExecuteNonQueryAsync(command);
    }

    public async Task InsertAdminMessageSiteAsync(AdminMessageSite adminMessage)
    {
        var command = new InsertInfoAdminSiteCommand
        {
            Men = adminMessage.Men,
            Mch = adminMessage.Mch,
            Member = adminMessage.Member,
            Agent = adminMessage.Agent,
            Ma = adminMessage.Ma,
            Sma = adminMessage.Sma,
            Creator = adminMessage.Creator,
            NewMessageID = adminMessage.NewMessageID,
            Remark = adminMessage.Remark
        };
        await baseService.ExecuteNonQueryAsync(command);
    }
}
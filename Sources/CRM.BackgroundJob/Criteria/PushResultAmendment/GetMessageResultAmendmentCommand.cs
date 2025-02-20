namespace CRM.BackgroundJob.Criteria.PushResultAmendment;

using CRM.Models;

public class GetMessageResultAmendmentCommand : BaseNonQueryCommand
{
    public int NotificationID { get; set; }

    public string JSNotification { get; set; }

    public int MessageID { get; set; }

    public override string GetSettingKey() => "MNC_PushNotification_MsgIns";

    public override bool IsValid() => true;
}
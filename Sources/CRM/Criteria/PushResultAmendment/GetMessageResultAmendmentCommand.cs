namespace CRM.Criteria.PushResultAmendment;

public class GetMessageResultAmendmentCommand : CRM.Models.Common.BaseNonQueryCommand
{
    public int NotificationID { get; set; }

    public string JSNotification { get; set; }

    public int MessageID { get; set; }

    public override string GetSettingKey() => "MNC_PushNotification_MsgIns";

    public override bool IsValid() => true;
}
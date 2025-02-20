namespace CRM.BackgroundJob.Criteria.PushResultAmendment;

using CRM.Models;

public class EditNotiResultAmendmentCommand : BaseNonQueryCommand
{
    public int NotificationID { get; set; }

    public string Environment { get; set; }

    public int MessageID { get; set; }

    public string UpdType { get; set; }

    public bool IsTest { get; set; }

    public byte ApproveMode { get; set; }

    public override string GetSettingKey() => "MNC_PushNotification_NotiUpd";

    public override bool IsValid() => true;
}
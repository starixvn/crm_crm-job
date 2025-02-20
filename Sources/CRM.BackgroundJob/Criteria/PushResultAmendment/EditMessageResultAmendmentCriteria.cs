namespace CRM.BackgroundJob.Criteria.PushResultAmendment;

using Fanex.Data.Repository;

public class EditMessageResultAmendmentCriteria : CriteriaBase
{
    public int MessageID { get; set; }

    public string UpdType { get; set; }

    public bool IsTest { get; set; }

    public byte NotiApproveMode { get; set; }

    public override string GetSettingKey() => "MNC_PushNotification_MsgUpd";

    public override bool IsValid() => true;
}